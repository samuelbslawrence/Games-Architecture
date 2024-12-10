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
using System.Diagnostics;

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

        Vector3 sourcePosition; // NEW for Audio
        int audioBuffer;        // NEW for Audio
        int audioSource;        // NEW for Audio

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

            // NEW for Audio
            // Setup Audio Source from the Audio Buffer
            audioBuffer = LoadAudio("Audio/buzz.wav");
            audioSource = AL.GenSource();
            AL.Source(audioSource, ALSourcei.Buffer, audioBuffer); // attach the buffer to a source
            AL.Source(audioSource, ALSourceb.Looping, true); // source loops infinitely
            sourcePosition = new Vector3(-2.0f, 0.0f, 0.0f); // place the source a position at the centre of the Moon
            AL.Source(audioSource, ALSource3f.Position, ref sourcePosition);
            AL.SourcePlay(audioSource); // play the ausio source


            // TODO: Add your initialization logic here
        }

        // New for Audio
        public static int LoadAudio(string filename)
        {
            int audioBuffer;
            // reserve a Handle for the audio file
            audioBuffer = AL.GenBuffer();

            // Load a .wav file from disk.
            int channels, bits_per_sample, sample_rate;
            byte[] sound_data = LoadWave(
                File.Open(filename, FileMode.Open),
                out channels,
                out bits_per_sample,
                out sample_rate);
            ALFormat sound_format =
                channels == 1 && bits_per_sample == 8 ? ALFormat.Mono8 :
                channels == 1 && bits_per_sample == 16 ? ALFormat.Mono16 :
                channels == 2 && bits_per_sample == 8 ? ALFormat.Stereo8 :
                channels == 2 && bits_per_sample == 16 ? ALFormat.Stereo16 :
                (ALFormat)0; // unknown
            AL.BufferData(audioBuffer, sound_format, ref sound_data[0], sound_data.Length, sample_rate);
            Debug.Assert(AL.GetError() == ALError.NoError, "Error loading audio");

            return audioBuffer;
        }
        /// <summary>
        /// Load a WAV file.
        /// </summary>
        private static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }
        private void CreateEntities()
        {
            Entity newEntity;

            newEntity = new Entity("Moon");
            newEntity.AddComponent(new ComponentPosition(-2.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Moon/moon.obj"));
            entityManager.AddEntity(newEntity);
        }

        private void CreateSystems()
        {
            ISystem newSystem;

            newSystem = new SystemRender();
            systemManager.AddSystem(newSystem);
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

            // NEW for Audio
            // Update OpenAL Listener Position and Orientation based on the camera
            AL.Listener(ALListener3f.Position, ref camera.cameraPosition);
            AL.Listener(ALListenerfv.Orientation, ref camera.cameraDirection, ref camera.cameraUp);

            // ***************************************************************************************
            // If Audio Sources are moving (in our case the Moon), then update audio sources positions
            // ***************************************************************************************

            // Find the moon Entity
            Entity moon = entityManager.FindEntity("Moon");
            IComponent positionComponent = moon.Components.Find(delegate (IComponent component)
            {
                return component.ComponentType == ComponentTypes.COMPONENT_POSITION;
            });
            // Move the moon
            ((ComponentPosition)positionComponent).Position = new Vector3(((ComponentPosition)positionComponent).Position.X + 0.01f,
                                                                          ((ComponentPosition)positionComponent).Position.Y,
                                                                          ((ComponentPosition)positionComponent).Position.Z);
            sourcePosition = ((ComponentPosition)positionComponent).Position;

            // Update the audio source position to the position of the moon
            AL.Source(audioSource, ALSource3f.Position, ref sourcePosition);


            // TODO: Add your update logic here
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

            AL.SourceStop(audioSource);     // NEW for Audio
            AL.DeleteSource(audioSource);   // NEW for Audio
            AL.DeleteBuffer(audioBuffer);   // NEW for Audio

            // Need to remove assets (except Text) from Resource Manager
        }

        public void Keyboard_KeyDown(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Up:
                    camera.MoveForward(0.1f);
                    break;
                case Keys.Down:
                    camera.MoveForward(-0.1f);
                    break;
                case Keys.Left:
                    camera.RotateY(-0.01f);
                    break;
                case Keys.Right:
                    camera.RotateY(0.01f);
                    break;
                case Keys.M:
                    sceneManager.StartMenu();
                    break;
            }
        }
    }
}