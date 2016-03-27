Shader "Wireframe/Surface/Diffuse" {
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_V_WIRE_Color("Wire Color (RGB) Trans (A)", color) = (0, 0, 0, 1)	
		_V_WIRE_Size("Wire Size", Range(0, 0.5)) = 0.05
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		#pragma target 3.0
		#pragma debug

		sampler2D _MainTex;

		fixed4 _V_WIRE_Color;
		half _V_WIRE_Size;			
		fixed4 _Color;

		struct Input 
		{
			float2 uv_MainTex;
			fixed4 mass;	
		};

		void vert (inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.mass = v.color;
		}

		/*vOutput vert(vInput v)
		{
			vOutput o;

			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.texcoord = half2(TRANSFORM_TEX(v.texcoord, _MainTex));

			o.mass = v.color;

			return o;
		}*/

		void surf (Input IN, inout SurfaceOutput o) 
		{
			// Albedo comes from a texture tinted by color
			fixed4 color = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			half3 width = abs(ddx(IN.mass.xyz)) + abs(ddy(IN.mass.xyz));
			half3 eF = smoothstep(half3(0, 0, 0), width * _V_WIRE_Size * 20, IN.mass.xyz);		
	  		half value = min(min(eF.x, eF.y), eF.z);
			color = lerp(lerp(color, _V_WIRE_Color, _V_WIRE_Color.a), color, value);

			o.Albedo = color.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
