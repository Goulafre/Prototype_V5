Shader "Hidden/AVProMovieCapture/ODSWatermark"
{
	Properties
	{
		_MainTex ("Stereo Equirectangular Texture", 2D) = "white" {}
		_Watermark ("Watermark", 2D) = "white" {}
		_Face ("Cubemap Face (-1 = all)", Float) = -1
		_Position ("Face-local Position (UV)", Vector) = (0.5, 0.5, 0, 0)
		_Size ("Watermark Size (UV)", Vector) = (0.2, 0.2, 0, 0)
		// Option to draw the border area where in the watermark will be drawn
		[Toggle] _DebugDrawBorder ("DebugDrawBorder", Float) = 0.0
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			// Variables
			sampler2D _MainTex;
			sampler2D _Watermark;
			float _Face;
			float4 _Position;
			float4 _Size;
			float _DebugDrawBorder;

			// Shader Things
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			// Vertex Shader (nothing special here)
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			/// <summary>
			/// Converts equirectangular UV coordinates to a normalized 3D direction vector.
			/// The input UV.x is mapped to longitude (theta), and UV.y is mapped to latitude (phi),
			/// following the equirectangular projection convention:
			///   - UV.x in [0,1] maps to theta in [-π, π] (longitude)
			///   - UV.y in [0,1] maps to phi in [π/2, -π/2] (latitude)
			/// Returns the corresponding direction vector in 3D space.
			/// </summary>
			float3 EquirectUVToDir(float2 uv)
			{
				float theta = uv.x * 2.0 * UNITY_PI - UNITY_PI;
				float phi = (0.5 - uv.y) * UNITY_PI;
				float3 dir;
				dir.x = cos(phi) * sin(theta);
				dir.y = sin(phi);
				dir.z = cos(phi) * cos(theta);
				return normalize(dir);
			}

			/// <summary>
			/// Determines which cubemap face a given 3D direction vector points to.
			/// Returns an integer in the range [0, 5] corresponding to the faces:
			///   0 = +X, 1 = -X, 2 = +Y, 3 = -Y, 4 = +Z, 5 = -Z.
			/// The function compares the absolute values of the direction's components
			/// to find the dominant axis, then checks the sign to select the face.
			/// </summary>
			int GetCubemapFace(float3 dir)
			{
				float3 absDir = abs(dir);
				if (absDir.x > absDir.y && absDir.x > absDir.z)
					return dir.x > 0.0 ? 0 : 1;
				else if (absDir.y > absDir.x && absDir.y > absDir.z)
					return dir.y > 0.0 ? 2 : 3;
				else
					return dir.z > 0.0 ? 4 : 5;
			}

			/// <summary>
			/// Converts a 3D direction vector and a cubemap face index to 2D UV coordinates on that face.
			/// The face index should be in [0, 5]:
			///   0 = +X, 1 = -X, 2 = +Y, 3 = -Y, 4 = +Z, 5 = -Z.
			/// The function projects the direction onto the specified face,
			/// applies necessary axis flips for correct orientation,
			/// and returns normalized UV coordinates in the [0, 1] range.
			/// </summary>
			float2 CubemapFaceUV(float3 dir, int face)
			{
				float u = 0, v = 0;
				float absX = abs(dir.x), absY = abs(dir.y), absZ = abs(dir.z);
				switch (face)
				{
					case 0: u = -dir.z / absX; v = dir.y / absX; break; // +X
					case 1: u = dir.z / absX;  v = dir.y / absX; break; // -X
					case 2: u = dir.x / absY;  v = -dir.z / absY; break; // -Y
					case 3: u = -dir.x / absY;  v = -dir.z / absY; break;  // +Y
					case 4: u = dir.x / absZ;  v = dir.y / absZ; break; // +Z
					case 5: u = -dir.x / absZ; v = dir.y / absZ; break; // -Z
				}
				return float2(u, v) * 0.5 + 0.5;
			}

			void processEye(int eye, float2 uv, float2 minUV, float2 maxUV, inout half4 finalColor, inout half4 wmColor, inout float blendAlpha)
			{
				float borderThickness = 0.01; // Thickness of the debug border
				half4 borderColor = half4(1.0, 0.0, 1.0, 1.0); // Pink color for border

				float yStart = eye == 0 ? 0.0 : 0.5;
				// Remap UV.y for the current eye (range [0,1])
				float2 eyeUV = float2(uv.x, (uv.y - yStart) * 2.0);
				if (eyeUV.y < 0.0 || eyeUV.y > 1.0)
				{
					return;
				}

				// Convert equirect UV to 3D direction
				float3 dir = EquirectUVToDir(eyeUV);
				// Determine cubemap face
				int faceIdx = GetCubemapFace(dir);
				// If a specific face is selected, skip others
				if (_Face != -1.0 && faceIdx != int(_Face))
				{
					return;
				}

				// Convert direction to face-local UV
				float2 faceUV = CubemapFaceUV(dir, faceIdx);

				// Draw debug border if enabled
				if (_DebugDrawBorder == 1)
				{
					if (faceUV.x < borderThickness || faceUV.x > 1.0 - borderThickness ||
						faceUV.y < borderThickness || faceUV.y > 1.0 - borderThickness)
					{
						finalColor = borderColor;
					}
				}

				// Blend watermark if within the specified region
				if (faceUV.x >= minUV.x && faceUV.x <= maxUV.x &&
					faceUV.y >= minUV.y && faceUV.y <= maxUV.y)
				{
					// Map faceUV to local watermark UV
					float2 localUV = (faceUV - minUV) / _Size.xy;
					localUV.y = 1.0 - localUV.y; // Flip vertically if needed
					half4 wm = tex2D(_Watermark, localUV);
					wmColor = lerp(wmColor, wm, wm.a);
					blendAlpha = max(blendAlpha, wm.a);
				}
			}

			/// <summary>
			/// Fragment shader for compositing a stereo equirectangular texture with an optional watermark.
			/// For each fragment:
			/// - Samples the base color from the main equirectangular texture.
			/// - For each eye (top and bottom halves of the texture), converts UV to a 3D direction,
			///   determines the cubemap face, and computes face-local UVs.
			/// - Optionally draws a debug border around the cubemap face area.
			/// - If the fragment is within the specified watermark region, blends the watermark texture
			///   over the base color using the watermark's alpha.
			/// - Returns the final composited color.
			/// </summary>
			half4 frag(v2f i) : SV_Target
			{
				float2 uv = i.uv;
				half4 baseColor = tex2D(_MainTex, uv); // Sample the base equirectangular texture

				float2 halfSize = _Size.xy * 0.5;
				// Watermark region in face-local UV space
				float2 minUV = _Position.xy;
				float2 maxUV = _Position.xy + _Size.xy;

				float blendAlpha = 0;
				half4 wmColor = half4(0, 0, 0, 0);
				half4 finalColor = baseColor;

				processEye(0, uv, minUV, maxUV, finalColor, wmColor, blendAlpha);
				processEye(1, uv, minUV, maxUV, finalColor, wmColor, blendAlpha);

				// Blend the watermark color over the base color
				finalColor = lerp(finalColor, wmColor, blendAlpha);
				return finalColor;
			}
			ENDCG
		}
	}
}
