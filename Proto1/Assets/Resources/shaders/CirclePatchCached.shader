Shader "Custom/CirclePatchCached" {
	Properties {
		_CirclePatchSize("Circle Patch Min,Current,Max Size", vector) = (0.0, 0.0, 1.0)
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
			//#pragma exclude_renderers d3d11 xbox360 gles
			#pragma multi_compile DO_SEGMENT_O DO_SEGMENT_1 DO_SEGMENT_2 DO_SEGMENT_3
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float4 _AddColor;
			float4 _CirclePatchSize;
					
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
			
			half4 frag(v2f i) : COLOR
			{
				float lll = length(i.uv2.xy);
				float ll = length(i.extras.zw);
				if(ll > _CirclePatchSize.y)
					discard;

				float4 color = tex2D(_MainTex, i.uv);
				color.rgb += _AddColor.rgb;
				return color;
			}
			ENDCG
		}
	} 
	FallBack Off
}
