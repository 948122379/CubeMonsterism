// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader"Levin/CubeShader"{
	Properties{
	//_Diffuse("Diffuse Color",Color) = (1,1,1,1)
		_Color("Color",Color)=(1,1,1,1)
		_MainTex("CubeTexture",2D) = "white"{}
		//_Specular("Specular Color",Color) = (1,1,1,1)
		//_MainTexColor("MainTex Color",Color)=(1,1,1,1)
		//_Gloss("Gloss",Range(10,200)) = 20
	}

		SubShader{
			Tags{"Queue" = "Transparent" "IngoreProjector" = "True" "RenderType" = "Tansparent"}
			Pass{
			Tags{"LightMode" = "ForwardBase"}

			Cull off
			ZWrite off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
		#include"Lighting.cginc"
#pragma vertex vert
#pragma fragment frag
			//fixed4 _Diffuse;
			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			//fixed4 _Specular;
			//fixed4 _MainTexColor;
			//half _Gloss;

		//application to vertex
			struct a2v {
				float4 vertex:POSITION;
				float3 normal:NORMAL;
				float4 texcoord:TEXCOORD0;
				};

			struct v2f {
				float4 svPos:SV_POSITION;
				float3 worldNormal:TEXCOORD0;
				float4 worldVertex:TEXCOORD1;
				float2 uv:TEXCOORD2;		
			};


			 v2f vert(a2v v) {
				 v2f f;
				 f.svPos = UnityObjectToClipPos(v.vertex);
				 f.worldNormal = UnityObjectToWorldNormal(v.normal);
				 f.worldVertex = mul(v.vertex, unity_WorldToObject);
				 f.uv = v.texcoord.xy*_MainTex_ST.xy + _MainTex_ST.zw;
				 return f;
			}

			fixed4 frag(v2f f) :SV_Target{
				fixed3 normalDir = normalize(f.worldNormal);
			
				fixed3 lightDir = normalize(WorldSpaceLightDir(f.worldVertex));//对于每一个顶点来说，光的位置就是光的方向

				fixed4 texColor = tex2D(_MainTex, f.uv.xy)*_Color;				

				fixed3 diffuse = _LightColor0.rgb*texColor.rbg*(dot(normalDir, lightDir)*0.5 + 0.5);

				fixed3 viewDir = UnityWorldSpaceViewDir(f.worldVertex);

				fixed3 halfDir = normalize(lightDir+viewDir);
				
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;

				//fixed3 specular = _LightColor0.rgb*_Specular.rgb*pow(max(dot(normalDir, halfDir), 0), _Gloss);

				fixed3 tempColor = diffuse + ambient*texColor;

				return fixed4(tempColor, texColor.a);
			}

			ENDCG
		}
	}
		Fallback"Diffuse"
}