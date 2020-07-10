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



////////////////////
//Variables to use//
///////////////////

float2 iResolution = float2(1280, 720);
float4 backColor = float4(0.25, 0.25, 0.25, 1.0);
float4 shapeColor = float4(1.0, 0.4, 0.0, 1.0);

//////////////////
//Dist functions//
//////////////////

float circleDist(float2 p, float radius)
{
	return length(p) - radius;
}

float boxDist(float2 p, float2 size)
{
	float2 d = abs(p) - size;  
	return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

float sceneDist(float2 p)
{
	float b1 =  boxDist(p - float2(200, 200), float2(100, 25));	
	float m =  b1;
    
	return m;
}


//////////////////////////////
//Light and Shadow Functions//
//////////////////////////////

float shadow(float2 p, float2 pos, float radius)
{
	float2 dir = normalize(pos - p);
	float dl = length(p - pos);
	
	// fraction of light visible, starts at one radius (second half added in the end);
	float lf = radius * dl;
	
	// distance traveled
	float dt = 0.01;

	for (int i = 0; i < 32; ++i)
	{				
		// distance to scene at current position
		float sd = sceneDist(p + dir * dt);

        // early out when this ray is guaranteed to be full shadow
        if (sd < -radius) 
            return 0.0;
        
		// width of cone-overlap at light
		// 0 in center, so 50% overlap: add one radius outside of loop to get total coverage
		// should be '(sd / dt) * dl', but '*dl' outside of loop
		lf = min(lf, sd / dt);
		
		// move ahead
		dt += max(1.0, abs(sd));

		if (dt > dl) 
			break;
	}

	// multiply by dl to get the real projected overlap (moved out of loop)
	// add one radius, before between -radius and + radius
	// normalize to 1 ( / 2*radius)
	lf = clamp((lf*dl + radius) / (2.0 * radius), 0.0, 1.0);
	lf = smoothstep(0.0, 1.0, lf);
	return lf;
}


float4 drawLight(float2 p, float2 pos, float4 color, float dist, float range, float radius)
{
	// distance to light
	float ld = length(p - pos);
	
	// out of range
	if (ld > range) 
		return float4(0, 0, 0, 0);
	
	// shadow and falloff
	float shad = shadow(p, pos, radius);
	float fall = (range - ld)/range;
	fall *= fall;
    
	float source = clamp(-circleDist(p - pos, radius), 0.0, 1.0);
	return (shad * fall + source) * color;
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

float4 ambientLight = float4(0.005, 0.005, 0.005, 1);
float3 halfVec = float3(0, 0, 1);

PixelToFrame PointLightShader(VertexToPixel PSIn) : COLOR0
{	
	PixelToFrame Output = (PixelToFrame)0;
	
	float4 colorMap = tex2D(ColorMapSampler, PSIn.TexCoord);	
		
	float2 p = PSIn.TexCoord.xy * iResolution.xy;


	//The angle of the pixel based on the normal map interpretation
	float3 normal = (2.0f * (tex2D(NormalMapSampler, PSIn.TexCoord))) - 1.0f;
	normal *= float3(1, -1, 1);


	
	
	float dist = sceneDist(p);
	float4 col = ambientLight;


	float2 light1Pos = float2(lightPosition.x, lightPosition.y);
	float4 light1Col = float4(1.0, 0.9, 0.5, 1);

	float2 light2Pos = float2(250, 250);
	float4 light2Col = float4(0.1, 1, 0.9, 1);

	
	
	//Get the direction of the current pixel from the center of the light
	float3 lightDirNorm = normalize(float3(light1Pos, 0) - float3(p.x, p.y, -25));	
	float amount = max(dot(normal, lightDirNorm), 0);		
	float3 reflect = normalize(2.0 * amount * normal - lightDirNorm);
	float specular = min(pow(saturate(dot(reflect, halfVec)), 0.01 * 255), amount); 
	setLuminance(light1Col, 1.7);

	col += drawLight(p, light1Pos, light1Col, dist, 800.0, 1.0) * specular;


	//Get the direction of the current pixel from the center of the light
	lightDirNorm = normalize(float3(light2Pos, 0) - float3(p.x, p.y, -25));	
	amount = max(dot(normal, lightDirNorm), 0);		
	reflect = normalize(2.0 * amount * normal - lightDirNorm);
	specular = min(pow(saturate(dot(reflect, halfVec)), 0.005 * 255), amount);
	setLuminance(light1Col, 0.7);

	col += drawLight(p, light2Pos, light2Col, dist, 400.0, 1.0) * specular;

	col = lerp(col, col, clamp(-dist, 0.0, 1.0));
	
    Output.Color = clamp(col, 0.0, 1.0);

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

