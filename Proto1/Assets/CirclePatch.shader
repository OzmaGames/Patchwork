Shader "Custom/CirclePatch" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CirclePatchSize("Circle Patch Size", Float) = 1.0
		_CirclePatchMaxSize("Circle Patch Max Size", Float) = 1.0
		_CirclePatchRadius("Circle Patch Radius", Float) = 1.0
		_PatternTexture1("Pattern Texture 1", 2D) = "white" {}
		_PatternTexture2("Pattern Texture 2", 2D) = "white" {}
		_CirclePatchLayer("Circle Patch Layer", Float) = 0.0
		_CirclePalette("Palette", Float) = 0.0

//		_Color0("Color 0", Color) = (0.1, 0.0, 0.2)
//		_Color1("Color 1", Color) = (0.9, 0.6, 0.4)
//		_Color2("Color 2", Color) = (0.3, 0.9, 0.2)
//		_Color3("Color 3", Color) = (0.0, 0.6, 0.6)
		
		_GradientTexture("Gradient Texture", 2D) = "white" {}
						
		_Color ("Color", Color) = (1,1,1,1)
		_Distort("Distort", vector) = (0.5, 0.5, 1.0, 1.0)
		_OuterRadius ("Outer Radius", float) = 0.5
		_InnerRadius ("Inner Radius", float) = -0.5
		_Hardness("Hardness", float) = 1.0
	}
	SubShader {
		Tags {
			"RenderType"="Transparent"
			"Queue"="Transparent"
			"AllowProjectors"="False"
		}
		blend SrcAlpha OneMinusSrcAlpha

/*#if 1
		CGPROGRAM
		#pragma surface surf NoLighting
 
		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			return fixed4(s.Albedo, s.Alpha);
		}
 
		sampler2D _MainTex;
 
		struct Input
		{
			float2 uv_MainTex;
		};
 
		float4 _Color, _Distort;
		float _OuterRadius, _InnerRadius, _Hardness;
		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
 
			float x = length((_Distort.xy - IN.uv_MainTex.xy) * _Distort.zw);
 
			float rc = (_OuterRadius + _InnerRadius) * 0.5f; // "central" radius
			float rd = _OuterRadius - rc; // distance from "central" radius to edge radii
 
			float circleTest = saturate(abs(x - rc) / rd);
 
			o.Albedo = _Color.rgb * c.rgb;
			o.Alpha = (1.0f - pow(circleTest, _Hardness)) * _Color.a * c.a;
		}
		ENDCG
		
#else*/
				
		Pass
		{
			CGPROGRAM			
// Upgrade NOTE: excluded shader from DX11, Xbox360, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 xbox360 gles
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _PatternTexture1;
			sampler2D _PatternTexture2;
			sampler2D _GradientTexture;
			
			struct appdata {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 extras : TANGENT;
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
				float3 color : COLOR0;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 extras : TEXCOORD2;
			};
			
			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = 0.5f;
				o.uv.xy = v.texcoord.xy;
				o.uv2.xy = v.texcoord1.xy;
				o.extras = v.extras;
				o.extras.zw = v.vertex.xy;
				return o;
			}
			
			float _CirclePatchSize;
			float _CirclePatchMaxSize;
			float _CirclePatchRadius;
			float _CirclePatchLayer;
//			float3 _Color0;
//			float3 _Color1;
//			float3 _Color2;
//			float3 _Color3;
			float _CirclePalette;
			
			float SmuttStep(float x, float y, float z)
			{
				return saturate((z - x) / (y - x));
			}
			
			float3 ColorFromPalette(float index)
			{
				return tex2D(_GradientTexture, float2(index, _CirclePalette)).rgb;
//				float3 color =	_Color0;
//				color =			mix(color, _Color1, SmuttStep(0.25f, 0.5f, index));
//				color =			mix(color, _Color2, SmuttStep(0.5f, 0.75f, index));
//				color =			mix(color, _Color3, SmuttStep(0.75f, 1.0f, index));
//				return color;
			}
			
			half4 frag(v2f i) : COLOR
			{
				return float4(i.uv.x, i.uv.y, 0.0f, 1.0f);
//				return float4(ColorFromPalette(i.uv.x), 1.0f);
//			float2 l = ((i.uv - 0.5f) * 2.0f) * _CirclePatchSize;
				float ll = length(i.extras.zw);
//				if(ll > _CirclePatchSize)
//					discard;

//				return (tex2D(_TestTexture, i.uv) * 0.5f) + (tex2D(_MainTex, i.uv2) * 0.5f);
//				return (tex2D(_MainTex, i.uv) * 0.5f) + (tex2D(_MainTex, i.uv2) * 0.5f);
				float bgtex = 0.0f;
				float gray = 0.0f;
				float4 color = 0.0f;
				float fade = 1.0f;
				if(ll < 1.0f)
				{
					gray = tex2D(_PatternTexture1, i.uv).r;
					bgtex = tex2D(_MainTex, i.uv * 8.0f).r * 0.25f;
					fade = lerp(1.0f, 0.7f, ll);
					if(ll > 0.9f)
						fade = 0.5f;
				}
				else if(ll < 2.0f)
				{
					gray = tex2D(_PatternTexture1, i.uv * 4.0f).b * 0.9f;
					bgtex = tex2D(_PatternTexture2, i.uv * 2.0f).r * 0.25f + 0.5f;
					fade = lerp(1.0f, 0.6f, ll - 1.5f);
					if(ll > 1.9f)
						fade = 0.5f;
				}
				else if(ll < 3.0f)
				{
					gray = tex2D(_PatternTexture1, i.uv).r;
					bgtex = dot(tex2D(_MainTex, 1.0f - i.uv2), float3(0.333f, 0.333f, 0.333f)) * 0.25f + 0.75f;
					fade = lerp(1.0f, 0.5f, ll - 3.0f);
//					if(ll > 2.9f)
//						fade = 0.5f;
				}
				else
				{
					gray = tex2D(_PatternTexture1, i.uv).b * 0.45f + 0.5f;
					//gray *= gray;//dot(tex2D(_TestTexture, i.uv), float3(0.333f, 0.333f, 0.333f));
					bgtex = tex2D(_MainTex, i.uv * 8.0f).b;
					fade = lerp(0.9f, 0.4f, ll - 4.0f);
//					if(ll > 3.9f)
//						fade = 0.5f;
				}
//				color = float4(gray, gray, gray, 1.0f);
				//color = float4(bgtex, bgtex, bgtex, 1.0f);//float4(ColorFromPalette(bgtex) * gray * fade, 1.0f);
				color = float4(ColorFromPalette(bgtex) * gray * fade, 1.0f);
				return color;
			}
			ENDCG
		}
//#endif
	} 
	FallBack Off
}
