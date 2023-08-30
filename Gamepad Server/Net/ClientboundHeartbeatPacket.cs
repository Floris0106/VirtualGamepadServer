namespace GamepadServer.Net;

public class ClientboundHeartbeatPacket : ClientboundPacket
{
	public override byte Id => 0xFF;
	
	public override void Encode(BinaryWriter writer)
	{
		
	}
}