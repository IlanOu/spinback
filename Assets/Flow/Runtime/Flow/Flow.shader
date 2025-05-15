//  FlowFx © NullTale - https://x.com/NullTale
Shader "Hidden/Vol/Flow"
{
    HLSLINCLUDE
    
    struct vert_in
    {
        float4 pos : POSITION;
        float2 uv  : TEXCOORD0;
    };

    struct frag_in
    {
        float2 uv     : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };
                
    frag_in vert(vert_in input)
    {
        frag_in output;
        output.vertex = input.pos;
        output.uv     = input.uv;
        
        return output;
    }
    
    ENDHLSL

	SubShader 
	{
        ZTest Always
        ZWrite Off
        ZClip false
        Cull Off
		
        Pass	// 0
		{
			name "Flow"
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
	        	        
	        sampler2D    _MainTex;
	        sampler2D    _FlowTex;
	        
	        float4		 _Weight;
	        float4		 _Tiling;
	        float4		 _Tint;
	        float4		 _Data;
	        
            #define _offset _Tiling.xy
            #define _scale  _Tiling.z
            #define _rot    _Tiling.w
	        
            inline half3 applyHue(half3 aColor, half aHue)
            {
                half angle = aHue;
                half3 k = float3(0.57735, 0.57735, 0.57735);
                half cosAngle = cos(angle);
                return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
            }

	        float2 rotate(float2 vec, float angle)
	        {
		        float c = cos(angle);
		        float s = sin(angle);
	        	
	        	return float2(dot(vec, float2(c, -s)), dot(vec, float2(s, c)));
	        }
	        
	        half4 frag(frag_in i) : SV_Target 
	        {	        
	            half4 col  = tex2D(_MainTex, i.uv);
	            half4 flow = tex2D(_FlowTex, frac(rotate(i.uv - float2(.5, .5), _Tiling.w) * _scale + float2(.5, .5) + _offset));

	        	flow.rgb = lerp(flow.rgb, flow.rgb * _Tint.rgb, _Tint.a);
	        	
				return saturate(col * _Weight.x + flow * _Weight.y);
	        }
			
			ENDHLSL
		}
		
        Pass	// 1
		{
			name "Blit"
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
	        	        
	        sampler2D    _MainTex;
	        	        
	        half4 frag(frag_in i) : SV_Target 
	        {
	            return tex2D(_MainTex, i.uv);
	        }
			
			ENDHLSL
		}
		
        Pass	// 2
		{
			name "Print"

			Blend SrcAlpha OneMinusSrcAlpha
			
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
			
	        sampler2D    _MainTex;
	        float4		 _Weight;
	        
            half luma(half3 rgb)
            {
                return dot(rgb, half3(.299, .585, .114));
            }
	        	      
	        	        
	        half4 frag(frag_in i) : SV_Target 
	        {
	            half4 result =  tex2D(_MainTex, i.uv);

	        	result.a *= _Weight.z * lerp(1, luma(result.rgb) *2, _Weight.w);
	        	return result;
	        }
			
			ENDHLSL
		}
		
        Pass	// 3
		{
			name "Filter"

			Blend SrcAlpha OneMinusSrcAlpha
			
			
	        HLSLPROGRAM
	        
			#pragma vertex vert
			#pragma fragment frag
			
	        sampler2D    _MainTex;
	        sampler2D    _ValueTex;
	        
            half luma(half3 rgb)
            {
                return dot(rgb, half3(.299, .585, .114));
            }
	        	        
	        half4 frag(frag_in i) : SV_Target 
	        {
	            half4 result =  tex2D(_MainTex, i.uv);
	        	result *= tex2D(_ValueTex, i.uv).r;

	        	return result;
	        }
			
			ENDHLSL
		}
	}
}