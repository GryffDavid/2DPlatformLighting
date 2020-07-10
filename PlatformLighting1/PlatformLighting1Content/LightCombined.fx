float ambient;
float4 ambientColor;
float lightAmbient;

Texture ColorMap;
sampler ColorMapSampler = sampler_state 
{
	texture = <ColorMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

Texture ShadingMap;
sampler ShadingMapSampler = sampler_state 
{
	texture = <ShadingMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

Texture NormalMap;
sampler NormalMapSampler = sampler_state 
{
	texture = <NormalMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};


float2 iResolution = float2(1280, 720);
float3 halfVec = float3(0, 0, 1);
float2 offset = float2(0.5/1280.0, 0.5/720.0);
float specVal = 0.008;

float4 CombinedPixelShader(float4 color : COLOR0, float2 texCoords : TEXCOORD0) : COLOR0
{	
	float4 color2 = tex2D(ColorMapSampler, texCoords);
	float4 shading = tex2D(ShadingMapSampler, texCoords);
	float normal = tex2D(NormalMapSampler, texCoords).rgb;
	
	//Ambient Occlusion
	float4 normalCol = tex2D(NormalMapSampler, texCoords);	
	float2 p = (texCoords.xy) * iResolution.xy;
	float3 normal1 = (2.0f * (tex2D(NormalMapSampler, texCoords + offset))) - 1.0f;
	normal1 *= float3(1, -1, 1);

	float4 col = float4(0.025, 0.025, 0.025, 255);

	//Get the direction of the current pixel from the center of the light
	float3 lightDirNorm = normalize(float3(0.25, 0, 0) - float3(p.x, p.y, -25));	
	float amount = max(dot(normal1, lightDirNorm), 0);		
	float3 reflect = normalize(2.0 * amount * normal1 - lightDirNorm);
	float specular = min(pow(saturate(dot(reflect, halfVec)), specVal * 255), amount); 

	col += specular;

	if (normal > 0.0f)
	{
		//float4 finalColor = color2 * ambientColor;
		//finalColor += shading;
		//return finalColor;

		//Darker
		float4 finalColor = color2 * ambientColor * ambient;
		finalColor += (shading * color2) * lightAmbient;
		
		return finalColor + (col * 0.25f);
	}
	else
	{
		return float4(0, 0, 0, 0);
	}
}

technique DeferredCombined2
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 CombinedPixelShader();
    }
}