// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_HealingPodBarrier"
{
	Properties
	{
		_Noise("Noise", 2D) = "white" {}
		_NoiseB("NoiseB", 2D) = "white" {}
		_Speeds("Speeds", Vector) = (0,0,0,0)
		_DisplacementIntensity("DisplacementIntensity", Float) = 0.1
		[HDR]_ColorA("ColorA", Color) = (0,0,0,0)
		[HDR]_ColorB("ColorB", Color) = (0,0,0,0)
		_MinMaxSmoothStep("MinMaxSmoothStep", Vector) = (0,0,0,0)
		_WorldNoiseTiling("World Noise Tiling", Vector) = (1,1,1,0)
		_WorldNoiseSpeeds("World Noise Speeds", Vector) = (1,1,1,0)
		_WorldNoiseIntensity("WorldNoiseIntensity", Float) = 0.1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float3 worldNormal;
		};

		uniform float3 _WorldNoiseTiling;
		uniform float3 _WorldNoiseSpeeds;
		uniform float _WorldNoiseIntensity;
		uniform float4 _ColorA;
		uniform float4 _ColorB;
		uniform sampler2D _Noise;
		uniform sampler2D _NoiseB;
		uniform float4 _Speeds;
		uniform float4 _NoiseB_ST;
		uniform float _DisplacementIntensity;
		uniform float4 _Noise_ST;
		uniform float2 _MinMaxSmoothStep;


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 temp_output_12_0_g14 = _WorldNoiseTiling;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 temp_output_2_0_g14 = ( temp_output_12_0_g14 * ase_worldPos );
			float simplePerlin3D5_g14 = snoise( ( temp_output_2_0_g14 + ( ( temp_output_12_0_g14 * _WorldNoiseSpeeds ) * _Time.y ) ) );
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( saturate( (0.0 + (simplePerlin3D5_g14 - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)) ) * _WorldNoiseIntensity * ase_vertexNormal );
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv0_NoiseB = i.uv_texcoord * _NoiseB_ST.xy + _NoiseB_ST.zw;
			float2 panner10 = ( 1.0 * _Time.y * (_Speeds).zw + uv0_NoiseB);
			float temp_output_20_0 = (-1.0 + (tex2D( _NoiseB, panner10 ).r - 0.0) * (1.0 - -1.0) / (1.0 - 0.0));
			float2 appendResult17 = (float2(temp_output_20_0 , -temp_output_20_0));
			float2 uv0_Noise = i.uv_texcoord * _Noise_ST.xy + _Noise_ST.zw;
			float2 panner2 = ( 1.0 * _Time.y * (_Speeds).xy + uv0_Noise);
			float4 tex2DNode1 = tex2D( _Noise, ( ( appendResult17 * _DisplacementIntensity ) + panner2 ) );
			float4 lerpResult13 = lerp( _ColorA , _ColorB , tex2DNode1.r);
			o.Emission = lerpResult13.rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float dotResult2_g12 = dot( ase_worldViewDir , ase_normWorldNormal );
			float temp_output_4_0_g12 = abs( dotResult2_g12 );
			float temp_output_5_0_g12 = ( temp_output_4_0_g12 * temp_output_4_0_g12 * temp_output_4_0_g12 * temp_output_4_0_g12 * temp_output_4_0_g12 );
			float temp_output_7_0_g12 = 0.0;
			float temp_output_2_0_g13 = temp_output_7_0_g12;
			float temp_output_8_0_g12 = 0.005;
			float2 temp_output_31_0_g7 = i.uv_texcoord;
			float2 break3_g7 = temp_output_31_0_g7;
			float temp_output_2_0_g8 = 0.0;
			float temp_output_41_0_g7 = 0.0;
			float temp_output_2_0_g9 = 0.0;
			float temp_output_47_0_g7 = ( saturate( ( ( ( 1.0 - break3_g7.x ) - temp_output_2_0_g8 ) / ( temp_output_41_0_g7 - temp_output_2_0_g8 ) ) ) * saturate( ( ( break3_g7.x - temp_output_2_0_g9 ) / ( temp_output_41_0_g7 - temp_output_2_0_g9 ) ) ) );
			float temp_output_2_0_g10 = 0.0;
			float temp_output_42_0_g7 = 0.1;
			float temp_output_2_0_g11 = 0.0;
			float temp_output_51_0_g7 = ( saturate( ( ( ( 1.0 - break3_g7.y ) - temp_output_2_0_g10 ) / ( temp_output_42_0_g7 - temp_output_2_0_g10 ) ) ) * saturate( ( ( break3_g7.y - temp_output_2_0_g11 ) / ( temp_output_42_0_g7 - temp_output_2_0_g11 ) ) ) );
			float smoothstepResult24 = smoothstep( _MinMaxSmoothStep.x , _MinMaxSmoothStep.y , tex2DNode1.r);
			o.Alpha = ( saturate( ( ( temp_output_5_0_g12 - temp_output_2_0_g13 ) / ( temp_output_8_0_g12 - temp_output_2_0_g13 ) ) ) * ( temp_output_47_0_g7 * temp_output_51_0_g7 ) * smoothstepResult24 );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17402
527;73;957;695;1248.925;454.3044;1.473178;True;False
Node;AmplifyShaderEditor.Vector4Node;5;-2281.879,-222.8821;Inherit;False;Property;_Speeds;Speeds;3;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;7;-2294.311,-448.4154;Inherit;True;Property;_NoiseB;NoiseB;2;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-2054.57,-361.3987;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;11;-2045.692,-160.7273;Inherit;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;10;-1782.865,-334.7609;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;8;-1578.642,-437.7604;Inherit;True;Property;_SySKRangedUpwardsParticlesTexture1;SySKRangedUpwardsParticlesTexture;0;0;Create;True;0;0;False;0;-1;26d6150eff26e144f8048740394d381a;26d6150eff26e144f8048740394d381a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;20;-1222.359,-339.7677;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;16;-1320.836,-129.3059;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;4;-2352.764,23.642;Inherit;True;Property;_Noise;Noise;1;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.ComponentMaskNode;6;-2006.623,-249.52;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;17;-1157.232,-91.20916;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-1566.855,-240.7504;Inherit;False;Property;_DisplacementIntensity;DisplacementIntensity;4;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-2116.036,39.97314;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-962.097,-233.6921;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;2;-1822.843,44.60567;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-869.2383,-17.78877;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;25;-464.3704,223.5206;Inherit;False;Property;_MinMaxSmoothStep;MinMaxSmoothStep;7;0;Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector3Node;31;-765.8793,812.6735;Inherit;False;Property;_WorldNoiseSpeeds;World Noise Speeds;9;0;Create;True;0;0;False;0;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;29;-774.6779,646.7684;Inherit;False;Property;_WorldNoiseTiling;World Noise Tiling;8;0;Create;True;0;0;False;0;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;1;-509.1506,-71.1208;Inherit;True;Property;_SySKRangedUpwardsParticlesTexture;SySKRangedUpwardsParticlesTexture;0;0;Create;True;0;0;False;0;-1;26d6150eff26e144f8048740394d381a;26d6150eff26e144f8048740394d381a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;15;-1069.983,-483.331;Inherit;False;Property;_ColorB;ColorB;6;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;22;-434.0935,417.9478;Inherit;True;StaticUVMask;-1;;7;82ccbee0cc092f2489ba46deb939d560;5,64,0,66,0,40,2,65,0,32,0;5;41;FLOAT;0;False;42;FLOAT;0.1;False;31;FLOAT2;0,0;False;33;FLOAT;0.86;False;34;FLOAT;1.71;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;27;-209.7938,81.13507;Inherit;False;QuickFresnelMask;-1;;12;ee8df21c6952d444aac02dca7d184b81;2,9,0,14,0;4;7;FLOAT;0;False;8;FLOAT;0.005;False;10;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;28;-471.0936,719.399;Inherit;False;WorldNoise;-1;;14;c34574f372ee14746be8cac9cd8d06ff;2,16,0,22,0;3;11;FLOAT3;0,0,0;False;12;FLOAT3;1,0.5,1;False;13;FLOAT3;1,0.25,-0.8;False;4;FLOAT;0;FLOAT;20;FLOAT;14;FLOAT3;17
Node;AmplifyShaderEditor.RangedFloatNode;30;-382.5382,927.0475;Inherit;False;Property;_WorldNoiseIntensity;WorldNoiseIntensity;10;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;32;-309.0067,998.1391;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;24;-241.1974,223.8109;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;14;-1073.329,-660.6658;Inherit;False;Property;_ColorA;ColorA;5;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.IntNode;34;-1601.622,-40.4967;Inherit;False;Constant;_Int0;Int 0;11;0;Create;True;0;0;False;0;0;0;0;1;INT;0
Node;AmplifyShaderEditor.LerpOp;13;-93.75616,-309.7607;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;33.33291,57.37837;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-41.22736,839.3398;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;225.0674,-61.53864;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;S_HealingPodBarrier;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;2;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;False;Custom;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;2;7;0
WireConnection;11;0;5;0
WireConnection;10;0;9;0
WireConnection;10;2;11;0
WireConnection;8;0;7;0
WireConnection;8;1;10;0
WireConnection;20;0;8;1
WireConnection;16;0;20;0
WireConnection;6;0;5;0
WireConnection;17;0;20;0
WireConnection;17;1;16;0
WireConnection;3;2;4;0
WireConnection;18;0;17;0
WireConnection;18;1;12;0
WireConnection;2;0;3;0
WireConnection;2;2;6;0
WireConnection;19;0;18;0
WireConnection;19;1;2;0
WireConnection;1;0;4;0
WireConnection;1;1;19;0
WireConnection;28;12;29;0
WireConnection;28;13;31;0
WireConnection;24;0;1;1
WireConnection;24;1;25;1
WireConnection;24;2;25;2
WireConnection;13;0;14;0
WireConnection;13;1;15;0
WireConnection;13;2;1;1
WireConnection;23;0;27;0
WireConnection;23;1;22;0
WireConnection;23;2;24;0
WireConnection;33;0;28;20
WireConnection;33;1;30;0
WireConnection;33;2;32;0
WireConnection;0;2;13;0
WireConnection;0;9;23;0
WireConnection;0;11;33;0
ASEEND*/
//CHKSM=A34B69A0352035E9C3714A3A7C966D56FED8FFBE