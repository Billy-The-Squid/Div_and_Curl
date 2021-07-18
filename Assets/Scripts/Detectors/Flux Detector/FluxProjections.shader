// The instancing/surface shader for the projection (and full---change this!) pointers on the flux detector surface.

Shader "Vectors/Detectors/FluxProjections" 
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
		#include "Assets/Scripts/Vectors/PointsPlot.hlsl"

		// The data about how much this point contributes to flux.
		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			StructuredBuffer<float> _FluxContributions;
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
				float dotP; 
				dotP = _FluxContributions[unity_InstanceID];
				if (length(_Vectors[unity_InstanceID]) == 0) {
					dotP = 0;
				}
				else {
					dotP = dotP / length(_Vectors[unity_InstanceID]);
				}
				//if (abs(_FluxContributions[unity_InstanceID]) == 0) {
				//	dotP = 0;
				//}
				//else if (_FluxContributions[unity_InstanceID] > 0) {
				//	dotP = 1;
				//}
				//else {
				//	dotP = -1;
				//}

				surface.Albedo.r = 1 - abs(dotP) / 6 + dotP / 6;
				surface.Albedo.g = 1 - 3 * abs(dotP) / 4 + dotP / 4;
				surface.Albedo.b = 1 - abs(dotP) / 2 - dotP / 2;
				surface.Alpha = 0.75;
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
