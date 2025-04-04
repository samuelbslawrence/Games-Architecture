using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Server
{
    private const string FILE_NAME = "HighScore.txt";
    // For time scores, lower is better. Start with MaxValue so any valid time will update it.
    private static double highScore = double.MaxValue;

    private static void GetHighScoreFromFile()
    {
        if (!File.Exists(FILE_NAME))
        {
            Console.WriteLine("HighScore.txt not found. Using default high score.");
            return;
        }

        using (StreamReader fileReader = new StreamReader(FILE_NAME))
        {
            string line = fileReader.ReadLine();
            if (string.IsNullOrEmpty(line) || !double.TryParse(line, out double score))
            {
                Console.WriteLine("HighScore.txt exists but could not be parsed. Using default high score.");
                return;
            }
            highScore = score;
            Console.WriteLine($"Loaded high score: {highScore:0.00} seconds");
        }
    }

    private static void SetHighScore(double score)
    {
        // For a time-based score, a lower number is better.
        // Update only if the new score is lower than the current high score.
        if (score >= highScore)
        {
            return;
        }
        highScore = score;
        Console.WriteLine($"New high score: {highScore:0.00} seconds");

        if (File.Exists(FILE_NAME))
        {
            File.Delete(FILE_NAME);
        }
        using (StreamWriter fileWriter = new StreamWriter(FILE_NAME))
        {
            fileWriter.Write(highScore.ToString("0.00"));
        }
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Starting server...");
        // Attempt to load the current high score from file.
        GetHighScoreFromFile();

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
                        Console.WriteLine("Received: " + inputLine);

                        if (inputLine.StartsWith("GetHighScore"))
                        {
                            // Send the current high score, or "0.00" if no score has been set.
                            string scoreToSend = (highScore == double.MaxValue) ? "0.00" : highScore.ToString("0.00");
                            writer.WriteLine(scoreToSend);
                            Console.WriteLine("Sent high score: " + scoreToSend);
                        }
                        else if (inputLine.StartsWith("Score:"))
                        {
                            // Extract and parse the score.
                            string scorePart = inputLine.Substring("Score:".Length).Trim();
                            if (double.TryParse(scorePart, out double score))
                            {
                                Console.WriteLine("Received score: " + score.ToString("0.00"));
                                // Update high score if the new score is lower.
                                SetHighScore(score);
                                writer.WriteLine("High score updated to: " + highScore.ToString("0.00"));
                            }
                            else
                            {
                                writer.WriteLine("Invalid score format.");
                            }
                        }
                        else
                        {
                            // Echo back the input for any other messages.
                            writer.WriteLine(inputLine);
                        }
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