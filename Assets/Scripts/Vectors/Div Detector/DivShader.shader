// This is our fragment shader.
// Requires that GPU instancing be enabled for the material.

Shader "Custom/DivShader"
{
	Properties
	{
		_Color ("Color", Color) = (0,0,1,1)
	} 

	SubShader
	{
		CGPROGRAM

		// Renders the surface. Requires a ConfigureSurface function.
		#pragma surface ConfigureSurface Standard fullforwardshadows addshadow
		// Does instancing, including(?) placing points. Requires a ConfigureProcedural function.
		#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
		#pragma editor_sync_compilation
		#pragma target 4.5

		//// This is where the work of calculating transformations is done. 
		//#include "../PointsPlot.hlsl" 

		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			StructuredBuffer<float3> _Positions;
			StructuredBuffer<float> _Sizes;
		#endif
		// Why is this check necessary?

		void ConfigureProcedural () 
		{
			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				float4x4 transformation = 0.0; 
				transformation._m33 = 1.0;
				float3 position = _Positions[unity_InstanceID];
				float3 size = float3(1, 1, 1) * _Sizes[unity_InstanceID];

				// The position is, well, the position. 
				transformation._m03_m13_m23 = position;
				transformation._m00_m11_m22 = size;
	
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
			void ConfigureSurface (Input input, inout SurfaceOutputStandard surface)
			{
				surface.Albedo = saturate(unity_ObjectToWorld._m02_m12_m22 * 0.5 + 0.5);
			}
		#endif


		ENDCG
	}

	FallBack "Diffuse"
}
