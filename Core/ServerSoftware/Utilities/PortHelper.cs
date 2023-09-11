using System;
using System.Threading.Tasks;
using Open.Nat;

public static class PortHelper
{
    public static async Task OpenPort(int externalPort = 7777, int internalPort = 7777)
    {
        try
        {
            var discoverer = new NatDiscoverer();
            var device = await discoverer.DiscoverDeviceAsync();

            if (device != null)
            {
                await device.CreatePortMapAsync(new Mapping(Protocol.Udp, internalPort, externalPort, "UDP Port Forwarding"));
                Server.ServerClass.hasPortForwarded = true;
                Server.ServerClass.UpdateWindow();
            }
        }
        catch (NatDeviceNotFoundException)
        {
            Console.WriteLine("No NAT device found.");
        }
        catch (MappingException ex)
        {
            Console.WriteLine($"Error forwarding UDP port: {ex.Message}");
        }
    }

    public static async Task Close(int externalPort = 7777, int internalPort = 7777)
    {
        try
        {
            var discoverer = new NatDiscoverer();
            var device = await discoverer.DiscoverDeviceAsync();

            if (device != null)
            {
                await device.DeletePortMapAsync(new Mapping(Protocol.Udp, internalPort, externalPort));
                Server.ServerClass.hasPortForwarded = false;
            }
            else
            {
                Console.WriteLine("No NAT device found.");
            }
        }
        catch (NatDeviceNotFoundException)
        {
            Console.WriteLine("No NAT device found.");
        }
        catch (MappingException ex)
        {
            Console.WriteLine($"Error removing UDP port forwarding: {ex.Message}");
        }
    }
}