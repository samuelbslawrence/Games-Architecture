using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Server
{
    private const string FILE_NAME = "HighScore.txt";
    private static int highScore;

    private static void GetHighScore()
    {
        // Corrected condition: if the file doesn't exist, show message.
        if (!File.Exists(FILE_NAME))
        {
            Console.WriteLine("HighScore.txt not found.");
            return;
        }

        using (StreamReader fileReader = new StreamReader(FILE_NAME))
        {
            string line = fileReader.ReadLine();
            if (string.IsNullOrEmpty(line) || !int.TryParse(line, out int score))
            {
                return;
            }

            highScore = score;
        }
    }

    private static void SetHighScore(int score)
    {
        GetHighScore();
        if (highScore >= score)
        {
            return;
        }

        if (File.Exists(FILE_NAME))
        {
            File.Delete(FILE_NAME);
        }

        using (StreamWriter fileWriter = new StreamWriter(FILE_NAME))
        {
            fileWriter.Write(score.ToString());
        }
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting server...");

        int port = 8080;
        TcpListener listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        Console.WriteLine($"Server started on port {port}. Waiting for connections...");

        while (true)
        {
            // Accept a client connection and process it on a new thread
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected.");

            // Launch a new thread to handle the client
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        using (client)
        {
            NetworkStream stream = client.GetStream();
            using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true })
            using (StreamReader reader = new StreamReader(stream, Encoding.ASCII))
            {
                try
                {
                    string inputLine;
                    // Read from the client until the connection is closed.
                    while ((inputLine = reader.ReadLine()) != null)
                    {
                        // Echo back the input to the client.
                        writer.WriteLine(inputLine);
                        Console.WriteLine("Received: " + inputLine);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
                Console.WriteLine("Client disconnected.");
            }
        }
    }
}