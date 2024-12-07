// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Toby Fredson/The Toby Foliage Engine/(TTFE) Tree Foliage"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.4
		[Header(__________(TTFE) TREE FOLIAGE SHADER___________)][Header(_____________________________________________________)][Header(Texture Maps)][NoScaleOffset]_AlbedoMap("Albedo Map", 2D) = "white" {}
		[NoScaleOffset][Normal]_NormalMap("Normal Map", 2D) = "bump" {}
		[NoScaleOffset]_MaskMapRGBA("Mask Map *RGB(A)", 2D) = "white" {}
		[NoScaleOffset]_NoiseMapGrayscale("Noise Map (Grayscale)", 2D) = "white" {}
		[Header(_____________________________________________________)][Header(Texture settings)][Header((Albedo))]_AlbedoColor("Albedo Color", Color) = (1,1,1,0)
		[Header((Normal))]_NormalIntenisty("Normal Intenisty", Float) = 1
		[Toggle]_NormalBackFaceFixBranch("Normal Back Face Fix (Branch)", Float) = 0
		[Header((Smoothness))]_SmoothnessIntensity("Smoothness Intensity", Range( 0 , 1)) = 1
		[Header((Ambient Occlusion))]_AmbientOcclusionIntensity("Ambient Occlusion Intensity", Range( 0 , 1)) = 1
		[Header((Specular))]_SpecularPower("Specular Power", Range( 0 , 1)) = 1
		[Header((Translucency))]_TranslucencyPower("Translucency Power", Range( 1 , 10)) = 1
		_TranslucencyRange("Translucency Range", Float) = 1
		[Toggle]_TranslucencyTreeTangents("Translucency Tree Tangents", Float) = 0
		[Header( _____________________________________________________)][Header(Shading Settings)][Header((Self Shading))]_VertexLighting("Vertex Lighting", Float) = 0
		_VertexShadow("Vertex Shadow", Float) = 0
		[Toggle(_SELFSHADINGVERTEXCOLOR_ON)] _SelfShadingVertexColor("Self Shading (Vertex Color)", Float) = 0
		[Toggle]_LightDetectBackface("Light Detect (Back face)", Float) = 1
		[Header( _____________________________________________________)][Header(Season Settings)][Header((Season Control))]_ColorVariation("Color Variation", Range( 0 , 1)) = 1
		_DryLeafColor("Dry Leaf Color", Color) = (0.5568628,0.3730685,0.1764706,0)
		_DryLeavesScale("Dry Leaves - Scale", Float) = 0
		_DryLeavesOffset("Dry Leaves - Offset", Float) = 0
		_SeasonChangeGlobal("Season Change - Global", Range( -2 , 2)) = 0
		[Toggle]_BranchMaskR("Branch Mask *(R)", Float) = 0
		[Toggle]_NormalizeSeasons("Normalize Seasons", Float) = 0
		[Header(_____________________________________________________)][Header(Wind Settings)][Header((Global Wind Settings))]_GlobalWindStrength("Global Wind Strength", Range( 0 , 1)) = 1
		_StrongWindSpeed("Strong Wind Speed", Range( 1 , 3)) = 1
		[KeywordEnum(GentleBreeze,WindOff)] _WindType("Wind Type", Float) = 0
		[Header((Trunk and Branch))]_BranchWindLarge("Branch Wind Large", Range( 0 , 20)) = 1
		_BranchWindSmall("Branch Wind Small", Range( 0 , 20)) = 1
		[Toggle(_LEAFFLUTTER_ON)] _LeafFlutter("Leaf Flutter", Float) = 1
		_GlobalFlutterIntensity("Global Flutter Intensity", Range( 0 , 20)) = 1
		[NoScaleOffset]_WindNoise("Wind Noise Map", 2D) = "white" {}
		[Toggle]_PivotSway("Pivot Sway", Float) = 0
		[Header((Wind Mask))]_Radius("Radius", Float) = 1
		_Hardness("Hardness", Float) = 1
		[Toggle]_CenterofMass("Center of Mass", Float) = 0
		[Toggle]_SwitchVGreenToRGBA("Switch VGreen To RGBA", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" "DisableBatching" = "True" }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _WINDTYPE_GENTLEBREEZE _WINDTYPE_WINDOFF
		#pragma shader_feature_local _LEAFFLUTTER_ON
		#pragma shader_feature_local _SELFSHADINGVERTEXCOLOR_ON
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			half ASEIsFrontFacing : VFACE;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform float _GlobalWindStrength;
		uniform float _Radius;
		uniform float _Hardness;
		uniform float _BranchWindLarge;
		uniform float _CenterofMass;
		uniform float _BranchWindSmall;
		uniform float _StrongWindSpeed;
		uniform float _SwitchVGreenToRGBA;
		uniform sampler2D _WindNoise;
		uniform float _GlobalFlutterIntensity;
		uniform float _PivotSway;
		uniform float _LightDetectBackface;
		uniform sampler2D _NormalMap;
		uniform float _NormalBackFaceFixBranch;
		uniform float _NormalIntenisty;
		uniform float4 _AlbedoColor;
		uniform sampler2D _AlbedoMap;
		uniform float4 _DryLeafColor;
		uniform sampler2D _NoiseMapGrayscale;
		uniform float _NormalizeSeasons;
		uniform float _SeasonChangeGlobal;
		uniform float _DryLeavesScale;
		uniform float _DryLeavesOffset;
		uniform float _ColorVariation;
		uniform float _BranchMaskR;
		uniform sampler2D _MaskMapRGBA;
		uniform float _VertexLighting;
		uniform float _VertexShadow;
		uniform float _TranslucencyTreeTangents;
		uniform float _TranslucencyRange;
		uniform float _TranslucencyPower;
		uniform float _SpecularPower;
		uniform float _SmoothnessIntensity;
		uniform float _AmbientOcclusionIntensity;
		uniform float _Cutoff = 0.4;


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


		inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }

		inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }

		inline float valueNoise (float2 uv)
		{
			float2 i = floor(uv);
			float2 f = frac( uv );
			f = f* f * (3.0 - 2.0 * f);
			uv = abs( frac(uv) - 0.5);
			float2 c0 = i + float2( 0.0, 0.0 );
			float2 c1 = i + float2( 1.0, 0.0 );
			float2 c2 = i + float2( 0.0, 1.0 );
			float2 c3 = i + float2( 1.0, 1.0 );
			float r0 = noise_randomValue( c0 );
			float r1 = noise_randomValue( c1 );
			float r2 = noise_randomValue( c2 );
			float r3 = noise_randomValue( c3 );
			float bottomOfGrid = noise_interpolate( r0, r1, f.x );
			float topOfGrid = noise_interpolate( r2, r3, f.x );
			float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
			return t;
		}


		float SimpleNoise(float2 UV)
		{
			float t = 0.0;
			float freq = pow( 2.0, float( 0 ) );
			float amp = pow( 0.5, float( 3 - 0 ) );
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(1));
			amp = pow(0.5, float(3-1));
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(2));
			amp = pow(0.5, float(3-2));
			t += valueNoise( UV/freq )*amp;
			return t;
		}


		float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
		{
			original -= center;
			float C = cos( angle );
			float S = sin( angle );
			float t = 1 - C;
			float m00 = t * u.x * u.x + C;
			float m01 = t * u.x * u.y - S * u.z;
			float m02 = t * u.x * u.z + S * u.y;
			float m10 = t * u.x * u.y + S * u.z;
			float m11 = t * u.y * u.y + C;
			float m12 = t * u.y * u.z - S * u.x;
			float m20 = t * u.x * u.z - S * u.y;
			float m21 = t * u.y * u.z + S * u.x;
			float m22 = t * u.z * u.z + C;
			float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
			return mul( finalMatrix, original ) + center;
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
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 normalizeResult710_g1382 = normalize( ase_worldPos );
			float mulTime716_g1382 = _Time.y * 0.25;
			float simplePerlin2D714_g1382 = snoise( ( normalizeResult710_g1382 + mulTime716_g1382 ).xy*0.43 );
			float WindMask_LargeB725_g1382 = ( simplePerlin2D714_g1382 * 1.5 );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 appendResult820_g1382 = (float3(0.0 , 0.0 , saturate( ase_vertex3Pos ).z));
			float3 break862_g1382 = ase_vertex3Pos;
			float3 appendResult819_g1382 = (float3(break862_g1382.x , ( break862_g1382.y * 0.15 ) , 0.0));
			float mulTime849_g1382 = _Time.y * 2.1;
			float3 temp_output_573_0_g1382 = ( ( ase_vertex3Pos - float3(0,-1,0) ) / _Radius );
			float dotResult574_g1382 = dot( temp_output_573_0_g1382 , temp_output_573_0_g1382 );
			float temp_output_577_0_g1382 = pow( saturate( dotResult574_g1382 ) , _Hardness );
			float SphearicalMaskCM735_g1382 = saturate( temp_output_577_0_g1382 );
			float3 temp_cast_1 = (ase_vertex3Pos.y).xxx;
			float2 appendResult810_g1382 = (float2(ase_vertex3Pos.x , ase_vertex3Pos.z));
			float3 temp_output_869_0_g1382 = ( cross( temp_cast_1 , float3( appendResult810_g1382 ,  0.0 ) ) * 0.005 );
			float3 appendResult813_g1382 = (float3(0.0 , ase_vertex3Pos.y , 0.0));
			float3 break845_g1382 = ase_vertex3Pos;
			float3 appendResult843_g1382 = (float3(break845_g1382.x , 0.0 , ( break845_g1382.z * 0.15 )));
			float mulTime850_g1382 = _Time.y * 2.3;
			float dotResult730_g1382 = dot( (ase_vertex3Pos*0.02 + 0.0) , ase_vertex3Pos );
			float CeneterOfMassThickness_Mask734_g1382 = saturate( dotResult730_g1382 );
			float3 appendResult854_g1382 = (float3(ase_vertex3Pos.x , 0.0 , 0.0));
			float3 break857_g1382 = ase_vertex3Pos;
			float3 appendResult842_g1382 = (float3(0.0 , ( break857_g1382.y * 0.2 ) , ( break857_g1382.z * 0.4 )));
			float mulTime851_g1382 = _Time.y * 2.0;
			float3 normalizeResult1560_g1382 = normalize( ase_vertex3Pos );
			float CenterOfMassTrunkUP_C1561_g1382 = saturate( distance( normalizeResult1560_g1382 , float3(0,1,0) ) );
			float3 normalizeResult718_g1382 = normalize( ase_worldPos );
			float mulTime723_g1382 = _Time.y * 0.26;
			float simplePerlin2D722_g1382 = snoise( ( normalizeResult718_g1382 + mulTime723_g1382 ).xy*0.7 );
			float WindMask_LargeC726_g1382 = ( simplePerlin2D722_g1382 * 1.5 );
			float mulTime795_g1382 = _Time.y * 3.2;
			float3 worldToObj796_g1382 = mul( unity_WorldToObject, float4( ase_vertex3Pos, 1 ) ).xyz;
			float3 temp_output_763_0_g1382 = ( mulTime795_g1382 + float3(0.4,0.3,0.1) + ( worldToObj796_g1382.x * 0.02 ) + ( 0.14 * worldToObj796_g1382.y ) + ( worldToObj796_g1382.z * 0.16 ) );
			float3 normalizeResult581_g1382 = normalize( ase_vertex3Pos );
			float CenterOfMassTrunkUP586_g1382 = saturate( (distance( normalizeResult581_g1382 , float3(0,1,0) )*1.0 + -0.05) );
			float3 ase_objectScale = float3( length( unity_ObjectToWorld[ 0 ].xyz ), length( unity_ObjectToWorld[ 1 ].xyz ), length( unity_ObjectToWorld[ 2 ].xyz ) );
			float mulTime794_g1382 = _Time.y * 2.3;
			float3 worldToObj797_g1382 = mul( unity_WorldToObject, float4( ase_vertex3Pos, 1 ) ).xyz;
			float3 temp_output_757_0_g1382 = ( mulTime794_g1382 + ( 0.2 * worldToObj797_g1382 ) + float3(0.4,0.3,0.1) );
			float mulTime793_g1382 = _Time.y * 3.6;
			float3 temp_cast_5 = (ase_vertex3Pos.x).xxx;
			float3 worldToObj799_g1382 = mul( unity_WorldToObject, float4( temp_cast_5, 1 ) ).xyz;
			float temp_output_787_0_g1382 = ( mulTime793_g1382 + ( 0.2 * worldToObj799_g1382.x ) );
			float3 normalizeResult647_g1382 = normalize( ase_vertex3Pos );
			float CenterOfMass651_g1382 = saturate( (distance( normalizeResult647_g1382 , float3(0,1,0) )*2.0 + 0.0) );
			float SphericalMaskProxySphere655_g1382 = (( _CenterofMass )?( ( temp_output_577_0_g1382 * CenterOfMass651_g1382 ) ):( temp_output_577_0_g1382 ));
			float StrongWindSpeed994_g1382 = _StrongWindSpeed;
			float2 appendResult1379_g1382 = (float2(ase_worldPos.x , ase_worldPos.z));
			float3 worldToObj1380_g1382 = mul( unity_WorldToObject, float4( float3( appendResult1379_g1382 ,  0.0 ), 1 ) ).xyz;
			float simpleNoise1430_g1382 = SimpleNoise( ( ( StrongWindSpeed994_g1382 * _Time.y ) + worldToObj1380_g1382 ).xy*4.0 );
			simpleNoise1430_g1382 = simpleNoise1430_g1382*2 - 1;
			float4 ase_vertexTangent = v.tangent;
			float3 worldToObj1376_g1382 = mul( unity_WorldToObject, float4( ase_vertex3Pos, 1 ) ).xyz;
			float mulTime1321_g1382 = _Time.y * 10.0;
			float3 temp_output_1316_0_g1382 = ( sin( ( ( worldToObj1376_g1382 * ( 1.0 * 10.0 * ase_objectScale ) ) + mulTime1321_g1382 + 1.0 ) ) * 0.028 );
			float3 MotionFlutterConstant1481_g1382 = ( temp_output_1316_0_g1382 * 33 );
			float4 temp_cast_12 = (v.color.g).xxxx;
			float4 LeafVertexColor_Main1540_g1382 = (( _SwitchVGreenToRGBA )?( v.color ):( temp_cast_12 ));
			float mulTime1349_g1382 = _Time.y * 0.4;
			float3 worldToObj1443_g1382 = mul( unity_WorldToObject, float4( ase_vertexTangent.xyz, 1 ) ).xyz;
			float2 panner1354_g1382 = ( mulTime1349_g1382 * float2( 1,1 ) + ( worldToObj1443_g1382 * 0.1 ).xy);
			float2 uv_TexCoord1355_g1382 = v.texcoord.xy * float2( 0.2,0.2 ) + panner1354_g1382;
			float3 normalizeResult589_g1382 = normalize( ase_worldPos );
			float mulTime590_g1382 = _Time.y * 0.2;
			float simplePerlin2D592_g1382 = snoise( ( normalizeResult589_g1382 + mulTime590_g1382 ).xy*0.4 );
			float WindMask_LargeA595_g1382 = ( simplePerlin2D592_g1382 * 1.5 );
			float3 worldToObjDir1435_g1382 = mul( unity_WorldToObject, float4( ( tex2Dlod( _WindNoise, float4( uv_TexCoord1355_g1382, 0, 0.0) ) * WindMask_LargeA595_g1382 * WindMask_LargeC726_g1382 ).rgb, 0 ) ).xyz;
			float dotResult4_g1383 = dot( float2( 0.2,0.2 ) , float2( 12.9898,78.233 ) );
			float lerpResult10_g1383 = lerp( 0.0 , 0.35 , frac( ( sin( dotResult4_g1383 ) * 43758.55 ) ));
			float2 appendResult1454_g1382 = (float2(ase_worldPos.x , ase_worldPos.z));
			float simpleNoise1455_g1382 = SimpleNoise( ( appendResult1454_g1382 + ( StrongWindSpeed994_g1382 * _Time.y ) )*4.0 );
			simpleNoise1455_g1382 = simpleNoise1455_g1382*2 - 1;
			float simplePerlin2D1395_g1382 = snoise( ( ( StrongWindSpeed994_g1382 * _Time.y ) + ( ase_vertexTangent.xyz * 1.0 ) ).xy );
			#ifdef _LEAFFLUTTER_ON
				float4 staticSwitch1263_g1382 = ( ( ( ( simpleNoise1430_g1382 * 0.9 ) * float4( float3(-1,-0.5,-1) , 0.0 ) * float4( ase_vertexTangent.xyz , 0.0 ) * saturate( ase_vertex3Pos.y ) * float4( MotionFlutterConstant1481_g1382 , 0.0 ) * WindMask_LargeC726_g1382 * LeafVertexColor_Main1540_g1382 ) + ( ( float4( worldToObjDir1435_g1382 , 0.0 ) * float4( float3(-1,-1,-1) , 0.0 ) * saturate( ase_vertex3Pos.y ) * LeafVertexColor_Main1540_g1382 * float4( ase_objectScale , 0.0 ) ) * 1 ) + ( ( float4( float3(-1,-1,-1) , 0.0 ) * lerpResult10_g1383 * simpleNoise1455_g1382 * saturate( ase_vertex3Pos.y ) * LeafVertexColor_Main1540_g1382 * float4( ase_vertexTangent.xyz , 0.0 ) ) * 2 ) + ( ( simplePerlin2D1395_g1382 * 0.11 ) * float4( float3(5.9,5.9,5.9) , 0.0 ) * float4( ase_vertexTangent.xyz , 0.0 ) * saturate( ase_vertex3Pos.y ) * WindMask_LargeA595_g1382 * LeafVertexColor_Main1540_g1382 ) + ( ( float4( temp_output_1316_0_g1382 , 0.0 ) * saturate( ase_vertex3Pos.y ) * LeafVertexColor_Main1540_g1382 ) * 3 ) ) * _GlobalFlutterIntensity );
			#else
				float4 staticSwitch1263_g1382 = float4( 0,0,0,0 );
			#endif
			float3 worldToObj1580_g1382 = mul( unity_WorldToObject, float4( ase_vertex3Pos, 1 ) ).xyz;
			float mulTime1587_g1382 = _Time.y * 4.0;
			float mulTime1579_g1382 = _Time.y * 0.2;
			float2 appendResult1576_g1382 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 normalizeResult1578_g1382 = normalize( appendResult1576_g1382 );
			float simpleNoise1588_g1382 = SimpleNoise( ( mulTime1579_g1382 + normalizeResult1578_g1382 )*1.0 );
			float WindMask_SimpleSway1593_g1382 = ( simpleNoise1588_g1382 * 1.5 );
			float3 rotatedValue1599_g1382 = RotateAroundAxis( float3( 0,0,0 ), ase_vertex3Pos, normalize( float3(0.6,1,0.1) ), ( ( cos( ( ( worldToObj1580_g1382 * 0.02 ) + mulTime1587_g1382 + ( float3(0.6,1,0.8) * 0.3 * worldToObj1580_g1382 ) ) ) * 0.1 ) * WindMask_SimpleSway1593_g1382 * saturate( ase_objectScale ) ).x );
			float4 temp_cast_30 = (0.0).xxxx;
			#if defined(_WINDTYPE_GENTLEBREEZE)
				float4 staticSwitch1496_g1382 = ( ( float4( ( ( WindMask_LargeB725_g1382 * ( ( ( ( ( appendResult820_g1382 + ( appendResult819_g1382 * cos( mulTime849_g1382 ) ) + ( cross( float3(1.2,0.6,1) , ( float3(0.7,1,0.8) * appendResult819_g1382 ) ) * sin( mulTime849_g1382 ) ) ) * SphearicalMaskCM735_g1382 * temp_output_869_0_g1382 ) * 0.08 ) + ( ( ( appendResult813_g1382 + ( appendResult843_g1382 * cos( mulTime850_g1382 ) ) + ( cross( float3(0.9,1,1.2) , ( float3(1,1,1) * appendResult843_g1382 ) ) * sin( mulTime850_g1382 ) ) ) * SphearicalMaskCM735_g1382 * CeneterOfMassThickness_Mask734_g1382 * temp_output_869_0_g1382 ) * 0.1 ) + ( ( ( appendResult854_g1382 + ( appendResult842_g1382 * cos( mulTime851_g1382 ) ) + ( cross( float3(1.1,1.3,0.8) , ( float3(1.4,0.8,1.1) * appendResult842_g1382 ) ) * sin( mulTime851_g1382 ) ) ) * SphearicalMaskCM735_g1382 * temp_output_869_0_g1382 ) * 0.05 ) ) * _BranchWindLarge ) ) * CenterOfMassTrunkUP_C1561_g1382 ) , 0.0 ) + float4( ( ( ( WindMask_LargeC726_g1382 * ( ( ( ( cos( temp_output_763_0_g1382 ) * sin( temp_output_763_0_g1382 ) * CenterOfMassTrunkUP586_g1382 * SphearicalMaskCM735_g1382 * CeneterOfMassThickness_Mask734_g1382 * saturate( ase_objectScale ) ) * 0.2 ) + ( ( cos( temp_output_757_0_g1382 ) * sin( temp_output_757_0_g1382 ) * CenterOfMassTrunkUP586_g1382 * CeneterOfMassThickness_Mask734_g1382 * SphearicalMaskCM735_g1382 * saturate( ase_objectScale ) ) * 0.2 ) + ( ( sin( temp_output_787_0_g1382 ) * cos( temp_output_787_0_g1382 ) * SphericalMaskProxySphere655_g1382 * CeneterOfMassThickness_Mask734_g1382 * CenterOfMassTrunkUP586_g1382 ) * 0.2 ) ) * _BranchWindSmall ) ) * 0.3 ) * CenterOfMassTrunkUP_C1561_g1382 ) , 0.0 ) + ( staticSwitch1263_g1382 * 0.3 ) + float4( (( _PivotSway )?( ( ( rotatedValue1599_g1382 - ase_vertex3Pos ) * 0.4 ) ):( float3( 0,0,0 ) )) , 0.0 ) ) * saturate( ase_vertex3Pos.y ) );
			#elif defined(_WINDTYPE_WINDOFF)
				float4 staticSwitch1496_g1382 = temp_cast_30;
			#else
				float4 staticSwitch1496_g1382 = ( ( float4( ( ( WindMask_LargeB725_g1382 * ( ( ( ( ( appendResult820_g1382 + ( appendResult819_g1382 * cos( mulTime849_g1382 ) ) + ( cross( float3(1.2,0.6,1) , ( float3(0.7,1,0.8) * appendResult819_g1382 ) ) * sin( mulTime849_g1382 ) ) ) * SphearicalMaskCM735_g1382 * temp_output_869_0_g1382 ) * 0.08 ) + ( ( ( appendResult813_g1382 + ( appendResult843_g1382 * cos( mulTime850_g1382 ) ) + ( cross( float3(0.9,1,1.2) , ( float3(1,1,1) * appendResult843_g1382 ) ) * sin( mulTime850_g1382 ) ) ) * SphearicalMaskCM735_g1382 * CeneterOfMassThickness_Mask734_g1382 * temp_output_869_0_g1382 ) * 0.1 ) + ( ( ( appendResult854_g1382 + ( appendResult842_g1382 * cos( mulTime851_g1382 ) ) + ( cross( float3(1.1,1.3,0.8) , ( float3(1.4,0.8,1.1) * appendResult842_g1382 ) ) * sin( mulTime851_g1382 ) ) ) * SphearicalMaskCM735_g1382 * temp_output_869_0_g1382 ) * 0.05 ) ) * _BranchWindLarge ) ) * CenterOfMassTrunkUP_C1561_g1382 ) , 0.0 ) + float4( ( ( ( WindMask_LargeC726_g1382 * ( ( ( ( cos( temp_output_763_0_g1382 ) * sin( temp_output_763_0_g1382 ) * CenterOfMassTrunkUP586_g1382 * SphearicalMaskCM735_g1382 * CeneterOfMassThickness_Mask734_g1382 * saturate( ase_objectScale ) ) * 0.2 ) + ( ( cos( temp_output_757_0_g1382 ) * sin( temp_output_757_0_g1382 ) * CenterOfMassTrunkUP586_g1382 * CeneterOfMassThickness_Mask734_g1382 * SphearicalMaskCM735_g1382 * saturate( ase_objectScale ) ) * 0.2 ) + ( ( sin( temp_output_787_0_g1382 ) * cos( temp_output_787_0_g1382 ) * SphericalMaskProxySphere655_g1382 * CeneterOfMassThickness_Mask734_g1382 * CenterOfMassTrunkUP586_g1382 ) * 0.2 ) ) * _BranchWindSmall ) ) * 0.3 ) * CenterOfMassTrunkUP_C1561_g1382 ) , 0.0 ) + ( staticSwitch1263_g1382 * 0.3 ) + float4( (( _PivotSway )?( ( ( rotatedValue1599_g1382 - ase_vertex3Pos ) * 0.4 ) ):( float3( 0,0,0 ) )) , 0.0 ) ) * saturate( ase_vertex3Pos.y ) );
			#endif
			float4 FinalWind_Output163_g1382 = ( _GlobalWindStrength * staticSwitch1496_g1382 );
			v.vertex.xyz += FinalWind_Output163_g1382.rgb;
			v.vertex.w = 1;
			float3 ase_vertexNormal = v.normal.xyz;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float dotResult494_g1366 = dot( ase_worldViewDir , ase_normWorldNormal );
			float2 uv_NormalMap789_g1366 = v.texcoord;
			float3 ifLocalVar497_g1366 = 0;
			if( dotResult494_g1366 > 0.0 )
				ifLocalVar497_g1366 = UnpackScaleNormal( -tex2Dlod( _NormalMap, float4( uv_NormalMap789_g1366, 0, 0.0) ), -1.0 );
			else if( dotResult494_g1366 == 0.0 )
				ifLocalVar497_g1366 = UnpackScaleNormal( -tex2Dlod( _NormalMap, float4( uv_NormalMap789_g1366, 0, 0.0) ), -1.0 );
			else if( dotResult494_g1366 < 0.0 )
				ifLocalVar497_g1366 = -ase_vertexNormal;
			float4 transform500_g1366 = mul(unity_ObjectToWorld,float4( ifLocalVar497_g1366 , 0.0 ));
			float dotResult504_g1366 = dot( float4( ase_worldlightDir , 0.0 ) , transform500_g1366 );
			float3 ifLocalVar511_g1366 = 0;
			if( dotResult504_g1366 >= 0.0 )
				ifLocalVar511_g1366 = ifLocalVar497_g1366;
			else
				ifLocalVar511_g1366 = -ifLocalVar497_g1366;
			float3 break514_g1366 = ifLocalVar511_g1366;
			float3 temp_cast_37 = (dotResult504_g1366).xxx;
			float4 appendResult525_g1366 = (float4(break514_g1366.x , ( break514_g1366.y + saturate( ( 1.0 - ( ( distance( float3( 0,0,0 ) , temp_cast_37 ) - 0.2 ) / max( 0.2 , 1E-05 ) ) ) ) ) , break514_g1366.z , 0.0));
			float4 LightDetectBackface595_g1366 = appendResult525_g1366;
			float4 LightDetect_Output597_g1366 = (( _LightDetectBackface )?( LightDetectBackface595_g1366 ):( float4( ase_vertexNormal , 0.0 ) ));
			v.normal = LightDetect_Output597_g1366.xyz;
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_NormalMap531_g1366 = i.uv_texcoord;
			float3 tex2DNode531_g1366 = UnpackScaleNormal( tex2D( _NormalMap, uv_NormalMap531_g1366 ), _NormalIntenisty );
			float3 break539_g1366 = tex2DNode531_g1366;
			float3 appendResult552_g1366 = (float3(break539_g1366.x , break539_g1366.y , ( break539_g1366.z * i.ASEIsFrontFacing )));
			float3 Normal_Output557_g1366 = (( _NormalBackFaceFixBranch )?( appendResult552_g1366 ):( tex2DNode531_g1366 ));
			o.Normal = Normal_Output557_g1366;
			float2 uv_AlbedoMap513_g1366 = i.uv_texcoord;
			float2 uv_AlbedoMap662_g1366 = i.uv_texcoord;
			float4 tex2DNode662_g1366 = tex2D( _AlbedoMap, uv_AlbedoMap662_g1366 );
			float2 uv_NoiseMapGrayscale669_g1366 = i.uv_texcoord;
			float4 transform741_g1366 = mul(unity_ObjectToWorld,float4( 1,1,1,1 ));
			float dotResult4_g1368 = dot( transform741_g1366.xy , float2( 12.9898,78.233 ) );
			float lerpResult10_g1368 = lerp( 0.0 , 1.0 , frac( ( sin( dotResult4_g1368 ) * 43758.55 ) ));
			float temp_output_742_0_g1366 = lerpResult10_g1368;
			float normalizeResult792_g1366 = normalize( temp_output_742_0_g1366 );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 normalizeResult439_g1366 = normalize( ase_vertex3Pos );
			float DryLeafPositionMask443_g1366 = ( (distance( normalizeResult439_g1366 , float3( 0,0.8,0 ) )*1.0 + 0.0) * 1 );
			float4 lerpResult677_g1366 = lerp( ( _DryLeafColor * ( tex2DNode662_g1366.g * 2 ) ) , tex2DNode662_g1366 , saturate( (( ( tex2D( _NoiseMapGrayscale, uv_NoiseMapGrayscale669_g1366 ).r * (( _NormalizeSeasons )?( normalizeResult792_g1366 ):( temp_output_742_0_g1366 )) * DryLeafPositionMask443_g1366 ) - _SeasonChangeGlobal )*_DryLeavesScale + _DryLeavesOffset) ));
			float4 SeasonControl_Output676_g1366 = lerpResult677_g1366;
			Gradient gradient752_g1366 = NewGradient( 0, 2, 2, float4( 1, 0.276868, 0, 0 ), float4( 0, 1, 0.7818019, 1 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float4 transform754_g1366 = mul(unity_ObjectToWorld,float4( 1,1,1,1 ));
			float dotResult4_g1369 = dot( transform754_g1366.xy , float2( 12.9898,78.233 ) );
			float lerpResult10_g1369 = lerp( 0.0 , 1.0 , frac( ( sin( dotResult4_g1369 ) * 43758.55 ) ));
			float4 lerpResult515_g1366 = lerp( SeasonControl_Output676_g1366 , ( ( SeasonControl_Output676_g1366 * 0.5 ) + ( SampleGradient( gradient752_g1366, lerpResult10_g1369 ) * SeasonControl_Output676_g1366 ) ) , _ColorVariation);
			float2 uv_MaskMapRGBA505_g1366 = i.uv_texcoord;
			float4 lerpResult521_g1366 = lerp( tex2D( _AlbedoMap, uv_AlbedoMap513_g1366 ) , lerpResult515_g1366 , (( _BranchMaskR )?( tex2D( _MaskMapRGBA, uv_MaskMapRGBA505_g1366 ).r ):( 1.0 )));
			float3 temp_output_465_0_g1366 = ( ( ase_vertex3Pos * float3( 2,1.3,2 ) ) / 25.0 );
			float dotResult471_g1366 = dot( temp_output_465_0_g1366 , temp_output_465_0_g1366 );
			float3 normalizeResult457_g1366 = normalize( ase_vertex3Pos );
			float SelfShading601_g1366 = saturate( (( pow( saturate( dotResult471_g1366 ) , 1.5 ) + ( ( 1.0 - (distance( normalizeResult457_g1366 , float3( 0,0.8,0 ) )*0.5 + 0.0) ) * 0.6 ) )*0.92 + -0.16) );
			#ifdef _SELFSHADINGVERTEXCOLOR_ON
				float4 staticSwitch618_g1366 = ( lerpResult521_g1366 * (SelfShading601_g1366*_VertexLighting + _VertexShadow) );
			#else
				float4 staticSwitch618_g1366 = lerpResult521_g1366;
			#endif
			float4 GrassColorVariation_Output586_g1366 = staticSwitch618_g1366;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float4 transform487_g1366 = mul(unity_ObjectToWorld,float4( ase_vertex3Pos , 0.0 ));
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			ase_vertexNormal = normalize( ase_vertexNormal );
			float dotResult566_g1366 = dot( float4( ase_worldViewDir , 0.0 ) , -( float4( ase_worldlightDir , 0.0 ) + ( (( _TranslucencyTreeTangents )?( float4( ase_vertexNormal , 0.0 ) ):( transform487_g1366 )) * _TranslucencyRange ) ) );
			float2 uv_MaskMapRGBA516_g1366 = i.uv_texcoord;
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float TobyTranslucency526_g1366 = ( saturate( dotResult566_g1366 ) * tex2D( _MaskMapRGBA, uv_MaskMapRGBA516_g1366 ).b * ase_lightColor.a );
			float TranslucencyIntensity616_g1366 = _TranslucencyPower;
			float4 Albedo_Output613_g1366 = ( ( _AlbedoColor * GrassColorVariation_Output586_g1366 ) * (1.0 + (TobyTranslucency526_g1366 - 0.0) * (TranslucencyIntensity616_g1366 - 1.0) / (1.0 - 0.0)) );
			o.Albedo = Albedo_Output613_g1366.rgb;
			float Specular_Output570_g1366 = ( 0.04 * 1.0 * _SpecularPower );
			float3 temp_cast_7 = (Specular_Output570_g1366).xxx;
			o.Specular = temp_cast_7;
			float2 uv_MaskMapRGBA535_g1366 = i.uv_texcoord;
			float4 tex2DNode535_g1366 = tex2D( _MaskMapRGBA, uv_MaskMapRGBA535_g1366 );
			float Smoothness_Output558_g1366 = ( tex2DNode535_g1366.a * _SmoothnessIntensity );
			o.Smoothness = Smoothness_Output558_g1366;
			float AoMapBase538_g1366 = tex2DNode535_g1366.g;
			float AmbientOcclusion_Output582_g1366 = ( pow( AoMapBase538_g1366 , _AmbientOcclusionIntensity ) * ( 1.5 / ( ( saturate( TobyTranslucency526_g1366 ) * TranslucencyIntensity616_g1366 ) + 1.5 ) ) );
			o.Occlusion = AmbientOcclusion_Output582_g1366;
			o.Alpha = 1;
			float2 uv_AlbedoMap555_g1366 = i.uv_texcoord;
			float Opacity_Output559_g1366 = tex2D( _AlbedoMap, uv_AlbedoMap555_g1366 ).a;
			clip( Opacity_Output559_g1366 - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecular keepalpha fullforwardshadows dithercrossfade vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19303
Node;AmplifyShaderEditor.FunctionNode;2854;954.8434,125.1689;Inherit;False;(TTFE) Tree Foliage_Shading;1;;1366;32f9493bbb6c2d44ab3d59bde623860f;0;0;7;COLOR;152;FLOAT3;153;FLOAT;24;FLOAT;27;FLOAT;25;FLOAT;26;FLOAT4;28
Node;AmplifyShaderEditor.FunctionNode;2862;928,416;Inherit;False;(TTFE) Tree Foliage_Wind System;26;;1382;ccec0b38fced125459cc01da4402fa7a;0;0;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1338.528,120.0737;Float;False;True;-1;2;;0;0;StandardSpecular;Toby Fredson/The Toby Foliage Engine/(TTFE) Tree Foliage;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Masked;0.4;True;True;0;False;TransparentCutout;;AlphaTest;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;0;2854;152
WireConnection;0;1;2854;153
WireConnection;0;3;2854;24
WireConnection;0;4;2854;27
WireConnection;0;5;2854;25
WireConnection;0;10;2854;26
WireConnection;0;11;2862;0
WireConnection;0;12;2854;28
ASEEND*/
//CHKSM=901D25522796FD267422F718233F72833475E7DD