using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

class Server
{
    static void Main(string[] args)
    {
        TcpListener server = null;
        try
        {
            IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
            int port = 9999;

            server = new TcpListener(iPAddress, port);

            server.Start();
            Console.WriteLine("Socket Server Has Started");

            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                NetworkStream stream = client.GetStream();

                byte[] bytes = new byte[255];
                int bytesRead = stream.Read(bytes, 0, bytes.Length);
                string receivedMessage = Encoding.ASCII.GetString(bytes, 0, bytesRead);
                Console.WriteLine("Received: {0}", receivedMessage);

                client.Close();
            }
        }catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
        }
        finally
        {
            server.Stop();
        }
    }
}