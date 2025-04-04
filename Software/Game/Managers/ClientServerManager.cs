using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace OpenGL_Game.Managers
{
    public static class Client
    {
        public static bool Connected;
        public static bool Connecting;

        // Set port number
        static int port = 8080;

        // Create a client
        static TcpClient client;

        // Get the stream
        static NetworkStream stream;

        // Create a writer and reader
        static StreamReader reader;
        static StreamWriter writer;

        public static void Connect(string[] args)
        {
            Console.WriteLine("Starting echo client...");

            try
            {
                while (true)
                {
                    Console.Write("Enter text to send: ");
                    string lineToSend = Console.ReadLine();
                    Console.WriteLine("Sending to server: " + lineToSend);
                    writer.WriteLine(lineToSend);
                    string lineReceived = reader.ReadLine();
                    Console.WriteLine("Received from server: " + lineReceived);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            Console.WriteLine("Client disconnected from server.");
            client.Close();
        }

        public static void tryConnecting(object? status)
        {
            if (Connecting || Connected)
            {
                return;
            }

            Console.WriteLine("Attempt to connect...");

            try
            {
                client = new TcpClient("localhost", port);
                stream = client.GetStream();
                stream.ReadTimeout = 2000;  // 2-second timeout
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream) { AutoFlush = true };

                writer.WriteLine("Connected");

                try
                {
                    string response = reader.ReadLine();
                    if (response == "Connected")
                    {
                        Connected = true;
                        Connecting = false;
                        Console.WriteLine("Server confirmed connection.");
                    }
                    else
                    {
                        Console.WriteLine($"Server responded with: '{response}'");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading server response: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting: " + ex.Message);
            }
        }

        public static void Disconnect()
        {
            try
            {
                if (client != null)
                {
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error disconnecting: " + ex.Message);
            }
            Connected = false;
            Connecting = false;
        }

        public static void SendScore(double score)
        {
            if (Connected && writer != null)
            {
                try
                {
                    writer.WriteLine($"Score:{score:0.00}");
                    Console.WriteLine($"Score sent to server: {score:0.00} seconds");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending score: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Not connected to server; score not sent.");
            }
        }

        public static double GetHighScore()
        {
            if (!Connected || writer == null || reader == null)
            {
                Console.WriteLine("Not connected to server; high score is 0.00.");
                return 0;
            }

            try
            {
                writer.WriteLine("GetHighScore");
                string response = reader.ReadLine();
                if (string.IsNullOrEmpty(response))
                {
                    Console.WriteLine("No response from server; high score is 0.00.");
                    return 0;
                }

                if (double.TryParse(response, out double highScore))
                {
                    Console.WriteLine($"High score received from server: {highScore:0.00} seconds");
                    return highScore;
                }
                else
                {
                    Console.WriteLine("Failed to parse high score from server response; high score is 0.00.");
                    return 0;
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error getting high score (possibly timeout): " + ex.Message);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error getting high score: " + ex.Message);
                return 0;
            }
        }
    }
}