﻿using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenGL_Game.Components;
using OpenGL_Game.OBJLoader;
using OpenGL_Game.Objects;
using OpenGL_Game.Scenes;
using OpenTK.Mathematics;

namespace OpenGL_Game.Systems
{
    class SystemRender : ISystem
    {
        // We require POSITION and GEOMETRY; the scale is optional.
        const ComponentTypes MASK = (ComponentTypes.COMPONENT_POSITION | ComponentTypes.COMPONENT_GEOMETRY);

        protected int pgmID;
        protected int vsID;
        protected int fsID;
        protected int uniform_stex;
        protected int uniform_mmodelviewproj;
        protected int uniform_mmodel;
        protected int uniform_diffuse;

        public SystemRender()
        {
            pgmID = GL.CreateProgram();
            LoadShader("Shaders/single-light.vert", ShaderType.VertexShader, pgmID, out vsID);
            LoadShader("Shaders/single-light.frag", ShaderType.FragmentShader, pgmID, out fsID);

            GL.LinkProgram(pgmID);
            GL.GetProgram(pgmID, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(pgmID);
                Console.WriteLine(infoLog);
            }
            Console.WriteLine(GL.GetProgramInfoLog(pgmID));

            uniform_stex = GL.GetUniformLocation(pgmID, "s_texture");
            uniform_mmodelviewproj = GL.GetUniformLocation(pgmID, "ModelViewProjMat");
            uniform_mmodel = GL.GetUniformLocation(pgmID, "ModelMat");
            uniform_diffuse = GL.GetUniformLocation(pgmID, "v_diffuse");
        }

        void LoadShader(string filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.GetShader(address, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(address);
                Console.WriteLine(infoLog);
            }
            GL.AttachShader(program, address);
        }

        public string Name => "SystemRender";

        public void OnAction(Entity entity)
        {
            if ((entity.Mask & MASK) != MASK)
                return;

            var geometryComponent = entity.GetComponent<ComponentGeometry>();
            var positionComponent = entity.GetComponent<ComponentPosition>();

            // Create the base model matrix from the entity's position.
            Matrix4 model = Matrix4.CreateTranslation(positionComponent.Position);

            // If a scale component exists, apply scaling.
            var scaleComp = entity.GetComponent<ComponentScale>();
            if (scaleComp != null)
            {
                Matrix4 scaleMat = Matrix4.CreateScale(scaleComp.Scale);
                model = scaleMat * model;
            }

            // If the entity is named "Skybox", render it with reversed face culling
            if (entity.Name == "Skybox")
            {
                DrawInside(model, geometryComponent.Geometry());
            }
            else
            {
                Draw(model, geometryComponent.Geometry());
            }
        }

        public void Draw(Matrix4 model, Geometry geometry)
        {
            GL.UseProgram(pgmID);
            GL.Uniform1(uniform_stex, 0);
            GL.ActiveTexture(TextureUnit.Texture0);

            GL.UniformMatrix4(uniform_mmodel, false, ref model);
            Matrix4 modelViewProjection = model * GameScene.gameInstance.camera.view * GameScene.gameInstance.camera.projection;
            GL.UniformMatrix4(uniform_mmodelviewproj, false, ref modelViewProjection);

            geometry.Render(uniform_diffuse);

            GL.UseProgram(0);
        }

        // DrawInside() reverses culling so the inside of the model is rendered.
        public void DrawInside(Matrix4 model, Geometry geometry)
        {
            // Save current culling state.
            bool cullEnabled = GL.IsEnabled(EnableCap.CullFace);
            GL.GetInteger(GetPName.CullFaceMode, out int oldCullFaceMode);

            // Reverse culling: Cull front faces so the inside is visible.
            GL.CullFace(CullFaceMode.Front);
            if (!cullEnabled)
                GL.Enable(EnableCap.CullFace);

            GL.UseProgram(pgmID);
            GL.Uniform1(uniform_stex, 0);
            GL.ActiveTexture(TextureUnit.Texture0);

            GL.UniformMatrix4(uniform_mmodel, false, ref model);
            Matrix4 modelViewProjection = model * GameScene.gameInstance.camera.view * GameScene.gameInstance.camera.projection;
            GL.UniformMatrix4(uniform_mmodelviewproj, false, ref modelViewProjection);

            geometry.Render(uniform_diffuse);
            GL.UseProgram(0);

            // Restore the previous culling state.
            GL.CullFace((CullFaceMode)oldCullFaceMode);
            if (!cullEnabled)
                GL.Disable(EnableCap.CullFace);
        }
    }
}
