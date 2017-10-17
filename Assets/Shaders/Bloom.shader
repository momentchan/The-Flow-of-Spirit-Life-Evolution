Shader "Hidden/Bloom"
{
	Properties 
	{
        _MainTex ("_MainTex (RGB)", 2D) = "black"
		_BlurTex ("_Blur (RGB)", 2D) = "" {}
	}

	SubShader
	{
		ZTest Always Cull Off ZWrite Off Fog { Mode Off } Lighting Off Blend Off

		CGINCLUDE
		#include "UnityCG.cginc"

		sampler2D	_MainTex;
		sampler2D	_BlurTex;

		float4		_MainTex_ST;
		half4		_MainTex_TexelSize;

		half		_Threshold;
		half4		_Weight;

		
		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v_blur
		{
			float4 pos : SV_POSITION;
			half2 uv0 : TEXCOORD0;
			half2 uv1 : TEXCOORD1;
			half2 uv2 : TEXCOORD2;
			half2 uv3 : TEXCOORD3;
			half2 uv4 : TEXCOORD4;
		};

		v2f vert(appdata_img v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv =  v.texcoord.xy;
			return o;
		}

		v_blur vertBlur(appdata_img v)
		{
			v_blur o;
			o.pos = UnityObjectToClipPos (v.vertex);
			o.uv0 = v.texcoord;
			o.uv1 = v.texcoord + _MainTex_TexelSize.xy * _Weight.xy;
			o.uv2 = v.texcoord - _MainTex_TexelSize.xy * _Weight.xy;
			o.uv3 = v.texcoord + _MainTex_TexelSize.xy * _Weight.zw;
			o.uv4 = v.texcoord - _MainTex_TexelSize.xy * _Weight.zw;
			return o;
		}

		// Binarization
		half4 fragBinary(v2f i) : SV_Target
		{
			half4 color = tex2D(_MainTex, i.uv);
			half luma = dot(color, half4(0.3h, 0.59h, 0.11h, 0.0h));  // gray
			return luma < _Threshold ? half4(0.0h, 0.0h, 0.0h, 0.0h):color;
		}

		// Blur
		half4 fragBlur(v_blur i) : COLOR
		{
			half4 color = half4(0.2h, 0.2h, 0.2h, 0.2h) * tex2D (_MainTex, i.uv0);
			color += half4(0.2h, 0.2h, 0.2h, 0.2h) * (tex2D (_MainTex, i.uv1) + tex2D (_MainTex, i.uv2) + tex2D (_MainTex, i.uv3) + tex2D (_MainTex, i.uv4));
			return color;
		}

		// Bloom
		half4 fragBloom(v2f i) : SV_Target
		{
			half4 color = tex2D(_MainTex, i.uv);
			half4 bloom = tex2D(_BlurTex, i.uv);

			half luma = dot(color, half4(0.3h, 0.59h, 0.11h, 0.0h));

			// Emphasis brighter
			if(luma>_Threshold * 1.1)	bloom *= _Weight.x;
			else 						bloom *= _Weight.y;
			return color + bloom;
		}
		ENDCG


		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragBinary
			ENDCG
		}

		Pass 
		{
			CGPROGRAM
			#pragma vertex vertBlur
			#pragma fragment fragBlur
			ENDCG
		}
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragBloom
			ENDCG
		}
	}

	FallBack Off
}

