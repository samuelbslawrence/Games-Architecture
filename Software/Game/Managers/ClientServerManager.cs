using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Game.Managers
{
    public static class Client
    {
        public static bool Connected;
        public static bool Connecting;

        // set port number
        static int port = 8080;
        // create a client
        static TcpClient client;
        // get the stream
        static NetworkStream stream;
        // create a writer and reader
        static StreamReader reader;
        static StreamWriter writer;

        public static void Connect(string[] args)
        {
            Console.WriteLine("Starting echo client...");

            try
            {
                while (true)
                {
                    // read from the console
                    Console.Write("Enter text to send: ");
                    string lineToSend = Console.ReadLine();
                    // send to the server
                    Console.WriteLine("Sending to server: " + lineToSend);
                    writer.WriteLine(lineToSend);
                    // read from the server and print to the console
                    string lineReceived = reader.ReadLine();
                    Console.WriteLine("Received from server: " + lineReceived);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            // close the connection
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

            // set port number
            port = 8080;
            // create a client
            client = new TcpClient("localhost", port);
            // get the stream
            stream = client.GetStream();
            // create a writer and reader
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };

            writer.WriteLine("Connected");
            while (true)
            {
                string response = reader.ReadLine();
                if (response == "Connected")
                {
                    Connected = true;
                    Connecting = false;
                }
            }
        }
    }
}
