Shader "Custom/FabricPiece" {
	Properties {
		_AddColor ("ShadeColor", Color) = (0,0,0,0)
		_Flip ("Flip", Vector) = (1,1,1,1)

		_BaseColor1 ("BaseColor1", Color) = (1,1,1,1)
		_BaseColor2 ("BaseColor2", Color) = (1,1,1,1)
		_ComplementColor1 ("ComplementColor1", Color) = (1,1,1,1)
		_ComplementColor2 ("ComplementColor2", Color) = (1,1,1,1)

		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags {
			"RenderType"="Opaque"
			"AllowProjectors"="False"
		}
		blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
			CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11, Xbox360, OpenGL ES 2.0 because it uses unsized arrays
			//#pragma exclude_renderers d3d11 xbox360 gles
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _AddColor;
			float4 _Flip;
			float4 _BaseColor1;
			float4 _BaseColor2;
			float4 _ComplementColor1;
			float4 _ComplementColor2;
			sampler2D _MainTex;

			struct appdata {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 texcoord : TEXCOORD0;
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
				float4 color : TEXCOORD1;
				float2 uv : TEXCOORD0;
			};
			
			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.uv.xy = v.texcoord.xy;
				return o;
			}

			float3 BGColorFromPalette(float index)
			{
				return lerp(_BaseColor2, _BaseColor1, index).rgb; // Was mix()
			}

			half4 frag(v2f i) : COLOR
			{
				//float3 texcol = tex2D(_MainTex, i.uv);
				float3 texcol = tex2D(_MainTex, float2(i.uv.x * (_Flip.x), i.uv.y * (_Flip.y)));
				float bgtex = saturate(texcol.r - 0.2f);
				bgtex *= bgtex * _Flip.z;
				float4 color = float4(BGColorFromPalette(bgtex), 1.0f);
				color.rgb += _AddColor.rgb;
				color.a = 1.0f;
				return color;//i.color + ((color.r + color.g + color.b) / 3.0f);
				//return mainColor;//i.color + ((color.r + color.g + color.b) / 3.0f);
			}
			ENDCG
		}
	} 
	FallBack Off
}
