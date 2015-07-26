﻿Shader "Custom/CirclePatch" {
	Properties {
		_CirclePatchSize("Circle Patch Min,Current,Max Size", vector) = (0.0, 0.0, 1.0)

		_AddColor ("ShadeColor", Color) = (0.0,0.0,0.0,0.0)

		_BaseColor1 ("BaseColor1", Color) = (1,1,1,1)
		_BaseColor2 ("BaseColor2", Color) = (1,1,1,1)
		_ComplementColor1 ("ComplementColor1", Color) = (1,1,1,1)
		_ComplementColor2 ("ComplementColor2", Color) = (1,1,1,1)
						
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_FabricTexture("Fabric Texture", 2D) = "white" {}
		_PatternTexture2("Pattern Texture 2", 2D) = "white" {}
		_CirclePatchLayer("Circle Patch Layer", float) = 0.0

		_AtlasTex ("Atlas Texture", 2D) = "white" {}
	}
	SubShader {
		Tags {
			"AllowProjectors"="False"
		}
		Pass
		{
			CGPROGRAM			
			// Upgrade NOTE: excluded shader from DX11, Xbox360, OpenGL ES 2.0 because it uses unsized arrays
			//#pragma exclude_renderers d3d11 xbox360 gles
			#pragma multi_compile DO_SEGMENT_O DO_SEGMENT_1 DO_SEGMENT_2 DO_SEGMENT_3
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _FabricTexture;
			sampler2D _PatternTexture2;
			sampler2D _AtlasTex;
			float4 _AtlasTex_ST;

			float4 _AddColor;
			float4 _BaseColor1;
			float4 _BaseColor2;
			float4 _ComplementColor1;
			float4 _ComplementColor2;
						
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
			
			float4 _CirclePatchSize;
			float _CirclePatchLayer;

			float SmuttStep(float x, float y, float z)
			{
				return saturate((z - x) / (y - x));
			}
			
			float3 FGColorFromPalette(float index)
			{
				return lerp(_ComplementColor1, _ComplementColor2, index).rgb; // Was mix()
			}

			float3 BGColorFromPalette(float index)
			{
				return lerp(_BaseColor1, _BaseColor2, index).rgb; // Was mix()
			}
			
			half4 frag(v2f i) : COLOR
			{
				float lll = length(i.uv2.xy);
				float ll = length(i.extras.zw);

				float bgtex = 0.0f;
				float fgtex = 0.0f;
				float gray = 0.0f;
				float4 color = 0.0f;
				float fade = 1.0f;
				float border = 1.0f;

#if DO_SEGMENT_O
				//gray = tex2D(_AtlasTex, TRANSFORM_TEX(((i.uv.yx * _CirclePatchLayer) / 2.0f), _AtlasTex)).r;
				gray = tex2D(_FabricTexture, (i.uv.yx * _CirclePatchLayer) / 2.0f).r;
				bgtex = tex2D(_MainTex, i.uv * 4.0f).r * 0.25f;
				bgtex += tex2D(_MainTex, i.uv * 8.0f).b * 0.25f;
				fade = lerp(1.0f, 0.9f, ll - _CirclePatchSize.x);
#endif
#if DO_SEGMENT_1
				//gray = tex2D(_AtlasTex, TRANSFORM_TEX(((i.uv * _CirclePatchLayer) / 2.0f), _AtlasTex)).r;
				gray = tex2D(_FabricTexture, (i.uv * _CirclePatchLayer) / 2.0f).r;
				bgtex = tex2D(_MainTex, i.uv.xy * 1.0f).r * 0.25f + 0.5f;
				fgtex = tex2D(_PatternTexture2, i.uv.xy * 1.0f).r;// * 0.25f + 0.5f;
				fade = lerp(1.0f, 0.9f, ll - _CirclePatchSize.x);
				fade = lerp(fade, 0.6f, _CirclePatchSize.z - ll);
				if(lll > 0.7f)
					fade = 0.5f;
#endif
#if DO_SEGMENT_2
				//gray = tex2D(_AtlasTex, TRANSFORM_TEX(((i.uv * _CirclePatchLayer) / 2.0f), _AtlasTex)).r;
				gray = tex2D(_FabricTexture, (i.uv * _CirclePatchLayer) / 2.0f).r;
				bgtex = tex2D(_MainTex, 1.0f - i.uv2).g;//dot(tex2D(_MainTex, 1.0f - i.uv2), float3(0.333f, 0.333f, 0.333f));// * 0.25f + 0.75f;
				fade = lerp(1.0f, 0.9f, ll - _CirclePatchSize.x);
				if(lll < 0.5f)
					fade = 0.5f;
#endif
#if DO_SEGMENT_3
				//gray = tex2D(_AtlasTex, TRANSFORM_TEX(((i.uv * _CirclePatchLayer) / 2.0f), _AtlasTex)).b * 0.45f + 0.5f;
				gray = tex2D(_FabricTexture, (i.uv * _CirclePatchLayer) / 2.0f).b * 0.45f + 0.5f;
				bgtex = tex2D(_MainTex, i.uv.xy * 4.0f).b;// * 0.25f + 0.5f;
				fgtex = tex2D(_PatternTexture2, i.uv2 * 0.55f).b;
				bgtex *= i.uv2.x / i.uv2.y * 0.10f;
#endif
				// Awful outline.
				if(ll > (_CirclePatchSize.z - 0.1f))
				{
					border = 0.5f;
					fade = 0.0f;
				}
				
				fade *= fade;
				gray *= gray;
				
				// Final color.
				color = float4(lerp(FGColorFromPalette(fgtex * fade), BGColorFromPalette(bgtex * fade), 1.0f - fgtex) * gray * border, 1.0f);
				color.rgb += _AddColor.rgb;
				return color;
			}
			ENDCG
		}
	} 
	FallBack Off
}
