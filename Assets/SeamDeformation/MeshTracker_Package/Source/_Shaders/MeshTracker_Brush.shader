Shader "Matej Vanco/Mesh Tracker/Mesh Tracker Brush"
{
	//---Shader written by Matej Vanco 2018
	//---Updated October 2022
	Properties
	{
		[Header(Brush Essential Params)]
		[Space]
		_Height("Height", Range(-2, 2)) = 0.5
		_HeightMulti("Multiplication", Float) = 1
		_Opacity("Opacity", Range(0, 1)) = 1.0
		_Cutout("Cutout",Range(0.0,1.0)) = 0.0
		[Toggle] _Inverse("Inverse Color",Int) = 0
		[Space]
		[Header(Brush Rotation (In Degrees))]
		[Space]
		_Rotation("Rotation", Float) = 0.0
		[Space]
		[Header(Brush Noise Params)]
		[Space]
		[Toggle]_Noise("Enable Noise", Int) = 0.0
		_NoiseSpeed("Noise Speed",Float) = 15.0
		_NoiseSize("Noise Size",Float) = 5.0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
		}

		Lighting Off Cull Off ZTest Always ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};

			sampler2D _MainTex;
			sampler2D _MainTex2;
			float4 _SourceTex;

			int _Inverse;
			int _Noise;

			float _Rotation;
			float _Height;
			float _HeightMulti;
			float _Opacity;
			float _Cutout;

			float _NoiseSpeed;
			float _NoiseSize;

			//---Noise formulas

			float rand(float2 n) 
			{
				return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
			}

			float nFractDat(float2 p) 
			{
				float2 ip = floor(p);
				float2 u = frac(p);
				u = u * u * (3.0 - 2.0 * u);
				float res = lerp(lerp(rand(ip), rand(ip + float2(1.0, 0.0)), u.x), lerp(rand(ip + float2(0.0, 1.0)), rand(ip + float2(1.0, 1.0)), u.x), u.y);
				return res * res;
			}

			float nSrc(float2 p)
			{
				float2x2 mtx = float2x2(0.80, 0.60, -0.60, 0.80);
				float f = 0.0;

				f += 0.500000 * nFractDat(p + _Time.x * _NoiseSpeed);
				p = p * mtx[1] * 2.02;
				f += 0.031250 * nFractDat(p); p = mtx[1] * p * 2.01;
				f += 0.250000 * nFractDat(p); p = mtx[1] * p * 2.03;
				f += 0.125000 * nFractDat(p); p = mtx[1] * p * 2.01;
				f += 0.062500 * nFractDat(p); p = mtx[1] * p * 2.04;
				f += 0.015625 * nFractDat(p + sin(_Time.x));

				return f / 0.96875;
			}

			float returnNoise(in float2 p)
			{
				return nSrc(p + nSrc(p + nSrc(p)));
			}

			struct appdata_t 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			//---Brush essentials

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);

				_Rotation = _Rotation * (3.1415926f / 180.0f);
				_Rotation *= -1;
				v.texcoord.xy -= 0.5;
				float s = sin(_Rotation);
				float c = cos(_Rotation);
				float2x2 rotationMatrix = float2x2(c, -s, s, c);
				rotationMatrix *= 0.5;
				rotationMatrix += 0.5;

				rotationMatrix = rotationMatrix * 2 - 1;

				v.texcoord.xy = mul(v.texcoord.xy, rotationMatrix);
				v.texcoord.xy += 0.5;
			
				o.uv = v.texcoord, _MainTex;
				o.texcoord1 = _SourceTex.xy + v.texcoord * _SourceTex.zw;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 track = tex2D(_MainTex, i.uv);
				fixed4 surface = tex2D(_MainTex2, i.texcoord1);

				float gradient = clamp(track.r - 1, 0, 1);
				float indent = clamp(track.r - 1, -1, 0);

				_Height *= _HeightMulti;
				float3 finalColor = surface.rgb + surface.rgb * gradient + indent.rrr + _Height;
				if (_Inverse == 1) finalColor *= -1.f;
				if (_Noise == 1) finalColor.rgb *= returnNoise(i.uv * _NoiseSize);
				if (track.a < _Cutout) discard;
				return fixed4(finalColor.rgb, track.a * _Opacity);
			}

			ENDCG
		}
	}
}
