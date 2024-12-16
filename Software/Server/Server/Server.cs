using System.Net;
using System.Net.Sockets;
using System.Text;
public class Server
{
    private const string FILE_NAME = "HighScore.txt";

    private static int highScore;

    private static void GetHighScore()
    {
        if (File.Exists(FILE_NAME))
        {
            Console.WriteLine("HighScore.txt not found.");
            return;
        }

        StreamReader fileReader = new(FILE_NAME);

        string line = fileReader.ReadLine();
        if (string.IsNullOrEmpty(line) || !int.TryParse(line, out int score))
        {
            return;
        }

        highScore = score;

        fileReader.Close();
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

        StreamWriter fileWriter = new(FILE_NAME);

        fileWriter.Write(highScore.ToString());

        fileWriter.Close();
    }


    public static void Main(string[] args)
    {
        Console.WriteLine("Starting server...");

        // set port number
        int port = 8080;
        // create a local listener
        TcpListener listener = new TcpListener(IPAddress.Loopback, port);
        // start listening
        listener.Start();

        // accept a client
        TcpClient client = listener.AcceptTcpClient();
        // get the stream
        NetworkStream stream = client.GetStream();
        // create a writer and reader
        StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
        StreamReader reader = new StreamReader(stream, Encoding.ASCII);

        // read from the client and echo back
        try
        {
            string inputLine = "";
            while (inputLine != null)
            {
                // read from the client
                inputLine = reader.ReadLine();
                // echo back to the client
                writer.WriteLine(inputLine);
                // print to the console
                Console.WriteLine("Input response: " + inputLine);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }

        // close the connection
        Console.WriteLine("Server disconnected from client.");
        client.Close();
        listener.Stop();
    }
}