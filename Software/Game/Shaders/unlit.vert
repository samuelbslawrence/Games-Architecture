#version 330 core

layout (location = 0) in vec3 a_Position;
layout (location = 1) in vec2 a_TexCoord;
layout (location = 2) in vec3 a_Normal;

uniform mat4 ModelViewProjMat;
// ModelMat is optional here if you don't need to compute world positions.
uniform mat4 ModelMat;

out vec2 v_TexCoord;

void main()
{
    gl_Position = ModelViewProjMat * vec4(a_Position, 1.0);
    v_TexCoord = a_TexCoord;
}