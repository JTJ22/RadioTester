using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace RadioTester
{
    internal class Program
    {
        private static TcpClient tcpClient;
        private static System.Timers.Timer timer;
        private static readonly Random Rand = new Random();

        static void Main(string[] args)
        {
            string ip;
            while (true)
            {
                Console.Write("Enter an IP: ");
                ip = Console.ReadLine();
                if (IsValidIPv4(ip))
                {
                    break;
                }
                Console.WriteLine("Invalid IPv4 address.");
            }

            int port;
            while (true)
            {
                Console.Write("Enter a port: ");
                if (int.TryParse(Console.ReadLine(), out port) && port > 0 && port <= ushort.MaxValue)
                {
                    break;
                }
                Console.WriteLine("Invalid port.");
            }

            Console.CancelKeyPress += (s, e) =>
            {
                timer?.Stop();
                tcpClient?.Close();
                Console.WriteLine("Exiting...");
            };

            StartSender(ip, port);
        }

        private static bool IsValidIPv4(string addr) => IPAddress.TryParse(addr, out var a) && a.AddressFamily == AddressFamily.InterNetwork;

        private static void OnTimerElapsed(NetworkStream stream)
        {
            try
            {
                int priority = Rand.Next(0, 5);
                string eti = $"{Rand.Next(0, 100000)}h{Rand.Next(0, 60):D2}m";
                int oldVolume = Rand.Next(0, 101);
                int newVolume = Rand.Next(0, 101);

                string data = $"<{priority}>1 - T6-140317 - - - [t6@4969 eti=\"{eti}\"] Setting am.global_loudspeaker_volume changed from {oldVolume} to {newVolume}\n";

                byte[] bytes = Encoding.UTF8.GetBytes(data);
                stream.Write(bytes, 0, bytes.Length);
                Console.WriteLine($"Sent at {DateTime.Now}: {data.TrimEnd()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send failed: {ex.Message}");
                timer?.Stop();
            }
        }

        private static void StartSender(string ip, int port)
        {
            try
            {
                tcpClient = new TcpClient(ip, port);
                using (NetworkStream stream = tcpClient.GetStream())
                {
                    timer = new System.Timers.Timer(3000) { AutoReset = true };
                    timer.Elapsed += (s, e) => OnTimerElapsed(stream);
                    timer.Start();

                    byte[] buffer = new byte[1024];
                    while(stream.Read(buffer, 0, buffer.Length) > 0)
                    {
                        // Do nothing I guess
                    }

                    Console.WriteLine("Server closed the connection.");
                    timer.Stop();
                    timer.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                timer?.Stop();
            }
            finally
            {
                tcpClient?.Close();
                tcpClient = null;
            }
        }

    }
}