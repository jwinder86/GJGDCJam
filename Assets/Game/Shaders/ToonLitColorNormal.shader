Shader "Toon/Lit Color Normal" {
    Properties {
        _Color ("Main Color", Color) = (0.5,0.5,0.5,1)
        _BumpMap ("Normal", 2D) = "bump" {}
        _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
    }

    SubShader {
        Tags { "RenderType" = "Opaque" }
        LOD 200

CGPROGRAM
#pragma surface surf ToonRamp

sampler2D _Ramp;
sampler2D _BumpMap;

// custom lighting function that uses a texture ramp based
// on angle between light direction and normal
#pragma lighting ToonRamp exclude_path:prepass
inline half4 LightingToonRamp(SurfaceOutput s, half3 lightDir, half atten)
{
    #ifndef USING_DIRECTIONAL_LIGHT
    lightDir = normalize(lightDir);
    #endif

    half d = dot(s.Normal, lightDir)*0.5 + 0.5;
    half3 ramp = tex2D(_Ramp, float2(d,d)).rgb;

    half4 c;
    c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
    c.a = 0;
    return c;
}


float4 _Color;

struct Input {
    float2 uv_BumpMap : TEXCOORD0;
};

void surf(Input IN, inout SurfaceOutput o) {
    o.Albedo = _Color.rgb;
    o.Alpha = _Color.a;
    o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG

    }

        Fallback "Diffuse"
}
