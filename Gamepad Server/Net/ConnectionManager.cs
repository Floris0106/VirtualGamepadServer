using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace GamepadServer.Net;

public class ConnectionManager
{
	private readonly UdpClient client;
	
	private readonly ConcurrentQueue<(byte, ClientboundPacket)> sendQueue = new();
	private readonly ConcurrentDictionary<Guid, byte> connectionIds = new();
	private readonly ConcurrentDictionary<byte, IPEndPoint> endpoints = new();
	private readonly ConcurrentDictionary<byte, DateTime> lastHeartbeats = new();
	
	private readonly CancellationTokenSource cancellationTokenSource = new();
	private readonly TaskFactory taskFactory;
	
	private readonly Action<byte, ServerboundPacket, IPEndPoint> packetHandler;

	public ConnectionManager(int port, Action<byte, ServerboundPacket, IPEndPoint> packetHandler, Action<byte> disconnectHandler)
	{
		this.packetHandler = packetHandler;
		
		client = new(port);
		taskFactory = new(cancellationTokenSource.Token);

		new Thread(token =>
		{
			CancellationToken cancellationToken = (CancellationToken)token!;
			while (!cancellationToken.IsCancellationRequested)
			{
				if (!sendQueue.TryDequeue(out (byte connectionId, ClientboundPacket packet) data))
					continue;
				
				using MemoryStream stream = new();
				BinaryWriter writer = new(stream);
				
				writer.Write(data.packet.Id);
				data.packet.Encode(writer);
				writer.Flush();
				
				client.Send(stream.GetBuffer(), (int)stream.Length, endpoints[data.connectionId]);
			}
		}).Start(cancellationTokenSource.Token);

		new Thread(token =>
		{
			CancellationToken cancellationToken = (CancellationToken)token!;
			while (!cancellationToken.IsCancellationRequested)
			{
				foreach (KeyValuePair<byte, DateTime> pair in lastHeartbeats)
					if (DateTime.Now > pair.Value.AddSeconds(10))
					{
						Guid key = connectionIds.FirstOrDefault(id => id.Value == pair.Key).Key;
						connectionIds.TryRemove(key, out _);
						endpoints.TryRemove(pair.Key, out _);
						lastHeartbeats.TryRemove(pair.Key, out _);
						
						disconnectHandler(pair.Key);
					}
				Thread.Sleep(1000);
			}
		}).Start(cancellationTokenSource.Token);
	}
	
	public void Send(byte connectionId, ClientboundPacket packet)
	{
		sendQueue.Enqueue((connectionId, packet));
	}

	public async Task Receive()
	{
		UdpReceiveResult result = await client.ReceiveAsync(cancellationTokenSource.Token);
		(byte connectionId, ServerboundPacket serverboundPacket) = ServerboundPacket.Decode(result.Buffer);
		if (connectionId != 0)
		{
			if (!endpoints.ContainsKey(connectionId))
				return;
			lastHeartbeats[connectionId] = DateTime.Now;
		}
		taskFactory.StartNew(data =>
		{
			(byte id, ServerboundPacket packet, IPEndPoint endpoint) = (ValueTuple<byte, ServerboundPacket, IPEndPoint>)data!;
			packetHandler(id, packet, endpoint);
		}, (connectionId, serverboundPacket, result.RemoteEndPoint));
	}

	public bool AddConnection(Guid token, IPEndPoint endPoint, out byte connectionId)
	{
		if (connectionIds.TryGetValue(token, out connectionId))
			return false;
		
		HashSet<byte> takenIds = new(connectionIds.Values);
		connectionId = 1;
		while (takenIds.Contains(connectionId))
			connectionId++;

		connectionIds.TryAdd(token, connectionId);
		endpoints.TryAdd(connectionId, endPoint);
		return true;
	}

	public void Close()
	{
		cancellationTokenSource.Cancel();
		client.Close();
	}
}