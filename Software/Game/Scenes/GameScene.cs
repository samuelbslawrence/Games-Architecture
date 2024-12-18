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
using OpenTK.Audio.OpenAL;
using System.IO;

namespace OpenGL_Game.Scenes
{
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
            camera = new Camera(new Vector3(0, 0, 0), new Vector3(1, 0, 0), (float)(sceneManager.Size.X) / (float)(sceneManager.Size.Y), 0.1f, 100f);

            CreateEntities();
            CreateSystems();
        }

        private void CreateEntities()
        {
            Entity newEntity;

            // Player
            newEntity = new Entity("Player");
            newEntity.AddComponent(new ComponentPlayer(camera, Vector3.UnitX));
            newEntity.AddComponent(new ComponentPosition(Vector3.Zero));
            newEntity.AddComponent(new ComponentVelocity(Vector3.Zero));
            entityManager.AddEntity(newEntity);

            // Moon
            newEntity = new Entity("Moon");
            newEntity.AddComponent(new ComponentPosition(-40.0f, 5.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Moon/moon.obj"));
            entityManager.AddEntity(newEntity);

            // Drone
            newEntity = new Entity("Intergalactic_Spaceship");
            newEntity.AddComponent(new ComponentVelocity(0.0f, 0.0f, +5.0f));
            newEntity.AddComponent(new ComponentPosition(0.0f, 5.0f, -20.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Intergalactic_Spaceship/Intergalactic_Spaceship.obj"));
            newEntity.AddComponent(new ComponentAudio("Audio/buzz.wav"));
            entityManager.AddEntity(newEntity);

            // Maze
            newEntity = new Entity("Maze");
            newEntity.AddComponent(new ComponentPosition(0.0f, -1.5f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Maze/maze.obj"));
            entityManager.AddEntity(newEntity);

            // Key 1
            newEntity = new Entity("Key1");
            newEntity.AddComponent(new ComponentPosition(9.0f, 0.1f, 9.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Key/key.obj"));
            entityManager.AddEntity(newEntity);

            // Key 2
            newEntity = new Entity("Key2");
            newEntity.AddComponent(new ComponentPosition(-9.0f, 0.1f, -9.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Key/key.obj"));
            entityManager.AddEntity(newEntity);

            // Key 3
            newEntity = new Entity("Key3");
            newEntity.AddComponent(new ComponentPosition(-9.0f, 0.1f, 9.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Key/key.obj"));
            entityManager.AddEntity(newEntity);
        }

        private void CreateSystems()
        {
            systemManager.AddSystem(new SystemPlayer());
            systemManager.AddSystem(new SystemRender());
            systemManager.AddSystem(new SystemPhysics());
            systemManager.AddSystem(new SystemAudio());
        }

        public override void Update(FrameEventArgs e)
        {
            dt = (float)e.Time;
            // System.Console.WriteLine("fps=" + (int)(1.0/dt));

            if (keysPressed[(char)Keys.M])
            {
                sceneManager.StartMenu();
            }
        }

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

        public override void Close()
        {
            sceneManager.keyboardDownDelegate -= Keyboard_KeyDown;

            ResourceManager.RemoveAllAssets();

            GUI.SetUpGUI(SceneManager.WindowWidth, SceneManager.WindowHeight);

            sceneManager.keyboardUpDelegate -= Keyboard_KeyUp;
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