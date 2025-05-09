﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL_Game.OBJLoader;
using OpenTK.Mathematics;

namespace OpenGL_Game.Components
{
    class ComponentShaderNoLights : ComponentShader
    {
        public int uniform_stex;
        public int uniform_mmodelviewproj;
        public int uniform_mmodel;
        public int uniform_diffuse;

        public ComponentShaderNoLights() : base("vertex_no_lights", "fragment_no_lights")
        {
            uniform_stex = ShaderLoader.GetUniformLocation(pgmID, "stex");
            uniform_mmodelviewproj = ShaderLoader.GetUniformLocation(pgmID, "mmodelviewproj");
            uniform_mmodel = ShaderLoader.GetUniformLocation(pgmID, "mmodel");
            uniform_diffuse = ShaderLoader.GetUniformLocation(pgmID, "diffuse");
        }

        public override void ApplyShader(Matrix4 model, Geometry geometry)
        {
            ShaderLoader.UseProgram(pgmID);
            ShaderLoader.SetUniformMatrix4(uniform_mmodelviewproj, model);
            geometry.Render(uniform_diffuse, 1);
        }
    }
}