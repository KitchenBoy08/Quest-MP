using Mono.Nat;
using ServerSoftware;

public static class PortHelper
{
    private static Mono.Nat.INatDevice upnpDevice;
    private static Mono.Nat.Mapping mapping;
    public static void OpenPort(int port = 7777)
    {
        mapping = new(Protocol.Udp, port, port);

        NatUtility.DeviceFound += OnDeviceFound;
        Mono.Nat.NatUtility.StartDiscovery();
    }

    public static void ClosePort(int port = 7777)
    {
        if (upnpDevice != null)
        {
            try
            {
                upnpDevice.DeletePortMap(mapping);
            }
            catch (Exception ex)
            {
                ServerClass.UpdateWindow($"Failed to delete port mapping with error: {ex}");
            }
        }
        else
        {
            ServerClass.UpdateWindow("Can't delete port mapping as the device is null!");
        }
    }

    private static void OnDeviceFound(object sender, DeviceEventArgs device)
    {
        NatUtility.StopDiscovery();
        try
        {
            ServerClass.hasPortForwarded = true;
            device.Device.CreatePortMap(mapping);
            upnpDevice = device.Device;
        }
        catch (Exception ex)
        {
            ServerClass.UpdateWindow($"Failed creating port map with exception: {ex}");
        }
    }
}