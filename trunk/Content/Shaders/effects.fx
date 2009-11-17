
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

float3 StepSize;
int Iterations;

float4 ScaleFactor;

texture3D Volume;

//------- Texture Samplers --------

Texture xTexture;
sampler TextureSampler = sampler_state { texture = <xTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = mirror; AddressV = mirror;};

//------- Technique: RayCast --------

VertexShaderOutput PositionVS(VertexShaderInput input)
{
    VertexShaderOutput output;
	
    output.Position = mul(input.Position * ScaleFactor, WorldViewProjection);
    
    output.texC = input.Position;
    output.pos = output.Position;

    return output;
}

float4 RayCastPS(VertexShaderOutput input) : COLOR0
{ 
	float4 ret = float4(255, 0, 0, 1);
	return ret;

	//calculate projective texture coordinates
	//used to project the front and back position textures onto the cube
	float2 texC = input.pos.xy /= input.pos.w;
	texC.x =  0.5f*texC.x + 0.5f; 
	texC.y = -0.5f*texC.y + 0.5f;  
	
    float3 front;// = tex2D(FrontS, texC).xyz;
    float3 back;// = tex2D(BackS, texC).xyz;
    
    float3 dir = normalize(back - front);
    float4 pos = float4(front, 0);
    
    float4 dst = float4(0, 0, 0, 0);
    float4 src = 0;
    
    float value = 0;
	
	float3 Step = dir * StepSize;
    
    for(int i = 0; i < Iterations; i++)
    {
		pos.w = 0;
		//value = tex3Dlod(VolumeS, pos).r;
				
		src = (float4)value;
		src.a *= .1f; //reduce the alpha to have a more transparent result
					  //this needs to be adjusted based on the step size
					  //i.e. the more steps we take, the faster the alpha will grow	
			
		//Front to back blending
		// dst.rgb = dst.rgb + (1 - dst.a) * src.a * src.rgb
		// dst.a   = dst.a   + (1 - dst.a) * src.a		
		src.rgb *= src.a;
		dst = (1.0f - dst.a)*src + dst;		
		
		//break from the loop when alpha gets high enough
		if(dst.a >= .95f)
			break;	
		
		//advance the current position
		pos.xyz += Step;
		
		//break if the position is greater than <1, 1, 1>
		if(pos.x > 1.0f || pos.y > 1.0f || pos.z > 1.0f)
			break;
    }
    
    return dst;
}

technique RayCast
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 PositionVS();
		PixelShader = compile ps_3_0 RayCastPS();
	}
}