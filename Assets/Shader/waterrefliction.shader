// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/waterrefliction"
{
    Properties{
    _Diff ("Diff", 2D) = "white" {}

		 _FloorPos("FloorPos",Vector) = (0,0,0,0)
		 _RefPlan("RefPlane",Vector) = (0,-1,0,0)
		 _Offset("Offset",float) = 0
		 _Radius("Radius",float) = 0.8
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct VertexInput {
                float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : POSITION;
				float4 color : COLOR;
				float2 texcord0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcord0 = v.texcord0;
                return o; 
            }
			uniform sampler2D _Diff; 
            fixed4 frag(VertexOutput i) : COLOR {
				return tex2D(_Diff, i.texcord0);
            }
            ENDCG
        }
		Pass { //这是倒影pass
			Tags { "QUEUE"="Overlay" "RenderType"="Transparent" }
			cull front
			//ZTest Off
			//ZWrite On
			Fog { Mode Off }
			Blend Srcalpha oneminussrcalpha
			CGPROGRAM
  			#pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct v2f { 
                float4 pos : SV_POSITION;
                half3 texcord0 : TEXCOORD0;
                half texcord1 : TEXCOORD1;
            };
			uniform half _Offset;
			uniform fixed4 _RefPlan;
			uniform half _Radius;
			uniform half4 _FloorPos;
            v2f vert(appdata_tan v)
            {
            	v2f o;
				float4 vt;
				vt = mul(unity_ObjectToWorld , v.vertex);//-fixed4(0,0,_Offset,0));
				vt -=_FloorPos;
				float3 vt2 = vt;
				vt.xyz = reflect(vt2.xyz,normalize(_RefPlan.xyz));
				vt.xyz += _FloorPos;
				vt = mul(UNITY_MATRIX_VP,vt);
				o.pos = vt;
				half dis2Plan = vt2.x * _RefPlan.x + vt2.y * _RefPlan.y + vt2.z * _RefPlan.z;
				half yf = -dis2Plan;
				vt2.y = 0;
				o.texcord1 = min(_Radius - length(vt2), yf);
				o.texcord0 = v.texcoord;
				return o;
            }
			uniform sampler2D _Diff; 
            float4 frag(v2f inData) : COLOR
            {
				clip(_Offset);
				clip(inData.texcord1);
				fixed4 col = tex2D(_Diff, inData.texcord0)*0.5;
				return col ;
            }

			ENDCG
		}
    }
    FallBack "Diffuse"
}
