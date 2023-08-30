namespace GamepadServer.Net;

public class ConnectionIdPacket : ClientboundPacket
{
	public override byte Id => 0x00;

	public byte connectionId;

	public ConnectionIdPacket(byte id)
	{
		connectionId = id;
	}
	
	public override void Encode(BinaryWriter writer)
	{
		writer.Write(connectionId);
	}
}