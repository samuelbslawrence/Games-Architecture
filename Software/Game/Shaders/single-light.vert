#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 aNormal;

uniform mat4 ModelMat;
uniform mat4 ModelViewProjMat;

out vec3 FragPos;
out vec2 TexCoord;
out vec3 Normal;

void main()
{
    // Transform the vertex position into world space.
    FragPos = vec3(ModelMat * vec4(aPos, 1.0));
    
    // Compute transformed normals.
    Normal = mat3(transpose(inverse(ModelMat))) * aNormal;
    
    // Pass through texture coordinates.
    TexCoord = aTexCoord;
    
    // Calculate final vertex position.
    gl_Position = ModelViewProjMat * vec4(aPos, 1.0);
}
