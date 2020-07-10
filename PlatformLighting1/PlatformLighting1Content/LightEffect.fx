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

Texture ShadowMap;
sampler ShadowMapSampler = sampler_state {
	texture = <ShadowMap>;
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



////////////////////
//Variables to use//
///////////////////

float2 iResolution = float2(1280, 720);
float4 backColor = float4(0.25, 0.25, 0.25, 1.0);
float4 shapeColor = float4(1.0, 0.4, 0.0, 1.0);

float3 LightPosition;
float4 LightColor, AmbientColor;
float LightPower, LightSize;

//////////////////
//Dist functions//
//////////////////

float circleDist(float2 p, float radius)
{
	return length(p) - radius;
}

//////////////////////////////
//Light and Shadow Functions//
//////////////////////////////

float4 drawLight(float2 p, float2 pos, float4 color, float dist, float range, float radius)
{
	// distance to light
	float ld = length(p - pos);
	
	// out of range
	if (ld > range) 
		return float4(0, 0, 0, 0);
	
	// shadow and falloff
	float fall = (range - ld)/range;
	fall *= fall;
    
	float source = clamp(-circleDist(p - pos, radius), 0.0, 1.0);
	return (fall + source) * color;
}


float luminance(float4 col)
{
	return 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b;
}

void setLuminance(inout float4 col, float lum)
{
	lum /= luminance(col);
	col *= lum;
}


//MAIN SCENE

//float4 ambientLight = float4(0.05, 0.05, 0.05, 1);
float3 halfVec = float3(0, 0, 1);

float2 offset = float2(0.5/1280.0, 0.5/720.0);
float specVal = 0.005;

PixelToFrame PointLightShader(VertexToPixel PSIn) : COLOR0
{	
	PixelToFrame Output = (PixelToFrame)0;
	
	float4 colorMap = tex2D(ColorMapSampler, PSIn.TexCoord);	
		
	float2 p = (PSIn.TexCoord.xy) * iResolution.xy;


	//The angle of the pixel based on the normal map interpretation
	float3 normal = (2.0f * (tex2D(NormalMapSampler, PSIn.TexCoord + offset))) - 1.0f;
	normal *= float3(1, -1, 1);

	
	float dist = 1.0;
	float4 col = AmbientColor;


	float2 light1Pos = float2(LightPosition.x, LightPosition.y);
	float4 light1Col = LightColor;
	
	//Get the direction of the current pixel from the center of the light
	float3 lightDirNorm = normalize(float3(light1Pos, 0) - float3(p.x, p.y, -25));	
	float amount = max(dot(normal, lightDirNorm), 0);		
	float3 reflect = normalize(2.0 * amount * normal - lightDirNorm);
	float specular = min(pow(saturate(dot(reflect, halfVec)), specVal * 255), amount); 
	setLuminance(light1Col, LightPower);

	col += drawLight(p, light1Pos, light1Col, dist, LightSize, 1.0) * specular;
	
	col = lerp(col, col, clamp(-dist, 0.0, 1.0));
	
	
	Output.Color = clamp(col, 0.0, 1.0) * tex2D(ShadowMapSampler, PSIn.TexCoord);	
	//Output.Color = colorMap;
	//Output.Color = col;
	return Output;
}

technique DeferredPointLight
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 MyVertexShader();
        PixelShader = compile ps_3_0 PointLightShader();
    }
}

