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

bool isHot(float4 v)
{
	return compare(v.y, FIRE) && v.z > 0.20;
}

void VS(
	inout float4 blend : COLOR0,
	inout float2 texCoord : TEXCOORD0,
	inout float4 position : SV_Position)
{
	position = mul(position, MatrixTransform);
}

float4 PS(
	in float4 blend : COLOR0,
	in float2 texCoord : TEXCOORD0) : SV_Target0
{
	float4 tl = tex2D(TextureSampler, texCoord - YUnit - XUnit);	// top left
	float4 tc = tex2D(TextureSampler, texCoord - YUnit);			// top center
	float4 tr = tex2D(TextureSampler, texCoord - YUnit + XUnit);	// top right
	float4 ml = tex2D(TextureSampler, texCoord - XUnit);			// middle left
	float4 mc = tex2D(TextureSampler, texCoord);					// middle center
	float4 mr = tex2D(TextureSampler, texCoord + XUnit);			// middle right
	float4 bl = tex2D(TextureSampler, texCoord + YUnit - XUnit);	// bottom left
	float4 bc = tex2D(TextureSampler, texCoord + YUnit);			// bottom center
	float4 br = tex2D(TextureSampler, texCoord + YUnit + XUnit);	// bottom right

	// regular upward fire movement
	if (compare(mc.x, NONE) && compare(mc.y, NONE) && compare(bc.x, NONE) && compare(bc.y, FIRE))
		return float4(NONE, FIRE, bc.z + 0.01 + 0.02 * rand(texCoord), NONE);
	if (compare(mc.x, NONE) && compare(mc.y, FIRE))
		return float4(NONE, FIRE, mc.z + 0.01 + 0.02 * rand(texCoord), NONE);

	// water turns into plant
	if (compare(mc.y, WATER) && (compare(tc.y, PLANT) || compare(ml.y, PLANT) || compare(mr.y, PLANT) || compare(bc.y, PLANT)))
		return float4(WALL, PLANT, NONE, MOVE_NONE);

	// plant burns in contact with fire
	if (compare(mc.y, PLANT) && (isHot(tc) || isHot(ml) || isHot(mr) || isHot(bc)))
		return float4(NONE, FIRE, NONE, MOVE_NONE);

	// dust sinks into fluids
	if (compare(mc.x, DUST) && compare(bc.x, FLUID))
		return float4(bc);
	if (compare(mc.x, FLUID) && compare(tc.x, DUST))
		return float4(tc);

	// water kills fire
	if (compare(mc.y, FIRE) && (compare(tc.y, WATER) || compare(ml.y, WATER) || compare(mr.y, WATER) || compare(bc.y, WATER)))
		return float4(NONE, NONE, NONE, MOVE_NONE);

	return mc;
}

technique
{
	pass
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}