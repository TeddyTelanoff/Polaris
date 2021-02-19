#version 330 core

in vec2 pass_UV;
in vec3 pass_Normal;

out vec4 Color;

uniform sampler2D tex;

void main()
{
	Color = texture(tex, pass_UV);
}