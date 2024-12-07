// Made with Amplify Shader Editor v1.9.3.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Toby Fredson/The Toby Foliage Engine/(TTFE) Tree Billboard"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[Header(__________(TTFE) TREE BILLBOARD SHADER___________)][Header(_____________________________________________________)][Header(Texture Maps)][NoScaleOffset]_AlbedoMap("Albedo Map", 2D) = "white" {}
		[NoScaleOffset][Normal]_NormalMap("Normal Map", 2D) = "bump" {}
		[NoScaleOffset]_MaskMapRGBA("Mask Map *RGB(A)", 2D) = "white" {}
		[NoScaleOffset]_NoiseMapGrayscale("Noise Map (Grayscale)", 2D) = "white" {}
		[Header(_____________________________________________________)][Header(Texture settings)][Header((Albedo))]_AlebedoColor("Alebedo Color", Color) = (1,1,1,0)
		[Header((Normal))]_NormalIntenisty("Normal Intenisty", Float) = 1
		[Header((Smoothness))]_SmoothnessIntensity("Smoothness Intensity", Range( 0 , 1)) = 1
		[Header((Ambient Occlusion))]_AmbientOcclusionIntensity("Ambient Occlusion Intensity", Range( 0 , 1)) = 1
		[Header((Specular))]_SpecularPower("Specular Power", Range( 0 , 1)) = 1
		[Header((Translucency))]_TranslucencyPower("Translucency Power", Range( 1 , 10)) = 1
		[Header( _____________________________________________________)][Header(Shading Settings)][Header((Self Shading))]_VertexLighting("Vertex Lighting", Float) = 0
		_VertexShadow("Vertex Shadow", Float) = 0
		[Toggle(_SELFSHADING_ON)] _SelfShading("Self Shading", Float) = 0
		[Header(Seasons Settings)][Header((Season Control))]_ColorVariation("Color Variation", Range( 0 , 1)) = 1
		_DryLeafColor("Dry Leaf Color", Color) = (0.5568628,0.3730685,0.1764706,0)
		_DryLeavesScale("Dry Leaves - Scale", Float) = 0
		_DryLeavesOffset("Dry Leaves - Offset", Float) = 0
		_SeasonChangeGlobal("Season Change - Global", Range( -2 , 2)) = 0
		[Toggle]_BranchMaskR("Branch Mask *(R)", Float) = 1
		[Header(_____________________________________________________)][Header(Wind Settings)][Header((Global Wind Settings))]_GlobalWindStrength("Global Wind Strength", Range( 0 , 1)) = 1
		[KeywordEnum(GentleBreeze,WindOff)] _WindType("Wind Type", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" "DisableBatching" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _WINDTYPE_GENTLEBREEZE _WINDTYPE_WINDOFF
		#pragma shader_feature_local _SELFSHADING_ON
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows dithercrossfade vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float _GlobalWindStrength;
		uniform sampler2D _NormalMap;
		uniform float _NormalIntenisty;
		uniform float4 _AlebedoColor;
		uniform sampler2D _AlbedoMap;
		uniform float4 _DryLeafColor;
		uniform sampler2D _NoiseMapGrayscale;
		uniform float _SeasonChangeGlobal;
		uniform float _DryLeavesScale;
		uniform float _DryLeavesOffset;
		uniform float _ColorVariation;
		uniform float _BranchMaskR;
		uniform sampler2D _MaskMapRGBA;
		uniform float _VertexLighting;
		uniform float _VertexShadow;
		uniform float _TranslucencyPower;
		uniform float _SpecularPower;
		uniform float _SmoothnessIntensity;
		uniform float _AmbientOcclusionIntensity;
		uniform float _Cutoff = 0.5;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		struct Gradient
		{
			int type;
			int colorsLength;
			int alphasLength;
			float4 colors[8];
			float2 alphas[8];
		};


		Gradient NewGradient(int type, int colorsLength, int alphasLength, 
		float4 colors0, float4 colors1, float4 colors2, float4 colors3, float4 colors4, float4 colors5, float4 colors6, float4 colors7,
		float2 alphas0, float2 alphas1, float2 alphas2, float2 alphas3, float2 alphas4, float2 alphas5, float2 alphas6, float2 alphas7)
		{
			Gradient g;
			g.type = type;
			g.colorsLength = colorsLength;
			g.alphasLength = alphasLength;
			g.colors[ 0 ] = colors0;
			g.colors[ 1 ] = colors1;
			g.colors[ 2 ] = colors2;
			g.colors[ 3 ] = colors3;
			g.colors[ 4 ] = colors4;
			g.colors[ 5 ] = colors5;
			g.colors[ 6 ] = colors6;
			g.colors[ 7 ] = colors7;
			g.alphas[ 0 ] = alphas0;
			g.alphas[ 1 ] = alphas1;
			g.alphas[ 2 ] = alphas2;
			g.alphas[ 3 ] = alphas3;
			g.alphas[ 4 ] = alphas4;
			g.alphas[ 5 ] = alphas5;
			g.alphas[ 6 ] = alphas6;
			g.alphas[ 7 ] = alphas7;
			return g;
		}


		float4 SampleGradient( Gradient gradient, float time )
		{
			float3 color = gradient.colors[0].rgb;
			UNITY_UNROLL
			for (int c = 1; c < 8; c++)
			{
			float colorPos = saturate((time - gradient.colors[c-1].w) / ( 0.00001 + (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, (float)gradient.colorsLength-1));
			color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
			}
			#ifndef UNITY_COLORSPACE_GAMMA
			color = half3(GammaToLinearSpaceExact(color.r), GammaToLinearSpaceExact(color.g), GammaToLinearSpaceExact(color.b));
			#endif
			float alpha = gradient.alphas[0].x;
			UNITY_UNROLL
			for (int a = 1; a < 8; a++)
			{
			float alphaPos = saturate((time - gradient.alphas[a-1].y) / ( 0.00001 + (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, (float)gradient.alphasLength-1));
			alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
			}
			return float4(color, alpha);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 appendResult256_g1332 = (float3(0.0 , 0.0 , saturate( ase_vertex3Pos ).z));
			float3 break252_g1332 = ase_vertex3Pos;
			float3 appendResult255_g1332 = (float3(break252_g1332.x , ( break252_g1332.y * 0.15 ) , 0.0));
			float mulTime263_g1332 = _Time.y * 2.1;
			float3 temp_cast_0 = (ase_vertex3Pos.y).xxx;
			float2 appendResult300_g1332 = (float2(ase_vertex3Pos.x , ase_vertex3Pos.z));
			float3 temp_output_303_0_g1332 = ( cross( temp_cast_0 , float3( appendResult300_g1332 ,  0.0 ) ) * 0.005 );
			float3 appendResult270_g1332 = (float3(0.0 , ase_vertex3Pos.y , 0.0));
			float3 break269_g1332 = ase_vertex3Pos;
			float3 appendResult271_g1332 = (float3(break269_g1332.x , 0.0 , ( break269_g1332.z * 0.15 )));
			float mulTime282_g1332 = _Time.y * 2.3;
			float3 appendResult293_g1332 = (float3(ase_vertex3Pos.x , 0.0 , 0.0));
			float3 break288_g1332 = ase_vertex3Pos;
			float3 appendResult292_g1332 = (float3(0.0 , ( break288_g1332.y * 0.2 ) , ( break288_g1332.z * 0.4 )));
			float mulTime249_g1332 = _Time.y * 2.0;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 normalizeResult155_g1332 = normalize( ase_worldPos );
			float mulTime161_g1332 = _Time.y * 0.25;
			float simplePerlin2D159_g1332 = snoise( ( normalizeResult155_g1332 + mulTime161_g1332 ).xy*0.43 );
			float WindMask_LargeB169_g1332 = ( simplePerlin2D159_g1332 * 1.5 );
			float3 normalizeResult162_g1332 = normalize( ase_worldPos );
			float mulTime167_g1332 = _Time.y * 0.26;
			float simplePerlin2D166_g1332 = snoise( ( normalizeResult162_g1332 + mulTime167_g1332 ).xy*0.7 );
			float WindMask_LargeC170_g1332 = ( simplePerlin2D166_g1332 * 1.5 );
			float mulTime133_g1332 = _Time.y * 3.2;
			float3 worldToObj126_g1332 = mul( unity_WorldToObject, float4( ase_vertex3Pos, 1 ) ).xyz;
			float3 temp_output_135_0_g1332 = ( mulTime133_g1332 + ( 0.02 * worldToObj126_g1332.x ) + ( worldToObj126_g1332.y * 0.14 ) + ( worldToObj126_g1332.z * 0.16 ) + float3(0.4,0.3,0.1) );
			float3 ase_objectScale = float3( length( unity_ObjectToWorld[ 0 ].xyz ), length( unity_ObjectToWorld[ 1 ].xyz ), length( unity_ObjectToWorld[ 2 ].xyz ) );
			float mulTime111_g1332 = _Time.y * 2.3;
			float3 worldToObj103_g1332 = mul( unity_WorldToObject, float4( ase_vertex3Pos, 1 ) ).xyz;
			float3 temp_output_106_0_g1332 = ( mulTime111_g1332 + ( 0.2 * worldToObj103_g1332 ) + float3(0.4,0.3,0.1) );
			float mulTime118_g1332 = _Time.y * 3.6;
			float3 temp_cast_4 = (ase_vertex3Pos.x).xxx;
			float3 worldToObj114_g1332 = mul( unity_WorldToObject, float4( temp_cast_4, 1 ) ).xyz;
			float temp_output_119_0_g1332 = ( mulTime118_g1332 + ( 0.2 * worldToObj114_g1332.x ) );
			float3 temp_cast_5 = (0.0).xxx;
			#if defined(_WINDTYPE_GENTLEBREEZE)
				float3 staticSwitch312_g1332 = ( ( ( ( ( ( appendResult256_g1332 + ( appendResult255_g1332 * cos( mulTime263_g1332 ) ) + ( cross( float3(1.2,0.6,1) , ( appendResult255_g1332 * float3(0.7,1,0.8) ) ) * sin( mulTime263_g1332 ) ) ) * temp_output_303_0_g1332 ) * 0.08 ) + ( ( ( appendResult270_g1332 + ( appendResult271_g1332 * cos( mulTime282_g1332 ) ) + ( cross( float3(0.9,1,1.2) , ( appendResult271_g1332 * float3(1,1,1) ) ) * sin( mulTime282_g1332 ) ) ) * temp_output_303_0_g1332 ) * 0.1 ) + ( ( ( appendResult293_g1332 + ( appendResult292_g1332 * cos( mulTime249_g1332 ) ) + ( cross( float3(1.1,1.3,0.8) , ( appendResult292_g1332 * float3(1.4,0.8,1.1) ) ) * sin( mulTime249_g1332 ) ) ) * temp_output_303_0_g1332 ) * 0.05 ) ) * WindMask_LargeB169_g1332 * saturate( ase_vertex3Pos.y ) ) + ( ( WindMask_LargeC170_g1332 * ( ( ( cos( temp_output_135_0_g1332 ) * sin( temp_output_135_0_g1332 ) * saturate( ase_objectScale ) ) * 0.2 ) + ( ( cos( temp_output_106_0_g1332 ) * sin( temp_output_106_0_g1332 ) * saturate( ase_objectScale ) ) * 0.2 ) + ( ( sin( temp_output_119_0_g1332 ) * cos( temp_output_119_0_g1332 ) ) * 0.2 ) ) * saturate( ase_vertex3Pos.x ) ) * 0.3 ) );
			#elif defined(_WINDTYPE_WINDOFF)
				float3 staticSwitch312_g1332 = temp_cast_5;
			#else
				float3 staticSwitch312_g1332 = ( ( ( ( ( ( appendResult256_g1332 + ( appendResult255_g1332 * cos( mulTime263_g1332 ) ) + ( cross( float3(1.2,0.6,1) , ( appendResult255_g1332 * float3(0.7,1,0.8) ) ) * sin( mulTime263_g1332 ) ) ) * temp_output_303_0_g1332 ) * 0.08 ) + ( ( ( appendResult270_g1332 + ( appendResult271_g1332 * cos( mulTime282_g1332 ) ) + ( cross( float3(0.9,1,1.2) , ( appendResult271_g1332 * float3(1,1,1) ) ) * sin( mulTime282_g1332 ) ) ) * temp_output_303_0_g1332 ) * 0.1 ) + ( ( ( appendResult293_g1332 + ( appendResult292_g1332 * cos( mulTime249_g1332 ) ) + ( cross( float3(1.1,1.3,0.8) , ( appendResult292_g1332 * float3(1.4,0.8,1.1) ) ) * sin( mulTime249_g1332 ) ) ) * temp_output_303_0_g1332 ) * 0.05 ) ) * WindMask_LargeB169_g1332 * saturate( ase_vertex3Pos.y ) ) + ( ( WindMask_LargeC170_g1332 * ( ( ( cos( temp_output_135_0_g1332 ) * sin( temp_output_135_0_g1332 ) * saturate( ase_objectScale ) ) * 0.2 ) + ( ( cos( temp_output_106_0_g1332 ) * sin( temp_output_106_0_g1332 ) * saturate( ase_objectScale ) ) * 0.2 ) + ( ( sin( temp_output_119_0_g1332 ) * cos( temp_output_119_0_g1332 ) ) * 0.2 ) ) * saturate( ase_vertex3Pos.x ) ) * 0.3 ) );
			#endif
			v.vertex.xyz += ( _GlobalWindStrength * staticSwitch312_g1332 );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_NormalMap87_g1333 = i.uv_texcoord;
			float3 Normal_Output155_g1333 = UnpackScaleNormal( tex2D( _NormalMap, uv_NormalMap87_g1333 ), _NormalIntenisty );
			o.Normal = Normal_Output155_g1333;
			float2 uv_AlbedoMap81_g1333 = i.uv_texcoord;
			float2 uv_AlbedoMap83_g1333 = i.uv_texcoord;
			float4 tex2DNode83_g1333 = tex2D( _AlbedoMap, uv_AlbedoMap83_g1333 );
			float2 uv_NoiseMapGrayscale98_g1333 = i.uv_texcoord;
			float4 transform94_g1333 = mul(unity_ObjectToWorld,float4( 1,1,1,1 ));
			float dotResult4_g1335 = dot( transform94_g1333.xy , float2( 12.9898,78.233 ) );
			float lerpResult10_g1335 = lerp( 0.0 , 1.0 , frac( ( sin( dotResult4_g1335 ) * 43758.55 ) ));
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 normalizeResult120_g1333 = normalize( ase_vertex3Pos );
			float DryLeafPositionMask124_g1333 = ( (distance( normalizeResult120_g1333 , float3( 0,0.8,0 ) )*1.0 + 0.0) * 1 );
			float4 lerpResult46_g1333 = lerp( ( _DryLeafColor * ( tex2DNode83_g1333.g * 2 ) ) , tex2DNode83_g1333 , saturate( (( ( tex2D( _NoiseMapGrayscale, uv_NoiseMapGrayscale98_g1333 ).r * lerpResult10_g1335 * DryLeafPositionMask124_g1333 ) - _SeasonChangeGlobal )*_DryLeavesScale + _DryLeavesOffset) ));
			float4 SeasonControl_Output88_g1333 = lerpResult46_g1333;
			Gradient gradient60_g1333 = NewGradient( 0, 2, 2, float4( 1, 0.276868, 0, 0 ), float4( 0, 1, 0.7818019, 1 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float4 transform62_g1333 = mul(unity_ObjectToWorld,float4( 1,1,1,1 ));
			float dotResult4_g1334 = dot( transform62_g1333.xy , float2( 12.9898,78.233 ) );
			float lerpResult10_g1334 = lerp( 0.0 , 1.0 , frac( ( sin( dotResult4_g1334 ) * 43758.55 ) ));
			float4 lerpResult70_g1333 = lerp( SeasonControl_Output88_g1333 , ( ( SeasonControl_Output88_g1333 * 0.5 ) + ( SampleGradient( gradient60_g1333, lerpResult10_g1334 ) * SeasonControl_Output88_g1333 ) ) , _ColorVariation);
			float2 uv_MaskMapRGBA82_g1333 = i.uv_texcoord;
			float4 lerpResult78_g1333 = lerp( tex2D( _AlbedoMap, uv_AlbedoMap81_g1333 ) , lerpResult70_g1333 , (( _BranchMaskR )?( tex2D( _MaskMapRGBA, uv_MaskMapRGBA82_g1333 ).r ):( 1.0 )));
			float3 temp_output_104_0_g1333 = ( ( ase_vertex3Pos * float3( 2,1.3,2 ) ) / 25.0 );
			float dotResult107_g1333 = dot( temp_output_104_0_g1333 , temp_output_104_0_g1333 );
			float3 normalizeResult103_g1333 = normalize( ase_vertex3Pos );
			float SelfShading115_g1333 = saturate( (( pow( saturate( dotResult107_g1333 ) , 1.5 ) + ( ( 1.0 - (distance( normalizeResult103_g1333 , float3( 0,0.8,0 ) )*0.5 + 0.0) ) * 0.6 ) )*0.92 + -0.16) );
			#ifdef _SELFSHADING_ON
				float4 staticSwitch74_g1333 = ( lerpResult78_g1333 * (SelfShading115_g1333*_VertexLighting + _VertexShadow) );
			#else
				float4 staticSwitch74_g1333 = lerpResult78_g1333;
			#endif
			float4 LeafColorVariationSeasons_Output91_g1333 = staticSwitch74_g1333;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult151_g1333 = dot( ase_worldViewDir , -( ase_worldlightDir + ase_vertex3Pos ) );
			float2 uv_MaskMapRGBA152_g1333 = i.uv_texcoord;
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float TobyTranslucency153_g1333 = ( saturate( dotResult151_g1333 ) * tex2D( _MaskMapRGBA, uv_MaskMapRGBA152_g1333 ).b * ase_lightColor.a );
			float TranslucencyIntensity39_g1333 = _TranslucencyPower;
			float4 Albedo_Output154_g1333 = ( ( _AlebedoColor * LeafColorVariationSeasons_Output91_g1333 ) * (1.0 + (TobyTranslucency153_g1333 - 0.0) * (TranslucencyIntensity39_g1333 - 1.0) / (1.0 - 0.0)) );
			o.Albedo = Albedo_Output154_g1333.rgb;
			float Specular_Output125_g1333 = ( 0.04 * 1.0 * _SpecularPower );
			float3 temp_cast_3 = (Specular_Output125_g1333).xxx;
			o.Specular = temp_cast_3;
			float2 uv_MaskMapRGBA79_g1333 = i.uv_texcoord;
			float4 tex2DNode79_g1333 = tex2D( _MaskMapRGBA, uv_MaskMapRGBA79_g1333 );
			float Smoothness_Output35_g1333 = ( tex2DNode79_g1333.a * _SmoothnessIntensity );
			o.Smoothness = Smoothness_Output35_g1333;
			float AoMapBase31_g1333 = tex2DNode79_g1333.g;
			float Ao_Output141_g1333 = ( pow( AoMapBase31_g1333 , _AmbientOcclusionIntensity ) * ( 1.5 / ( ( saturate( TobyTranslucency153_g1333 ) * TranslucencyIntensity39_g1333 ) + 1.5 ) ) );
			o.Occlusion = Ao_Output141_g1333;
			o.Alpha = 1;
			float2 uv_AlbedoMap80_g1333 = i.uv_texcoord;
			float Opacity_Output86_g1333 = tex2D( _AlbedoMap, uv_AlbedoMap80_g1333 ).a;
			clip( Opacity_Output86_g1333 - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19302
Node;AmplifyShaderEditor.FunctionNode;751;-257.7573,279.2351;Inherit;False;(TTFE) Tree Billboard_Wind System;21;;1332;7781363c3f1900c46819cf845d29a41f;0;0;1;FLOAT3;229
Node;AmplifyShaderEditor.FunctionNode;750;-247.1451,-4.363159;Inherit;False;(TTFE) Tree Billboard_Shading;1;;1333;0f57c3e4aefb35640bedd1f6e47c6f57;0;0;6;COLOR;162;FLOAT3;168;FLOAT;164;FLOAT;167;FLOAT;163;FLOAT;166
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;117.2345,-37.59569;Float;False;True;-1;2;;0;0;StandardSpecular;Toby Fredson/The Toby Foliage Engine/(TTFE) Tree Billboard;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;5;False;;10;False;;0;5;False;;10;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;0;750;162
WireConnection;0;1;750;168
WireConnection;0;3;750;164
WireConnection;0;4;750;167
WireConnection;0;5;750;163
WireConnection;0;10;750;166
WireConnection;0;11;751;229
ASEEND*/
//CHKSM=D25EE789115AE35CCC06ACB91ECE79C073FCB3C8