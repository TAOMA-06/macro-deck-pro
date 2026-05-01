using System.IO.Pipes;
using SuchByte.MacroDeck.Logging;

namespace SuchByte.MacroDeck.Pipe;

public static class MacroDeckPipeClient
{

    internal static Task<bool> SendShowMainWindowMessage()
    {
        return SendPipeMessage("show");
    }

    private static async Task<bool> SendPipeMessage(string message, int maxRetries = 3)
    {
        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                await using var client = new NamedPipeClientStream("macrodeck");
                var timeout = Math.Min(1000 * (attempt + 1), 3000);
                await client.ConnectAsync(timeout);
                if (client.IsConnected)
                {
                    var bytes = Encoding.ASCII.GetBytes(message);
                    client.Write(bytes, 0, bytes.Length);
                    return true;
                }
            }
            catch (TimeoutException)
            {
                MacroDeckLogger.Warning($"Pipe send attempt {attempt + 1}/{maxRetries} timed out for message: {message}");
            }
            catch (Exception ex)
            {
                MacroDeckLogger.Warning($"Pipe send attempt {attempt + 1}/{maxRetries} failed: {ex.Message}");
            }

            if (attempt < maxRetries - 1)
            {
                await Task.Delay(500 * (attempt + 1));
            }
        }

        return false;
    }

}