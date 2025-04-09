using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;
using OpenGL_Game.Managers;

namespace OpenGL_Game.Scenes
{
    class PauseScene : Scene
    {
        // Store the reference to the game scene that is being paused.
        private GameScene pausedGameScene;

        public PauseScene(SceneManager sceneManager, GameScene gameScene)
    : base(sceneManager)
        {
            pausedGameScene = gameScene;
            sceneManager.Title = "Paused";
            // Reset the P key state so we don't immediately resume.
            ControlsManager.keysPressed[(char)Keys.P] = false;
            // Override the renderer and updater with our pause methods.
            sceneManager.renderer = Render;
            sceneManager.updater = Update;
            sceneManager.mouseDelegate += Mouse_ButtonPressed;
            // Set background to orange.
            GL.ClearColor(1.0f, 0.5f, 0.0f, 1.0f);
        }


        public override void Update(FrameEventArgs e)
        {
            // If the user presses the P key, resume the game.
            if (ControlsManager.keysPressed[(char)Keys.P])
            {
                ResumeGame();
            }
        }

        public override void Render(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Set up a 2D orthographic projection.
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, sceneManager.Size.X, sceneManager.Size.Y, 0, -1, 1);

            // Define centered text styles.
            float centerX = sceneManager.Size.X * 0.5f;
            float centerY = sceneManager.Size.Y * 0.5f;
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

            // Draw pause text.
            GUI.DrawText("Game Paused", centerX, centerY - 40, outlinePaint);
            GUI.DrawText("Game Paused", centerX, centerY - 40, fillPaint);
            GUI.DrawText("Press P or click to resume", centerX, centerY + 40, outlinePaint);
            GUI.DrawText("Press P or click to resume", centerX, centerY + 40, fillPaint);
            GUI.Render();
        }

        public override void Close()
        {
            sceneManager.mouseDelegate -= Mouse_ButtonPressed;
        }

        private void Mouse_ButtonPressed(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                ResumeGame();
            }
        }

        private void ResumeGame()
        {
            // Reset the P key state to avoid immediate toggles.
            ControlsManager.keysPressed[(char)Keys.P] = false;
            // Optionally, signal the game scene to resume any paused state.
            pausedGameScene.Resume();
            // Restore the GameScene as the active update and render delegates.
            sceneManager.renderer = pausedGameScene.Render;
            sceneManager.updater = pausedGameScene.Update;
            // Remove the pause scene's extra event handlers.
            this.Close();
        }
    }
}