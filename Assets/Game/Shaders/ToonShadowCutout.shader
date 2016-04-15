Shader "Toon/Shadow Cutout" {
    Properties{
        _Color("Main Color", Color) = (0.5,0.5,0.5,1)
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.5
        _MainTex("Base (RGB)", 2D) = "white" {}
    }

        SubShader{
        Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
        LOD 200

        CGPROGRAM
        #pragma multi_compile_instancing
        #pragma surface surf Lambert alphatest:_Cutoff nolightmap addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        float4 _Color;

        struct Input {
            float2 uv_MainTex : TEXCOORD0;
        };

        void surf(Input IN, inout SurfaceOutput o) {
            half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG

    }

    Fallback "Transparent/Cutout/Diffuse"
}
