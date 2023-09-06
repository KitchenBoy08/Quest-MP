using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using Riptide;
using Riptide.Utils;

class Server
{
    static void Main()
    {
        Console.WriteLine("Starting Server")

        currentserver = new Server();
        currentserver.TimeoutTime = 20000;
        currentserver.HeartbeatInterval = 5000;

        currentserver.Start(7777, 256);
    }
}
