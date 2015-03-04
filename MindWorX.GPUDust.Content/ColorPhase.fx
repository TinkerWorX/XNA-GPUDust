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

float rand(float2 co){
	return 0.50 + (frac(sin(dot(co.xy, float2(12.9898, 78.2330))) * 43758.5453)) * 0.50;
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
	float4 mc = tex2D(TextureSampler, texCoord);

	if (compare(mc.y, NONE)) return float4(0.00, 0.00, 0.00, 1.00) * blend;
	if (compare(mc.y, SAND)) return float4(0.98, 0.98, 0.82, 1.00) * blend * (0.5 + 0.5 * (rand(texCoord)));
	if (compare(mc.y, DIRT)) return float4(0.96, 0.64, 0.38, 1.00) * blend * (0.5 + 0.5 * (rand(texCoord)));
	if (compare(mc.y, CONCRETE)) return float4(0.50, 0.50, 0.50, 1.00) * blend * (rand(texCoord));

	if (compare(mc.y, FIRE) && mc.z > 0.90) return float4(0.60, 0.00, 0.00, 1.00) * blend * (rand(texCoord));
	if (compare(mc.y, FIRE) && mc.z > 0.80) return float4(0.80, 0.00, 0.00, 1.00) * blend * (rand(texCoord));
	if (compare(mc.y, FIRE) && mc.z > 0.70) return float4(1.00, 0.00, 0.00, 1.00) * blend * (rand(texCoord));
	if (compare(mc.y, FIRE) && mc.z > 0.60) return float4(1.00, 0.20, 0.00, 1.00) * blend * (rand(texCoord));
	if (compare(mc.y, FIRE) && mc.z > 0.50) return float4(1.00, 0.40, 0.20, 1.00) * blend * (rand(texCoord));
	if (compare(mc.y, FIRE) && mc.z > 0.40) return float4(1.00, 0.60, 0.40, 1.00) * blend * (rand(texCoord));
	if (compare(mc.y, FIRE) && mc.z > 0.30) return float4(1.00, 0.80, 0.40, 1.00) * blend * (rand(texCoord));
	if (compare(mc.y, FIRE) && mc.z > 0.20) return float4(1.00, 0.80, 0.20, 1.00) * blend * (rand(texCoord));
	if (compare(mc.y, FIRE) && mc.z > 0.10) return float4(1.00, 1.00, 0.40, 1.00) * blend * (rand(texCoord));
	if (compare(mc.y, FIRE) && mc.z > 0.00) return float4(1.00, 1.00, 0.60, 1.00) * blend * (rand(texCoord));

	if (compare(mc.y, WATER)) return float4(0.00, 0.64, 0.91, 1.00) * blend;
	if (compare(mc.y, PLANT)) return float4(0.71, 0.90, 0.11, 1.00) * blend;

	return mc * blend;
}

technique FloodFill
{
	pass
	{
		VertexShader = compile vs_3_0 FloodFillVS();
		PixelShader = compile ps_3_0 FloodFillPS();
	}
}