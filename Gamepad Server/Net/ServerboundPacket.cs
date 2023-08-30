namespace GamepadServer.Net;

public abstract class ServerboundPacket
{
    private static readonly Dictionary<byte, Func<BinaryReader, ServerboundPacket>> PacketTypes = new()
    {
	    { 0x00, reader => new ConnectionRequestPacket(reader) },
	    { 0x01, reader => new GamepadStatePacket(reader) },
	    { 0xFF, _ => new ServerboundHeartbeatPacket()}
    };
    
    public static (byte, ServerboundPacket) Decode(byte[] bytes)
    {
	    BinaryReader reader = new(new MemoryStream(bytes[2..]));
	    ServerboundPacket packet = PacketTypes[bytes[1]](reader);
	    reader.Close();
	    return (bytes[0], packet);
    }
}