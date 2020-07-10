float4x4 Projection;

struct VertexShaderInput
{
    float4 Position : SV_Position;
	float2 texCoord : TEXCOORD0;
	float4 color : COLOR0;
};

struct VertexShaderOutput
{
	float2 texCoord : TEXCOORD0;
    float4 position : POSITION0;
	float4 Color : COLOR0;
};

VertexShaderOutput SpriteVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.texCoord = input.texCoord;
    output.position = mul(input.Position, Projection);
	output.Color = input.color;

	return output;
}

texture InputTexture; 
sampler inputSampler = sampler_state      
{
            Texture   = <InputTexture>;
            MipFilter = Point;
            MinFilter = Point;
            MagFilter = Point;
            AddressU  = Clamp;
            AddressV  = Clamp;
};

float2 renderTargetSize = float2(1280, 720);
float2 texSize = float2(1280, 720);

float Gaussian (float sigma, float x)
{
    return exp(-(x*x) / (2.0 * sigma*sigma));
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 uv = input.texCoord;
	float2 OnePixel = float2(1,1)/texSize;

	float4 color = tex2D(inputSampler, uv);
	float4 c = float4(0, 0, 0, 0);

	int offset = 12;

		for (int x = -offset; x <= offset; x+=2)
		{
			for (int y = -offset; y <= offset; y+=2)
			{
				float fx = Gaussian(40.0f, x);
				float fy = Gaussian(40.0f, y);
				c += 0.6f*tex2D(inputSampler, uv + float2(OnePixel.x * x, OnePixel.y * y)).xyzw * fx * fy;
			}
		}
	
	float4 newCol = c/(offset*offset);
	return newCol;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
