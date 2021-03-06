// The instancing/surface shader for the div detector bubbles

Shader "Vectors/Detectors/DivBubbles"
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



		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			StructuredBuffer<float> _Distances;
			StructuredBuffer<float3> _Divergence;
		#endif
		// Why is this check necessary?

		int _ParticlesPerStream;
		float _StartDistance;
		float _TravelDistance;
		float _StartingSize;
		float3 _CenterPosition;

		float3 IDToStream(int id) {
			float3 IDToStreamList[6] =
			{
				float3(1, 0, 0),
				float3(-1, 0, 0),
				float3(0, 1, 0),
				float3(0, -1, 0),
				float3(0, 0, 1),
				float3(0, 0, -1)
			};
			return IDToStreamList[id];
		}

		int StreamToAxis(int stream) {
			return stream / ((int) 2); // "int divides are slow, try uint"
		}




		void ConfigureProcedural () 
		{
			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				int streamNumber = ((unity_InstanceID) / (uint) _ParticlesPerStream); // "int divides are slow, try uint"
				int particleNumber = fmod(unity_InstanceID, 6);
				float dist;

				if (_Divergence[0][StreamToAxis(streamNumber)] > 0) {
					dist = fmod(_Distances[streamNumber] + particleNumber * _TravelDistance / _ParticlesPerStream, _TravelDistance);
				}
				else if (_Divergence[0][StreamToAxis(streamNumber)] < 0) {
					dist = fmod(_Distances[streamNumber] - particleNumber * _TravelDistance / _ParticlesPerStream, _TravelDistance);
				}
				else { dist = 0; }

				float3 position = _CenterPosition + IDToStream(streamNumber) * (_StartDistance + dist);
				float size = _StartingSize * (_TravelDistance - abs(dist)) / _TravelDistance;

				if (_Divergence[0][StreamToAxis(streamNumber)] == 0) {
					size = 0;
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
			void ConfigureSurface (Input input, inout SurfaceOutputStandard surface)
			{
				surface.Albedo = saturate(unity_ObjectToWorld._m02_m12_m22 * 0.5 + 0.5);
			}
		#endif


		ENDCG
	}

	FallBack "Diffuse"
}
