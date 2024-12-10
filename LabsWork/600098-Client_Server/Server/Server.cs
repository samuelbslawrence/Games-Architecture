using System.Net;
using System.Net.Sockets;
using System.Text;
public class Server
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting echo server...");

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
                writer.WriteLine("Echoing string: " + inputLine);
                // print to the console
                Console.WriteLine("Echoing string: " + inputLine);
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