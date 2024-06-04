#version 330 core

layout(location=0) in vec3 pos;
layout(location=1) in vec3 col;

uniform mat4 proj_view;

out vec3 vertexColor;

void main(void){
	gl_Position =  vec4(pos, 1.f) * proj_view;
	vertexColor = col;
}