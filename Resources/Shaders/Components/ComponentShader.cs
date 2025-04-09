using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL_Game.OBJLoader;
using OpenTK.Mathematics;

namespace OpenGL_Game.Components
{
    abstract class ComponentShader : IComponent
    {
        public int pgmID;

        public ComponentShader(string vertexShaderName, string fragmentShaderName)
        {
            pgmID = ShaderLoader.LoadShaderProgram(vertexShaderName, fragmentShaderName);
        }

        public ComponentTypes ComponentType
        {
            get { return ComponentTypes.COMPONENT_SHADER; }
        }

        public abstract void ApplyShader(Matrix4 model, Geometry geometry);
    }
}