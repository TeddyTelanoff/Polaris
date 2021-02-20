#version 330 core

layout (location = 0) in vec4 Position;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec3 Normal;

out vec2 pass_UV;
out vec3 pass_Normal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 proj;

void main()
{
	pass_UV = UV;
	pass_Normal = Normal;
	gl_Position = proj * view * model * Position;
}