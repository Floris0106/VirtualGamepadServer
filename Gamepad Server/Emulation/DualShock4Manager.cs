using System.Numerics;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace GamepadServer.Emulation;

public class DualShock4Manager : GamepadManager
{
	private static readonly DualShock4DPadDirection[,] Directions =
	{
		{ DualShock4DPadDirection.Northwest, DualShock4DPadDirection.North, DualShock4DPadDirection.Northeast },
		{ DualShock4DPadDirection.West, DualShock4DPadDirection.None, DualShock4DPadDirection.East },
		{ DualShock4DPadDirection.Southwest, DualShock4DPadDirection.South, DualShock4DPadDirection.Southeast }
	};
	
	public override void AddGamepad(byte id)
	{
		DS4 ds4 = new(Client.CreateDualShock4Controller());
		ds4.Controller.Connect();
		Gamepads[id] = ds4;
	}
	
	public override void SetGamepadState(byte id, GamepadState state)
	{
		DS4 ds4 = (DS4)Gamepads[id];
		
		if (ds4.State.A != state.A)
			ds4.DualShock4.SetButtonState(DualShock4Button.Circle, state.A);
		if (ds4.State.B != state.B)
			ds4.DualShock4.SetButtonState(DualShock4Button.Cross, state.B);
		if (ds4.State.X != state.X)
			ds4.DualShock4.SetButtonState(DualShock4Button.Triangle, state.X);
		if (ds4.State.Y != state.Y)
			ds4.DualShock4.SetButtonState(DualShock4Button.Square, state.Y);
		if (ds4.State.DpadX != state.DpadX || ds4.State.DpadY != state.DpadY)
			ds4.DualShock4.SetDPadDirection(Directions[state.DpadY + 1, state.DpadX + 1]);
		if (Vector2.DistanceSquared(ds4.State.LeftStick, state.LeftStick) > 1e-13)
		{
			ds4.DualShock4.SetAxisValue(DualShock4Axis.LeftThumbX, Remap.ToByte(state.LeftStick.X));
			ds4.DualShock4.SetAxisValue(DualShock4Axis.LeftThumbY, Remap.ToByte(-state.LeftStick.Y));
		}
		if (Vector2.DistanceSquared(ds4.State.RightStick, state.RightStick) > 1e-13)
		{
			ds4.DualShock4.SetAxisValue(DualShock4Axis.RightThumbX, Remap.ToByte(state.RightStick.X));
			ds4.DualShock4.SetAxisValue(DualShock4Axis.RightThumbY, Remap.ToByte(-state.RightStick.Y));
		}
		if (ds4.State.L != state.L)
			ds4.DualShock4.SetButtonState(DualShock4Button.ShoulderLeft, state.L);
		if (ds4.State.ZL != state.ZL)
			ds4.DualShock4.SetButtonState(DualShock4Button.TriggerLeft, state.ZL);
		if (ds4.State.R != state.R)
			ds4.DualShock4.SetButtonState(DualShock4Button.ShoulderRight, state.R);
		if (ds4.State.ZR != state.ZR)
			ds4.DualShock4.SetButtonState(DualShock4Button.TriggerRight, state.ZR);
		if (ds4.State.Minus != state.Minus)
			ds4.DualShock4.SetButtonState(DualShock4Button.Share, state.Minus);
		if (ds4.State.Plus != state.Plus)
			ds4.DualShock4.SetButtonState(DualShock4Button.Options, state.Plus);
		
		ds4.State = state;
	}
	
	private class DS4 : Gamepad
	{
		public IDualShock4Controller DualShock4 => (IDualShock4Controller) Controller;
		
		public DS4(IDualShock4Controller controller) : base(controller)
		{
			
		}
	}
}