namespace GamepadServer.Net;

public class ConnectionRequestPacket : ServerboundPacket
{
	public readonly Guid Token;
	
	public ConnectionRequestPacket(BinaryReader reader)
	{
		Token = new(reader.ReadBytes(16));
	}
}