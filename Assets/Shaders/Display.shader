// Vertex format:
// position.xyz = vertex position
// texcoord0.xy = uv for texturing
// texcoord1.xy = uv for position/rotation buffer
//
// Position buffer format:
// .xyz = particle position
// .w   = life (+0.5 -> -0.5)
//
// Rotation buffer format:
// .xyzw = particle rotation
//
Shader "Display Shader"
{
    Properties
    {
        _PositionBuffer ("-", 2D) = "black"{}
        _RotationBuffer ("-", 2D) = "red"{}
       
        _Color1		("-", Color) = (1, 1, 1, 1)
        _Color2		("-", Color) = (0.5, 0.5, 0.5, 1)
        _FadeIn  	("-", Range(0,0.5)) = 0.3
        _FadeOut	("-", Range(0.5,1)) = 0.7
        _Decay		("-", Range(0, 1))	= 1.0

        _MainTex ("Color (RGB) Alpha (A)", 2D) = "white"{}
        _OcclusionMap ("-", 2D) = "white"{}

        _ScaleMin ("-", Float) = 1
        _ScaleMax ("-", Float) = 1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite off
        Blend SrcAlpha OneMinusSrcAlpha 
        CGPROGRAM

        #pragma surface surf Standard Lambert alpha:blend vertex:vert nolightmap addshadow alpha:blend
        #pragma shader_feature _ALBEDOMAP
        #pragma target 3.0

        #include "Utility.cginc"

        sampler2D _MainTex;
        sampler2D _OcclusionMap;
        half _FadeIn, _FadeOut, _Decay;

        struct Input
        {
            float2 uv_MainTex;
            half4 color : COLOR;

        };

        void vert(inout appdata_full v)
        {
        	
            float4 uv = float4(v.texcoord1.xy + _BufferOffset, 0, 0);

            float4 p = tex2Dlod(_PositionBuffer, uv);
            float4 r = tex2Dlod(_RotationBuffer, uv);

            float l = p.w + 0.5;
            float s = calc_scale(uv, l);

            v.vertex.xyz = rotate_vector(v.vertex.xyz, r) * s + p.xyz;
            v.normal = rotate_vector(v.normal, r);
            v.color = calc_color(uv, l);
            v.color.a = calc_alpha(uv, l, _FadeIn, _FadeOut);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
        #if _ALBEDOMAP
            half4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = IN.color.rgb * c.rgb;
            o.Alpha = IN.color.a * c.a * _Decay;
        #else
            o.Albedo = IN.color.rgb;
        #endif
        }

        ENDCG
    }
    CustomEditor "DisplayShaderEditor"
}
