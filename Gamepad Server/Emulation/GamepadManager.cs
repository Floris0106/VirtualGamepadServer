using System.Collections.Concurrent;
using Nefarius.ViGEm.Client;

namespace GamepadServer.Emulation;

public abstract class GamepadManager
{
	protected readonly ViGEmClient Client = new();
	
	protected readonly ConcurrentDictionary<byte, Gamepad> Gamepads = new();

	public abstract void AddGamepad(byte id);

	public void RemoveGamepad(byte id)
	{
		if (Gamepads.TryRemove(id, out Gamepad? gamepad))
			gamepad.Controller.Disconnect();
	}

	public abstract void SetGamepadState(byte id, GamepadState state);
	
	
	public void Stop()
	{
		foreach (Gamepad gamepad in Gamepads.Values)
			gamepad.Controller.Disconnect();
	}
	
	protected class Gamepad
	{
		public readonly IVirtualGamepad Controller;
		
		public GamepadState State;

		public Gamepad(IVirtualGamepad controller)
		{
			Controller = controller;
		}
	}
}