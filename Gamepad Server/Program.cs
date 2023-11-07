using System.Collections.Concurrent;
using GamepadServer.Emulation;
using GamepadServer.Net;

#pragma warning disable CS8602
// ReSharper disable AccessToModifiedClosure

GamepadManager? gamepadManager = null;
while (gamepadManager == null)
{
    Console.Write("Controller Mode: ");
    switch (Console.ReadLine())
    {
        case "ds4":
            gamepadManager = new DualShock4Manager();
            break;
        case "x360":
            gamepadManager = new Xbox360Manager();
            break;
        default:
            Console.WriteLine("Invalid controller mode. Press any key to try again...");
            Console.ReadKey();
            Console.Clear();
            break;
    }
}

ConnectionManager? connectionManager = null;
ConcurrentDictionary<byte, ulong> lastTimestamps = new();
try
{
    connectionManager = new(1055, (connectionId, packet, endPoint) =>
    {
        switch (packet)
        {
            case ConnectionRequestPacket connectionRequest:
            {
                if (connectionManager.AddConnection(connectionRequest.Token, endPoint, out connectionId))
                {
                    Console.WriteLine($"Incoming connection from {endPoint.Address}, assigning ID: {connectionId}");
                    gamepadManager.AddGamepad(connectionId);
                }
                
                connectionManager.Send(connectionId, new ConnectionIdPacket(connectionId));
                break;
            }
            case GamepadStatePacket gamepadState:
            {
                if (lastTimestamps.TryGetValue(connectionId, out ulong lastTimestamp) && gamepadState.Timestamp < lastTimestamp)
                    break;
                
                lastTimestamps[connectionId] = gamepadState.Timestamp;
                gamepadManager.SetGamepadState(connectionId, gamepadState.State);
                break;
            }
        }
    }, connectionId =>
    {
        Console.WriteLine($"Timeout, disconnecting ID: {connectionId}");
        gamepadManager.RemoveGamepad(connectionId);
    });
    
    while (true)
        await connectionManager.Receive();
}
catch (Exception e)
{
    Console.WriteLine(e);
}
finally
{
    gamepadManager.Stop();
    connectionManager?.Close();
}