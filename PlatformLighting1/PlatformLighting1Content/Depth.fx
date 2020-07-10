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

Texture Texture;
sampler TextureSampler = sampler_state 
{
	texture = <Texture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

float depth;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 tex;
	tex = tex2D(TextureSampler, input.texCoord);
	
	if (tex.a == 0)
		clip(tex.a - 1);

	return float4(depth, depth, depth, tex.a);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
