Shader "Custom/TileMeltShader"
{
    Properties
{
    _MainTex ("Tile Data Texture", 2D) = "white" {}
    _TileSize ("Tile Size", Float) = 32
    _BlurStrength ("Blur Strength", Range(0, 1)) = 0.5
    _PixelSize ("Pixel Size", Float) = 1
}
SubShader
{
    Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
    LOD 100
 
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    ENDHLSL
 
    Pass
    {
        HLSLPROGRAM
        #pragma vertex vert
        #pragma fragment frag
 
        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };
 
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };
 
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
 
        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float _TileSize;
            float _BlurStrength;
            float _PixelSize;
        CBUFFER_END
 
        Varyings vert(Attributes IN)
        {
            Varyings OUT;
            OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
            OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
            return OUT;
        }
 
        float4 sampleTile(float2 uv)
        {
            return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
        }
 
        float4 frag(Varyings IN) : SV_Target
        {
            float2 pixelCoord = floor(IN.uv * _TileSize / _PixelSize) * _PixelSize;
            float2 fracCoord = frac(IN.uv * _TileSize / _PixelSize) * _PixelSize;
 
            float2 uvCenter = (pixelCoord + 0.5 * _PixelSize) / _TileSize;
 
            float4 centerTile = sampleTile(uvCenter);
 
            float4 neighbors[4];
            neighbors[0] = sampleTile(uvCenter + float2(0, _PixelSize) / _TileSize);
            neighbors[1] = sampleTile(uvCenter + float2(_PixelSize, 0) / _TileSize);
            neighbors[2] = sampleTile(uvCenter + float2(0, -_PixelSize) / _TileSize);
            neighbors[3] = sampleTile(uvCenter + float2(-_PixelSize, 0) / _TileSize);
 
            float4 blendColor = centerTile;
            float totalWeight = 1;
 
            for (int i = 0; i < 4; i++)
            {
                float weight = 1 - length(fracCoord - float2(0.5, 0.5) * _PixelSize) / (_PixelSize * sqrt(2));
                weight = saturate(weight * 2) * _BlurStrength;
 
                if (!all(neighbors[i] == centerTile))
                {
                    blendColor += neighbors[i] * weight;
                    totalWeight += weight;
                }
            }
 
            blendColor /= totalWeight;
 
            return float4(blendColor.rgb, 1.0); 
        }
        ENDHLSL
    }
}
}