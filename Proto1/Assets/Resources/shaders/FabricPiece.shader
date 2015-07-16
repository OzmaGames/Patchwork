Shader "Custom/FabricPiece" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AddColor ("ShadeColor", vector) = (0.0,0.0,0.0,0.0)
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

			sampler2D _MainTex;
			float4 _AddColor;

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

			half4 frag(v2f i) : COLOR
			{
				float4 mainColor = tex2D(_MainTex, i.uv * 0.01);
				float4 color = mainColor;
				color.rgb += _AddColor.rgb;
				color.a = 1.0f;
				return i.color + ((color.r + color.g + color.b) / 3.0f);
			}
			ENDCG
		}
	} 
	FallBack Off
}
