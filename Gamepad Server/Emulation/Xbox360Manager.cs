using System.Numerics;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace GamepadServer.Emulation;

public class Xbox360Manager : GamepadManager
{
	public override void AddGamepad(byte id)
	{
		X360 x360 = new(Client.CreateXbox360Controller());
		x360.Controller.Connect();
		Gamepads[id] = x360;
	}

	public override void SetGamepadState(byte id, GamepadState state)
	{
		X360 x360 = (X360)Gamepads[id];
		
		if (x360.State.A != state.A)
			x360.Xbox360.SetButtonState(Xbox360Button.B, state.A);
		if (x360.State.B != state.B)
			x360.Xbox360.SetButtonState(Xbox360Button.A, state.B);
		if (x360.State.X != state.X)
			x360.Xbox360.SetButtonState(Xbox360Button.Y, state.X);
		if (x360.State.Y != state.Y)
			x360.Xbox360.SetButtonState(Xbox360Button.X, state.Y);
		if (x360.State.DpadUp != state.DpadUp)
			x360.Xbox360.SetButtonState(Xbox360Button.Up, state.DpadUp);
		if (x360.State.DpadDown != state.DpadDown)
			x360.Xbox360.SetButtonState(Xbox360Button.Down, state.DpadDown);
		if (x360.State.DpadLeft != state.DpadLeft)
			x360.Xbox360.SetButtonState(Xbox360Button.Left, state.DpadLeft);
		if (x360.State.DpadRight != state.DpadRight)
			x360.Xbox360.SetButtonState(Xbox360Button.Right, state.DpadRight);
		if (Vector2.DistanceSquared(x360.State.LeftStick, state.LeftStick) > 1e-13)
		{
			x360.Xbox360.SetAxisValue(Xbox360Axis.LeftThumbX, Remap.ToShort(state.LeftStick.X));
			x360.Xbox360.SetAxisValue(Xbox360Axis.LeftThumbY, Remap.ToShort(state.LeftStick.Y));
		}
		if (Vector2.DistanceSquared(x360.State.RightStick, state.RightStick) > 1e-13)
		{
			x360.Xbox360.SetAxisValue(Xbox360Axis.RightThumbX, Remap.ToShort(state.RightStick.X));
			x360.Xbox360.SetAxisValue(Xbox360Axis.RightThumbY, Remap.ToShort(state.RightStick.Y));
		}
		if (x360.State.L != state.L)
			x360.Xbox360.SetButtonState(Xbox360Button.LeftShoulder, state.L);
		if (x360.State.ZL != state.ZL)
			x360.Xbox360.SetSliderValue(Xbox360Slider.LeftTrigger, Remap.ToByte(state.ZL));
		if (x360.State.R != state.R)
			x360.Xbox360.SetButtonState(Xbox360Button.RightShoulder, state.R);
		if (x360.State.ZR != state.ZR)
			x360.Xbox360.SetSliderValue(Xbox360Slider.RightTrigger, Remap.ToByte(state.ZR));
		if (x360.State.Minus != state.Minus)
			x360.Xbox360.SetButtonState(Xbox360Button.Back, state.Minus);
		if (x360.State.Plus != state.Plus)
			x360.Xbox360.SetButtonState(Xbox360Button.Start, state.Plus);
		
		x360.State = state;
	}
	
	private class X360 : Gamepad
	{
		public IXbox360Controller Xbox360 => (IXbox360Controller) Controller;
		
		public X360(IXbox360Controller controller) : base(controller)
		{
			
		}
	}
}