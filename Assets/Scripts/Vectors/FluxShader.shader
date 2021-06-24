// This is our fragment shader.

Shader "Vectors/FluxShader" 
{
	Properties
	{
		// _Scaling ("Scaling Factor", float) = 0.1
	} // This needs to be used somewhere. Where?

	SubShader
	{
		CGPROGRAM

		// Renders the surface. Requires a ConfigureSurface function.
		#pragma surface ConfigureSurface Standard fullforwardshadows addshadow
		// Does instancing, including(?) placing points. Requires a ConfigureProcedural function.
		#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
		#pragma editor_sync_compilation
		#pragma target 4.5

		// This is where the work of calculating transformations is done. 
		#include "PointsPlot.hlsl"

		float4 _DetectorCenter;

		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		// #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		// 	float2 FindMaxMinMagnitudes() {
		// 		float max = _Magnitudes[0];
		// 		float min = _Magnitudes[0];
		// 		for(int i = 1; i < _Magnitudes.Length; i++) {
		// 			float mag = _Magnitudes[i];
		// 			if(mag < min) {
		// 				min = mag;
		// 			}
		// 			if (mag > max) {
		// 				max = mag;
		// 			}
		// 		}
		// 		return float2(max, min);
		// 	}
		// #endif

		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) {
				// This won't work for non-circular detectors
				float3 normal = _Positions[unity_InstanceID] - float3(_DetectorCenter.x, _DetectorCenter.y, _DetectorCenter.z);
				float3 vect = _Vectors[unity_InstanceID];
				float dotP = dot(normalize(vect), normalize(normal));

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
