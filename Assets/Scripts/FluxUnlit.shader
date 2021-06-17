Shader "Custom/FluxUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma target 4.5
            #pragma multi_compile_fog

            #include "UnityCG.cginc"



            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint id : SV_VertexID;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            StructuredBuffer<float3> _Vectors; // any direction, any length;



            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                float3 vect = normalize(_Vectors[v.id]);   // These are comin
                float3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal)); // FINISH THIS LINE
                //o.color = float4(worldNormal.r, worldNormal.g, worldNormal.b, 1.0);
                //o.color = float4(vect.r, vect.g, vect.b, 1.0);
                float dotP = dot(worldNormal, vect); // WROOOOOOONG NORMAL
                //o.color = float4(dotP, 1.0, 0.0, 1.0);
                o.color.r = saturate(-abs(dotP + 1)+2);
                o.color.g = saturate(-abs(dotP - 1)+2);
                o.color.b = saturate(1-abs(dotP));
                o.color.a = 0.5;

                //float3 vect = normalize(_Vectors[v.id]);
                //o.color = float4(vect.r, vect.g, vect.b, 0.5);

                //float3 vect = normalize(_Vectors[v.id]) * 0.5 + float3(0.5, 0.5, 0.5);
                //float3 vect = _Vectors[v.id];
                //o.color = float4(vect.r, vect.g, vect.b, 1.0);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col;
                //fixed4 col = tex2D(_MainTex, i.uv);
                col = i.color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
