			float3 GetLightPos( int index )
			{
				return float3(unity_4LightPosX0[index],unity_4LightPosY0[index],unity_4LightPosZ0[index]);
			}
			
			float3 GetLightDir( int index , float3 worldPos )
			{
				return normalize(worldPos - GetLightPos(index));
			}
			
			float3 GetLightningGradient( int index , float3 worldPos , float3 worldNormal )
			{
				return dot(worldNormal,GetLightDir(index, worldPos));
			}
			
			float4 GetLightColor(int index)
			{
				return float4(unity_LightColor[index].xyz, unity_4LightAtten0[index].x);
			}
			
			float Remap(float value, float minOld, float maxOld, float minNew, float maxNew)
			{
				return ((minNew + (value - minOld)*(maxNew - minNew)/(maxOld - minOld)));
			}
			
			float SampleGradient(sampler2D tex, int index, float3 worldPos, float3 worldNormal, out float attenuation)
			{
				float xVal = GetLightningGradient(index, worldPos, worldNormal);
				xVal = Remap(xVal,-1,1,0.01,0.99);
				
				float4 color = GetLightColor(index);
				attenuation = color.w;
				
				return tex2D(tex, float2(xVal, color.w)).r;
			}

			
			

				