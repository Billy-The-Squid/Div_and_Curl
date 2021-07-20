// Instancing/surface shader for the partial derivative detector bubbles.

Shader "Vectors/Detectors//PartialBubbles"
{
    Properties
    {
        _Color("Color", Color) = (0,0,1,1)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" } // Allowing for transparency
        LOD 100
        //ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM

        // Renders the surface. Requires a ConfigureSurface function.
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        // Does instancing, including(?) placing points. Requires a ConfigureProcedural function.
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        #pragma editor_sync_compilation
        #pragma target 4.5

        #include "UnityCG.cginc"

        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            StructuredBuffer<float> _Distances;
            StructuredBuffer<float3> _Partial;
        #endif

        int _ParticlesPerStream;
        float _StartDistance;
        float _TravelDistance;
        float _StartingSize;
        float3 _CenterPosition;
        float3 _Direction; // The (normalized) direction we're taking the derivative in. 

        void ConfigureProcedural()
        {
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            int streamNumber = ((unity_InstanceID) / (uint) _ParticlesPerStream);
            int particleNumber = fmod(unity_InstanceID, _ParticlesPerStream); // I think this is right?
            float dist = 0;

            float partialMag = length(_Partial[0]);

            if (partialMag != 0) {
                dist = fmod(_Distances[streamNumber] + particleNumber * _TravelDistance / _ParticlesPerStream, _TravelDistance);
            }

            float3 position;
            float size;

            float streamSign; // Either 1 or -1;
            if (streamNumber) {
                streamSign = -1;
            }
            else {
                streamSign = 1;
            }

            if (partialMag == 0) {
                position = _CenterPosition + _Direction * _StartDistance * streamSign;
                size = 0;
            }
            else {
                position = _CenterPosition + streamSign * (_Direction * _StartDistance + normalize(_Partial[0]) * dist);
                size = _StartingSize * (_TravelDistance - abs(dist)) / _TravelDistance;
            }

            float4x4 transformation = 0.0;
            transformation._m33 = 1.0;

            // The position is, well, the position. 
            transformation._m03_m13_m23 = position;
            transformation._m00_m11_m22 = size; // Might be a type issue here...

            // And exporting it. 
            unity_ObjectToWorld = transformation;
            #endif
        }

        float4 _Color;

        struct Input
        {
            float3 worldPos;
        };

        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) {
            surface.Albedo = _Color.rgb;
            surface.Alpha = _Color.a;
        }
        #else
        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface)
        {
            surface.Albedo = saturate(unity_ObjectToWorld._m02_m12_m22 * 0.5 + 0.5);
        }
        #endif


        ENDCG
    }
}
