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
				
		Pass
		{
			CGPROGRAM			
// Upgrade NOTE: excluded shader from DX11, Xbox360, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 xbox360 gles
			#pragma multi_compile DO_SEGMENT_O DO_SEGMENT_1 DO_SEGMENT_2 DO_SEGMENT_3
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
			}
			
			half4 frag(v2f i) : COLOR
			{
//				return float4(i.uv.x, i.uv.y, 0.0f, 1.0f);
				float ll = length(i.extras.zw);
//				if(ll > _CirclePatchSize)
//					discard;

				float bgtex = 0.0f;
				float gray = 0.0f;
				float4 color = 0.0f;
				float fade = 1.0f;

#if DO_SEGMENT_O
				gray = tex2D(_PatternTexture1, i.uv).r;
				bgtex = tex2D(_MainTex, i.uv * 8.0f).r * 0.25f;
				fade = lerp(1.0f, 0.7f, ll);
				if(ll > 0.9f)
					fade = 0.5f;
#endif
#if DO_SEGMENT_1
				gray = tex2D(_PatternTexture1, i.uv * 4.0f).b * 0.9f;
				bgtex = tex2D(_PatternTexture2, i.uv * 2.0f).r * 0.25f + 0.5f;
				fade = lerp(1.0f, 0.6f, ll - 1.5f);
				if(ll > 1.9f)
					fade = 0.5f;
#endif
#if DO_SEGMENT_2
				gray = tex2D(_PatternTexture1, i.uv).r;
				bgtex = dot(tex2D(_MainTex, 1.0f - i.uv2), float3(0.333f, 0.333f, 0.333f)) * 0.25f + 0.75f;
				fade = lerp(1.0f, 0.5f, ll - 3.0f);
				if(ll > 2.9f)
					fade = 0.5f;
#endif
#if DO_SEGMENT_3
				gray = tex2D(_PatternTexture1, i.uv).b * 0.45f + 0.5f;
				//gray *= gray;//dot(tex2D(_TestTexture, i.uv), float3(0.333f, 0.333f, 0.333f));
				bgtex = tex2D(_MainTex, i.uv * 8.0f).b;
				fade = lerp(0.9f, 0.4f, ll - 4.0f);
#endif
//				color = float4(gray, gray, gray, 1.0f);
				//color = float4(bgtex, bgtex, bgtex, 1.0f);//float4(ColorFromPalette(bgtex) * gray * fade, 1.0f);
				color = float4(ColorFromPalette(bgtex) * gray * fade, 1.0f);
				return color;
			}
			ENDCG
		}
	} 
	FallBack Off
}
