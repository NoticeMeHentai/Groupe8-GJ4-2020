// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ToonTest"
{
	Properties
	{
		_index("index", Int) = 0
		_GradientTex("GradientTex", 2D) = "white" {}

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
			#include "Assets/Shaders/ToonFunctions.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float3 ase_normal : NORMAL;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
			};

			uniform int _index;
			uniform sampler2D _GradientTex;
			float Sampler39( int index , float3 worldPos , float3 worldNormal , sampler2D tex , out float outAttenuation )
			{
				return SampleGradient(tex, index, worldPos, worldNormal, outAttenuation);
			}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.ase_texcoord.xyz = ase_worldPos;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord1.xyz = ase_worldNormal;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.w = 0;
				o.ase_texcoord1.w = 0;
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
				int index39 = _index;
				float3 ase_worldPos = i.ase_texcoord.xyz;
				float3 worldPos39 = ase_worldPos;
				float3 ase_worldNormal = i.ase_texcoord1.xyz;
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 worldNormal39 = normalizedWorldNormal;
				sampler2D tex39 = _GradientTex;
				float outAttenuation39 = 0.0;
				float localSampler39 = Sampler39( index39 , worldPos39 , worldNormal39 , tex39 , outAttenuation39 );
				float4 temp_cast_0 = (( 1.0 - outAttenuation39 )).xxxx;
				
				
				finalColor = temp_cast_0;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=17402
201;73;1287;695;605.7588;1503.065;1.666896;True;False
Node;AmplifyShaderEditor.CommentaryNode;42;-756.8623,-1018.192;Inherit;False;1345.193;388.7678;Comment;6;39;14;31;9;40;41;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;14;-437.9018,-951.2576;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.IntNode;31;-272.4893,-968.1917;Inherit;False;Property;_index;index;2;0;Create;True;0;0;False;0;0;0;0;1;INT;0
Node;AmplifyShaderEditor.WorldNormalVector;9;-229.3327,-885.0087;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TexturePropertyNode;40;-706.8623,-859.4238;Inherit;True;Property;_GradientTex;GradientTex;3;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.CustomExpressionNode;39;122.683,-917.8645;Inherit;False;return SampleGradient(tex, index, worldPos, worldNormal, outAttenuation)@;1;False;5;True;index;INT;0;In;;Float;False;True;worldPos;FLOAT3;0,0,0;In;;Float;False;True;worldNormal;FLOAT3;0,0,0;In;;Float;False;True;tex;SAMPLER2D;;In;;Float;False;True;outAttenuation;FLOAT;0;Out;;Float;False;Sampler;True;False;0;5;0;INT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;SAMPLER2D;;False;4;FLOAT;0;False;2;FLOAT;0;FLOAT;5
Node;AmplifyShaderEditor.RegisterLocalVarNode;41;416.793,-977.2097;Inherit;False;GradientSample;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;946.5514,-1237.668;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;47;691.4224,-1840.555;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;45;285.7054,-1213.083;Inherit;False;Property;_Unlit;Unlit;1;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CustomExpressionNode;50;127.6754,-761.2953;Inherit;False;return GetLightColor(index);1;False;5;True;index;INT;0;In;;Float;False;True;worldPos;FLOAT3;0,0,0;In;;Float;False;True;worldNormal;FLOAT3;0,0,0;In;;Float;False;True;tex;SAMPLER2D;;In;;Float;False;True;outAttenuation;FLOAT;0;Out;;Float;False;Sampler;True;False;0;5;0;INT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;SAMPLER2D;;False;4;FLOAT;0;False;2;FLOAT;0;FLOAT;5
Node;AmplifyShaderEditor.ColorNode;44;278.6268,-1407.745;Inherit;False;Property;_Lit;Lit;0;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;48;422.3368,-1727.83;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;46;197.534,-1858.727;Inherit;True;Property;_Albedo;Albedo;4;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.LerpOp;43;661.0796,-1130.813;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;51;679.418,-876.3118;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;5;1222.885,-1120.438;Float;False;True;-1;2;ASEMaterialInspector;100;1;ToonTest;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;False;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;2;Include;;False;;Native;Include;;True;9f3600547644cf349bf5bff84d72a216;Custom;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;0
WireConnection;39;0;31;0
WireConnection;39;1;14;0
WireConnection;39;2;9;0
WireConnection;39;3;40;0
WireConnection;41;0;39;0
WireConnection;49;0;47;0
WireConnection;49;1;43;0
WireConnection;47;0;46;0
WireConnection;47;1;48;0
WireConnection;48;2;46;0
WireConnection;43;0;44;0
WireConnection;43;1;45;0
WireConnection;43;2;41;0
WireConnection;51;0;39;5
WireConnection;5;0;51;0
ASEEND*/
//CHKSM=485B7100AAF5817719FEB275652EAE2B67402E80