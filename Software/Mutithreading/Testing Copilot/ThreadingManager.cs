
// class that manages the threading pool
class ThreadingManager
{
    // number of threads
    private int numOfThreads;

    // constructor that takes the number of threads
    public ThreadingManager(int threads)
    {
        int minWorker, minIOC, maxWorker, maxIOC;
        // Get the current settings.
        ThreadPool.GetMinThreads(out minWorker, out minIOC);
        ThreadPool.GetMaxThreads(out maxWorker, out maxIOC);

        // set the number of threads
        numOfThreads = threads;
        // create thread pool
        ThreadPool.SetMinThreads(numOfThreads, minIOC);
        ThreadPool.SetMaxThreads(numOfThreads, maxIOC);
    }

    // method that take a method group parameter and a state object parameter
    public void Start(WaitCallback method, object state)
    {
        // start the method with the state object
        ThreadPool.QueueUserWorkItem(method, state);
    }

    // method that returns the number of running threads
    private int GetRunningThreads()
    {
        int workerThreads, completionPortThreads;
        ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
        return workerThreads;
    }

    // method that returns true if the thread pool is running
    public bool IsThreadPoolRunning()
    {
        return GetRunningThreads() != numOfThreads;
    }

    // method that returns the number of tasks remaining
    public long GetTasksRemaining()
    {
        return ThreadPool.PendingWorkItemCount;
    }
}