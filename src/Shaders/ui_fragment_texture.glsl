#version 330 core
out vec4 FragColor;

in vec2 TexCoord; // Interpolated texture coordinate from vertex shader

uniform sampler2D textureSampler; // The font texture
uniform vec3 textColor;           // Color to tint the text (e.g., black, white)

void main()
{
    vec4 sampled = texture(textureSampler, TexCoord);
    // Modulate texture color with textColor. Allows for colored text from a monochrome (e.g., white) font texture.
    // If the font texture itself has color, you might adjust this.
    FragColor = vec4(textColor * sampled.rgb, sampled.a);

    // Optional: Discard pixels with low alpha to avoid rendering transparent parts of the texture
    if (FragColor.a < 0.1)
        discard;
}
