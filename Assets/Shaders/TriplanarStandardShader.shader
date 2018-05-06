// Standard shader with triplanar mapping
// https://github.com/keijiro/StandardTriplanar

Shader "Custom/Standard Triplanar"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _MainTex("Main Texture", 2D) = "white" {}

        _BumpScale("Bump Scale", Range(0, 2)) = 1
        _BumpMap("Bump Map", 2D) = "bump" {}
        
        _MinMetallic("Minimum Metallic", Range(0, 1)) = 0
        _MetallicMap("Metallic Map", 2D) = "black" {}
        
        _MaxRoughness("Maximum Roughness", Range(0, 1)) = 1
        _RoughnessMap("Roughness Map", 2D) = "white" {}
        
//        _HeightMapScale("Height Scale", Range(0, 1)) = 1
//        _HeightMap("Height Map", 2D) = "white" {}

        _OcclusionStrength("Occlusion Strength", Range(0, 2)) = 1
        _OcclusionMap("Occlusion Map", 2D) = "white" {}

        _MapScale("Map Scale", Range(0.001, 4)) = 1
        _FadeRange("Fade Range", Range(1, 8)) = 1
        
        _Offset("Offset", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM

        #pragma surface surf Standard vertex:vert fullforwardshadows addshadow

        #pragma target 3.0

        half4 _Color;
        sampler2D _MainTex;

        half _BumpScale;
        sampler2D _BumpMap;
        
        half _MaxRoughness;
        sampler2D _MetallicMap;
        
        half _MinMetallic;
        sampler2D _RoughnessMap;
        
//        half _HeightMapScale;
//        sampler2D _HeightMap;

        half _OcclusionStrength;
        sampler2D _OcclusionMap;

        half _MapScale;
        half _FadeRange;
        
        float3 _Offset;

        struct Input
        {
            float3 localCoord;
            float3 localNormal;
            float3 viewDir;
			float2 uv_MainTex;
            float3 vertexColor; // Vertex color stored here by vert() method
		};
 
        void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.localCoord = v.vertex.xyz;
            data.localNormal = v.normal.xyz;
            data.vertexColor = v.color; // Save the Vertex Color in the Input for the surf() method
        }
        
        float4 trimap(sampler2D tex, float3 t, float3 b)
        {
            half4 cx = tex2D(tex, t.yz) * b.x;
            half4 cy = tex2D(tex, t.zx) * b.y;
            half4 cz = tex2D(tex, t.xy) * b.z;
            return cx + cy + cz;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Triplanar mapping
            float3 bf = normalize(pow(abs(IN.localNormal), pow(_FadeRange,2)));
            bf /= dot(bf, (float3)1);
            float3 lcs = (IN.localCoord + _Offset) * _MapScale;

//            float2 thx = IN.localCoord.yz * _MapScale;
//            float2 thy = IN.localCoord.zx * _MapScale;
//            float2 thz = IN.localCoord.xy * _MapScale;
//            
//            half4 hx = tex2D(_HeightMap, thx).r * bf.x;
//            half4 hy = tex2D(_HeightMap, thy).r * bf.y;
//            half4 hz = tex2D(_HeightMap, thz).r * bf.z;
//
//		    float hs = _HeightMapScale;
//            float2 offset = ParallaxOffset(hx + hy + hz, hs, IN.viewDir);
            
            half4 color = trimap(_MainTex, lcs, bf) * _Color;
            o.Albedo = color.rgb * IN.vertexColor.r;
            o.Alpha = color.a;
            o.Normal = UnpackScaleNormal(trimap(_BumpMap, lcs, bf), _BumpScale);
            o.Occlusion = lerp((half4)1, trimap(_OcclusionMap, lcs, bf), _OcclusionStrength);
            o.Metallic = max(_MinMetallic, trimap(_MetallicMap, lcs, bf).x);
            o.Smoothness = 1 - min(_MaxRoughness, trimap(_RoughnessMap, lcs, bf).x);
        }
        ENDCG
    }
    FallBack "Diffuse"
    CustomEditor "StandardTriplanarInspector"
}