// Standard shader with triplanar mapping
// https://github.com/keijiro/StandardTriplanar

Shader "Custom/Standard Triplanar"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        
        [Header(Texture for black vertex color)]
        
        [NoScaleOffset]
        _MainTex("Main Albedo", 2D) = "white" {}

        _BumpScale("Bump Scale", Range(0, 2)) = 1
        [Normal]
        [NoScaleOffset]
        _BumpMap("Bump Map", 2D) = "bump" {}
        
        _MinMetallic("Minimum Metallic", Range(0, 1)) = 0
        [NoScaleOffset]
        _MetallicMap("Metallic Map", 2D) = "black" {}
        
        _MaxRoughness("Maximum Roughness", Range(0, 1)) = 1
        [NoScaleOffset]
        _RoughnessMap("Roughness Map", 2D) = "white" {}
        
        _OcclusionStrength("Occlusion Strength", Range(0, 2)) = 1
        [NoScaleOffset]
        _OcclusionMap("Occlusion Map", 2D) = "white" {}

        [Header(Texture for white vertex color)]
        
        _SecondColor("Second Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]
        _SecondTex("Second Albedo", 2D) = "white" {}

        _SecondBumpScale("Second Bump Scale", Range(0, 2)) = 1
        [Normal]
        [NoScaleOffset]
        _SecondBumpMap("Second Bump Map", 2D) = "bump" {}
        
        _SecondMinMetallic("Second Minimum Metallic", Range(0, 1)) = 0
        [NoScaleOffset]
        _SecondMetallicMap("Second Metallic Map", 2D) = "black" {}
        
        _SecondMaxRoughness("Second Maximum Roughness", Range(0, 1)) = 1
        [NoScaleOffset]
        _SecondRoughnessMap("Second Roughness Map", 2D) = "white" {}
        
        _SecondOcclusionStrength("Second Occlusion Strength", Range(0, 2)) = 1
        [NoScaleOffset]
        _SecondOcclusionMap("Second Occlusion Map", 2D) = "white" {}

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
        
        // Main

        half4 _Color;
        sampler2D _MainTex;

        half _BumpScale;
        sampler2D _BumpMap;
        
        half _MaxRoughness;
        sampler2D _MetallicMap;
        
        half _MinMetallic;
        sampler2D _RoughnessMap;
        
        half _OcclusionStrength;
        sampler2D _OcclusionMap;
        
        // Second

        half4 _SecondColor;
        sampler2D _SecondTex;

        half _SecondBumpScale;
        sampler2D _SecondBumpMap;
        
        half _SecondMaxRoughness;
        sampler2D _SecondMetallicMap;
        
        half _SecondMinMetallic;
        sampler2D _SecondRoughnessMap;
        
        half _SecondOcclusionStrength;
        sampler2D _SecondOcclusionMap;

        half _MapScale;
        half _FadeRange;
        
        float3 _Offset;

        struct Input
        {
            float3 localCoord;
            float3 localNormal;
            float3 viewDir;
			float2 uv_MainTex;
            float3 vertexColor;
		};
 
        void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.localCoord = v.vertex.xyz;
            data.localNormal = v.normal.xyz;
            data.vertexColor = v.color;
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

            half r = IN.vertexColor.r;
            half mr = 1 - r;

            half4 color = trimap(_MainTex, lcs, bf) * _Color;
            half4 color2 = trimap(_SecondTex, lcs, bf) * _SecondColor;
            o.Albedo = color.rgb * r + color2 * mr;
            o.Alpha = color.a * r + color2.a * mr;
            
            o.Normal = UnpackScaleNormal(trimap(_BumpMap, lcs, bf), _BumpScale) * r + UnpackScaleNormal(trimap(_SecondBumpMap, lcs, bf), _SecondBumpScale) * mr;
            o.Occlusion = lerp((half4)1, trimap(_OcclusionMap, lcs, bf), _OcclusionStrength) * r + lerp((half4)1, trimap(_SecondOcclusionMap, lcs, bf), _SecondOcclusionStrength) * mr;
            o.Metallic = max(_MinMetallic, trimap(_MetallicMap, lcs, bf).x) * r + max(_SecondMinMetallic, trimap(_SecondMetallicMap, lcs, bf).x) * mr;
            o.Smoothness = 1 - (min(_MaxRoughness, trimap(_RoughnessMap, lcs, bf).x) * r + min(_SecondMaxRoughness, trimap(_SecondRoughnessMap, lcs, bf).x) * mr);
        }
        ENDCG
    }
    FallBack "Diffuse"
}