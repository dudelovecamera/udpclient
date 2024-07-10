using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using udpclient;

class BestUdpClient
{
    private static byte[] data = new byte[2048];

    private static IPEndPoint sender = new
        IPEndPoint(IPAddress.Any, 0);

    private static EndPoint Remote = (EndPoint)sender;
    private static int size = 300;

    private static int AdvSndRcvData(Socket s, byte[] message,
        EndPoint rmtdevice)
    {
        int recv = 0;
        int retry = 0;
        while (true)
        {
            Console.WriteLine("Attempt #{0}", retry);
            try
            {
                s.SendTo(message, message.Length, SocketFlags.None,
                    rmtdevice);
                data = new byte[size];
                recv = s.ReceiveFrom(data, ref Remote);
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10054)
                    recv = 0;
                else if (e.ErrorCode == 10040)
                {
                    Console.WriteLine("Error receiving packet");
                    size += 100;
                    recv = 0;
                }
            }

            if (recv > 0)
            {
                return recv;
            }
            else
            {
                retry++;
                if (retry > 4)
                {
                    return 0;
                }
            }
        }
    }

    public static void Main()
    {
        string input, stringData;
        int recv;
        IPEndPoint ipep = new IPEndPoint(
            IPAddress.Parse("127.0.0.1"), 9050);
        Socket server = new Socket(AddressFamily.InterNetwork,
            SocketType.Dgram, ProtocolType.Udp);
        int sockopt = (int)server.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
        Console.WriteLine("Default timeout: {0}", sockopt);
        server.SetSocketOption(SocketOptionLevel.Socket,
            SocketOptionName.ReceiveTimeout, 3000);
        sockopt = (int)server.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
        Console.WriteLine("New timeout: {0}", sockopt);
        
        
        
        data = createMessage("welcome to our server");
        
        
        recv = AdvSndRcvData(server, data, ipep);
        if (recv > 0)
        {
            stringData = Encoding.ASCII.GetString(data, 0, recv);
            Console.WriteLine(stringData);
        }
        else
        {
            Console.WriteLine("Unable to communicate with remote host");
            return;
        }

        while (true)
        {
            input = Console.ReadLine();
            if (input == "exit")
                break;
            
            data = createMessage(input);
            recv = AdvSndRcvData(server, data,
                ipep);
            if (recv > 0)
            {
                stringData = Encoding.ASCII.GetString(data, 0,
                    recv);
                Console.WriteLine(stringData);
            }
            else
                Console.WriteLine("Did not receive an answer");
        }

        Console.WriteLine("Stopping client");
        server.Close();
    }

    private static byte[] createMessage(string msg)
    {
        var message = new Message
        {
            Date = DateTime.UtcNow,
            TemperatureCelsius = 25,
            Summary = msg
        };
        return Encoding.ASCII.GetBytes(JsonSerializer.Serialize(message));
    }
}