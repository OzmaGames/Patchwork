Shader "Custom/Playfield" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
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

			struct appdata {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 texcoord : TEXCOORD0;
				//float4 extras : TANGENT;
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
				float4 color = 0.0f;
				float4 mainColor = tex2D(_MainTex, i.uv);
				float gray = 0.0f;
				gray = mainColor.r;
				color.rgb = i.color.rgb * float3(i.uv.r, i.uv.g, 1.0f);// * mainColor;
				color.a = i.color.a;
				return color;
			}
			ENDCG
		}
	} 
	FallBack Off
}
