namespace GamepadServer.Net;

public abstract class ClientboundPacket
{
	public abstract byte Id { get; }
	
	public abstract void Encode(BinaryWriter writer);
}