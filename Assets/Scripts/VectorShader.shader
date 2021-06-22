// This is our fragment shader.
// Requires that GPU instancing be enabled for the material.

Shader "Vectors/Shader"
{
	Properties
	{
		_MinColor ("Minimum strength color", Color) = (0,0,1,1)
		_MaxColor ("Maximum strength color", Color) = (1,1,0,1)
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

		float _MaxMagnitude;
		float4 _MinColor;
		float4 _MaxColor;

		struct Input
		{
			float3 worldPos;
		};


		float3 HUEtoRGB(in float H)
		{ // Borrowed this from the internet: https://www.chilliant.com/rgb2hsv.html
		float R = abs(H * 6 - 3) - 1;
		float G = 2 - abs(H * 6 - 2);
		float B = 2 - abs(H * 6 - 4);
		return saturate(float3(R,G,B));
		}

		float RGBtoH(in float3 RGB)
			{
				float Epsilon = 1e-10;
				// Based on work by Sam Hocevar and Emil Persson
				float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
				float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
				float C = Q.x - min(Q.w, Q.y);
				float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
				return H;
			}

		#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
			void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) {
				float vect_mag = _Magnitudes[unity_InstanceID];
				float val = vect_mag / _MaxMagnitude;
				//float3 output = HUEtoRGB(vect_mag);
				float3 output = HUEtoRGB(RGBtoH(_MinColor.rgb) + val * (RGBtoH(_MaxColor.rgb) - RGBtoH(_MinColor.rgb)));
				surface.Albedo = output;
				//rgb2hsv(_MaxColor)
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
