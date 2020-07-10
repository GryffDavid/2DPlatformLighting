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
	float b1 =  boxDist(p - float2(200, 250), float2(40, 40));	
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


PixelToFrame PointLightShader(VertexToPixel PSIn) : COLOR0
{	
	PixelToFrame Output = (PixelToFrame)0;
	
	//The color map texture
	float4 colorMap = tex2D(ColorMapSampler, PSIn.TexCoord);	

	//The angle of the pixel based on the normal map interpretation
	float3 normal = (2.0f * (tex2D(NormalMapSampler, PSIn.TexCoord))) - 1.0f;
	normal *= float3(1, -1, 1);
	
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
	float specular = min(pow(saturate(dot(reflect, halfVec)), 0.0005 * 255), amount); 
	//Multiply the "10" here by a specular map value to be able to change the specularity of each pixel
				
	Output.Color = colorMap * coneAttenuation * lightColor * lightStrength + (specular * coneAttenuation * specularStrength);
	//Output.Color = specMap;

	
	//PixelToFrame Output = (PixelToFrame)0;
	float2 p = PSIn.TexCoord.xy * iResolution.xy;

	float dist = sceneDist(p);
	float4 col = backColor;


	float2 light1Pos = float2(50, 50);
	float4 light1Col = float4(1.0, 1.0, 1.0, 1.0);
	setLuminance(light1Col, 0.4);



	col += drawLight(p, light1Pos, light1Col, dist, 600.0, 6.0);    
	col = lerp(col, shapeColor, clamp(-dist, 0.0, 1.0));
	
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

