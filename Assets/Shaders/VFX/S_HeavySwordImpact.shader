// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_HeavySwordImpact"
{
	Properties
	{
		_Value("Value", Range( 0 , 1)) = 0
		_NoiseTex("NoiseTex", 2D) = "white" {}
		[Enum(R,0,G,1,B,2,A,3)]_NoiseChannel("NoiseChannel", Int) = 0
		_MipMap("MipMap", Float) = 8.81
		_IsolateSize("IsolateSize", Range( 0 , 0.15)) = 0.06470589
		_MinMaxNoiseIntensity("MinMaxNoiseIntensity", Vector) = (0,0,0,0)
		_IsolateGradient("IsolateGradient", Range( 0 , 2)) = 0.05882353
		_MinMaxLerp("MinMaxLerp", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		ZWrite Off
		Blend OneMinusDstColor One
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float2 _MinMaxLerp;
		uniform float _Value;
		uniform float _IsolateSize;
		uniform float _IsolateGradient;
		uniform int _NoiseChannel;
		uniform sampler2D _NoiseTex;
		uniform float4 _NoiseTex_ST;
		uniform float _MipMap;
		uniform float2 _MinMaxNoiseIntensity;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float Value49 = ( _Value * _Value * _Value * _Value * _Value );
			float lerpResult62 = lerp( _MinMaxLerp.x , _MinMaxLerp.y , Value49);
			float temp_output_6_0_g8 = ( 1.0 - lerpResult62 );
			float temp_output_43_0 = ( _IsolateSize + ( _IsolateSize * _IsolateGradient ) );
			float temp_output_3_0_g8 = temp_output_43_0;
			float temp_output_12_0_g8 = _IsolateSize;
			float2 temp_output_18_0 = ( i.uv_texcoord - float2( 0.5,0.5 ) );
			int temp_output_4_0_g7 = _NoiseChannel;
			float2 uv0_NoiseTex = i.uv_texcoord * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
			float2 break4_g5 = temp_output_18_0;
			float temp_output_4_0_g6 = ( ( 2.0 * UNITY_PI ) * frac( (0.0 + (atan2( break4_g5.x , break4_g5.y ) - -UNITY_PI) * (1.0 - 0.0) / (UNITY_PI - -UNITY_PI)) ) );
			float2 appendResult8_g6 = (float2(sin( temp_output_4_0_g6 ) , cos( temp_output_4_0_g6 )));
			float4 tex2DNode4 = tex2Dlod( _NoiseTex, float4( ( uv0_NoiseTex + appendResult8_g6 ), 0, floor( _MipMap )) );
			float temp_output_7_0_g7 = (( (float)temp_output_4_0_g7 == 0.0 ) ? tex2DNode4.r :  tex2DNode4.g );
			float temp_output_12_0_g7 = (( (float)temp_output_4_0_g7 == 2.0 ) ? tex2DNode4.b :  temp_output_7_0_g7 );
			float lerpResult51 = lerp( _MinMaxNoiseIntensity.x , _MinMaxNoiseIntensity.y , Value49);
			float NoiseIntensity53 = lerpResult51;
			float temp_output_2_0_g8 = ( 1.0 - ( distance( ( temp_output_18_0 + ( (-1.0 + ((( (float)temp_output_4_0_g7 == 3.0 ) ? tex2DNode4.a :  temp_output_12_0_g7 ) - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) * NoiseIntensity53 ) ) , float2( 0,0 ) ) * 2.0 ) );
			float smoothstepResult9_g8 = smoothstep( ( temp_output_6_0_g8 + temp_output_3_0_g8 ) , ( temp_output_6_0_g8 + temp_output_12_0_g8 ) , temp_output_2_0_g8);
			float smoothstepResult8_g8 = smoothstep( ( temp_output_6_0_g8 - temp_output_12_0_g8 ) , ( temp_output_6_0_g8 - temp_output_3_0_g8 ) , temp_output_2_0_g8);
			float Debug32 = ( smoothstepResult9_g8 - smoothstepResult8_g8 );
			float3 temp_cast_3 = (Debug32).xxx;
			o.Emission = temp_cast_3;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17402
2455;0;1384;1059;1431.366;1167.52;1;True;False
Node;AmplifyShaderEditor.TexCoordVertexDataNode;6;-1510.401,-621.2661;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;18;-1291.575,-636.403;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;3;-894.1949,-447.3871;Inherit;True;Property;_NoiseTex;NoiseTex;2;0;Create;True;0;0;False;0;9826a89fc70098e4399e93e3ffe070c3;9826a89fc70098e4399e93e3ffe070c3;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.FunctionNode;25;-1043.942,-191.4128;Inherit;True;Get Angular Mask;-1;;5;e3a2b7905983df846be90230f16f470c;0;1;2;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1;-1043.926,-978.0367;Inherit;False;Property;_Value;Value;1;0;Create;True;0;0;False;0;0;0.75;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;46;-636.4275,-334.9664;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;-744.3657,-944.5203;Inherit;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-496.0751,-152.8522;Inherit;False;Property;_MipMap;MipMap;4;0;Create;True;0;0;False;0;8.81;6.29;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;28;-728.8297,-200.1952;Inherit;False;CoordinatesDirection;-1;;6;4087e5fe85268b148a4621477071c200;0;1;1;FLOAT;0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FloorOpNode;31;-333.1291,-169.2279;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;47;-392.1887,-267.3783;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;-596.9321,-953.3479;Inherit;False;Value;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;4;-181.4799,-284.3151;Inherit;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;MipLevel;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.IntNode;11;-107.8054,-383.0301;Inherit;False;Property;_NoiseChannel;NoiseChannel;3;1;[Enum];Create;True;4;R;0;G;1;B;2;A;3;0;False;0;0;2;0;1;INT;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-44.32373,112.1407;Inherit;False;49;Value;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;50;-31.97748,-63.79159;Inherit;False;Property;_MinMaxNoiseIntensity;MinMaxNoiseIntensity;7;0;Create;True;0;0;False;0;0,0;0.05,0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;51;258.1563,-43.72913;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;10;279.1349,-341.3383;Inherit;True;Switch;-1;;7;aee5c6d08ca784945b154b9b7d527020;1,9,2;5;4;INT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;53;501.7687,-14.10339;Inherit;False;NoiseIntensity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;12;583.6111,-289.4543;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;844.3681,-231.6448;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;24;1066.108,-696.3202;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;41;1815.265,-600.4191;Inherit;False;Property;_IsolateSize;IsolateSize;6;0;Create;True;0;0;False;0;0.06470589;0.0243;0;0.15;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;19;1294.729,-708.7845;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;66;2233.783,-1140.472;Inherit;False;Property;_MinMaxLerp;MinMaxLerp;9;0;Create;True;0;0;False;0;0,0;-0.05,0.7;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;56;1605.878,-892.3824;Inherit;False;49;Value;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;1775.265,-503.4191;Inherit;False;Property;_IsolateGradient;IsolateGradient;8;0;Create;True;0;0;False;0;0.05882353;0.66;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;2089.265,-538.4191;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;1513.301,-711.2851;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;62;2453.877,-913.3824;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;22;1653.819,-706.0535;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;2126.265,-473.4191;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;64;2502.877,-770.3824;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;29;2648.094,-702.6296;Inherit;True;Isolate Smooth;-1;;8;c841d60e2a8864df4894dbd824731159;0;4;2;FLOAT;0.5;False;6;FLOAT;0.5;False;12;FLOAT;0.1;False;3;FLOAT;0.15;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;32;2923.808,-737.3561;Inherit;False;Debug;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;61;2141.877,-976.3824;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;33;646.4339,493.4642;Inherit;False;32;Debug;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;1867.265,-740.4191;Inherit;False;Property;_IsolatePosition;IsolatePosition;5;0;Create;True;0;0;False;0;0.6705883;0.779;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;65;2173.877,-885.3824;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;55;1676.878,-1026.382;Inherit;False;53;NoiseIntensity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;58;1994.878,-971.3824;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;60;1875.878,-1030.382;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;2050.877,-878.3824;Inherit;False;3;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;861.3446,459.7837;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;S_HeavySwordImpact;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;2;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Custom;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;5;4;False;-1;1;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;18;0;6;0
WireConnection;25;2;18;0
WireConnection;46;2;3;0
WireConnection;67;0;1;0
WireConnection;67;1;1;0
WireConnection;67;2;1;0
WireConnection;67;3;1;0
WireConnection;67;4;1;0
WireConnection;28;1;25;0
WireConnection;31;0;30;0
WireConnection;47;0;46;0
WireConnection;47;1;28;0
WireConnection;49;0;67;0
WireConnection;4;0;3;0
WireConnection;4;1;47;0
WireConnection;4;2;31;0
WireConnection;51;0;50;1
WireConnection;51;1;50;2
WireConnection;51;2;52;0
WireConnection;10;4;11;0
WireConnection;10;2;4;1
WireConnection;10;3;4;2
WireConnection;10;5;4;3
WireConnection;10;6;4;4
WireConnection;53;0;51;0
WireConnection;12;0;10;0
WireConnection;14;0;12;0
WireConnection;14;1;53;0
WireConnection;24;0;18;0
WireConnection;24;1;14;0
WireConnection;19;0;24;0
WireConnection;44;0;41;0
WireConnection;44;1;42;0
WireConnection;21;0;19;0
WireConnection;62;0;66;1
WireConnection;62;1;66;2
WireConnection;62;2;56;0
WireConnection;22;0;21;0
WireConnection;43;0;41;0
WireConnection;43;1;44;0
WireConnection;64;0;62;0
WireConnection;29;2;22;0
WireConnection;29;6;64;0
WireConnection;29;12;41;0
WireConnection;29;3;43;0
WireConnection;32;0;29;0
WireConnection;61;0;58;0
WireConnection;61;1;43;0
WireConnection;65;1;63;0
WireConnection;58;0;60;0
WireConnection;60;0;55;0
WireConnection;60;1;55;0
WireConnection;63;0;55;0
WireConnection;63;1;55;0
WireConnection;63;2;43;0
WireConnection;0;2;33;0
ASEEND*/
//CHKSM=E6261D6230B856ECF3829AFAEE02731F597E5620