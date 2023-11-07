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
	private readonly ConcurrentDictionary<byte, Thread> handlers = new();
	private readonly ConcurrentDictionary<byte, ConcurrentQueue<ServerboundPacket>> receiveQueues = new();

	private readonly CancellationTokenSource cancellationTokenSource = new();

	private readonly Action<byte, ServerboundPacket, IPEndPoint> packetHandler;

	public ConnectionManager(int port, Action<byte, ServerboundPacket, IPEndPoint> packetHandler, Action<byte> disconnectHandler)
	{
		this.packetHandler = packetHandler;

		client = new(port);

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

				if (endpoints.TryGetValue(data.connectionId, out IPEndPoint? endPoint))
					client.Send(stream.GetBuffer(), (int)stream.Length, endpoints[data.connectionId]);
			}
		}).Start(cancellationTokenSource.Token);

		new Thread(token =>
		{
			CancellationToken cancellationToken = (CancellationToken)token!;
			while (!cancellationToken.IsCancellationRequested)
			{
				foreach (byte connectionId in connectionIds.Values)
					Send(connectionId, new HeartbeatPacket());
				Thread.Sleep(100);
				foreach (KeyValuePair<byte, DateTime> connection in lastHeartbeats)
					if (DateTime.Now > connection.Value.AddSeconds(5))
					{
						connectionIds.TryRemove(connectionIds.First(pair => pair.Value == connection.Key).Key, out _);
						endpoints.TryRemove(connection.Key, out _);
						lastHeartbeats.TryRemove(connection.Key, out _);

						disconnectHandler(connection.Key);
					}
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
		
		if (receiveQueues.TryGetValue(connectionId, out ConcurrentQueue<ServerboundPacket>? receiveQueue))
			receiveQueue.Enqueue(serverboundPacket);
		else if (connectionId == 0)
			packetHandler(connectionId, serverboundPacket, result.RemoteEndPoint);
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
		lastHeartbeats[connectionId] = DateTime.Now;
		if (!handlers.ContainsKey(connectionId))
		{
			receiveQueues.TryAdd(connectionId, new());
			
			Thread handler = new(data =>
			{
				(byte id, CancellationToken cancellationToken) = (ValueTuple<byte, CancellationToken>)data!;
				ConcurrentQueue<ServerboundPacket> receiveQueue = receiveQueues[id];
				while (!cancellationToken.IsCancellationRequested)
					if (receiveQueue.TryDequeue(out ServerboundPacket? packet))
					{
						packetHandler(id, packet, endpoints[id]);
						if (id != 0)
							lastHeartbeats[id] = DateTime.Now;
					}
			});
			handler.Start((connectionId, cancellationTokenSource.Token));
			handlers.TryAdd(connectionId, handler);
		}
		
		return true;
	}

	public void Close()
	{
		cancellationTokenSource.Cancel();
		client.Close();
	}
}