using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenGL_Game
{
    public static class ShaderLoader
    {
        public static int LoadShaderProgram(string vertexShaderPath, string fragmentShaderPath)
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderPath);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderPath);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus != (int)All.True)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Program linking failed: {infoLog}");
            }

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }

        private static int CompileShader(ShaderType type, string filePath)
        {
            string shaderSource = File.ReadAllText(filePath);
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, shaderSource);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int compileStatus);
            if (compileStatus != (int)All.True)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"{type} compilation failed: {infoLog}");
            }

            return shader;
        }

        public static int GetUniformLocation(int program, string name)
        {
            int location = GL.GetUniformLocation(program, name);
            if (location == -1)
            {
                Console.WriteLine($"Warning: Uniform '{name}' not found in shader program {program}.");
            }
            return location;
        }

        public static void UseProgram(int program)
        {
            GL.UseProgram(program);
        }

        public static void SetUniformMatrix4(int location, Matrix4 matrix)
        {
            GL.UniformMatrix4(location, false, ref matrix);
        }

        public static void SetUniformInt(int location, int value)
        {
            GL.Uniform1(location, value);
        }

        public static void SetUniformVector3(int location, Vector3 vector)
        {
            GL.Uniform3(location, vector);
        }
    }
}