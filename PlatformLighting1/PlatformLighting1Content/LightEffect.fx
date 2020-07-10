float screenWidth;
float screenHeight;
float4 ambientColor;

float lightStrength;
float lightDecay;
float3 lightPosition;
float4 lightColor;
float lightRadius;
float specularStrength;

Texture NormalMap;
sampler NormalMapSampler = sampler_state {
	texture = <NormalMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

Texture ColorMap;
sampler ColorMapSampler = sampler_state {
	texture = <ColorMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

struct VertexToPixel
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
};

struct PixelToFrame
{
	float4 Color : COLOR0;
};

VertexToPixel MyVertexShader(float4 inPos: POSITION0, float2 texCoord: TEXCOORD0, float4 color: COLOR0)
{
	VertexToPixel Output = (VertexToPixel)0;
	
	Output.Position = inPos;
	Output.TexCoord = texCoord;
	Output.Color = color;
	
	return Output;
}

PixelToFrame PointLightShader(VertexToPixel PSIn) : COLOR0
{	
	PixelToFrame Output = (PixelToFrame)0;
	
	//The color map texture
	float4 colorMap = tex2D(ColorMapSampler, PSIn.TexCoord);	

	//The angle of the pixel based on the normal map interpretation
	float3 normal = (2.0f * (tex2D(NormalMapSampler, PSIn.TexCoord))) - 1.0f;
	
	//Current pixels' actual position	
	float3 pixelPosition;
	pixelPosition.x = screenWidth * PSIn.TexCoord.x;
	pixelPosition.y = screenHeight * PSIn.TexCoord.y;
	pixelPosition.z = 0;

	//Get the direction of the current pixel from the center of the light
	float3 lightDirection = lightPosition - pixelPosition;
	float3 lightDirNorm = normalize(lightDirection);
	float3 halfVec = float3(0, 0, 1);
	
	//Pretty sure this handles diffuse light	
	float lightSize = 1.25f;
	float amount = max(dot(normal, lightDirNorm), 0);
	float coneAttenuation = saturate(lightSize - length(lightDirection) / 250); 
	
	//Pretty sure this handles specular reflections			
	float3 reflect = normalize(2.0 * amount * normal - lightDirNorm);
	float specular = min(pow(saturate(dot(reflect, halfVec)), 0.5 * 255), amount); 
	//Multiply the "10" here by a specular map value to be able to change the specularity of each pixel
				
	Output.Color = colorMap * coneAttenuation * lightColor * lightStrength + (specular * coneAttenuation * specularStrength);

	//Output.Color = specMap;

	return Output;
}

technique DeferredPointLight
{
    pass Pass1
    {
		VertexShader = compile vs_2_0 MyVertexShader();
        PixelShader = compile ps_2_0 PointLightShader();
    }
}

