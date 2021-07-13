// The instancing/surface shader used to create bubbles for the curl detector

Shader "Vectors/Detectors/CurlBubbles"
{
	Properties
	{
		_Color("Color", Color) = (0,0,1,1)
	}

	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" } // Allowing for transparency
		LOD 100
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM

		// Renders the surface. Requires a ConfigureSurface function.
		#pragma surface ConfigureSurface Standard fullforwardshadows addshadow alpha:fade
		// Does instancing, including(?) placing points. Requires a ConfigureProcedural function.
		#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
		#pragma editor_sync_compilation
		#pragma target 4.5



		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			StructuredBuffer<float> _Distances;
			StructuredBuffer<float3> _Curl;
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

		int StreamSign(int stream) {
			if(fmod(stream, 2) == 0) {
				return 1;
			} else {
				return -1;
			}
		}




		void ConfigureProcedural () 
		{
			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				int streamNumber = (((int) unity_InstanceID) / _ParticlesPerStream); // "int divides are slow, try uint"
				int particleNumber = fmod(unity_InstanceID, 6);
				float dist;

				float componentMag = sqrt(pow(_Curl[StreamToAxis(streamNumber)][0], 2) + pow(_Curl[StreamToAxis(streamNumber)][1], 2) + pow(_Curl[StreamToAxis(streamNumber)][2], 2));

				if (componentMag != 0) 
				{
					dist = fmod(_Distances[streamNumber] + particleNumber * _TravelDistance / _ParticlesPerStream, _TravelDistance);
				}
				else { dist = 0; }

				float3 position;
				float size;

				if (componentMag != 0) { // using pow(-1, ...) always returns NAN. 
					position = _CenterPosition + IDToStream(streamNumber) * _StartDistance + dist * normalize(_Curl[StreamToAxis(streamNumber)]) * StreamSign(streamNumber);
					size = _StartingSize * (_TravelDistance - abs(dist)) / _TravelDistance;
				}
				else {
					position = _CenterPosition + IDToStream(streamNumber) * _StartDistance;
					size = 0;
				}
				
				//// Debug code --- delete me!
				//position = _CenterPosition + IDToStream(streamNumber) * _StartDistance;
				// Sizes ARE getting computed properly. 


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
