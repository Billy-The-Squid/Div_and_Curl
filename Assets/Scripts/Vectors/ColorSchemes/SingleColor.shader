// A instancing/surface shader for standard vectors. Single color. 

Shader "Vectors/SingleColor"
{
	Properties
	{
		_Color ("Color", Color) = (0,0,1,1)
	}

	SubShader
	{
		//Tags { "RenderType" = "Transparent" "Queue" = "Transparent" } // Allowing for transparency
		//LOD 100
		//ZWrite Off // makes the rendering weird
		//Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM

		// Renders the surface. Requires a ConfigureSurface function.
		#pragma surface ConfigureSurface Standard fullforwardshadows addshadow
		// Does instancing, including(?) placing points. Requires a ConfigureProcedural function.
		#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
		#pragma editor_sync_compilation
		#pragma target 4.5

		// This is where the work of calculating transformations is done. 
		#include "Assets/Scripts/Vectors/PointsPlot.hlsl" 

		float _MaxMagnitude;
		float4 _Color;
		float _CullDistance;

		struct Input
		{
			float3 worldPos;
		};




		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) {
				surface.Albedo = _Color.rgb;

				float3 displ = _Positions[unity_InstanceID] - _WorldSpaceCameraPos;
				clip(displ.x * displ.x + displ.y * displ.y + displ.z * displ.z - _CullDistance * _CullDistance);
			}
		#else
			void ConfigureSurface (Input input, inout SurfaceOutputStandard surface)
			{
				surface.Albedo = saturate(unity_ObjectToWorld._m02_m12_m22 * 0.5 + 0.5);
			}
		#endif


		ENDCG
	}

	FallBack "Diffuse"
}
