using OpenTK.Mathematics;
using OpenGL_Game.Managers;
using OpenGL_Game.OBJLoader;
using OpenTK.Graphics.OpenGL4;

namespace OpenGL_Game.Components
{
    class ComponentShaderText : ComponentShader
    {
        private int uniform_mvp;
        private int uniform_textColor;

        public ComponentShaderText() : base("Shaders/text.vert", "Shaders/text.frag")
        {
            uniform_mvp = ShaderLoader.GetUniformLocation(pgmID, "mvp");
            uniform_textColor = ShaderLoader.GetUniformLocation(pgmID, "textColor");
        }

        public override void ApplyShader(Matrix4 model, Geometry geometry)
        {
            ShaderLoader.UseProgram(pgmID);

            ShaderLoader.SetUniformMatrix4(uniform_mvp, model);

            ShaderLoader.SetUniformVector3(uniform_textColor, new Vector3(1.0f, 1.0f, 1.0f));

            GL.ActiveTexture(TextureUnit.Texture0);

            geometry.Render();
        }
    }
}
