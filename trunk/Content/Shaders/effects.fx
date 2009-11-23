
struct VertexShaderInput
{
	float4 Position	: POSITION0;
	float2 texC		: TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position	: POSITION0;
	float3 texC		: TEXCOORD0;
	float4 pos		: TEXCOORD1;
};

//------- XNA-to-HLSL variables --------
float4x4 World;
float4x4 WorldInverseTransform;
float4x4 WorldViewProjection;
float3 CameraPosition;

float3 StepSize;
int Iterations;

float BaseSampleDist = .5f;
float ActualSampleDist = .5f;

int Side = 2;

float4 ScaleFactor;

texture2D Front;
texture2D Back;
texture3D Volume;
texture2D Transfer;

//------- Texture Samplers --------

sampler2D FrontS = sampler_state
{
	Texture = <Front>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	
	AddressU = Border;				// border sampling in U
    AddressV = Border;				// border sampling in V
    BorderColor = float4(0,0,0,0);	// outside of border should be black
};

sampler2D BackS = sampler_state
{
	Texture = <Back>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	
	AddressU = Border;				// border sampling in U
    AddressV = Border;				// border sampling in V
    BorderColor = float4(0,0,0,0);	// outside of border should be black
};

sampler3D VolumeS = sampler_state
{
	Texture = <Volume>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	
	AddressU = Border;				// border sampling in U
    AddressV = Border;				// border sampling in V
    AddressW = Border;
    BorderColor = float4(0,0,0,0);	// outside of border should be black
};

sampler1D TransferS = sampler_state
{
	Texture = <Transfer>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
	
	AddressU  = CLAMP;
    AddressV  = CLAMP;
};

//------- Technique: RayCast --------

VertexShaderOutput PositionVS(VertexShaderInput input)
{
    VertexShaderOutput output;
	
    output.Position = mul(input.Position * ScaleFactor, WorldViewProjection);
    
    output.texC = input.Position;
    output.pos = output.Position;

    return output;
}

float4 PositionPS(VertexShaderOutput input) : COLOR0
{
    return float4(input.texC, 1.0f);
}

float4 WireFramePS(VertexShaderOutput input) : COLOR0
{
    return float4(1.0f, .5f, 0.0f, .85f);
}

float4 RayCastPS(VertexShaderOutput input) : COLOR0
{   
	//calculate projective texture coordinates
	//used to project the front and back position textures onto the cube
	float2 texC = input.pos.xy /= input.pos.w;
	texC.x = 0.5f*texC.x + 0.5f; 
	texC.y = 0.5f*texC.y + 0.5f;  
	
    float3 front = tex2D(FrontS, texC);
    float3 back = tex2D(BackS, texC);
    
    float3 dir = normalize(back - front);
    float4 pos = float4(front, 0);
    
    float4 dst = float4(0, 0, 0, 0);
    float4 src = 0;
    
    float4 value = 0;
	
	float3 Step = dir * StepSize;
    
    for(int i = 0; i < Iterations; i++)
    {
		pos.w = 0;
		
		// Opacity value is stored in the alpha channel, XYZ partial gradients are stored in RGB channels
		value = tex3Dlod(VolumeS, pos);
		
		// Use the transfer function to get new RGBA values
		src = tex1Dlod(TransferS, value.a);
		
		// Opacity correction for varying ray cast sample distances
		src.a = 1 - pow((1 - src.a), ActualSampleDist / BaseSampleDist);

		//float s = dot(value.xyz, float3(0, 1, 1));
				
		//diffuse shading + fake ambient lighting
		//src.rgb = s * src.rgb + .1f * src.rgb;

		//Front to back blending
		src.rgb *= src.a;
		dst = (1.0f - dst.a)*src + dst;		
		
		//break from the loop when alpha gets high enough
		if(dst.a >= .95f)
			i = Iterations; // effectively a "break"
		
		//advance the current position
		pos.xyz += Step;
		
		//break if the position is greater than <1, 1, 1>
		if(pos.x > 1.0f || pos.y > 1.0f || pos.z > 1.0f)
			i = Iterations; // effectively a "break"
    }

    return dst;
}

technique RenderPosition
{
    pass Pass1
    {		
        VertexShader = compile vs_2_0 PositionVS();
        PixelShader = compile ps_2_0 PositionPS();
    }
}

technique RayCast
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 PositionVS();
		PixelShader = compile ps_3_0 RayCastPS();
	}
}

technique WireFrame
{
    pass Pass1
    {		
        VertexShader = compile vs_2_0 PositionVS();
        PixelShader = compile ps_2_0 WireFramePS();
    }
}