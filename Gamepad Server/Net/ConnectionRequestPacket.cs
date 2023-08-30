namespace GamepadServer.Net;

public class ConnectionRequestPacket : ServerboundPacket
{
	public readonly Guid token;
	
	public ConnectionRequestPacket(BinaryReader reader)
	{
		token = new(reader.ReadBytes(16));
	}
}