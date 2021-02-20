#version 330 core

in vec2 pass_UV;
in vec3 pass_Normal;

out vec4 Color;

uniform vec3 MainColor;

void main()
{
	Color = vec4(MainColor.xyz, 1);
}