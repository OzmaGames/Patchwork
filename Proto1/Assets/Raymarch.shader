Shader "Custom/Raymarch" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue" = "Geometry" }
		Pass {
GLSLPROGRAM
#include "UnityCG.glslinc"
#ifdef VERTEX
void main()
{
	gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
}
#endif

#ifdef FRAGMENT
// http://www.plane9.com/wiki/distancefields
// http://wiki.unity3d.com/index.php/Triangulator
float inObj(in vec3 p)
{
	return length(p) - 3.0;
}

void main()
{
	const int NUMSTEPS = 128;

	// Center the uv.
	vec2 vPos = -1.0 + 2.0 * gl_FragCoord.xy / _ScreenParams.xy;
	
	// Camera properties.
	const vec3 vuv = vec3(0.0, 1.0, 0.0);	// Camera up (specifies  camera orientation).
	const vec3 vrp = vec3(0.0, 0.0, 0.0);	// View reference point - where is the camera looking.
	const vec3 prp = vec3(0.0, 4.0, 3.0);	// Camera position.
	
	// Camera setup.
	vec3 vpn = normalize(vrp - prp);		// Viewpoint normalized - normalized direction in which the camera is pointing.
	vec3 u = normalize(cross(vuv, vpn));	// u (horizontal) coordintate of the view plane.
	vec3 v = cross(vpn, u);					// v (vertical) coordintate of the view plane.
	vec3 vcv = (prp + vpn);					// Center point of the view plane.
	vec3 scrCoord = vcv + vPos.x * u * _ScreenParams.x / _ScreenParams.y + vPos.y * v; // World-coordinates of the current fragment.
	vec3 scp = normalize(scrCoord - prp);
	
	// Raymarching.
	const float maxd = 5.0;		// Far clipping plane.
	float s = 0.1;				// The scalar value of the distance field at p.
	vec3 p;						// The point on the ray.
	const vec3 c = vec3(0.0, 1.0, 0.0);	// Sphere diffuse color.
	// Speed optimization - advance ray (simple raytracing) until plane y=3.0
	float f = -(prp.y - 3.0) / scp.y;
	
	if(f > 0.0)
	{
		p = prp + scp * f;
	}
	else
	{
		f = maxd;
	}
	
	for(int stepNr = 0; stepNr < NUMSTEPS; stepNr++)
	{
		if(abs(s) < 0.01 || f > maxd)
		{
			break;
		}
		
		f += s;
		p = prp + scp * f;
		s = inObj(p);
	}
	
	if(f < maxd)
	{
		if(p.y < -3.0)
		{
			// Do not draw below the plane level.
			return;
		}
		
		float b = inObj(p + 0.4 * (prp - p)); // Diffuse light.
		gl_FragColor = vec4(b * c, 1.0);
	}
	else
	{
		gl_FragColor = vec4(0.0, 0.2, 0.2, 1.0); // Color of the horizon.
	}
}

#if 0
#define NumberOfParticles 64
#define Pi 3.141592

vec3 palette(float x)
{
	return vec3(
		sin(x*2.0*Pi)+1.5,
		sin((x+1.0/3.0)*2.0*Pi)+1.5,
		sin((x+2.0/3.0)*2.0*Pi)+1.5
	)/2.5;
}

float starline(vec2 relpos,float confradius,float filmsize)
{
	if(abs(relpos.y)>confradius) return 0.0;
	float y=relpos.y/confradius;
	float d=abs(relpos.x/filmsize);
	return sqrt(1.0-y*y)/(0.0001+d*d)*0.00001;
}

float star(vec2 relpos,float confradius,float filmsize)
{
	vec2 rotpos=mat2(cos(Pi/3.0),-sin(Pi/3.0),sin(Pi/3.0),cos(Pi/3.0))*relpos;
	vec2 rotpos2=mat2(cos(Pi/3.0),sin(Pi/3.0),-sin(Pi/3.0),cos(Pi/3.0))*relpos;
	return starline(relpos,confradius,filmsize)+
		starline(rotpos,confradius,filmsize)+
		starline(rotpos2,confradius,filmsize);
}

void main(void)
{
	const vec2 iResolution = vec2(800,600);
	const float iGlobalTime = 0.0;
	vec2 screenpos=(2.0*gl_FragCoord.xy-iResolution.xy)/max(iResolution.x,iResolution.y);

	float focaldistance=0.5+sin(iGlobalTime*0.05)*0.013;
	float focallength=0.100;
	float filmsize=0.036;
	float minconf=filmsize/1000.0;
	float lensradius=focallength/1.0;

	float filmdistance=1.0/(1.0/focallength-1.0/focaldistance);
	
	vec3 c=vec3(0.0);
	for(int i=0;i<NumberOfParticles;i++)
	{
		float t=float(i)/float(NumberOfParticles);
		float a=t*2.0*Pi+iGlobalTime*0.1;

		vec3 pos=vec3(sin(a)+2.0*sin(2.0*a),cos(a)-2.0*cos(2.0*a),-sin(3.0*a))*0.01;

		float a1=0.1*iGlobalTime;
		pos.xz*=mat2(cos(a1),-sin(a1),sin(a1),cos(a1));
		//float a2=0.1;
		//pos.yz*=mat2(cos(a2),-sin(a2),sin(a2),cos(a2));

		pos.z+=0.5;
		
		float intensity=0.0000002;

		vec2 filmpos=pos.xy/pos.z*filmdistance;
		float confradius=lensradius*filmdistance*abs(1.0/focaldistance-1.0/pos.z)+minconf;

		float diffusedintensity=intensity/(confradius*confradius);

		vec3 colour=palette(t);

		vec2 relpos=filmpos-screenpos/2.0*filmsize;
		if(length(relpos)<confradius) c+=colour*diffusedintensity;

		c+=colour*diffusedintensity*star(relpos,confradius,filmsize);
	}

	gl_FragColor=vec4(pow(c,vec3(1.0/2.2)),1.0);
}
#endif
#endif
ENDGLSL
		}
	}
	FallBack "Diffuse"
}
