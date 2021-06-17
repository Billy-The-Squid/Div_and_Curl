Shader "Custom/Flux1"
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
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 5.0

        sampler2D _MainTex;
        float3 _CenterPosition;

        #ifdef SHADER_API_D3D11
        #include "./Vector-Field-Implementation/FieldLibrary.hlsl"
        #endif

        struct Input
        {
            float2 uv_MainTex;
            float3 position;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            #ifdef SHADER_API_D3D11
            Outwards field;
            #endif

            // Albedo comes from a texture tinted by color
            fixed4 c;
            #ifdef SHADER_API_D3D11
                float3 vect = field.Field(IN.position);
                c = fixed4(vect.r, vect.g, vect.b, 1.0);
            #endif
            #ifndef SHADER_API_D3D11
                c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            #endif


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
