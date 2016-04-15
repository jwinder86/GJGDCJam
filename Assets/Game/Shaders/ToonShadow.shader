Shader "Toon/Shadow" {
    Properties{
        _Color("Main Color", Color) = (0.5,0.5,0.5,1)
    }

        SubShader{
        Tags{ "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma multi_compile_instancing
        #pragma surface surf Lambert nolightmap addshadow

        float4 _Color;

        struct Input {
            float2 uv_MainTex : TEXCOORD0;
        };

        void surf(Input IN, inout SurfaceOutput o) {
            half4 c = _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG

    }

    Fallback "Diffuse"
}
