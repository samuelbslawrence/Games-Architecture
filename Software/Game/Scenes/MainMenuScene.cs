using OpenTK.Graphics.OpenGL;
using OpenGL_Game.Managers;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;
using System.Threading;

namespace OpenGL_Game.Scenes
{
    class MainMenuScene : Scene
    {
        #region High Score Variables
        private double highScore = 0;
        #endregion

        public MainMenuScene(SceneManager sceneManager) : base(sceneManager)
        {
            // Set the title of the window
            sceneManager.Title = "Main Menu";

            // Force a fresh connection to the server
            Client.Disconnect();
            Thread connectionThread = new Thread(Client.tryConnecting);
            connectionThread.Start();
            // Wait briefly for the connection to establish
            Thread.Sleep(2500);

            // Retrieve the high score from the server
            highScore = Client.GetHighScore();

            // Set the Render and Update delegates to the Update and Render methods of this class
            sceneManager.renderer = Render;
            sceneManager.updater = Update;

            sceneManager.mouseDelegate += Mouse_BottonPressed;

            // Set background to orange
            GL.ClearColor(1.0f, 0.5f, 0.0f, 1.0f);
        }

        public override void Update(FrameEventArgs e) { }

        public override void Render(FrameEventArgs e)
        {
            // Setup viewport and clear the screen
            GL.Viewport(0, 0, sceneManager.Size.X, sceneManager.Size.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Use a top-left coordinate system by flipping the y-axis
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, sceneManager.Size.X, sceneManager.Size.Y, 0, -1, 1);

            // Uniform text style: text size 60, center-aligned, white fill with a 3-pixel black outline.
            SKPaint outlinePaint = new SKPaint
            {
                TextSize = 60,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true,
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3
            };
            SKPaint fillPaint = new SKPaint
            {
                TextSize = 60,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true,
                Color = SKColors.White,
                Style = SKPaintStyle.Fill
            };

            float centerX = sceneManager.Size.X * 0.5f;
            // New positions: title at 150 pixels from the top, best time at 230 pixels.
            float titleY = 150;
            float scoreY = titleY + 80;

            // Draw title and best time:
            GUI.DrawText("Main Menu", centerX, titleY, outlinePaint);
            GUI.DrawText("Main Menu", centerX, titleY, fillPaint);

            GUI.DrawText($"Best Time: {highScore:0.00}s", centerX, scoreY, outlinePaint);
            GUI.DrawText($"Best Time: {highScore:0.00}s", centerX, scoreY, fillPaint);

            GUI.Render();
        }

        public void Mouse_BottonPressed(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                sceneManager.ChangeScene(SceneTypes.SCENE_GAME);
            }
        }

        public override void Close()
        {
            sceneManager.mouseDelegate -= Mouse_BottonPressed;
        }
    }
}