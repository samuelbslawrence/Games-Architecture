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
        public EntityManager entityManager;
        SystemManager systemManager;
        public Camera camera;
        public static GameScene gameInstance;
        bool[] keysPressed = new bool[349];
        public int keysCollected = 0;

        #region Timer Variables
        private double elapsedTime = 0;
        private bool gameEnded = false;
        public static double finalTime { get; private set; }
        #endregion

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

        #region Create Entities
        private void CreateEntities()
        {
            Entity newEntity;

            #region Entity Player
            newEntity = new Entity("Player");
            newEntity.AddComponent(new ComponentPlayer(camera, -Vector3.UnitX));
            newEntity.AddComponent(new ComponentPosition(new Vector3(9.0f, 0.0f, -9.0f)));
            newEntity.AddComponent(new ComponentVelocity(Vector3.Zero));
            entityManager.AddEntity(newEntity);
            #endregion

            #region Entity Moon
            newEntity = new Entity("Moon");
            newEntity.AddComponent(new ComponentPosition(-40.0f, 5.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Moon/moon.obj"));
            entityManager.AddEntity(newEntity);
            #endregion

            #region Entity Drone
            newEntity = new Entity("Intergalactic_Spaceship");
            newEntity.AddComponent(new ComponentVelocity(0.0f, 0.0f, +5.0f));
            newEntity.AddComponent(new ComponentPosition(0.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Intergalactic_Spaceship/Intergalactic_Spaceship.obj"));
            newEntity.AddComponent(new ComponentScale(new Vector3(0.5f, 0.5f, 0.5f)));
            newEntity.AddComponent(new ComponentAudio("Audio/buzz.wav"));
            entityManager.AddEntity(newEntity);
            #endregion

            #region Entity Maze
            newEntity = new Entity("Maze");
            newEntity.AddComponent(new ComponentPosition(0.0f, -1.5f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Maze/maze.obj"));
            entityManager.AddEntity(newEntity);
            #endregion

            #region Entity Keys
            // Key 1
            newEntity = new Entity("Key1");
            newEntity.AddComponent(new ComponentPosition(9.0f, 0.1f, 9.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Key/key.obj"));
            newEntity.AddComponent(new ComponentSphereCollider(new Vector3(9.0f, 0.1f, 9.0f), 1.0f));
            entityManager.AddEntity(newEntity);

            // Key 2
            newEntity = new Entity("Key2");
            newEntity.AddComponent(new ComponentPosition(-9.0f, 0.1f, -9.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Key/key.obj"));
            newEntity.AddComponent(new ComponentSphereCollider(new Vector3(-9.0f, 0.1f, -9.0f), 1.0f));
            entityManager.AddEntity(newEntity);

            // Key 3
            newEntity = new Entity("Key3");
            newEntity.AddComponent(new ComponentPosition(-9.0f, 0.1f, 9.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Key/key.obj"));
            newEntity.AddComponent(new ComponentSphereCollider(new Vector3(-9.0f, 0.1f, 9.0f), 1.0f));
            entityManager.AddEntity(newEntity);
            #endregion

            #region Portal Frame
            Entity portalFrame = new Entity("PortalFrame");
            portalFrame.AddComponent(new ComponentPosition(new Vector3(0f, -1.5f, 0f)));
            portalFrame.AddComponent(new ComponentGeometry("Geometry/Portal/PortalFrame.obj"));
            portalFrame.AddComponent(new ComponentSphereCollider(new Vector3(0f, -1.5f, 0f), 2.0f));
            entityManager.AddEntity(portalFrame);
            #endregion

            #region Box Colliders
            Entity testBox = new Entity("TestBoxCollider");
            testBox.AddComponent(new ComponentPosition(-20.0f, 0.0f, 0.0f));
            testBox.AddComponent(new ComponentBoxCollider(
                new Vector3(-20.0f, 0.0f, 0.0f),
                false,
                new Vector2(4.0f, 4.0f),
                true
            ));
            testBox.AddComponent(new ComponentGeometry("Geometry/Debug/red_box.obj"));
            entityManager.AddEntity(testBox);
            #endregion

            #region Entity Skybox
            newEntity = new Entity("Skybox");
            newEntity.AddComponent(new ComponentPosition(0.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Skybox/moon.obj"));
            newEntity.AddComponent(new ComponentScale(new Vector3(10.0f, 10.0f, 10.0f)));
            entityManager.AddEntity(newEntity);
            #endregion
        }
        #endregion

        private void CreateSystems()
        {
            systemManager.AddSystem(new SystemPlayer());
            systemManager.AddSystem(new SystemRender());
            systemManager.AddSystem(new SystemPhysics());
            systemManager.AddSystem(new SystemAudio());
            systemManager.AddSystem(new SystemCollision(entityManager));
            systemManager.AddSystem(new SystemScale());
            systemManager.AddSystem(new SystemDroneAI());
        }

        public override void Update(FrameEventArgs e)
        {
            dt = (float)e.Time;
            // Update the timer only if the game hasn't ended
            if (!gameEnded)
            {
                elapsedTime += e.Time;
            }

            if (keysPressed[(char)Keys.M])
            {
                sceneManager.StartMenu();
            }
        }

        public override void Render(FrameEventArgs e)
        {
            GL.Viewport(0, 0, sceneManager.Size.X, sceneManager.Size.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            systemManager.ActionSystems(entityManager);

            // Render key collection counter and timer on HUD
            GUI.DrawText($"Keys: {keysCollected} / 3", 30, 80, 30, 255, 255, 255);
            GUI.DrawText($"Time: {elapsedTime:0.00} s", 30, 120, 30, 255, 255, 255);
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

        // Called by collision system when the portal is touched and all keys are collected
        public void GoToEndScene()
        {
            if (!gameEnded)
            {
                finalTime = elapsedTime;
                gameEnded = true;
                sceneManager.ChangeScene(SceneTypes.SCENE_END);
            }
        }
    }
}