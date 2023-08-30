using GamepadServer.Emulation;

namespace GamepadServer.Net;

public class GamepadStatePacket : ServerboundPacket
{
	public readonly GamepadState State;
	
	public GamepadStatePacket(BinaryReader reader)
	{
		ushort buttonStates = reader.ReadUInt16();
		bool[] buttons = new bool[14];
		for (int i = 0; i < buttons.Length; i++)
			buttons[i] = (buttonStates & (1 << i)) != 0;

		State.A = buttons[0];
		State.B = buttons[1];
		State.X = buttons[2];
		State.Y = buttons[3];
		State.DpadUp = buttons[4];
		State.DpadDown = buttons[5];
		State.DpadLeft = buttons[6];
		State.DpadRight = buttons[7];
		State.L = buttons[8];
		State.ZL = buttons[9];
		State.R = buttons[10];
		State.ZR = buttons[11];
		State.Minus = buttons[12];
		State.Plus = buttons[13];
		
		State.LeftStick = new(reader.ReadSingle(), reader.ReadSingle());
		State.RightStick = new(reader.ReadSingle(), reader.ReadSingle());
	}
}