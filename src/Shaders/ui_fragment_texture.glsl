#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D textureSampler;
uniform vec3 textColor;

void main()
{
    vec4 texColor = texture(textureSampler, TexCoord);
    // If the texture is pure white for the glyph, and transparent otherwise,
    // this will make the glyph the specified textColor and keep transparency.
    FragColor = vec4(textColor, texColor.a); 
    // Optional: Discard fully transparent pixels to avoid issues with blending order if many transparent quads overlap.
    // However, with SrcAlpha, OneMinusSrcAlpha blending, this might not be strictly necessary
    // and can sometimes cause issues with anti-aliased edges if not handled carefully.
    if (FragColor.a < 0.01) discard; // RE-ENABLED
}
