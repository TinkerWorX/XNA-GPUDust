texture2D Texture;
sampler TextureSampler
	: register(s0) = sampler_state { Texture = (Texture); };

float4x4 MatrixTransform
	: register(vs, c0)
	: register(ps, c0);

float2 XUnit;
float2 YUnit;

float Direction;

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
	float4 tl = tex2D(TextureSampler, texCoord - YUnit - XUnit);	// top left
	float4 tc = tex2D(TextureSampler, texCoord - YUnit);			// top center
	float4 tr = tex2D(TextureSampler, texCoord - YUnit + XUnit);	// top right
	float4 ml = tex2D(TextureSampler, texCoord - XUnit);			// middle left
	float4 mc = tex2D(TextureSampler, texCoord);					// middle center
	float4 mr = tex2D(TextureSampler, texCoord + XUnit);			// middle right
	float4 bl = tex2D(TextureSampler, texCoord + YUnit - XUnit);	// bottom left
	float4 bc = tex2D(TextureSampler, texCoord + YUnit);			// bottom center
	float4 br = tex2D(TextureSampler, texCoord + YUnit + XUnit);	// bottom right

	if (compare(Direction, MOVE_DOWN))
	{
		// ???    ???
		// ?X? -> ?O?
		// ?O?    ?X?
		if (compare(mc.x, DUST) && compare(bc.x, NONE))
			return float4(mc.xyz, MOVE_DOWN);
	}

	if (compare(Direction, MOVE_RIGHT)) // why isn't this MOVE_LEFT? Only MOVE_RIGHT works ...
	{
		// ???    ???
		// OX? -> XO?
		// OX?    OX?
		if (compare(mc.x, DUST) && compare(ml.x, NONE) && !compare(bc.x, NONE) && compare(bl.x, NONE))
			return float4(mc.xyz, MOVE_LEFT);
	}

	if (compare(Direction, MOVE_LEFT)) // why isn't this MOVE_RIGHT? Only MOVE_LEFT works ...
	{
		// ???    ???
		// ?XO -> ?OX
		// ?XO    ?XO
		if (compare(mc.x, DUST) && compare(mr.x, NONE) && !compare(bc.x, NONE) && compare(br.x, NONE))
			return float4(mc.xyz, MOVE_RIGHT);
	}

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