
class SystemTest : ISystem
{
    // method to be executed by the thread
    public void DoWork(object? state)
    {
        Console.WriteLine("Foor loop " + state + ", Thread " + Thread.CurrentThread.ManagedThreadId + " started");
        //Thread.Sleep(5000);
        double sum = 0;
        for(int n = 0; n < 999999999; ++n)
        {
            sum += Math.Sqrt(n);
        }
        Console.WriteLine("Foor loop " + state + ", Thread " + Thread.CurrentThread.ManagedThreadId + " finished");
    }
}
