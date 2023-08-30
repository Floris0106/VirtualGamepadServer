namespace GamepadServer;

public static class Remap
{
	public static byte ToByte(float value)
	{
		return (byte)Math.Round((value * 0.5f + 0.5f) * byte.MaxValue);
	}

	public static short ToShort(float value)
	{
		return (short)(Math.Round((value * 0.5f + 0.5f) * ushort.MaxValue) + short.MinValue);
	}
	
	public static byte ToByte(bool value)
	{
		return value ? byte.MaxValue : byte.MinValue;
	}
}