namespace GamepadServer.Net;

public class HeartbeatPacket : ClientboundPacket
{
	public override byte Id => 0xFF;
	
	public override void Encode(BinaryWriter writer)
	{
		
	}
}