Shader "Wireframe/Unlit/Transparent"
{ 
    Properties 
    {
		_MainTex("Base (RGB)", 2D) = "white"{}
		
		_V_WIRE_Color("Wire Color (RGB) Trans (A)", color) = (0, 0, 0, 1)	
		_V_WIRE_Size("Wire Size", Range(0, 0.5)) = 0.05
		
    }

    SubShader 
    {		
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 

		Pass
	    {			  
		 
            CGPROGRAM 
		    #pragma vertex vert
	    	#pragma fragment frag
			#pragma target 3.0

			fixed4 _V_WIRE_Color;
			half _V_WIRE_Size;	

			fixed4 _Color;
			sampler2D _MainTex;
			half4 _MainTex_ST;


			struct vInput
			{
				float4 vertex : POSITION;
				half4 texcoord : TEXCOORD0;
				
				fixed4 color : COLOR;
			};

			struct vOutput
			{
				float4 pos :SV_POSITION;
				half3 uv : TEXCOORD0;

				fixed4 mass : TEXCOORD1;		
			};

			vOutput vert(vInput v)
			{
				vOutput o;

				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = half3(v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw, 0);

				o.mass = v.color;

				return o;
			}

			fixed4 frag(vOutput i) : SV_Target 
			{	
				fixed4 retColor = tex2D(_MainTex, i.uv.xy);
				 
				half3 width = abs(ddx(i.mass.xyz)) + abs(ddy(i.mass.xyz));
				half3 eF = smoothstep(half3(0, 0, 0), width * _V_WIRE_Size * 20, i.mass.xyz);		
	  			half value = min(min(eF.x, eF.y), eF.z);	
				
				return lerp(lerp(retColor, _V_WIRE_Color, _V_WIRE_Color.a), retColor, value);
			}


			ENDCG 

    	}
			
        
    }


}
