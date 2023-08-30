using System.Numerics;

namespace GamepadServer.Emulation;

public struct GamepadState
{
	public bool A;
	public bool B;
	public bool X;
	public bool Y;
	public bool DpadUp;
	public bool DpadDown;
	public bool DpadLeft;
	public bool DpadRight;
	public Vector2 LeftStick;
	public Vector2 RightStick;
	public bool L;
	public bool ZL;
	public bool R;
	public bool ZR;
	public bool Minus;
	public bool Plus;
	
	public int DpadX
	{
		get
		{
			int value = 0;
			if (DpadLeft)
				value--;
			if (DpadRight)
				value++;
			return value;
		}
	}
	public int DpadY
	{
		get
		{
			int value = 0;
			if (DpadUp)
				value--;
			if (DpadDown)
				value++;
			return value;
		}
	}
}