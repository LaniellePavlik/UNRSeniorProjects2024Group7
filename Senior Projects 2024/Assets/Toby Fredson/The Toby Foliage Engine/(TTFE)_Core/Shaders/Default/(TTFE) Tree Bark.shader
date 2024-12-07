// Made with Amplify Shader Editor v1.9.3.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Toby Fredson/The Toby Foliage Engine/(TTFE) Tree Bark"
{
	Properties
	{
		[Header(__________(TTFE) TREE BARK SHADER___________)][Header(_____________________________________________________)][Header(Texture Maps)][NoScaleOffset]_AlbedoMap("Albedo Map", 2D) = "white" {}
		[NoScaleOffset][Normal]_NormalMap("Normal Map", 2D) = "bump" {}
		[NoScaleOffset]_MaskMap("Mask Map", 2D) = "white" {}
		[Header(_____________________________________________________)][Header(Texture Settings)][Header((Tiling and Offset))]_Tiling("Tiling", Vector) = (1,1,0,0)
		_Offset("Offset", Vector) = (1,1,0,0)
		[Header((Normal))]_NormalIntensity("Normal Intensity", Range( -3 , 3)) = 1
		[Header((Smoothness))]_Smoothnessintensity("Smoothness intensity", Range( 0 , 1)) = 1
		[Header((Ambient Occlusion))]_Aointensity("Ao intensity", Range( 0 , 1)) = 1
		[Header(_____________________________________________________)][Header(Wind Settings)][Header((Global Wind Settings))]_GlobalWindStrength("Global Wind Strength", Range( 0 , 1)) = 1
		[KeywordEnum(GentleBreeze,WindOff)] _WindType("Wind Type", Float) = 0
		[Header((Trunk and Branch))]_BranchWindLarge("Branch Wind Large", Range( 0 , 20)) = 1
		_BranchWindSmall("Branch Wind Small", Range( 0 , 20)) = 1
		[Header((Wind Mask))]_Radius("Radius", Float) = 1
		_Hardness("Hardness", Float) = 1
		[Toggle]_CenterofMass("Center of Mass", Float) = 0
		[Toggle]_PivotSway("Pivot Sway", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "DisableBatching" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _WINDTYPE_GENTLEBREEZE _WINDTYPE_WINDOFF
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows dithercrossfade vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float _GlobalWindStrength;
		uniform float _Radius;
		uniform float _Hardness;
		uniform float _BranchWindLarge;
		uniform float _CenterofMass;
		uniform float _BranchWindSmall;
		uniform float _PivotSway;
		uniform sampler2D _NormalMap;
		uniform float2 _Tiling;
		uniform float2 _Offset;
		uniform float _NormalIntensity;
		uniform sampler2D _AlbedoMap;
		uniform sampler2D _MaskMap;
		uniform float _Smoothnessintensity;
		uniform float _Aointensity;


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


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 appendResult939_g236 = (float3(0.0 , 0.0 , saturate( ase_vertex3Pos ).z));
			float3 break989_g236 = ase_vertex3Pos;
			float3 appendResult938_g236 = (float3(break989_g236.x , ( break989_g236.y * 0.15 ) , 0.0));
			float mulTime975_g236 = _Time.y * 2.1;
			float3 temp_output_624_0_g236 = ( ( ase_vertex3Pos - float3(0,-1,0) ) / _Radius );
			float dotResult625_g236 = dot( temp_output_624_0_g236 , temp_output_624_0_g236 );
			float temp_output_628_0_g236 = pow( saturate( dotResult625_g236 ) , _Hardness );
			float SphearicalMaskCM763_g236 = saturate( temp_output_628_0_g236 );
			float3 temp_cast_0 = (ase_vertex3Pos.y).xxx;
			float2 appendResult928_g236 = (float2(ase_vertex3Pos.x , ase_vertex3Pos.z));
			float3 temp_output_996_0_g236 = ( cross( temp_cast_0 , float3( appendResult928_g236 ,  0.0 ) ) * 0.005 );
			float3 appendResult931_g236 = (float3(0.0 , ase_vertex3Pos.y , 0.0));
			float3 break971_g236 = ase_vertex3Pos;
			float3 appendResult967_g236 = (float3(break971_g236.x , 0.0 , ( break971_g236.z * 0.15 )));
			float mulTime976_g236 = _Time.y * 2.3;
			float dotResult849_g236 = dot( (ase_vertex3Pos*0.02 + 0.0) , ase_vertex3Pos );
			float CenterOfMassThicknessMask854_g236 = saturate( dotResult849_g236 );
			float3 appendResult981_g236 = (float3(ase_vertex3Pos.x , 0.0 , 0.0));
			float3 break984_g236 = ase_vertex3Pos;
			float3 appendResult966_g236 = (float3(0.0 , ( break984_g236.y * 0.2 ) , ( break984_g236.z * 0.4 )));
			float mulTime977_g236 = _Time.y * 2.0;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 normalizeResult765_g236 = normalize( ase_worldPos );
			float mulTime772_g236 = _Time.y * 0.25;
			float simplePerlin2D769_g236 = snoise( ( normalizeResult765_g236 + mulTime772_g236 ).xy*0.43 );
			float WindMask_LargeB770_g236 = ( simplePerlin2D769_g236 * 1.5 );
			float3 normalizeResult1092_g236 = normalize( ase_vertex3Pos );
			float CenterOfMassTrunkUP_C1098_g236 = saturate( distance( normalizeResult1092_g236 , float3(0,1,0) ) );
			float3 normalizeResult774_g236 = normalize( ase_worldPos );
			float mulTime780_g236 = _Time.y * 0.26;
			float simplePerlin2D778_g236 = snoise( ( normalizeResult774_g236 + mulTime780_g236 ).xy*0.7 );
			float WindMask_LargeC779_g236 = ( simplePerlin2D778_g236 * 1.5 );
			float mulTime906_g236 = _Time.y * 3.2;
			float3 worldToObj907_g236 = mul( unity_WorldToObject, float4( ase_vertex3Pos, 1 ) ).xyz;
			float3 temp_output_872_0_g236 = ( mulTime906_g236 + ( 0.02 * worldToObj907_g236.x ) + ( worldToObj907_g236.y * 0.14 ) + ( worldToObj907_g236.z * 0.16 ) + float3(0.4,0.3,0.1) );
			float3 normalizeResult632_g236 = normalize( ase_vertex3Pos );
			float CenterOfMassTrunkUP636_g236 = saturate( (distance( normalizeResult632_g236 , float3(0,1,0) )*1.0 + -0.05) );
			float3 ase_objectScale = float3( length( unity_ObjectToWorld[ 0 ].xyz ), length( unity_ObjectToWorld[ 1 ].xyz ), length( unity_ObjectToWorld[ 2 ].xyz ) );
			float mulTime905_g236 = _Time.y * 2.3;
			float3 worldToObj908_g236 = mul( unity_WorldToObject, float4( ase_vertex3Pos, 1 ) ).xyz;
			float3 temp_output_866_0_g236 = ( mulTime905_g236 + ( 0.2 * worldToObj908_g236 ) + float3(0.4,0.3,0.1) );
			float mulTime904_g236 = _Time.y * 3.6;
			float3 temp_cast_4 = (ase_vertex3Pos.x).xxx;
			float3 worldToObj910_g236 = mul( unity_WorldToObject, float4( temp_cast_4, 1 ) ).xyz;
			float temp_output_898_0_g236 = ( mulTime904_g236 + ( 0.2 * worldToObj910_g236.x ) );
			float3 normalizeResult697_g236 = normalize( ase_vertex3Pos );
			float CenterOfMass701_g236 = saturate( (distance( normalizeResult697_g236 , float3(0,1,0) )*2.0 + 0.0) );
			float SphericalMaskProxySphere704_g236 = (( _CenterofMass )?( ( temp_output_628_0_g236 * CenterOfMass701_g236 ) ):( temp_output_628_0_g236 ));
			float3 worldToObj1131_g236 = mul( unity_WorldToObject, float4( ase_vertex3Pos, 1 ) ).xyz;
			float mulTime1138_g236 = _Time.y * 4.0;
			float mulTime1129_g236 = _Time.y * 0.2;
			float2 appendResult1126_g236 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 normalizeResult1128_g236 = normalize( appendResult1126_g236 );
			float simpleNoise1139_g236 = SimpleNoise( ( mulTime1129_g236 + normalizeResult1128_g236 )*1.0 );
			float WindMask_SimpleSway1145_g236 = ( simpleNoise1139_g236 * 1.5 );
			float3 rotatedValue1151_g236 = RotateAroundAxis( float3( 0,0,0 ), ase_vertex3Pos, normalize( float3(0.6,1,0.1) ), ( ( cos( ( ( worldToObj1131_g236 * 0.02 ) + mulTime1138_g236 + ( float3(0.6,1,0.8) * 0.3 * worldToObj1131_g236 ) ) ) * 0.1 ) * WindMask_SimpleSway1145_g236 * saturate( ase_objectScale ) ).x );
			float3 temp_cast_6 = (0.0).xxx;
			#if defined(_WINDTYPE_GENTLEBREEZE)
				float3 staticSwitch1044_g236 = ( ( ( ( ( ( ( ( ( appendResult939_g236 + ( appendResult938_g236 * cos( mulTime975_g236 ) ) + ( cross( float3(1.2,0.6,1) , ( appendResult938_g236 * float3(0.7,1,0.8) ) ) * sin( mulTime975_g236 ) ) ) * SphearicalMaskCM763_g236 * temp_output_996_0_g236 ) * 0.08 ) + ( ( ( appendResult931_g236 + ( appendResult967_g236 * cos( mulTime976_g236 ) ) + ( cross( float3(0.9,1,1.2) , ( appendResult967_g236 * float3(1,1,1) ) ) * sin( mulTime976_g236 ) ) ) * SphearicalMaskCM763_g236 * CenterOfMassThicknessMask854_g236 * temp_output_996_0_g236 ) * 0.1 ) + ( ( ( appendResult981_g236 + ( appendResult966_g236 * cos( mulTime977_g236 ) ) + ( cross( float3(1.1,1.3,0.8) , ( appendResult966_g236 * float3(1.4,0.8,1.1) ) ) * sin( mulTime977_g236 ) ) ) * SphearicalMaskCM763_g236 * temp_output_996_0_g236 ) * 0.05 ) ) * _BranchWindLarge ) * WindMask_LargeB770_g236 ) * CenterOfMassTrunkUP_C1098_g236 ) + ( ( ( WindMask_LargeC779_g236 * ( ( ( ( cos( temp_output_872_0_g236 ) * sin( temp_output_872_0_g236 ) * CenterOfMassTrunkUP636_g236 * SphearicalMaskCM763_g236 * CenterOfMassThicknessMask854_g236 * saturate( ase_objectScale ) ) * 0.2 ) + ( ( cos( temp_output_866_0_g236 ) * sin( temp_output_866_0_g236 ) * CenterOfMassTrunkUP636_g236 * CenterOfMassThicknessMask854_g236 * SphearicalMaskCM763_g236 * saturate( ase_objectScale ) ) * 0.2 ) + ( ( sin( temp_output_898_0_g236 ) * cos( temp_output_898_0_g236 ) * SphericalMaskProxySphere704_g236 * CenterOfMassThicknessMask854_g236 * CenterOfMassTrunkUP636_g236 ) * 0.2 ) ) * _BranchWindSmall ) ) * 0.3 ) * CenterOfMassTrunkUP_C1098_g236 ) + (( _PivotSway )?( ( ( rotatedValue1151_g236 - ase_vertex3Pos ) * 0.4 ) ):( float3( 0,0,0 ) )) ) * saturate( ase_vertex3Pos.y ) );
			#elif defined(_WINDTYPE_WINDOFF)
				float3 staticSwitch1044_g236 = temp_cast_6;
			#else
				float3 staticSwitch1044_g236 = ( ( ( ( ( ( ( ( ( appendResult939_g236 + ( appendResult938_g236 * cos( mulTime975_g236 ) ) + ( cross( float3(1.2,0.6,1) , ( appendResult938_g236 * float3(0.7,1,0.8) ) ) * sin( mulTime975_g236 ) ) ) * SphearicalMaskCM763_g236 * temp_output_996_0_g236 ) * 0.08 ) + ( ( ( appendResult931_g236 + ( appendResult967_g236 * cos( mulTime976_g236 ) ) + ( cross( float3(0.9,1,1.2) , ( appendResult967_g236 * float3(1,1,1) ) ) * sin( mulTime976_g236 ) ) ) * SphearicalMaskCM763_g236 * CenterOfMassThicknessMask854_g236 * temp_output_996_0_g236 ) * 0.1 ) + ( ( ( appendResult981_g236 + ( appendResult966_g236 * cos( mulTime977_g236 ) ) + ( cross( float3(1.1,1.3,0.8) , ( appendResult966_g236 * float3(1.4,0.8,1.1) ) ) * sin( mulTime977_g236 ) ) ) * SphearicalMaskCM763_g236 * temp_output_996_0_g236 ) * 0.05 ) ) * _BranchWindLarge ) * WindMask_LargeB770_g236 ) * CenterOfMassTrunkUP_C1098_g236 ) + ( ( ( WindMask_LargeC779_g236 * ( ( ( ( cos( temp_output_872_0_g236 ) * sin( temp_output_872_0_g236 ) * CenterOfMassTrunkUP636_g236 * SphearicalMaskCM763_g236 * CenterOfMassThicknessMask854_g236 * saturate( ase_objectScale ) ) * 0.2 ) + ( ( cos( temp_output_866_0_g236 ) * sin( temp_output_866_0_g236 ) * CenterOfMassTrunkUP636_g236 * CenterOfMassThicknessMask854_g236 * SphearicalMaskCM763_g236 * saturate( ase_objectScale ) ) * 0.2 ) + ( ( sin( temp_output_898_0_g236 ) * cos( temp_output_898_0_g236 ) * SphericalMaskProxySphere704_g236 * CenterOfMassThicknessMask854_g236 * CenterOfMassTrunkUP636_g236 ) * 0.2 ) ) * _BranchWindSmall ) ) * 0.3 ) * CenterOfMassTrunkUP_C1098_g236 ) + (( _PivotSway )?( ( ( rotatedValue1151_g236 - ase_vertex3Pos ) * 0.4 ) ):( float3( 0,0,0 ) )) ) * saturate( ase_vertex3Pos.y ) );
			#endif
			float3 FinalWind_Output1060_g236 = ( _GlobalWindStrength * staticSwitch1044_g236 );
			v.vertex.xyz += FinalWind_Output1060_g236;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord18_g226 = i.uv_texcoord * _Tiling + _Offset;
			o.Normal = UnpackScaleNormal( tex2D( _NormalMap, uv_TexCoord18_g226 ), _NormalIntensity );
			o.Albedo = tex2D( _AlbedoMap, uv_TexCoord18_g226 ).rgb;
			float4 tex2DNode11_g226 = tex2D( _MaskMap, uv_TexCoord18_g226 );
			o.Smoothness = ( tex2DNode11_g226.a * _Smoothnessintensity );
			o.Occlusion = pow( tex2DNode11_g226.g , _Aointensity );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19303
Node;AmplifyShaderEditor.FunctionNode;2681;-249.7405,77.25287;Inherit;False;(TTFE) Tree Bark_Shading;0;;226;07105d68e42fa94479944563340e838c;0;0;4;COLOR;0;FLOAT3;12;FLOAT;13;FLOAT;14
Node;AmplifyShaderEditor.FunctionNode;2691;-265.5672,239.9212;Inherit;False;(TTFE) Tree Bark_Wind System;9;;236;58360699feb112c40b86ba9ba75062e6;0;0;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;73.45525,20.03703;Float;False;True;-1;2;;0;0;Standard;Toby Fredson/The Toby Foliage Engine/(TTFE) Tree Bark;False;False;False;False;False;False;False;False;False;False;False;False;True;True;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;0;2681;0
WireConnection;0;1;2681;12
WireConnection;0;4;2681;13
WireConnection;0;5;2681;14
WireConnection;0;11;2691;0
ASEEND*/
//CHKSM=7A0C4700C40E142F85FB42AD24289671CD0CA380