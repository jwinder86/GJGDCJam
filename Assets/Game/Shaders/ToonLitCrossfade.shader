Shader "Toon/Lit Crossfade" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {} 
	}

	SubShader {
		Tags { "RenderType" = "Opaque" }
		LOD 200
		
        CGPROGRAM
        #pragma multi_compile_instancing
        #pragma multi_compile _ LOD_FADE_CROSSFADE
        #pragma surface surf ToonRamp vertex:vert
        #pragma target 3.0

        sampler2D _Ramp;

        // custom lighting function that uses a texture ramp based
        // on angle between light direction and normal
        #pragma lighting ToonRamp exclude_path:prepass
        inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
        {
	        #ifndef USING_DIRECTIONAL_LIGHT
	        lightDir = normalize(lightDir);
	        #endif
	
	        half d = dot (s.Normal, lightDir)*0.5 + 0.5;
	        half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;
	
	        half4 c;
	        c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
	        c.a = 0;
	        return c;
        }


        sampler2D _MainTex;
        float4 _Color;

        struct Input {
	        float2 uv_MainTex : TEXCOORD0;
            UNITY_DITHER_CROSSFADE_COORDS
        };

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            UNITY_TRANSFER_DITHER_CROSSFADE(o, v.vertex);
        }

        void surf (Input IN, inout SurfaceOutput o) {
	        half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	        o.Albedo = c.rgb;
	        o.Alpha = c.a;

        #ifdef LOD_FADE_CROSSFADE
            UNITY_APPLY_DITHER_CROSSFADE(IN);
        #endif
        }
        ENDCG

	} 

	Fallback "Toon/Shadow"
}
