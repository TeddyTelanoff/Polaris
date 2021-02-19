#version 330 core

in vec2 pass_UV;
in vec3 pass_Normal;

out vec4 Color;

uniform sampler2D MainTexture;

void main()
{
	Color = texture(MainTexture, pass_UV);
}