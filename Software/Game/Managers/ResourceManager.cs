using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SkiaSharp;
using OpenTK.Graphics.OpenGL;
using OpenGL_Game.OBJLoader;
using OpenTK.Audio.OpenAL;

namespace OpenGL_Game.Managers
{
    static class ResourceManager
    {
        static Dictionary<string, Geometry> geometryDictionary = new Dictionary<string, Geometry>();
        static Dictionary<string, int> textureDictionary = new Dictionary<string, int>();
        // New dictionary to cache audio buffers by filename.
        static Dictionary<string, int> audioDictionary = new Dictionary<string, int>();

        public static void RemoveAllAssets()
        {
            foreach (var geometry in geometryDictionary)
            {
                geometry.Value.RemoveGeometry();
            }
            geometryDictionary.Clear();
            foreach (var texture in textureDictionary)
            {
                GL.DeleteTexture(texture.Value);
            }
            textureDictionary.Clear();
            // Optionally delete audio buffers as needed
            foreach (var audio in audioDictionary.Values)
            {
                AL.DeleteBuffer(audio);
            }
            audioDictionary.Clear();
        }

        public static Geometry LoadGeometry(string filename)
        {
            Geometry geometry;
            geometryDictionary.TryGetValue(filename, out geometry);
            if (geometry == null)
            {
                geometry = new Geometry();
                geometry.LoadObject(filename);
                geometryDictionary.Add(filename, geometry);
            }

            return geometry;
        }

        public static int LoadTexture(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                throw new ArgumentException(filename);

            int texture;
            textureDictionary.TryGetValue(filename, out texture);
            if (texture == 0)
            {
                texture = GL.GenTexture();
                textureDictionary.Add(filename, texture);
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, texture);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                SKBitmap bmp = SKBitmap.Decode(filename);
                if (bmp.ColorType == SKColorType.Bgra8888)
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0,
                                  OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp.GetPixels());
                }

                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Disable(EnableCap.Texture2D);
            }

            return texture;
        }

        // Modified LoadAudio method with caching.
        public static int LoadAudio(string filename)
        {
            if (audioDictionary.TryGetValue(filename, out int audioBuffer))
            {
                return audioBuffer;
            }

            // Reserve a Handle for the audio file.
            audioBuffer = AL.GenBuffer();

            // Load a .wav file from disk.
            int channels, bits_per_sample, sample_rate;
            byte[] sound_data = LoadWave(File.Open(filename, FileMode.Open), out channels, out bits_per_sample, out sample_rate);
            ALFormat sound_format =
                channels == 1 && bits_per_sample == 8 ? ALFormat.Mono8 :
                channels == 1 && bits_per_sample == 16 ? ALFormat.Mono16 :
                channels == 2 && bits_per_sample == 8 ? ALFormat.Stereo8 :
                channels == 2 && bits_per_sample == 16 ? ALFormat.Stereo16 :
                (ALFormat)0;
            AL.BufferData(audioBuffer, sound_format, ref sound_data[0], sound_data.Length, sample_rate);
            Debug.Assert(AL.GetError() == ALError.NoError, "Error loading audio");

            // Cache the loaded audio buffer.
            audioDictionary.Add(filename, audioBuffer);
            return audioBuffer;
        }

        // New helper method to play audio directly.
        public static void PlayAudio(string filename)
        {
            int buffer = LoadAudio(filename);
            // Create a new audio source.
            int source = AL.GenSource();
            AL.Source(source, ALSourcei.Buffer, buffer);
            AL.SourcePlay(source);
            // For a real game, you may want to track sources and delete them when playback is complete.
        }

        private static byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header.
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header.
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
    }
}