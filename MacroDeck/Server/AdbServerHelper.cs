using System.IO;
using System.Threading;
using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Models;
using AdvancedSharpAdbClient.Receivers;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Notifications;
using SuchByte.MacroDeck.Startup;

namespace SuchByte.MacroDeck.Server;

public class AdbServerHelper
{
    private static AdbServer? _adbServer;
    private static DeviceMonitor? _deviceMonitor;

    private const string AdbFolderName = "Android Debug Bridge";
    private const string MacroDeckPackageName = "com.suchbyte.macrodeck";
    private const string MacroDeckMainActivity = "com.suchbyte.macrodeck/.MainActivity";

    private static readonly string AdbPath = Path.Combine(ApplicationPaths.MainDirectoryPath, AdbFolderName, "adb.exe");
    
    public static async Task Initialize()
    {
        if (!MacroDeck.Configuration.EnableAdbServer)
        {
            return;
        }
        
        if (!File.Exists(AdbPath))
        {
            MacroDeckLogger.Warning(typeof(AdbServerHelper), $"Cannot start adb server at {AdbPath}: File not found");
            NotificationManager.SystemNotification(
                "Macro Deck",
                "Cannot start ADB server: adb.exe not found",
                false);
            return;
        }
        
        MacroDeckLogger.Info(typeof(AdbServerHelper), $"Starting ADB server using {AdbPath}");

        _adbServer = new AdbServer();
        var result = await _adbServer.StartServerAsync(AdbPath, true);
        if (result != StartServerResult.Started)
        {
            MacroDeckLogger.Info(typeof(AdbServerHelper), "Unable to start ADB server");
        }
        
        _deviceMonitor = new DeviceMonitor(new AdbSocket(AdbClient.AdbServerEndPoint));
        _deviceMonitor.DeviceConnected += Monitor_DeviceConnected;
        _deviceMonitor.DeviceDisconnected += Monitor_DeviceDisconnected;
        await _deviceMonitor.StartAsync();
    }

    public static void Shutdown()
    {
        try
        {
            _deviceMonitor?.Dispose();
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Warning(typeof(AdbServerHelper), $"Error while disposing DeviceMonitor: {ex.Message}");
        }
        
        try
        {
            _adbServer?.StopServer();
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Warning(typeof(AdbServerHelper), $"Error while stopping ADB server: {ex.Message}");
        }
    }

    private static async Task RunForDevice(string serial, Func<AdbClient, DeviceData, Task> action)
    {
        var adbClient = await GetAdbClient();
        if (adbClient is null)
        {
            return;
        }
        var device = await GetDevice(adbClient, serial);
        if (!device.HasValue)
        {
            return;
        }

        await action(adbClient, device.Value);
    }

    private static async Task<DeviceData?> GetDevice(AdbClient adbDeviceClient, string serial)
    {
        var devices = await adbDeviceClient.GetDevicesAsync();
        return devices.FirstOrDefault(x => x.Serial.Equals(serial));
    }

    private static async Task<AdbClient?> GetAdbClient()
    {
        var serverEndpoint = GetAdbServerEndpoint();
        if (serverEndpoint is null)
        {
            return null;
        }
        
        var adbClient = new AdbClient();
        await adbClient.ConnectAsync(serverEndpoint);
        return adbClient;
    }

    private static string? GetAdbServerEndpoint()
    {
        var adbServerEndpoint = _adbServer?.EndPoint.ToString();
        if (adbServerEndpoint is not null)
        {
            return adbServerEndpoint;
        }
        
        MacroDeckLogger.Info(typeof(AdbServerHelper), "Endpoint was null");
        return null;

    }

    private static void Monitor_DeviceDisconnected(object sender, DeviceDataEventArgs e)
    {
        MacroDeckLogger.Info(typeof(AdbServerHelper), $"{e.Device.Name} disconnected");
        Task.Run(async () => await RunForDevice(e.Device.Serial, async (adbDeviceClient, deviceData) =>
        {
            await ExitMacroDeckClient(adbDeviceClient, deviceData);
        }));
    }

    private static async void Monitor_DeviceConnected(object sender, DeviceDataEventArgs e)
    {
        try
        {
            if (e.Device.Serial.StartsWith("127.0.0.1"))
            {
                return;
            }
            
            MacroDeckLogger.Info(typeof(AdbServerHelper), $"{e.Device.Name} connected");
            await RunForDevice(e.Device.Serial, async (adbDeviceClient, deviceData) =>
            {
                await StartMacroDeckClient(adbDeviceClient, deviceData);
                await StartReverseForward(adbDeviceClient, deviceData);
            });
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(typeof(AdbServerHelper), $"Error in Monitor_DeviceConnected: {ex.Message}");
        }
    }

    private static async Task<bool> IsDeviceOnline(AdbClient adbClient, DeviceData device)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                var devices = await adbClient.GetDevicesAsync();
                var freshDevice = devices.FirstOrDefault(x => x.Serial == device.Serial);
                if (freshDevice != null && freshDevice.State == DeviceState.Online)
                {
                    return true;
                }
                await Task.Delay(100, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }

        MacroDeckLogger.Info(typeof(AdbServerHelper), $"Device {device.Serial} is still not online after timeout");
        return false;
    }
    
    private static async Task ExitMacroDeckClient(AdbClient adbDeviceClient, DeviceData device)
    {
        var deviceIsOnline = await IsDeviceOnline(adbDeviceClient, device);
        if (!deviceIsOnline)
        {
            return;
        }
        
        await adbDeviceClient.ExecuteRemoteCommandAsync($"am force-stop {MacroDeckPackageName}",
            device,
            new ConsoleOutputReceiver());
        await adbDeviceClient.SendKeyEventAsync(device, "KEYCODE_SLEEP");
    }

    private static async Task StartMacroDeckClient(AdbClient adbDeviceClient, DeviceData device)
    {
        if (!MacroDeck.Configuration.EnableAdbAutoStartApp)
        {
            return;
        }
        
        var deviceIsOnline = await IsDeviceOnline(adbDeviceClient, device);
        if (!deviceIsOnline)
        {
            return;
        }
        
        await adbDeviceClient.SendKeyEventAsync(device, "KEYCODE_WAKEUP");
        await adbDeviceClient.ExecuteRemoteCommandAsync($"am start -n {MacroDeckMainActivity}",
            device,
            new ConsoleOutputReceiver());
    }

    private static async Task StartReverseForward(AdbClient adbDeviceClient, DeviceData device)
    {
        var deviceIsOnline = await IsDeviceOnline(adbDeviceClient, device);
        if (!deviceIsOnline)
        {
            return;
        }
        
        try
        {
            await adbDeviceClient.CreateReverseForwardAsync(
                device,
                $"tcp:{MacroDeck.Configuration.HostPort}",
                $"tcp:{MacroDeck.Configuration.HostPort}",
                true);
            
            MacroDeckLogger.Info(typeof(AdbServerHelper), $"Started reverse forward on {device.Name}");
        } catch (Exception ex)
        {
            MacroDeckLogger.Warning(typeof(AdbServerHelper),
                $"Unable to start reverse forward on {device.Name}: {ex.Message}");
        }
    }

}