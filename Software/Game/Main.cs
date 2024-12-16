using System;
using System.Threading;
using OpenGL_Game.Managers;

namespace OpenGL_Game
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class MainEntry
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ThreadPool.QueueUserWorkItem(Client.tryConnecting);
            using (var game = new SceneManager())
                game.Run();
        }
    }
#endif
}
