using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class DeviceInfo : MonoBehaviour
{
    public static DeviceInfo instance;
    public string Information;

    // Get local IP address
    public string GetLocalIPAddress()
    {
        string localIP = "";
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
        }
        catch (SocketException ex)
        {
            Debug.Log("SocketException: " + ex.Message);
        }
        return localIP;
    }

    // Get device name
    public string GetDeviceName()
    {
        return Environment.MachineName;
    }

    // Example usage
    void Start()
    {
        instance = this;

        string deviceIP = "device IP: " + GetLocalIPAddress();    // Get the local IP address
        string deviceName = "device NAME: " + GetDeviceName();      // Get the device name

        Debug.Log("Device IP: " + deviceIP);
        Debug.Log("Device Name: " + deviceName);

        // Correct concatenation of IP address and device name into Information string
        Information = $"{deviceIP}_{deviceName}";
    }

}
