#version 330 core

in vec3 FragPos;
in vec2 TexCoord;
in vec3 Normal;
out vec4 FragColor;

uniform sampler2D s_texture;
uniform vec3 v_diffuse;

void main()
{
    vec3 lightPos = vec3(10.0, 10.0, 10.0);
    float ambientStrength = 0.5;
    
    vec3 lightColor = v_diffuse;
    if(lightColor == vec3(0.0))
        lightColor = vec3(1.0, 1.0, 1.0);
    
    vec3 ambient = ambientStrength * lightColor;
    
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);

    diff = max(diff, 0.2);
    vec3 diffuse = diff * lightColor;
    
    vec3 textureColor = texture(s_texture, TexCoord).rgb;
    vec3 result = (ambient + diffuse) * textureColor;
    FragColor = vec4(result, 1.0);
}