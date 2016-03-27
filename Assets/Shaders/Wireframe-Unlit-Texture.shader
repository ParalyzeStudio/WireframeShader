Shader "Wireframe/Unlit/Texture"
{ 
    Properties 
    {
		_MainTex("Base (RGB)", 2D) = "white"{}
		
		_V_WIRE_Color("Wire Color (RGB) Trans (A)", color) = (0, 0, 0, 1)	
		_V_WIRE_Size("Wire Size", Range(0, 0.5)) = 0.05
		
    }

    SubShader 
    {
		Tags { "RenderType"="Opaque" }

		Pass
	    {
            CGPROGRAM 
		    #pragma vertex vert
	    	#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			fixed4 _V_WIRE_Color;
			half _V_WIRE_Size;	

			fixed4 _Color;
			sampler2D _MainTex;
			half4 _MainTex_ST;


			struct vInput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				
				fixed4 color : COLOR;
			};

			struct vOutput
			{
				float4 pos :SV_POSITION;
				half2 texcoord : TEXCOORD0;

				fixed4 mass : TEXCOORD1;		
			};

			void vert (inout appdata_full v) 
			{
				
			}

			vOutput vert(vInput v)
			{
				vOutput o;

				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = half2(TRANSFORM_TEX(v.texcoord, _MainTex));

				o.mass = v.color;

				return o;
			}

			fixed4 frag(vOutput i) : SV_Target 
			{	
			    fixed4 retColor = tex2D(_MainTex, i.texcoord);
				 
				half3 width = abs(ddx(i.mass.xyz)) + abs(ddy(i.mass.xyz));
				half3 eF = smoothstep(half3(0, 0, 0), width * _V_WIRE_Size * 20, i.mass.xyz);		
	  			half value = min(min(eF.x, eF.y), eF.z);	
				
				return lerp(lerp(retColor, _V_WIRE_Color, _V_WIRE_Color.a), retColor, value);
			}

			ENDCG 

    	}
			
        
    }


}
