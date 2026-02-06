/*
    This shader is used to blit the Watermark on the the texture that is being captured
    it can be customised to apply effects.
*/
Shader "Hidden/AVProMovieCapture/CustomWatermarkTexture" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {} // Required (the shader being drawn to)
        _WatermarkTex ("Watermark Texture", 2D) = "white" {} // Required (the watemark to draw)
        _WatermarkRect ("Watermark Rect (x, y, w, h)", Vector) = (0, 0, 0.2, 0.2) // Required (The size and position of the watermark)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _WatermarkTex;
            float4 _WatermarkTex_ST;
            float4 _WatermarkRect;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 mainTexColor = tex2D(_MainTex, i.uv);
                // Calculate watermark UV coordinates based on the rect
                float UVX = (i.uv.x - _WatermarkRect.x) / _WatermarkRect.z;
                float UVY = (i.uv.y - _WatermarkRect.y) / _WatermarkRect.w;
                float2 watermarkUV = float2(UVX, UVY);
                // Check if the current pixel is within the watermark rect
                if (watermarkUV.x >= 0 && watermarkUV.x <= 1 && watermarkUV.y >= 0 && watermarkUV.y <= 1) {
                    fixed4 watermarkColor = tex2D(_WatermarkTex, watermarkUV);
                    return lerp(mainTexColor, watermarkColor, watermarkColor.a); // Blend using alpha
                } else {
                    return mainTexColor; // If outside the rect, just return the main texture color
                }
            }
            ENDCG
        }
    }
}