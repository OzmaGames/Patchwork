Shader "Custom/CirclePatch" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CirclePatchSize("Circle Patch Size", Float) = 1.0
		
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
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			
			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
			};
			
			struct v2f {
				float4 pos : SV_POSITION;
				float3 color : COLOR0;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};
			
			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.normal * 0.5 + 0.5;
				o.uv.xy = v.texcoord.xy;
				o.uv2.xy = v.texcoord1.xy;
				//o.uv.xy = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
				return o;
			}
			
			float _CirclePatchSize;
			
			half4 frag(v2f i) : COLOR
			{
				//return half4(i.color, 1);
				float2 l = (i.uv - 0.5f) * 2.0f;
				if(length(l) > _CirclePatchSize)
					discard;

				return (tex2D(_MainTex, i.uv) * 0.5f) + (tex2D(_MainTex, i.uv2) * 0.5f);
			}
			ENDCG
		}
//#endif
	} 
	FallBack Off
}
