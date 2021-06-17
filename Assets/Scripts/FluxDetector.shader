Shader "Custom/FluxDetector"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }


    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows //vertex:ComputeColors

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 5.0
        #pragma require compute


        #if defined(SHADER_API_D3D11)
            RWStructuredBuffer<float3> _Vectors; 
        #endif

        sampler2D _MainTex; 

        //struct appdata_id 
        //{
        //    float3 normal : NORMAL;
        //    float4 vertex : POSITION;
        //    float4 texcoord0 : TEXCOORD0; // See if we can drop a couple of these
        //    float4 texcoord1 : TEXCOORD1;
        //    float4 texcoord2 : TEXCOORD2;
        //    float4 texcoord3 : TEXCOORD3;
        //    uint id : SV_VertexID;
        //};

        struct Input
        {
            float2 uv_MainTex;
            float color;
            uint id : SV_VertexID;
        };
        
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        //void ComputeColors(inout appdata_id vertex) //uint id : SV_VertexID)    
        //{
        //    Input output;
        //    #ifdef SHADER_API_D3D11
        //        output.color = dot(vertex.normal, _Vectors[vertex.id]); // Make sure this is the correct normal
        //    #endif
        //}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c;
            #if defined(SHADER_API_D3D11)
            c = fixed4(_Vectors[IN.id].r, _Vectors[IN.id].g, 1.0, 1.0); //_Vectors[IN.id].b, 1.0);
            //c = fixed4(1.0, 1.0, 0.0, 1.0);
            #else
            c = fixed4(0.0, 1.0, 1.0, 1.0);
            #endif
            //c = fixed4(IN.color, 0.0, 1.0, 1.0);
            //c = fixed4(1.0, 1.0, 0.0, 1.0);
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
