#version 330 core
layout (location = 0) in vec2 aPosition; // Vertex position
layout (location = 1) in vec2 aTexCoord; // Texture coordinate

out vec2 TexCoord;

// Uniforms that can be set from C#
uniform mat4 projection; // For orthographic projection
uniform mat4 model;      // For position, rotation, scale of the UI element

void main()
{
    gl_Position = projection * model * vec4(aPosition, 0.0, 1.0);
    TexCoord = aTexCoord;
}
