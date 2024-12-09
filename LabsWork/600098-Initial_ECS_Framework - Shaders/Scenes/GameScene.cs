using OpenTK.Graphics.OpenGL;
using OpenGL_Game.Components;
using OpenGL_Game.Systems;
using OpenGL_Game.Managers;
using OpenGL_Game.Objects;
using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using SkiaSharp;

namespace OpenGL_Game.Scenes
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class GameScene : Scene
    {
        public static float dt = 0;
        EntityManager entityManager;
        SystemManager systemManager;
        public Camera camera;
        public static GameScene gameInstance;
        bool[] keysPressed = new bool[349];

        public GameScene(SceneManager sceneManager) : base(sceneManager)
        {
            gameInstance = this;
            entityManager = new EntityManager();
            systemManager = new SystemManager();

            // Set the title of the window
            sceneManager.Title = "Game";
            // Set the Render and Update delegates to the Update and Render methods of this class
            sceneManager.renderer = Render;
            sceneManager.updater = Update;
            // Set Keyboard events to go to a method in this class
            sceneManager.keyboardDownDelegate += Keyboard_KeyDown;
            sceneManager.keyboardUpDelegate += Keyboard_KeyUp;


            // Enable Depth Testing
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            // Set Camera
            camera = new Camera(new Vector3(0, 4, 7), new Vector3(0, 0, 0), (float)(sceneManager.Size.X) / (float)(sceneManager.Size.Y), 0.1f, 100f);

            CreateEntities();
            CreateSystems();

            // TODO: Add your initialization logic here
        }

        private void CreateEntities()
        {
            Entity newEntity;

            newEntity = new Entity("Moon");
            newEntity.AddComponent(new ComponentPosition(-5.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Moon/moon.obj"));
            entityManager.AddEntity(newEntity);

            newEntity = new Entity("Wraith_Raider_Starship");
            newEntity.AddComponent(new ComponentPosition(+2.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Wraith_Raider_Starship/Wraith_Raider_Starship.obj"));
            entityManager.AddEntity(newEntity);

            newEntity = new Entity("Intergalactic_Spaceship");
            newEntity.AddComponent(new ComponentVelocity(0.0f, 0.0f, +5.0f));
            newEntity.AddComponent(new ComponentPosition(0.0f, 0.0f, -20.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Intergalactic_Spaceship/Intergalactic_Spaceship.obj"));
            entityManager.AddEntity(newEntity);
        }

        private void CreateSystems()
        {
            systemManager.AddSystem(new SystemRender());
            systemManager.AddSystem(new SystemPhysics());
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="e">Provides a snapshot of timing values.</param>
        public override void Update(FrameEventArgs e)
        {
            dt = (float)e.Time;
            //System.Console.WriteLine("fps=" + (int)(1.0/dt));

            // TODO: Add your update logic here


            if (keysPressed[(char)Keys.Up])
            {
                camera.MoveForward(0.1f);
            }

            if (keysPressed[(char)Keys.Down])
            {
                camera.MoveForward(-0.1f);
            }

            if (keysPressed[(char)Keys.Left])
            {
                camera.RotateY(-0.01f);
            }

            if (keysPressed[(char)Keys.Right])
            {
                camera.RotateY(0.01f);
            }

            if (keysPressed[(char)Keys.M])
            {
                sceneManager.StartMenu();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="e">Provides a snapshot of timing values.</param>
        public override void Render(FrameEventArgs e)
        {
            GL.Viewport(0, 0, sceneManager.Size.X, sceneManager.Size.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Action ALL systems
            systemManager.ActionSystems(entityManager);

            // Render score
            GUI.DrawText("Score: 000", 30, 80, 30, 255, 255, 255);
            GUI.Render();
        }

        /// <summary>
        /// This is called when the game exits.
        /// </summary>
        public override void Close()
        {
            sceneManager.keyboardDownDelegate -= Keyboard_KeyDown;

            ResourceManager.RemoveAllAssets();

            GUI.SetUpGUI(SceneManager.WindowWidth, SceneManager.WindowHeight);

            sceneManager.keyboardUpDelegate -= Keyboard_KeyUp;


            // Need to remove assets (except Text) from Resource Manager
        }

        public void Keyboard_KeyDown(KeyboardKeyEventArgs e)
        {
            keysPressed[(char)e.Key] = true;

            
        }

        public void Keyboard_KeyUp(KeyboardKeyEventArgs e)
        {
            keysPressed[(char)e.Key] = false;
        }
    }
}