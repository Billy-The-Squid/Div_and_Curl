Shader "Custom/FluxCounter"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard fullforwardshadows

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0


            float3 _CenterPosition;// = float3(4.0,5.0,0.0); // Insert this %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
        #ifdef SHADER_API_D3D11
            // DirectX11-specific code, for example
            #include "FieldLibrary 1.hlsl"
        #endif
        

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
            float3 worldRefl; // Ditch this if not used
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        #ifdef SHADER_API_D3D11
            float Flux(Input IN) {
                Outwards field;                     // CHANGE THIS %%%%%%%%%%%%%%%
                float ret;
                float3 normedNormal = normalize(IN.worldNormal);
                float3 normedPos = normalize(IN.worldPos);
                //ret = normedNormal.x * normedPos.x + normedNormal.y * normedPos.y + normedNormal.z * normedPos.z;
                ret = IN.worldNormal.g;
                //ret = dot(normalize(IN.worldNormal), normalize(IN.worldPos));
                    //normalize(field.Field(IN.worldPos)));
                return ret * 0.5 +0.5;
                //return 0.3;
            } // Change this to some other normalization method.
        #endif

        float FluxWrapper(Input IN) {
            #ifdef SHADER_API_D3D11
                return Flux(IN); // This is returning something greater than 1.
                //return 0.3;
            #else
                return 0.7;
            #endif
        }

        //void surf (Input IN, inout SurfaceOutputStandard o)
        //{
        //    // Albedo comes from a texture tinted by color
        //    fixed4 c;
        //    //c = fixed4(IN.worldNormal.r, IN.worldNormal.g, IN.worldNormal.b, 1.0);
        //    //c = fixed4(0.0, 0.0, 1.0, 0.01);
        //    //#ifdef SHADER_API_D3D11
        //        //c = fixed4(IN.worldNormal.r, IN.worldNormal.g, IN.worldNormal.b, 1.0); 
        //        //c = fixed4(0.0, 1.0, 0.0, 0.01);
        //        c = FluxWrapper(IN);
        //    //#else
        //        //c = fixed4(IN.worldNormal.r, IN.worldNormal.g, IN.worldNormal.b, 1.0);
        //        //c = fixed4(1.0, 0.0, 1.0, 0.01);
        //    //#endif
        //    o.Albedo = c.rgb; // From 0 to 1
        //    // Metallic and smoothness come from slider variables
        //    o.Metallic = _Metallic;
        //    o.Smoothness = _Glossiness;
        //    o.Alpha = c.a;
        //}

        void surf(Input IN, inout SurfaceOutputStandard o) {
            fixed4 c;
            //float3 normed = normalize(IN.worldNormal);
            //c = fixed4(normed.r, normed.g, normed.b, 1.0);
            //c = fixed4(IN.worldNormal.r, IN.worldNormal.g, IN.worldNormal.b, 1.0);
            float3 field;
            #ifdef SHADER_API_D3D11
                Outwards field2;
                field = field2.Field(IN.worldPos);
            #else
                field = float3(0.0, 0.0, 0.0);
            #endif
            float dotProd = dot(IN.worldNormal, field) * 0.5 + 0.5;
            c = fixed4(field.r, field.g, field.b, 1.0);
            //c = fixed4(dotProd, 0.0, 1.0, 1.0);
            //c = fixed4(FluxWrapper(IN), 0.0, 0.0, 1.0);
            o.Albedo = c.rgb; // From 0 to 1
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    //FallBack "Diffuse"                    // RE-ENABLE
}
