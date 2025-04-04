using OpenTK.Graphics.OpenGL;
using OpenGL_Game.Managers;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;

namespace OpenGL_Game.Scenes
{
    class EndScene : Scene
    {
        double finalTime;
        bool scoreSent = false;

        public EndScene(SceneManager sceneManager) : base(sceneManager)
        {
            sceneManager.Title = "End Scene";
            sceneManager.renderer = Render;
            sceneManager.updater = Update;
            sceneManager.mouseDelegate += Mouse_ButtonPressed;
            // Use the same orange background as MainMenuScene
            GL.ClearColor(1.0f, 0.5f, 0.0f, 1.0f);

            // Get final time from GameScene
            finalTime = GameScene.finalTime;
        }

        public override void Update(FrameEventArgs e)
        {
            if (!scoreSent)
            {
                Client.SendScore(finalTime);
                scoreSent = true;
            }
        }

        public override void Render(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Use a top-left coordinate system:
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
            // New positions: title at 150, final time at 230, instruction at 310.
            float titleY = 150;
            float timeY = titleY + 80;
            float instructionY = timeY + 80;

            GUI.DrawText("Congratulations! You Win!", centerX, titleY, outlinePaint);
            GUI.DrawText("Congratulations! You Win!", centerX, titleY, fillPaint);

            GUI.DrawText($"Your Time: {finalTime:0.00}s", centerX, timeY, outlinePaint);
            GUI.DrawText($"Your Time: {finalTime:0.00}s", centerX, timeY, fillPaint);

            GUI.DrawText("Click to return to Main Menu", centerX, instructionY, outlinePaint);
            GUI.DrawText("Click to return to Main Menu", centerX, instructionY, fillPaint);

            GUI.Render();
        }

        private void Mouse_ButtonPressed(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                sceneManager.ChangeScene(SceneTypes.SCENE_MAIN_MENU);
            }
        }

        public override void Close()
        {
            sceneManager.mouseDelegate -= Mouse_ButtonPressed;
        }
    }
}