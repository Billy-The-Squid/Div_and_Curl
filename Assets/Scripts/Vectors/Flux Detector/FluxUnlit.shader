// This is the flux detector shell shader. 

Shader "Custom/FluxDetector"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" } // Allowing for transparency
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma target 4.5 // necessary? I'm not certain. 
            #pragma multi_compile_fog

            #include "UnityCG.cginc"


            // The information about each vertex that goes into the vert shader. 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint id : SV_VertexID;
                float4 normal : NORMAL;
            };

            // The information about each vertex coming out of the vert shader and going into the pixel shader. 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // The values of my vector field at each input. 
            StructuredBuffer<float3> _Vectors;



            // The vert shader. Calculates some vertex-specific information. 
            v2f vert (appdata v)
            {
                // Default stuff: positions, coordinates, "fog."
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                // Calculating the flux at each point
                // The scaling isn't fair if we normalize these. 
                float3 vect = normalize(_Vectors[v.id]);
                float3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
                float dotP = dot(worldNormal, vect);

                // Red -> blue colors
                //o.color.r = saturate(-abs(dotP + 1)+2);
                //o.color.b = saturate(-abs(dotP - 1)+2);
                //o.color.g = saturate(1-abs(dotP));
                //o.color.a = 0.75;

                // Purple -> Orange colors
                o.color.r = 1 - abs(dotP) / 6 + dotP / 6;
                o.color.g = 1 - 3 * abs(dotP) / 4 + dotP / 4;
                o.color.b = 1 - abs(dotP) / 2 - dotP / 2;
                o.color.a = 0.75; 

                return o;
            }



            // The pixel shader. Calculates pixel-specific information. 
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
