Shader "UI/CopyColorStd" {
	Properties
	{
		_Color("Multiplicative color", Color) = (1.0, 1.0, 1.0, 1.0)
	}

	SubShader{
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			//Blend One One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			// UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
			// uniform float4 _MainTex_ST;
			uniform float4 _Color;
			float4 SpriteUVs;
			float4 SpriteRect;
			//float2 rtRect;
			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				// o.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return float4(i.vertex.xy, 0, 1);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				// return UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.texcoord) * _Color;
				return _Color;
			}
			ENDCG

		}
	}

	Fallback Off
}