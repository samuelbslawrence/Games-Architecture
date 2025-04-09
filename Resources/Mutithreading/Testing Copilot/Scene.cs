
class Scene
{
    // create a new instance of SystemTest
    private ISystem systemTest = new SystemTest();
    // create a new ThreadingManager with 1 thread
    private ThreadingManager manager = new ThreadingManager(10);

    // method to start the threading
    public void Start()
    {
        // create stopwatch
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        // create a new WaitCallback method
        WaitCallback method = new WaitCallback(systemTest.DoWork);

        // start 10 methods
        Console.WriteLine("Start 10 methods ...");
        for (int i = 0; i < 10; i++)
        {
            // start a new method with the ThreadingManager
            manager.Start(method, i);
        }
        Console.WriteLine("Started 10 methods, now wait until all complete ...");

        // wait until all methods are completed
        while(manager.IsThreadPoolRunning())
        {
            // print the number of tasks remaining
            Console.WriteLine("Tasks remaining = " + manager.GetTasksRemaining());
            Thread.Sleep(1000);
        }
        stopwatch.Stop();
        Console.WriteLine("All methods completed in " + stopwatch.Elapsed.TotalSeconds + " seconds");
    }
}