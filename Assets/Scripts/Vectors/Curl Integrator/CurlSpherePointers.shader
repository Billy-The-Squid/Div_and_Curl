// The instancing/surface shader for the projection vectors on a curl integral surface.

Shader "Vectors/Detectors/CurlSpherePointers" 
{
	Properties
	{
		// _Scaling ("Scaling Factor", float) = 0.1
	} // This needs to be used somewhere. Where?

	SubShader
	{
		CGPROGRAM
		
		// The compiler directives. 
		// Renders the surface. Requires a ConfigureSurface function.
		#pragma surface ConfigureSurface Standard fullforwardshadows addshadow
		// Does instancing, including(?) placing points. Requires a ConfigureProcedural function.
		#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
		#pragma editor_sync_compilation
		#pragma target 4.5

		// This is where the work of calculating transformations is done. 
		#include "../PointsPlot.hlsl"

		// The data about how much this point contributes to curl.
		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			StructuredBuffer<float3> _CurlContributions; // RETYPED
		#endif
		
		//float4 _DetectorCenter; 

		// The format of the input taken by ConfigureSurface.
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		}; // Verify that both are necessary. 

		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) {
				float dotP = length(_CurlContributions[unity_InstanceID]); // RETYPED (kinda)

				surface.Albedo.r = 1 - abs(dotP) / 6 + dotP / 6;
				surface.Albedo.g = 1 - 3 * abs(dotP) / 4 + dotP / 4;
				surface.Albedo.b = 1 - abs(dotP) / 2 - dotP / 2;
				surface.Alpha = 0.75;

				//// Debug code
				//surface.Albedo.r = 0;
				//surface.Albedo.g = 1;
				//surface.Albedo.b = 0;
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
