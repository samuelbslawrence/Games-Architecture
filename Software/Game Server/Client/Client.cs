using System.Net.Sockets;

public class Client
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting echo client...");

        // set port number
        int port = 8080;
        // create a client
        TcpClient client = new TcpClient("localhost", port);
        // get the stream
        NetworkStream stream = client.GetStream();
        // create a writer and reader
        StreamReader reader = new StreamReader(stream);
        StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

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
}