#version 330 core

in vec2 v_TexCoord;
uniform sampler2D s_texture;

out vec4 Color;

void main()
{
    // Simply output the texture color at full brightness.
    Color = texture(s_texture, v_TexCoord);
}