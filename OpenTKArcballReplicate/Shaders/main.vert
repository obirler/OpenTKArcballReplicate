#version 330 core

layout(location=0) in vec3 pos;
layout(location=1) in vec3 disp;
layout(location=2) in vec3 col;

uniform float frameFrac; 
uniform mat4 proj_view;

out vec3 vertexColor;

void main(void){
	gl_Position = vec4(pos.x + frameFrac * disp.x, pos.y + frameFrac * disp.y, pos.z + frameFrac * disp.z, 1.0f) * proj_view;
	vertexColor = col;
}