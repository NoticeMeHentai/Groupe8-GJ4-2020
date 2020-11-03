// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ToonTest"
{
	Properties
	{
		_LightIndex("LightIndex", Int) = 0
		_ToonGradient("ToonGradient", 2D) = "white" {}
		_Albedo("Albedo", 2D) = "white" {}
		[Enum(Use Texture,1,Do not use Texture ,0,OnlyTexture,2)]_TextureOrNot("TextureOrNot", Int) = 0
		_GeneralColor("GeneralColor", Color) = (0,0,0,0)
		_MinRemap("MinRemap", Range( 0 , 0.99)) = 0
		_MaxRemap("MaxRemap", Range( 0.01 , 1)) = 1
		_Desaturate("Desaturate", Float) = 0

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "Assets/Shaders/Toon/ToonFunctions.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord : TEXCOORD0;
				float3 ase_normal : NORMAL;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
			};

			uniform int _TextureOrNot;
			uniform float _Desaturate;
			uniform sampler2D _Albedo;
			uniform float4 _Albedo_ST;
			uniform float4 _GeneralColor;
			uniform int _LightIndex;
			uniform sampler2D _ToonGradient;
			uniform float _MinRemap;
			uniform float _MaxRemap;
			float Sampler39( int index , float3 worldPos , float3 worldNormal , sampler2D tex )
			{
				return SampleGradient(tex, index, worldPos, worldNormal);
			}
			
			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.ase_texcoord1.xyz = ase_worldPos;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord2.xyz = ase_worldNormal;
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				o.ase_texcoord1.w = 0;
				o.ase_texcoord2.w = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				float2 uv0_Albedo = i.ase_texcoord.xy * _Albedo_ST.xy + _Albedo_ST.zw;
				float4 tex2DNode47 = tex2D( _Albedo, uv0_Albedo );
				float3 desaturateInitialColor86 = tex2DNode47.rgb;
				float desaturateDot86 = dot( desaturateInitialColor86, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar86 = lerp( desaturateInitialColor86, desaturateDot86.xxx, 1.0 );
				float4 temp_output_85_0 = (( _Desaturate == 1.0 ) ? float4( desaturateVar86 , 0.0 ) :  tex2DNode47 );
				int index39 = _LightIndex;
				float3 ase_worldPos = i.ase_texcoord1.xyz;
				float3 worldPos39 = ase_worldPos;
				float3 ase_worldNormal = i.ase_texcoord2.xyz;
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 worldNormal39 = normalizedWorldNormal;
				sampler2D tex39 = _ToonGradient;
				float localSampler39 = Sampler39( index39 , worldPos39 , worldNormal39 , tex39 );
				float GradientSample41 = localSampler39;
				float temp_output_71_0 = (_MinRemap + (GradientSample41 - 0.01) * (_MaxRemap - _MinRemap) / (0.99 - 0.01));
				float4 lerpResult80 = lerp( ( _GeneralColor * temp_output_71_0 ) , temp_output_85_0 , _GeneralColor.a);
				float3 hsvTorgb76 = RGBToHSV( _GeneralColor.rgb );
				float3 hsvTorgb77 = HSVToRGB( float3(hsvTorgb76.x,hsvTorgb76.y,1.0) );
				
				
				finalColor = (( (float)_TextureOrNot == 2.0 ) ? temp_output_85_0 :  (( (float)_TextureOrNot == 1.0 ) ? lerpResult80 :  float4( ( hsvTorgb77 * temp_output_71_0 ) , 0.0 ) ) );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=17402
219;73;1423;583;-557.7206;2407.184;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;42;-756.8623,-1018.192;Inherit;False;1385.906;526.1724;Comment;6;41;39;40;9;31;14;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;14;-437.9018,-951.2576;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.IntNode;31;-45.16962,-974.9814;Inherit;False;Property;_LightIndex;LightIndex;2;0;Create;True;0;0;False;0;0;0;0;1;INT;0
Node;AmplifyShaderEditor.WorldNormalVector;9;-285.3123,-859.5635;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TexturePropertyNode;40;-657.4693,-916.9089;Inherit;True;Property;_ToonGradient;ToonGradient;3;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.CustomExpressionNode;39;115.6045,-960.3353;Inherit;False;return SampleGradient(tex, index, worldPos, worldNormal)@;1;False;4;True;index;INT;0;In;;Float;False;True;worldPos;FLOAT3;0,0,0;In;;Float;False;True;worldNormal;FLOAT3;0,0,0;In;;Float;False;True;tex;SAMPLER2D;;In;;Float;False;Sampler;True;False;0;4;0;INT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;SAMPLER2D;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;46;640.116,-1996.379;Inherit;True;Property;_Albedo;Albedo;4;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;41;403.3471,-967.4616;Inherit;False;GradientSample;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;48;864.9188,-1865.481;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;68;452.2881,-1572.067;Inherit;False;Property;_GeneralColor;GeneralColor;7;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;47;1134.005,-1978.206;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;82;857.0437,-1394.698;Inherit;False;41;GradientSample;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;69;425.4496,-1405.763;Inherit;False;Property;_MinRemap;MinRemap;8;0;Create;True;0;0;False;0;0;0;0;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;70;367.4087,-1326.027;Inherit;False;Property;_MaxRemap;MaxRemap;9;0;Create;True;0;0;False;0;1;0;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;972.226,-2350.196;Inherit;False;Property;_Desaturate;Desaturate;10;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;86;1512.571,-2023.685;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RGBToHSVNode;76;661.8892,-1558.795;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TFHCRemapNode;71;1090.408,-1377.627;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0.01;False;2;FLOAT;0.99;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;77;945.2889,-1543.194;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;1356.509,-1610.187;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCCompareEqual;85;1720.686,-1985.939;Inherit;False;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT3;0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.IntNode;62;1119.023,-1754.036;Inherit;False;Property;_TextureOrNot;TextureOrNot;6;1;[Enum];Create;True;3;Use Texture;1;Do not use Texture ;0;OnlyTexture;2;0;False;0;0;0;0;1;INT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;1395.372,-1407.378;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;80;1614.963,-1626.495;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCCompareEqual;67;1858.902,-1690.84;Inherit;False;4;0;INT;0;False;1;FLOAT;1;False;2;COLOR;0,0,0,0;False;3;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;59;1182.745,-1004.443;Inherit;False;Property;_Float0;Float 0;5;0;Create;True;0;0;False;0;0.5264549;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;53;670.9517,-794.1976;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;83;806.3364,-2507.669;Inherit;False;41;GradientSample;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;54;1499.195,-531.2364;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;57;1196.619,-910.1021;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;58;1418.597,-929.5251;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;56;999.6113,-930.7895;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCCompareEqual;79;2048.05,-1830.405;Inherit;False;4;0;INT;0;False;1;FLOAT;2;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DesaturateOpNode;64;942.0046,-2838.016;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;44;663.5573,-2866.776;Inherit;False;Property;_Lit;Lit;0;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;45;654.8002,-2686.631;Inherit;False;Property;_Unlit;Unlit;1;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCCompareEqual;63;1381.156,-2829.391;Inherit;False;4;0;INT;0;False;1;FLOAT;1;False;2;FLOAT3;0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;66;1159.746,-2745.641;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;43;1088.239,-2550.254;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldSpaceLightPos;55;1173.38,-422.6309;Inherit;False;0;3;FLOAT4;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.DesaturateOpNode;65;927.4887,-2715.289;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;1683.026,-2819.899;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;5;2312.135,-1833.107;Float;False;True;-1;2;ASEMaterialInspector;100;1;ToonTest;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;False;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;3;Include;;False;;Native;Include;UnityLightingCommon.cginc;False;;Custom;Include;;True;9f3600547644cf349bf5bff84d72a216;Custom;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;0
WireConnection;39;0;31;0
WireConnection;39;1;14;0
WireConnection;39;2;9;0
WireConnection;39;3;40;0
WireConnection;41;0;39;0
WireConnection;48;2;46;0
WireConnection;47;0;46;0
WireConnection;47;1;48;0
WireConnection;86;0;47;0
WireConnection;76;0;68;0
WireConnection;71;0;82;0
WireConnection;71;3;69;0
WireConnection;71;4;70;0
WireConnection;77;0;76;1
WireConnection;77;1;76;2
WireConnection;81;0;68;0
WireConnection;81;1;71;0
WireConnection;85;0;84;0
WireConnection;85;2;86;0
WireConnection;85;3;47;0
WireConnection;75;0;77;0
WireConnection;75;1;71;0
WireConnection;80;0;81;0
WireConnection;80;1;85;0
WireConnection;80;2;68;4
WireConnection;67;0;62;0
WireConnection;67;2;80;0
WireConnection;67;3;75;0
WireConnection;54;0;53;0
WireConnection;54;1;55;1
WireConnection;54;2;55;2
WireConnection;57;0;56;0
WireConnection;58;0;59;0
WireConnection;58;1;57;0
WireConnection;56;0;9;0
WireConnection;56;1;53;0
WireConnection;79;0;62;0
WireConnection;79;2;85;0
WireConnection;79;3;67;0
WireConnection;64;0;44;0
WireConnection;63;0;62;0
WireConnection;63;2;66;0
WireConnection;63;3;43;0
WireConnection;66;0;64;0
WireConnection;66;1;65;0
WireConnection;66;2;83;0
WireConnection;43;0;44;0
WireConnection;43;1;45;0
WireConnection;43;2;83;0
WireConnection;65;0;45;0
WireConnection;49;0;47;0
WireConnection;49;1;63;0
WireConnection;5;0;79;0
ASEEND*/
//CHKSM=C93F0807743B91F8A91B6E08A6106B7C73C4DA8C