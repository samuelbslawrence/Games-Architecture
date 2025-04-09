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
        private int playerLives = 3;

        // New: Store the player entity reference for access in Render().
        private Entity playerEntity;

        #region Timer Variables
        private double elapsedTime = 0;
        private bool gameEnded = false;

        private bool paused = false;

        public static bool debugMovementEnabled = true;
        private bool mKeyHandled = false;

        public static bool wallsCollisionEnabled = true;
        private bool cKeyHandled = false;


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
            camera = new Camera(new Vector3(0, 0, 0), new Vector3(1, 0, 0),
                        (float)(sceneManager.Size.X) / (float)(sceneManager.Size.Y), 0.1f, 100f);

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
            playerEntity = newEntity;
            #endregion

            #region Entity Moon
            newEntity = new Entity("Moon");
            newEntity.AddComponent(new ComponentPosition(-20.0f, 5.0f, 0.0f));
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
            newEntity.AddComponent(new ComponentSphereCollider(new Vector3(0.0f, 0.0f, 0.0f), 1.5f));
            entityManager.AddEntity(newEntity);
            #endregion

            #region Entity Maze
            newEntity = new Entity("MazeWall");
            newEntity.AddComponent(new ComponentPosition(0.0f, -1.5f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Maze/mazeWall.obj"));
            entityManager.AddEntity(newEntity);

            newEntity = new Entity("MazeFloor");
            newEntity.AddComponent(new ComponentPosition(0.0f, -1.5f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Maze/mazeFloor.obj"));
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

            #region Portal
            Entity portalFrame = new Entity("PortalFrame");
            portalFrame.AddComponent(new ComponentPosition(new Vector3(8f, -1.5f, -9.8f)));
            portalFrame.AddComponent(new ComponentGeometry("Geometry/Portal/PortalFrame.obj"));
            portalFrame.AddComponent(new ComponentSphereCollider(new Vector3(0f, -1.5f, 0f), 2.0f));
            portalFrame.AddComponent(new ComponentScale(new Vector3(0.3f, 0.3f, 0.3f)));
            newEntity.AddComponent(new ComponentAudio("Audio/portal.wav"));
            entityManager.AddEntity(portalFrame);

            Entity portalInner = new Entity("PortalInner");
            portalInner.AddComponent(new ComponentPosition(new Vector3(8f, -1.5f, -9.8f)));
            portalInner.AddComponent(new ComponentGeometry("Geometry/Portal/PortalInner.obj"));
            portalInner.AddComponent(new ComponentScale(new Vector3(0.5f, 0.5f, 0.5f)));
            entityManager.AddEntity(portalInner);
            #endregion

            #region 2D Box Colliders

            #region Collider 1
            Vector2 pointA1 = new Vector2(-10f, 10f);
            Vector2 pointB1 = new Vector2(10f, 10f);

            // Horizontal collider
            float posX1 = (pointA1.X + pointB1.X) * 0.5f;
            float posZ1 = 10f;

            float width1 = Math.Abs(pointB1.X - pointA1.X);
            float thickness1 = 0.1f;

            Vector2 boxSize1 = new Vector2(width1, thickness1);

            // Collider offset (centered)
            Vector3 colliderOffset1 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox1 = new Entity("LineBoxCollider");
            lineBox1.AddComponent(new ComponentPosition(posX1, 0.0f, posZ1));

            // Add the box collider component
            lineBox1.AddComponent(new ComponentBoxCollider(
                colliderOffset1,
                false,
                boxSize1,
                true
            ));

            entityManager.AddEntity(lineBox1);
            #endregion

            #region Collider 2
            Vector2 pointA2 = new Vector2(10f, 10f);
            Vector2 pointB2 = new Vector2(10f, -10f);

            // Vertical collider
            float posX2 = 10f;
            float posZ2 = (pointA2.Y + pointB2.Y) * 0.5f;

            float length2 = Math.Abs(pointB2.Y - pointA2.Y);
            float thickness2 = 0.1f;

            Vector2 boxSize2 = new Vector2(thickness2, length2);

            // Collider offset (centered)
            Vector3 colliderOffset2 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox2 = new Entity("LineBoxCollider");
            lineBox2.AddComponent(new ComponentPosition(posX2, 0.0f, posZ2));

            // Add the box collider component
            lineBox2.AddComponent(new ComponentBoxCollider(
                colliderOffset2,
                false,
                boxSize2,
                true
            ));

            entityManager.AddEntity(lineBox2);
            #endregion

            #region Collider 3
            Vector2 pointA3 = new Vector2(10f, -10f);
            Vector2 pointB3 = new Vector2(-10f, -10f);

            // Horizontal collider
            float posX3 = (pointA3.X + pointB3.X) * 0.5f;
            float posZ3 = -10f;

            float width3 = Math.Abs(pointB3.X - pointA3.X);
            float thickness3 = 0.1f;

            Vector2 boxSize3 = new Vector2(width3, thickness3);

            // Collider offset (centered)
            Vector3 colliderOffset3 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox3 = new Entity("LineBoxCollider");
            lineBox3.AddComponent(new ComponentPosition(posX3, 0.0f, posZ3));

            // Add the box collider component
            lineBox3.AddComponent(new ComponentBoxCollider(
                colliderOffset3,
                false,
                boxSize3,
                true
            ));

            entityManager.AddEntity(lineBox3);
            #endregion

            #region Collider 4
            Vector2 pointA4 = new Vector2(-10f, -10f);
            Vector2 pointB4 = new Vector2(-10f, 10f);

            // Vertical collider
            float posX4 = -10f;
            float posZ4 = (pointA4.Y + pointB4.Y) * 0.5f;

            float length4 = Math.Abs(pointB4.Y - pointA4.Y);
            float thickness4 = 0.1f;

            Vector2 boxSize4 = new Vector2(thickness4, length4);

            // Collider offset (centered)
            Vector3 colliderOffset4 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox4 = new Entity("LineBoxCollider");
            lineBox4.AddComponent(new ComponentPosition(posX4, 0.0f, posZ4));

            // Add the box collider component
            lineBox4.AddComponent(new ComponentBoxCollider(
                colliderOffset4,
                false,
                boxSize4,
                true
            ));

            entityManager.AddEntity(lineBox4);
            #endregion

            #region Collider 5
            Vector2 pointA5 = new Vector2(-8.75f, 6.25f);
            Vector2 pointB5 = new Vector2(-10f, 6.25f);

            // Horizontal collider
            float posX5 = (pointA5.X + pointB5.X) * 0.5f;
            float posZ5 = 6.25f;

            float width5 = Math.Abs(pointB5.X - pointA5.X);
            float thickness5 = 0.1f;

            Vector2 boxSize5 = new Vector2(width5, thickness5);

            // Collider offset (centered)
            Vector3 colliderOffset5 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox5 = new Entity("LineBoxCollider");
            lineBox5.AddComponent(new ComponentPosition(posX5, 0.0f, posZ5));

            // Add the box collider component
            lineBox5.AddComponent(new ComponentBoxCollider(
                colliderOffset5,
                false,
                boxSize5,
                true
            ));

            entityManager.AddEntity(lineBox5);
            #endregion

            #region Collider 6
            Vector2 pointA6 = new Vector2(-6.25f, 8.75f);
            Vector2 pointB6 = new Vector2(-6.25f, 10f);

            // Vertical collider
            float posX6 = -6.25f;
            float posZ6 = (pointA6.Y + pointB6.Y) * 0.5f;

            float length6 = Math.Abs(pointB6.Y - pointA6.Y);
            float thickness6 = 0.1f;

            Vector2 boxSize6 = new Vector2(thickness6, length6);

            // Collider offset (centered)
            Vector3 colliderOffset6 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox6 = new Entity("LineBoxCollider");
            lineBox6.AddComponent(new ComponentPosition(posX6, 0.0f, posZ6));

            // Add the box collider component
            lineBox6.AddComponent(new ComponentBoxCollider(
                colliderOffset6,
                false,
                boxSize6,
                true
            ));

            entityManager.AddEntity(lineBox6);
            #endregion

            #region Collider 7
            Vector2 pointA7 = new Vector2(6.25f, 8.75f);
            Vector2 pointB7 = new Vector2(6.25f, 10f);

            // Vertical collider
            float posX7 = 6.25f;
            float posZ7 = (pointA7.Y + pointB7.Y) * 0.5f;

            float length7 = Math.Abs(pointB7.Y - pointA7.Y);
            float thickness7 = 0.1f;

            Vector2 boxSize7 = new Vector2(thickness7, length7);

            // Collider offset (centered)
            Vector3 colliderOffset7 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox7 = new Entity("LineBoxCollider");
            lineBox7.AddComponent(new ComponentPosition(posX7, 0.0f, posZ7));

            // Add the box collider component
            lineBox7.AddComponent(new ComponentBoxCollider(
                colliderOffset7,
                false,
                boxSize7,
                true
            ));

            entityManager.AddEntity(lineBox7);
            #endregion

            #region Collider 8
            Vector2 pointA8 = new Vector2(6.25f, 8.75f);
            Vector2 pointB8 = new Vector2(-6.25f, 8.75f);

            // Horizontal collider
            float posX8 = (pointA8.X + pointB8.X) * 0.5f;
            float posZ8 = 8.75f;

            float width8 = Math.Abs(pointB8.X - pointA8.X);
            float thickness8 = 0.1f;

            Vector2 boxSize8 = new Vector2(width8, thickness8);

            // Collider offset (centered)
            Vector3 colliderOffset8 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox8 = new Entity("LineBoxCollider");
            lineBox8.AddComponent(new ComponentPosition(posX8, 0.0f, posZ8));

            // Add the box collider component
            lineBox8.AddComponent(new ComponentBoxCollider(
                colliderOffset8,
                false,
                boxSize8,
                true
            ));

            entityManager.AddEntity(lineBox8);
            #endregion

            #region Collider 9
            Vector2 pointA9 = new Vector2(-6.25f, 7.25f);
            Vector2 pointB9 = new Vector2(-1f, 7.25f);

            // Horizontal collider
            float posX9 = (pointA9.X + pointB9.X) * 0.5f;
            float posZ9 = 7.25f;

            float width9 = Math.Abs(pointB9.X - pointA9.X);
            float thickness9 = 0.1f;

            Vector2 boxSize9 = new Vector2(width9, thickness9);

            // Collider offset (centered)
            Vector3 colliderOffset9 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox9 = new Entity("LineBoxCollider");
            lineBox9.AddComponent(new ComponentPosition(posX9, 0.0f, posZ9));

            // Add the box collider component
            lineBox9.AddComponent(new ComponentBoxCollider(
                colliderOffset9,
                false,
                boxSize9,
                true
            ));

            entityManager.AddEntity(lineBox9);
            #endregion

            #region Collider 10
            Vector2 pointA10 = new Vector2(-1f, 7.25f);
            Vector2 pointB10 = new Vector2(-1f, 3f);

            // Vertical collider
            float posX10 = -1f;
            float posZ10 = (pointA10.Y + pointB10.Y) * 0.5f;

            float length10 = Math.Abs(pointB10.Y - pointA10.Y);
            float thickness10 = 0.1f;

            Vector2 boxSize10 = new Vector2(thickness10, length10);

            // Collider offset (centered)
            Vector3 colliderOffset10 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox10 = new Entity("LineBoxCollider");
            lineBox10.AddComponent(new ComponentPosition(posX10, 0.0f, posZ10));

            // Add the box collider component
            lineBox10.AddComponent(new ComponentBoxCollider(
                colliderOffset10,
                false,
                boxSize10,
                true
            ));

            entityManager.AddEntity(lineBox10);
            #endregion

            #region Collider 11
            Vector2 pointA11 = new Vector2(-1f, 3f);
            Vector2 pointB11 = new Vector2(-3f, 3f);

            // Horizontal collider
            float posX11 = (pointA11.X + pointB11.X) * 0.5f;
            float posZ11 = 3f;

            float width11 = Math.Abs(pointB11.X - pointA11.X);
            float thickness11 = 0.1f;

            Vector2 boxSize11 = new Vector2(width11, thickness11);

            // Collider offset (centered)
            Vector3 colliderOffset11 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox11 = new Entity("LineBoxCollider");
            lineBox11.AddComponent(new ComponentPosition(posX11, 0.0f, posZ11));

            // Add the box collider component
            lineBox11.AddComponent(new ComponentBoxCollider(
                colliderOffset11,
                false,
                boxSize11,
                true
            ));

            entityManager.AddEntity(lineBox11);
            #endregion

            #region Collider 12
            Vector2 pointA12 = new Vector2(-3f, 3f);
            Vector2 pointB12 = new Vector2(-3f, 1f);

            // Vertical collider
            float posX12 = -3f;
            float posZ12 = (pointA12.Y + pointB12.Y) * 0.5f;

            float length12 = Math.Abs(pointB12.Y - pointA12.Y);
            float thickness12 = 0.1f;

            Vector2 boxSize12 = new Vector2(thickness12, length12);

            // Collider offset (centered)
            Vector3 colliderOffset12 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox12 = new Entity("LineBoxCollider");
            lineBox12.AddComponent(new ComponentPosition(posX12, 0.0f, posZ12));

            // Add the box collider component
            lineBox12.AddComponent(new ComponentBoxCollider(
                colliderOffset12,
                false,
                boxSize12,
                true
            ));

            entityManager.AddEntity(lineBox12);
            #endregion

            #region Collider 13
            Vector2 pointA13 = new Vector2(-3f, 1f);
            Vector2 pointB13 = new Vector2(-7.25f, 1f);

            // Horizontal collider
            float posX13 = (pointA13.X + pointB13.X) * 0.5f;
            float posZ13 = 1f;

            float width13 = Math.Abs(pointB13.X - pointA13.X);
            float thickness13 = 0.1f;

            Vector2 boxSize13 = new Vector2(width13, thickness13);

            // Collider offset (centered)
            Vector3 colliderOffset13 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox13 = new Entity("LineBoxCollider");
            lineBox13.AddComponent(new ComponentPosition(posX13, 0.0f, posZ13));

            // Add the box collider component
            lineBox13.AddComponent(new ComponentBoxCollider(
                colliderOffset13,
                false,
                boxSize13,
                true
            ));

            entityManager.AddEntity(lineBox13);
            #endregion

            #region Collider 14
            Vector2 pointA14 = new Vector2(-7.25f, 1f);
            Vector2 pointB14 = new Vector2(-7.25f, 6.25f);

            // Vertical collider
            float posX14 = -7.25f;
            float posZ14 = (pointA14.Y + pointB14.Y) * 0.5f;

            float length14 = Math.Abs(pointB14.Y - pointA14.Y);
            float thickness14 = 0.1f;

            Vector2 boxSize14 = new Vector2(thickness14, length14);

            // Collider offset (centered)
            Vector3 colliderOffset14 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox14 = new Entity("LineBoxCollider");
            lineBox14.AddComponent(new ComponentPosition(posX14, 0.0f, posZ14));

            // Add the box collider component
            lineBox14.AddComponent(new ComponentBoxCollider(
                colliderOffset14,
                false,
                boxSize14,
                true
            ));

            entityManager.AddEntity(lineBox14);
            #endregion

            #region Collider 15
            Vector2 pointA15 = new Vector2(-7.25f, 6.25f);
            Vector2 pointB15 = new Vector2(-6.25f, 6.25f);

            // Horizontal collider
            float posX15 = (pointA15.X + pointB15.X) * 0.5f;
            float posZ15 = 6.25f;

            float width15 = Math.Abs(pointB15.X - pointA15.X);
            float thickness15 = 0.1f;

            Vector2 boxSize15 = new Vector2(width15, thickness15);

            // Collider offset (centered)
            Vector3 colliderOffset15 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox15 = new Entity("LineBoxCollider");
            lineBox15.AddComponent(new ComponentPosition(posX15, 0.0f, posZ15));

            // Add the box collider component
            lineBox15.AddComponent(new ComponentBoxCollider(
                colliderOffset15,
                false,
                boxSize15,
                true
            ));

            entityManager.AddEntity(lineBox15);
            #endregion

            #region Collider 16
            Vector2 pointA16 = new Vector2(-6.25f, 6.25f);
            Vector2 pointB16 = new Vector2(-6.25f, 7.25f);

            // Vertical collider
            float posX16 = -6.25f;
            float posZ16 = (pointA16.Y + pointB16.Y) * 0.5f;

            float length16 = Math.Abs(pointB16.Y - pointA16.Y);
            float thickness16 = 0.1f;

            Vector2 boxSize16 = new Vector2(thickness16, length16);

            // Collider offset (centered)
            Vector3 colliderOffset16 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox16 = new Entity("LineBoxCollider");
            lineBox16.AddComponent(new ComponentPosition(posX16, 0.0f, posZ16));

            // Add the box collider component
            lineBox16.AddComponent(new ComponentBoxCollider(
                colliderOffset16,
                false,
                boxSize16,
                true
            ));

            entityManager.AddEntity(lineBox16);
            #endregion

            #region Collider 17
            Vector2 pointA17 = new Vector2(-8.75f, 6.25f);
            Vector2 pointB17 = new Vector2(-8.75f, -6.25f);

            // Vertical collider
            float posX17 = -8.75f;
            float posZ17 = (pointA17.Y + pointB17.Y) * 0.5f;

            float length17 = Math.Abs(pointB17.Y - pointA17.Y);
            float thickness17 = 0.1f;

            Vector2 boxSize17 = new Vector2(thickness17, length17);

            // Collider offset (centered)
            Vector3 colliderOffset17 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox17 = new Entity("LineBoxCollider");
            lineBox17.AddComponent(new ComponentPosition(posX17, 0.0f, posZ17));

            // Add the box collider component
            lineBox17.AddComponent(new ComponentBoxCollider(
                colliderOffset17,
                false,
                boxSize17,
                true
            ));

            entityManager.AddEntity(lineBox17);
            #endregion

            #region Collider 18
            Vector2 pointA18 = new Vector2(-8.75f, -6.25f);
            Vector2 pointB18 = new Vector2(-10f, -6.25f);

            // Horizontal collider
            float posX18 = (pointA18.X + pointB18.X) * 0.5f;
            float posZ18 = -6.25f;

            float width18 = Math.Abs(pointB18.X - pointA18.X);
            float thickness18 = 0.1f;

            Vector2 boxSize18 = new Vector2(width18, thickness18);

            // Collider offset (centered)
            Vector3 colliderOffset18 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox18 = new Entity("LineBoxCollider");
            lineBox18.AddComponent(new ComponentPosition(posX18, 0.0f, posZ18));

            // Add the box collider component
            lineBox18.AddComponent(new ComponentBoxCollider(
                colliderOffset18,
                false,
                boxSize18,
                true
            ));

            entityManager.AddEntity(lineBox18);
            #endregion

            #region Collider 19
            Vector2 pointA19 = new Vector2(8.75f, 6.25f);
            Vector2 pointB19 = new Vector2(10f, 6.25f);

            // Horizontal collider
            float posX19 = (pointA19.X + pointB19.X) * 0.5f;
            float posZ19 = 6.25f;

            float width19 = Math.Abs(pointB19.X - pointA19.X);
            float thickness19 = 0.1f;

            Vector2 boxSize19 = new Vector2(width19, thickness19);

            Vector3 colliderOffset19 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox19 = new Entity("LineBoxCollider");
            lineBox19.AddComponent(new ComponentPosition(posX19, 0.0f, posZ19));

            lineBox19.AddComponent(new ComponentBoxCollider(
                colliderOffset19,
                false,
                boxSize19,
                true
            ));

            entityManager.AddEntity(lineBox19);
            #endregion

            #region Collider 20
            Vector2 pointA20 = new Vector2(8.75f, 6.25f);
            Vector2 pointB20 = new Vector2(8.75f, -6.25f);

            // Vertical collider
            float posX20 = 8.75f;
            float posZ20 = (pointA20.Y + pointB20.Y) * 0.5f;

            float length20 = Math.Abs(pointB20.Y - pointA20.Y);
            float thickness20 = 0.1f;

            Vector2 boxSize20 = new Vector2(thickness20, length20);

            Vector3 colliderOffset20 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox20 = new Entity("LineBoxCollider");
            lineBox20.AddComponent(new ComponentPosition(posX20, 0.0f, posZ20));

            lineBox20.AddComponent(new ComponentBoxCollider(
                colliderOffset20,
                false,
                boxSize20,
                true
            ));

            entityManager.AddEntity(lineBox20);
            #endregion

            #region Collider 21
            Vector2 pointA21 = new Vector2(8.75f, -6.25f);
            Vector2 pointB21 = new Vector2(10f, -6.25f);

            // Horizontal collider
            float posX21 = (pointA21.X + pointB21.X) * 0.5f;
            float posZ21 = -6.25f;

            float width21 = Math.Abs(pointB21.X - pointA21.X);
            float thickness21 = 0.1f;

            Vector2 boxSize21 = new Vector2(width21, thickness21);

            Vector3 colliderOffset21 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox21 = new Entity("LineBoxCollider");
            lineBox21.AddComponent(new ComponentPosition(posX21, 0.0f, posZ21));

            lineBox21.AddComponent(new ComponentBoxCollider(
                colliderOffset21,
                false,
                boxSize21,
                true
            ));

            entityManager.AddEntity(lineBox21);
            #endregion

            #region Collider 22
            Vector2 pointA22 = new Vector2(7.25f, 1f);
            Vector2 pointB22 = new Vector2(7.25f, 6.25f);

            // Vertical collider
            float posX22 = 7.25f;
            float posZ22 = (pointA22.Y + pointB22.Y) * 0.5f;

            float length22 = Math.Abs(pointB22.Y - pointA22.Y);
            float thickness22 = 0.1f;

            Vector2 boxSize22 = new Vector2(thickness22, length22);

            Vector3 colliderOffset22 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox22 = new Entity("LineBoxCollider");
            lineBox22.AddComponent(new ComponentPosition(posX22, 0.0f, posZ22));

            lineBox22.AddComponent(new ComponentBoxCollider(
                colliderOffset22,
                false,
                boxSize22,
                true
            ));

            entityManager.AddEntity(lineBox22);
            #endregion

            #region Collider 23
            Vector2 pointA23 = new Vector2(7.25f, 6.25f);
            Vector2 pointB23 = new Vector2(6.25f, 6.25f);

            // Horizontal collider
            float posX23 = (pointA23.X + pointB23.X) * 0.5f;
            float posZ23 = 6.25f;

            float width23 = Math.Abs(pointB23.X - pointA23.X);
            float thickness23 = 0.1f;

            Vector2 boxSize23 = new Vector2(width23, thickness23);

            Vector3 colliderOffset23 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox23 = new Entity("LineBoxCollider");
            lineBox23.AddComponent(new ComponentPosition(posX23, 0.0f, posZ23));

            lineBox23.AddComponent(new ComponentBoxCollider(
                colliderOffset23,
                false,
                boxSize23,
                true
            ));

            entityManager.AddEntity(lineBox23);
            #endregion

            #region Collider 24
            Vector2 pointA24 = new Vector2(6.25f, 6.25f);
            Vector2 pointB24 = new Vector2(6.25f, 7.25f);

            // Vertical collider
            float posX24 = 6.25f;
            float posZ24 = (pointA24.Y + pointB24.Y) * 0.5f;

            float length24 = Math.Abs(pointB24.Y - pointA24.Y);
            float thickness24 = 0.1f;

            Vector2 boxSize24 = new Vector2(thickness24, length24);

            Vector3 colliderOffset24 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox24 = new Entity("LineBoxCollider");
            lineBox24.AddComponent(new ComponentPosition(posX24, 0.0f, posZ24));

            lineBox24.AddComponent(new ComponentBoxCollider(
                colliderOffset24,
                false,
                boxSize24,
                true
            ));

            entityManager.AddEntity(lineBox24);
            #endregion

            #region Collider 25
            Vector2 pointA25 = new Vector2(6.25f, 7.25f);
            Vector2 pointB25 = new Vector2(1f, 7.25f);

            // Horizontal collider
            float posX25 = (pointA25.X + pointB25.X) * 0.5f;
            float posZ25 = 7.25f;

            float width25 = Math.Abs(pointB25.X - pointA25.X);
            float thickness25 = 0.1f;

            Vector2 boxSize25 = new Vector2(width25, thickness25);

            Vector3 colliderOffset25 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox25 = new Entity("LineBoxCollider");
            lineBox25.AddComponent(new ComponentPosition(posX25, 0.0f, posZ25));

            lineBox25.AddComponent(new ComponentBoxCollider(
                colliderOffset25,
                false,
                boxSize25,
                true
            ));

            entityManager.AddEntity(lineBox25);
            #endregion

            #region Collider 26
            Vector2 pointA26 = new Vector2(1f, 7.25f);
            Vector2 pointB26 = new Vector2(1f, 3f);

            // Vertical collider
            float posX26 = 1f;
            float posZ26 = (pointA26.Y + pointB26.Y) * 0.5f;

            float length26 = Math.Abs(pointB26.Y - pointA26.Y);
            float thickness26 = 0.1f;

            Vector2 boxSize26 = new Vector2(thickness26, length26);

            Vector3 colliderOffset26 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox26 = new Entity("LineBoxCollider");
            lineBox26.AddComponent(new ComponentPosition(posX26, 0.0f, posZ26));

            lineBox26.AddComponent(new ComponentBoxCollider(
                colliderOffset26,
                false,
                boxSize26,
                true
            ));

            entityManager.AddEntity(lineBox26);
            #endregion

            #region Collider 27
            Vector2 pointA27 = new Vector2(1f, 3f);
            Vector2 pointB27 = new Vector2(3f, 3f);

            // Horizontal collider
            float posX27 = (pointA27.X + pointB27.X) * 0.5f;
            float posZ27 = 3f;

            float width27 = Math.Abs(pointB27.X - pointA27.X);
            float thickness27 = 0.1f;

            Vector2 boxSize27 = new Vector2(width27, thickness27);

            Vector3 colliderOffset27 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox27 = new Entity("LineBoxCollider");
            lineBox27.AddComponent(new ComponentPosition(posX27, 0.0f, posZ27));

            lineBox27.AddComponent(new ComponentBoxCollider(
                colliderOffset27,
                false,
                boxSize27,
                true
            ));

            entityManager.AddEntity(lineBox27);
            #endregion

            #region Collider 28
            Vector2 pointA28 = new Vector2(3f, 3f);
            Vector2 pointB28 = new Vector2(3f, 1f);

            // Vertical collider
            float posX28 = 3f;
            float posZ28 = (pointA28.Y + pointB28.Y) * 0.5f;

            float length28 = Math.Abs(pointB28.Y - pointA28.Y);
            float thickness28 = 0.1f;

            Vector2 boxSize28 = new Vector2(thickness28, length28);

            Vector3 colliderOffset28 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox28 = new Entity("LineBoxCollider");
            lineBox28.AddComponent(new ComponentPosition(posX28, 0.0f, posZ28));

            lineBox28.AddComponent(new ComponentBoxCollider(
                colliderOffset28,
                false,
                boxSize28,
                true
            ));

            entityManager.AddEntity(lineBox28);
            #endregion

            #region Collider 29
            Vector2 pointA29 = new Vector2(3f, 1f);
            Vector2 pointB29 = new Vector2(7.25f, 1f);

            // Horizontal collider
            float posX29 = (pointA29.X + pointB29.X) * 0.5f;
            float posZ29 = 1f;

            float width29 = Math.Abs(pointB29.X - pointA29.X);
            float thickness29 = 0.1f;

            Vector2 boxSize29 = new Vector2(width29, thickness29);

            Vector3 colliderOffset29 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox29 = new Entity("LineBoxCollider");
            lineBox29.AddComponent(new ComponentPosition(posX29, 0.0f, posZ29));

            lineBox29.AddComponent(new ComponentBoxCollider(
                colliderOffset29,
                false,
                boxSize29,
                true
            ));

            entityManager.AddEntity(lineBox29);
            #endregion

            #region Collider 30
            Vector2 pointA30 = new Vector2(-8.75f, -6.25f);
            Vector2 pointB30 = new Vector2(-10f, -6.25f);

            // Horizontal collider
            float posX30 = (pointA30.X + pointB30.X) * 0.5f;
            float posZ30 = -6.25f;

            float width30 = Math.Abs(pointB30.X - pointA30.X);
            float thickness30 = 0.1f;

            Vector2 boxSize30 = new Vector2(width30, thickness30);

            Vector3 colliderOffset30 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox30 = new Entity("LineBoxCollider");
            lineBox30.AddComponent(new ComponentPosition(posX30, 0.0f, posZ30));

            lineBox30.AddComponent(new ComponentBoxCollider(
                colliderOffset30,
                false,
                boxSize30,
                true
            ));

            entityManager.AddEntity(lineBox30);
            #endregion

            #region Collider 31
            Vector2 pointA31 = new Vector2(-6.25f, -8.75f);
            Vector2 pointB31 = new Vector2(-6.25f, -10f);

            // Vertical collider
            float posX31 = -6.25f;
            float posZ31 = (pointA31.Y + pointB31.Y) * 0.5f;

            float length31 = Math.Abs(pointB31.Y - pointA31.Y);
            float thickness31 = 0.1f;

            Vector2 boxSize31 = new Vector2(thickness31, length31);

            Vector3 colliderOffset31 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox31 = new Entity("LineBoxCollider");
            lineBox31.AddComponent(new ComponentPosition(posX31, 0.0f, posZ31));

            lineBox31.AddComponent(new ComponentBoxCollider(
                colliderOffset31,
                false,
                boxSize31,
                true
            ));

            entityManager.AddEntity(lineBox31);
            #endregion

            #region Collider 32
            Vector2 pointA32 = new Vector2(6.25f, -8.75f);
            Vector2 pointB32 = new Vector2(6.25f, -10f);

            // Vertical collider
            float posX32 = 6.25f;
            float posZ32 = (pointA32.Y + pointB32.Y) * 0.5f;

            float length32 = Math.Abs(pointB32.Y - pointA32.Y);
            float thickness32 = 0.1f;

            Vector2 boxSize32 = new Vector2(thickness32, length32);

            Vector3 colliderOffset32 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox32 = new Entity("LineBoxCollider");
            lineBox32.AddComponent(new ComponentPosition(posX32, 0.0f, posZ32));

            lineBox32.AddComponent(new ComponentBoxCollider(
                colliderOffset32,
                false,
                boxSize32,
                true
            ));

            entityManager.AddEntity(lineBox32);
            #endregion

            #region Collider 33
            Vector2 pointA33 = new Vector2(6.25f, -8.75f);
            Vector2 pointB33 = new Vector2(-6.25f, -8.75f);

            // Horizontal collider
            float posX33 = (pointA33.X + pointB33.X) * 0.5f;
            float posZ33 = -8.75f;

            float width33 = Math.Abs(pointB33.X - pointA33.X);
            float thickness33 = 0.1f;

            Vector2 boxSize33 = new Vector2(width33, thickness33);

            Vector3 colliderOffset33 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox33 = new Entity("LineBoxCollider");
            lineBox33.AddComponent(new ComponentPosition(posX33, 0.0f, posZ33));

            lineBox33.AddComponent(new ComponentBoxCollider(
                colliderOffset33,
                false,
                boxSize33,
                true
            ));

            entityManager.AddEntity(lineBox33);
            #endregion

            #region Collider 34
            Vector2 pointA34 = new Vector2(-6.25f, -7.25f);
            Vector2 pointB34 = new Vector2(-1f, -7.25f);

            // Horizontal collider
            float posX34 = (pointA34.X + pointB34.X) * 0.5f;
            float posZ34 = -7.25f;

            float width34 = Math.Abs(pointB34.X - pointA34.X);
            float thickness34 = 0.1f;

            Vector2 boxSize34 = new Vector2(width34, thickness34);

            Vector3 colliderOffset34 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox34 = new Entity("LineBoxCollider");
            lineBox34.AddComponent(new ComponentPosition(posX34, 0.0f, posZ34));

            lineBox34.AddComponent(new ComponentBoxCollider(
                colliderOffset34,
                false,
                boxSize34,
                true
            ));

            entityManager.AddEntity(lineBox34);
            #endregion

            #region Collider 35
            Vector2 pointA35 = new Vector2(-1f, -7.25f);
            Vector2 pointB35 = new Vector2(-1f, -3f);

            // Vertical collider
            float posX35 = -1f;
            float posZ35 = (pointA35.Y + pointB35.Y) * 0.5f;

            float length35 = Math.Abs(pointB35.Y - pointA35.Y);
            float thickness35 = 0.1f;

            Vector2 boxSize35 = new Vector2(thickness35, length35);

            Vector3 colliderOffset35 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox35 = new Entity("LineBoxCollider");
            lineBox35.AddComponent(new ComponentPosition(posX35, 0.0f, posZ35));

            lineBox35.AddComponent(new ComponentBoxCollider(
                colliderOffset35,
                false,
                boxSize35,
                true
            ));

            entityManager.AddEntity(lineBox35);
            #endregion

            #region Collider 36
            Vector2 pointA36 = new Vector2(-1f, -3f);
            Vector2 pointB36 = new Vector2(-3f, -3f);

            // Horizontal collider
            float posX36 = (pointA36.X + pointB36.X) * 0.5f;
            float posZ36 = -3f;

            float width36 = Math.Abs(pointB36.X - pointA36.X);
            float thickness36 = 0.1f;

            Vector2 boxSize36 = new Vector2(width36, thickness36);

            Vector3 colliderOffset36 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox36 = new Entity("LineBoxCollider");
            lineBox36.AddComponent(new ComponentPosition(posX36, 0.0f, posZ36));

            lineBox36.AddComponent(new ComponentBoxCollider(
                colliderOffset36,
                false,
                boxSize36,
                true
            ));

            entityManager.AddEntity(lineBox36);
            #endregion

            #region Collider 37
            Vector2 pointA37 = new Vector2(-3f, -3f);
            Vector2 pointB37 = new Vector2(-3f, -1f);

            // Vertical collider
            float posX37 = -3f;
            float posZ37 = (pointA37.Y + pointB37.Y) * 0.5f;

            float length37 = Math.Abs(pointB37.Y - pointA37.Y);
            float thickness37 = 0.1f;

            Vector2 boxSize37 = new Vector2(thickness37, length37);

            Vector3 colliderOffset37 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox37 = new Entity("LineBoxCollider");
            lineBox37.AddComponent(new ComponentPosition(posX37, 0.0f, posZ37));

            lineBox37.AddComponent(new ComponentBoxCollider(
                colliderOffset37,
                false,
                boxSize37,
                true
            ));

            entityManager.AddEntity(lineBox37);
            #endregion

            #region Collider 38
            Vector2 pointA38 = new Vector2(-3f, -1f);
            Vector2 pointB38 = new Vector2(-7.25f, -1f);

            // Horizontal collider
            float posX38 = (pointA38.X + pointB38.X) * 0.5f;
            float posZ38 = -1f;

            float width38 = Math.Abs(pointB38.X - pointA38.X);
            float thickness38 = 0.1f;

            Vector2 boxSize38 = new Vector2(width38, thickness38);

            Vector3 colliderOffset38 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox38 = new Entity("LineBoxCollider");
            lineBox38.AddComponent(new ComponentPosition(posX38, 0.0f, posZ38));

            lineBox38.AddComponent(new ComponentBoxCollider(
                colliderOffset38,
                false,
                boxSize38,
                true
            ));

            entityManager.AddEntity(lineBox38);
            #endregion

            #region Collider 39
            Vector2 pointA39 = new Vector2(-7.25f, -1f);
            Vector2 pointB39 = new Vector2(-7.25f, -6.25f);

            // Vertical collider
            float posX39 = -7.25f;
            float posZ39 = (pointA39.Y + pointB39.Y) * 0.5f;

            float length39 = Math.Abs(pointB39.Y - pointA39.Y);
            float thickness39 = 0.1f;

            Vector2 boxSize39 = new Vector2(thickness39, length39);

            Vector3 colliderOffset39 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox39 = new Entity("LineBoxCollider");
            lineBox39.AddComponent(new ComponentPosition(posX39, 0.0f, posZ39));

            lineBox39.AddComponent(new ComponentBoxCollider(
                colliderOffset39,
                false,
                boxSize39,
                true
            ));

            entityManager.AddEntity(lineBox39);
            #endregion

            #region Collider 40
            Vector2 pointA40 = new Vector2(-7.25f, -6.25f);
            Vector2 pointB40 = new Vector2(-6.25f, -6.25f);

            // Horizontal collider
            float posX40 = (pointA40.X + pointB40.X) * 0.5f;
            float posZ40 = -6.25f;

            float width40 = Math.Abs(pointB40.X - pointA40.X);
            float thickness40 = 0.1f;

            Vector2 boxSize40 = new Vector2(width40, thickness40);

            Vector3 colliderOffset40 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox40 = new Entity("LineBoxCollider");
            lineBox40.AddComponent(new ComponentPosition(posX40, 0.0f, posZ40));

            lineBox40.AddComponent(new ComponentBoxCollider(
                colliderOffset40,
                false,
                boxSize40,
                true
            ));

            entityManager.AddEntity(lineBox40);
            #endregion

            #region Collider 41
            Vector2 pointA41 = new Vector2(-6.25f, -6.25f);
            Vector2 pointB41 = new Vector2(-6.25f, -7.25f);

            // Vertical collider
            float posX41 = -6.25f;
            float posZ41 = (pointA41.Y + pointB41.Y) * 0.5f;

            float length41 = Math.Abs(pointB41.Y - pointA41.Y);
            float thickness41 = 0.1f;

            Vector2 boxSize41 = new Vector2(thickness41, length41);

            Vector3 colliderOffset41 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox41 = new Entity("LineBoxCollider");
            lineBox41.AddComponent(new ComponentPosition(posX41, 0.0f, posZ41));

            lineBox41.AddComponent(new ComponentBoxCollider(
                colliderOffset41,
                false,
                boxSize41,
                true
            ));

            entityManager.AddEntity(lineBox41);
            #endregion

            #region Collider 42
            Vector2 pointA42 = new Vector2(8.75f, -6.25f);
            Vector2 pointB42 = new Vector2(10f, -6.25f);

            // Horizontal collider
            float posX42 = (pointA42.X + pointB42.X) * 0.5f;
            float posZ42 = -6.25f;

            float width42 = Math.Abs(pointB42.X - pointA42.X);
            float thickness42 = 0.1f;

            Vector2 boxSize42 = new Vector2(width42, thickness42);

            Vector3 colliderOffset42 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox42 = new Entity("LineBoxCollider");
            lineBox42.AddComponent(new ComponentPosition(posX42, 0.0f, posZ42));

            lineBox42.AddComponent(new ComponentBoxCollider(
                colliderOffset42,
                false,
                boxSize42,
                true
            ));

            entityManager.AddEntity(lineBox42);
            #endregion

            #region Collider 43
            Vector2 pointA43 = new Vector2(6.25f, -8.75f);
            Vector2 pointB43 = new Vector2(6.25f, -10f);

            // Vertical collider
            float posX43 = 6.25f;
            float posZ43 = (pointA43.Y + pointB43.Y) * 0.5f;

            float length43 = Math.Abs(pointB43.Y - pointA43.Y);
            float thickness43 = 0.1f;

            Vector2 boxSize43 = new Vector2(thickness43, length43);

            Vector3 colliderOffset43 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox43 = new Entity("LineBoxCollider");
            lineBox43.AddComponent(new ComponentPosition(posX43, 0.0f, posZ43));

            lineBox43.AddComponent(new ComponentBoxCollider(
                colliderOffset43,
                false,
                boxSize43,
                true
            ));

            entityManager.AddEntity(lineBox43);
            #endregion

            #region Collider 44
            Vector2 pointA44 = new Vector2(8.75f, -6.25f);
            Vector2 pointB44 = new Vector2(8.75f, 6.25f);

            // Vertical collider
            float posX44 = 8.75f;
            float posZ44 = (pointA44.Y + pointB44.Y) * 0.5f;

            float length44 = Math.Abs(pointB44.Y - pointA44.Y);
            float thickness44 = 0.1f;

            Vector2 boxSize44 = new Vector2(thickness44, length44);

            Vector3 colliderOffset44 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox44 = new Entity("LineBoxCollider");
            lineBox44.AddComponent(new ComponentPosition(posX44, 0.0f, posZ44));

            lineBox44.AddComponent(new ComponentBoxCollider(
                colliderOffset44,
                false,
                boxSize44,
                true
            ));

            entityManager.AddEntity(lineBox44);
            #endregion

            #region Collider 45
            Vector2 pointA45 = new Vector2(7.25f, -1f);
            Vector2 pointB45 = new Vector2(7.25f, -6.25f);

            // Vertical collider
            float posX45 = 7.25f;
            float posZ45 = (pointA45.Y + pointB45.Y) * 0.5f;

            float length45 = Math.Abs(pointB45.Y - pointA45.Y);
            float thickness45 = 0.1f;

            Vector2 boxSize45 = new Vector2(thickness45, length45);

            Vector3 colliderOffset45 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox45 = new Entity("LineBoxCollider");
            lineBox45.AddComponent(new ComponentPosition(posX45, 0.0f, posZ45));

            lineBox45.AddComponent(new ComponentBoxCollider(
                colliderOffset45,
                false,
                boxSize45,
                true
            ));

            entityManager.AddEntity(lineBox45);
            #endregion

            #region Collider 46
            Vector2 pointA46 = new Vector2(7.25f, -6.25f);
            Vector2 pointB46 = new Vector2(6.25f, -6.25f);

            // Horizontal collider
            float posX46 = (pointA46.X + pointB46.X) * 0.5f;
            float posZ46 = -6.25f;

            float width46 = Math.Abs(pointB46.X - pointA46.X);
            float thickness46 = 0.1f;

            Vector2 boxSize46 = new Vector2(width46, thickness46);

            Vector3 colliderOffset46 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox46 = new Entity("LineBoxCollider");
            lineBox46.AddComponent(new ComponentPosition(posX46, 0.0f, posZ46));

            lineBox46.AddComponent(new ComponentBoxCollider(
                colliderOffset46,
                false,
                boxSize46,
                true
            ));

            entityManager.AddEntity(lineBox46);
            #endregion

            #region Collider 47
            Vector2 pointA47 = new Vector2(6.25f, -6.25f);
            Vector2 pointB47 = new Vector2(6.25f, -7.25f);

            // Vertical collider
            float posX47 = 6.25f;
            float posZ47 = (pointA47.Y + pointB47.Y) * 0.5f;

            float length47 = Math.Abs(pointB47.Y - pointA47.Y);
            float thickness47 = 0.1f;

            Vector2 boxSize47 = new Vector2(thickness47, length47);

            Vector3 colliderOffset47 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox47 = new Entity("LineBoxCollider");
            lineBox47.AddComponent(new ComponentPosition(posX47, 0.0f, posZ47));

            lineBox47.AddComponent(new ComponentBoxCollider(
                colliderOffset47,
                false,
                boxSize47,
                true
            ));

            entityManager.AddEntity(lineBox47);
            #endregion

            #region Collider 48
            Vector2 pointA48 = new Vector2(6.25f, -7.25f);
            Vector2 pointB48 = new Vector2(1f, -7.25f);

            // Horizontal collider
            float posX48 = (pointA48.X + pointB48.X) * 0.5f;
            float posZ48 = -7.25f;

            float width48 = Math.Abs(pointB48.X - pointA48.X);
            float thickness48 = 0.1f;

            Vector2 boxSize48 = new Vector2(width48, thickness48);

            Vector3 colliderOffset48 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox48 = new Entity("LineBoxCollider");
            lineBox48.AddComponent(new ComponentPosition(posX48, 0.0f, posZ48));

            lineBox48.AddComponent(new ComponentBoxCollider(
                colliderOffset48,
                false,
                boxSize48,
                true
            ));

            entityManager.AddEntity(lineBox48);
            #endregion

            #region Collider 49
            Vector2 pointA49 = new Vector2(1f, -7.25f);
            Vector2 pointB49 = new Vector2(1f, -3f);

            // Vertical collider
            float posX49 = 1f;
            float posZ49 = (pointA49.Y + pointB49.Y) * 0.5f;

            float length49 = Math.Abs(pointB49.Y - pointA49.Y);
            float thickness49 = 0.1f;

            Vector2 boxSize49 = new Vector2(thickness49, length49);

            Vector3 colliderOffset49 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox49 = new Entity("LineBoxCollider");
            lineBox49.AddComponent(new ComponentPosition(posX49, 0.0f, posZ49));

            lineBox49.AddComponent(new ComponentBoxCollider(
                colliderOffset49,
                false,
                boxSize49,
                true
            ));

            entityManager.AddEntity(lineBox49);
            #endregion

            #region Collider 50
            Vector2 pointA50 = new Vector2(1f, -3f);
            Vector2 pointB50 = new Vector2(3f, -3f);

            // Horizontal collider
            float posX50 = (pointA50.X + pointB50.X) * 0.5f;
            float posZ50 = -3f;

            float width50 = Math.Abs(pointB50.X - pointA50.X);
            float thickness50 = 0.1f;

            Vector2 boxSize50 = new Vector2(width50, thickness50);

            Vector3 colliderOffset50 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox50 = new Entity("LineBoxCollider");
            lineBox50.AddComponent(new ComponentPosition(posX50, 0.0f, posZ50));

            lineBox50.AddComponent(new ComponentBoxCollider(
                colliderOffset50,
                false,
                boxSize50,
                true
            ));

            entityManager.AddEntity(lineBox50);
            #endregion

            #region Collider 51
            Vector2 pointA51 = new Vector2(3f, -3f);
            Vector2 pointB51 = new Vector2(3f, -1f);

            // Vertical collider
            float posX51 = 3f;
            float posZ51 = (pointA51.Y + pointB51.Y) * 0.5f;

            float length51 = Math.Abs(pointB51.Y - pointA51.Y);
            float thickness51 = 0.1f;

            Vector2 boxSize51 = new Vector2(thickness51, length51);

            Vector3 colliderOffset51 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox51 = new Entity("LineBoxCollider");
            lineBox51.AddComponent(new ComponentPosition(posX51, 0.0f, posZ51));

            lineBox51.AddComponent(new ComponentBoxCollider(
                colliderOffset51,
                false,
                boxSize51,
                true
            ));

            entityManager.AddEntity(lineBox51);
            #endregion

            #region Collider 52
            Vector2 pointA52 = new Vector2(3f, -1f);
            Vector2 pointB52 = new Vector2(7.25f, -1f);

            // Horizontal collider
            float posX52 = (pointA52.X + pointB52.X) * 0.5f;
            float posZ52 = -1f;

            float width52 = Math.Abs(pointB52.X - pointA52.X);
            float thickness52 = 0.1f;

            Vector2 boxSize52 = new Vector2(width52, thickness52);

            Vector3 colliderOffset52 = new Vector3(0.0f, 0.0f, 0.0f);

            Entity lineBox52 = new Entity("LineBoxCollider");
            lineBox52.AddComponent(new ComponentPosition(posX52, 0.0f, posZ52));

            lineBox52.AddComponent(new ComponentBoxCollider(
                colliderOffset52,
                false,
                boxSize52,
                true
            ));

            entityManager.AddEntity(lineBox52);
            #endregion

            #endregion

            #region Entity Skybox
            newEntity = new Entity("Skybox");
            newEntity.AddComponent(new ComponentPosition(0.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Skybox/moon.obj"));
            newEntity.AddComponent(new ComponentScale(new Vector3(20.0f, 20.0f, 20.0f)));
            entityManager.AddEntity(newEntity);
            #endregion

            #region RollingObject
            newEntity = new Entity("RollingObject");
            newEntity.AddComponent(new ComponentPosition(new Vector3(9.0f, -1.2f, 9.0f)));
            newEntity.AddComponent(new ComponentVelocity(new Vector3(1.0f, 0.0f, 0.5f)));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Moon/moon.obj"));
            newEntity.AddComponent(new ComponentSphereCollider(new Vector3(0.0f, 0.0f, 0.0f), 1.0f));
            newEntity.AddComponent(new ComponentScale(new Vector3(0.2f, 0.2f, 0.2f)));
            entityManager.AddEntity(newEntity);
            #endregion

            #region BouncingObject
            newEntity = new Entity("BouncingObject");
            newEntity.AddComponent(new ComponentPosition(new Vector3(-10.0f, -1.2f, -10.0f)));
            newEntity.AddComponent(new ComponentVelocity(new Vector3(2.0f, 0.0f, 0.0f)));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Moon/moon.obj"));
            newEntity.AddComponent(new ComponentSphereCollider(new Vector3(0.0f, 0.0f, 0.0f), 1.0f));
            newEntity.AddComponent(new ComponentScale(new Vector3(0.2f, 0.2f, 0.2f)));
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
            systemManager.AddSystem(new SystemRollingObject());
            systemManager.AddSystem(new SystemBouncingObject());
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

            var skybox = entityManager.FindEntity("Skybox");
            if (skybox != null)
            {
                var playerPos = playerEntity.GetComponent<ComponentPosition>().Position;
                var skyboxPos = skybox.GetComponent<ComponentPosition>();
                if (skyboxPos != null)
                {
                    // Set skybox X and Z to player's X and Z but keep its current Y value.
                    skyboxPos.Position = new Vector3(playerPos.X, skyboxPos.Position.Y, playerPos.Z);
                }
            }
        }

        public void HandlePlayerHit()
        {
            playerLives--;

            // Reset the player's position and velocity to the starting point.
            var posComp = playerEntity.GetComponent<ComponentPosition>();
            var velComp = playerEntity.GetComponent<ComponentVelocity>();
            posComp.Position = new Vector3(9.0f, 0.0f, -9.0f); // Starting position from your setup.
            velComp.Velocity = Vector3.Zero;

            // Check for game over: if lives drop to 0 or less, transition back to the Main Menu.
            if (playerLives <= 0)
            {
                sceneManager.ChangeScene(SceneTypes.SCENE_MAIN_MENU);
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

            // New: Render the player's X and Y location
            // Assuming that the player's position is stored in a ComponentPosition with a public 'Position' property.
            ComponentPosition posComponent = playerEntity.GetComponent<ComponentPosition>();
            Vector3 pos = posComponent.Position;
            GUI.DrawText($"Player Pos: {pos.X:0.00}, {pos.Z:0.00}", 30, 160, 30, 255, 255, 255);
            GUI.DrawText($"Lives: {playerLives}", 30, 200, 30, 255, 0, 0);
            GUI.DrawText($"Drone: {(debugMovementEnabled ? "ON" : "OFF")}", 30, 240, 30, 255, 255, 0);
            GUI.DrawText($"Wall Collision: {(wallsCollisionEnabled ? "ON" : "OFF")}", 30, 280, 30, 255, 255, 0);
            GUI.Render();
        }

        public override void Close()
        {
            // Remove player input event subscriptions.
            sceneManager.keyboardDownDelegate -= Keyboard_KeyDown;
            sceneManager.keyboardUpDelegate -= Keyboard_KeyUp;

            // Clean up any audio components (e.g., the drone’s audio)
            foreach (var entity in entityManager.Entities())
            {
                var audioComponent = entity.GetComponent<ComponentAudio>();
                if (audioComponent != null)
                {
                    audioComponent.Stop();    // Stop the audio playback.
                    audioComponent.CleanUp(); // Delete the audio source and buffer.
                }
            }

            // Optionally remove all asset resources as before.
            ResourceManager.RemoveAllAssets();

            // Reset the GUI for the new scene.
            GUI.SetUpGUI(SceneManager.WindowWidth, SceneManager.WindowHeight);
        }

        public void Keyboard_KeyDown(KeyboardKeyEventArgs e)
        {
            keysPressed[(char)e.Key] = true;

            // Toggle the debug movement flag on press of M (only once per press)
            if (e.Key == Keys.M && !mKeyHandled)
            {
                debugMovementEnabled = !debugMovementEnabled;
                mKeyHandled = true;
                Console.WriteLine("Debug Movement Enabled: " + debugMovementEnabled);
            }

            if (e.Key == Keys.C && !cKeyHandled)
            {
                wallsCollisionEnabled = !wallsCollisionEnabled;
                cKeyHandled = true;
                Console.WriteLine("Maze Wall Collision Enabled: " + wallsCollisionEnabled);
            }

            // (Other key handling remains, for example, for the Pause functionality.)
            if (e.Key == Keys.P && !paused)
            {
                paused = true;
                PauseScene pauseScene = new PauseScene(sceneManager, this);
            }
        }

        public void Keyboard_KeyUp(KeyboardKeyEventArgs e)
        {
            keysPressed[(char)e.Key] = false;
            if (e.Key == Keys.M)
            {
                mKeyHandled = false;
            }

            if (e.Key == Keys.C)
            {
                cKeyHandled = false;
            }
        }

        public void Resume()
        {
            paused = false;
        }

        public double CurrentElapsedTime
        {
            get { return elapsedTime; }
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

            foreach (var entity in entityManager.Entities())
            {
                var audioComponent = entity.GetComponent<ComponentAudio>();
                if (audioComponent != null)
                {
                    audioComponent.Stop();
                    audioComponent.CleanUp();
                }
            }
        }
    }
}