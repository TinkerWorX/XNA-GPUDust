texture2D Texture;
sampler TextureSampler
	: register(s0) = sampler_state { Texture = (Texture); };

float4x4 MatrixTransform
	: register(vs, c0)
	: register(ps, c0);

float2 XUnit;
float2 YUnit;



float NONE = 0.00;
float DUST = 0.02;
float WALL = 0.04;
float FLUID = 0.06;

float SAND = 0.02;
float DIRT = 0.04;
float CONCRETE = 0.06;
float FIRE = 0.08;
float WATER = 0.10;
float PLANT = 0.12;

float MOVE_NONE = 0.00;
float MOVE_RIGHT = 0.02;
float MOVE_DOWN = 0.04;
float MOVE_LEFT = 0.06;
float MOVE_UP = 0.08;

bool compare(float a, float b){
	return abs(a - b) <= 0.01;
}

void FloodFillVS(
	inout float4 blend : COLOR0,
	inout float2 texCoord : TEXCOORD0,
	inout float4 position : SV_Position)
{
	position = mul(position, MatrixTransform);
}

float4 FloodFillPS(
	in float4 blend : COLOR0,
	in float2 texCoord : TEXCOORD0) : SV_Target0
{
	// edge of concrete
	if (texCoord.x <= 0.00 + 8 * XUnit.x) return float4(WALL, CONCRETE, NONE, MOVE_NONE);
	if (texCoord.y <= 0.00 + 8 * YUnit.y) return float4(WALL, CONCRETE, NONE, MOVE_NONE);
	if (texCoord.x >= 1.00 - 8 * XUnit.x) return float4(WALL, CONCRETE, NONE, MOVE_NONE);
	if (texCoord.y >= 1.00 - 8 * YUnit.y) return float4(WALL, CONCRETE, NONE, MOVE_NONE);

	float4 mc = tex2D(TextureSampler, texCoord);

	// fire is removed after a while
	if (compare(mc.y, FIRE) && mc.z >= 1.00)
		return float4(NONE, NONE, NONE, MOVE_NONE);

	return mc;
}

technique FloodFill
{
	pass
	{
		VertexShader = compile vs_3_0 FloodFillVS();
		PixelShader = compile ps_3_0 FloodFillPS();
	}
}