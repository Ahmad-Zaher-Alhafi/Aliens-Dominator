// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Griffin/BuiltinRP/Terrain/Lambert_8Splats"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Control0("Control0", 2D) = "black" {}
		_Control1("Control1", 2D) = "black" {}
		_Splat0("Splat0", 2D) = "white" {}
		_Splat1("Splat1", 2D) = "white" {}
		_Splat2("Splat2", 2D) = "white" {}
		_Splat3("Splat3", 2D) = "white" {}
		_Splat4("Splat4", 2D) = "white" {}
		_Splat5("Splat5", 2D) = "white" {}
		_Splat6("Splat6", 2D) = "white" {}
		_Splat7("Splat7", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "DisableBatching" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert keepalpha addshadow fullforwardshadows noinstancing 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _Color;
		uniform sampler2D _Splat0;
		uniform float4 _Splat0_ST;
		uniform sampler2D _Control0;
		uniform sampler2D _Splat1;
		uniform float4 _Splat1_ST;
		uniform sampler2D _Splat2;
		uniform float4 _Splat2_ST;
		uniform sampler2D _Splat3;
		uniform float4 _Splat3_ST;
		uniform sampler2D _Splat4;
		uniform float4 _Splat4_ST;
		uniform sampler2D _Control1;
		uniform sampler2D _Splat5;
		uniform float4 _Splat5_ST;
		uniform sampler2D _Splat6;
		uniform float4 _Splat6_ST;
		uniform sampler2D _Splat7;
		uniform float4 _Splat7_ST;

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 _Color59 = _Color;
			float2 uv0_Splat0 = i.uv_texcoord * _Splat0_ST.xy + _Splat0_ST.zw;
			half4 break34_g20 = tex2D( _Control0, i.uv_texcoord );
			float2 uv0_Splat1 = i.uv_texcoord * _Splat1_ST.xy + _Splat1_ST.zw;
			float2 uv0_Splat2 = i.uv_texcoord * _Splat2_ST.xy + _Splat2_ST.zw;
			float2 uv0_Splat3 = i.uv_texcoord * _Splat3_ST.xy + _Splat3_ST.zw;
			float4 splatColor52_g20 = ( ( tex2D( _Splat0, uv0_Splat0 ) * break34_g20.r ) + ( tex2D( _Splat1, uv0_Splat1 ) * break34_g20.g ) + ( tex2D( _Splat2, uv0_Splat2 ) * break34_g20.b ) + ( tex2D( _Splat3, uv0_Splat3 ) * break34_g20.a ) );
			float4 splatColor055 = splatColor52_g20;
			float2 uv0_Splat4 = i.uv_texcoord * _Splat4_ST.xy + _Splat4_ST.zw;
			half4 break34_g19 = tex2D( _Control1, i.uv_texcoord );
			float2 uv0_Splat5 = i.uv_texcoord * _Splat5_ST.xy + _Splat5_ST.zw;
			float2 uv0_Splat6 = i.uv_texcoord * _Splat6_ST.xy + _Splat6_ST.zw;
			float2 uv0_Splat7 = i.uv_texcoord * _Splat7_ST.xy + _Splat7_ST.zw;
			float4 splatColor52_g19 = ( ( tex2D( _Splat4, uv0_Splat4 ) * break34_g19.r ) + ( tex2D( _Splat5, uv0_Splat5 ) * break34_g19.g ) + ( tex2D( _Splat6, uv0_Splat6 ) * break34_g19.b ) + ( tex2D( _Splat7, uv0_Splat7 ) * break34_g19.a ) );
			float4 splatColor1117 = splatColor52_g19;
			float4 albedoColor125 = ( _Color59 * ( splatColor055 + splatColor1117 ) );
			o.Albedo = albedoColor125.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=17700
7;14;1906;1005;3486.251;1090.533;2.223665;True;False
Node;AmplifyShaderEditor.CommentaryNode;5;-2709.488,-2432.581;Inherit;False;1792.826;1170.514;;22;59;58;9;6;26;12;71;72;73;70;84;11;10;8;31;7;68;83;66;67;69;13;Inputs;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;83;-1515.058,-2377.599;Float;True;Property;_Control1;Control1;2;0;Create;True;0;0;False;0;None;None;False;black;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;13;-2070.795,-1503.025;Float;True;Property;_Splat3;Splat3;6;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;67;-1505.157,-1716.046;Float;True;Property;_Splat6;Splat6;9;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;66;-1515.157,-2158.046;Float;True;Property;_Splat4;Splat4;7;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;69;-1510.157,-1926.045;Float;True;Property;_Splat5;Splat5;8;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;68;-1506.701,-1500.49;Float;True;Property;_Splat7;Splat7;10;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;-2079.251,-2160.582;Float;True;Property;_Splat0;Splat0;3;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;8;-2074.251,-1928.582;Float;True;Property;_Splat1;Splat1;4;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;10;-2069.251,-1718.582;Float;True;Property;_Splat2;Splat2;5;0;Create;True;0;0;False;0;None;None;False;white;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;31;-2079.251,-2382.581;Float;True;Property;_Control0;Control0;1;0;Create;True;0;0;False;0;None;None;False;black;LockedToTexture2D;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-1781.251,-2378.581;Float;False;_Control0;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;12;-1772.795,-1499.026;Float;False;_Splat3;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.CommentaryNode;102;-3088.116,-454.5433;Inherit;False;962.8787;466.6581;;7;117;137;113;114;115;110;112;Second layer;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;65;-3024.415,-1017.926;Inherit;False;882.7046;478.625;;7;55;41;38;39;40;42;136;First layer;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;6;-1781.251,-2156.582;Float;False;_Splat0;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-1776.251,-1924.582;Float;False;_Splat1;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-1771.251,-1714.582;Float;False;_Splat2;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;70;-1208.702,-1496.49;Float;False;_Splat7;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;84;-1217.058,-2373.599;Float;False;_Control1;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;71;-1217.158,-2154.046;Float;False;_Splat4;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;73;-1207.158,-1712.046;Float;False;_Splat6;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;72;-1212.158,-1922.045;Float;False;_Splat5;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;38;-2981.511,-958.7596;Inherit;False;26;_Control0;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;41;-2974.511,-718.7598;Inherit;False;11;_Splat2;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;40;-2977.511,-798.7598;Inherit;False;9;_Splat1;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-2978.511,-878.7599;Inherit;False;6;_Splat0;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;115;-3009.49,-164.5444;Inherit;False;73;_Splat6;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;110;-3016.49,-404.5437;Inherit;False;84;_Control1;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;114;-3013.49,-324.5442;Inherit;False;71;_Splat4;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;113;-3012.49,-244.5443;Inherit;False;72;_Splat5;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;112;-3008.49,-86.54466;Inherit;False;70;_Splat7;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-2973.511,-640.7599;Inherit;False;12;_Splat3;1;0;OBJECT;0;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.FunctionNode;137;-2677.408,-287.2572;Inherit;False;Sample4Splats;-1;;19;563b5268ffef6d8479bb5dc663a94e53;0;5;54;SAMPLER2D;0;False;55;SAMPLER2D;0;False;56;SAMPLER2D;0;False;57;SAMPLER2D;0;False;58;SAMPLER2D;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;136;-2711.357,-831.6314;Inherit;False;Sample4Splats;-1;;20;563b5268ffef6d8479bb5dc663a94e53;0;5;54;SAMPLER2D;0;False;55;SAMPLER2D;0;False;56;SAMPLER2D;0;False;57;SAMPLER2D;0;False;58;SAMPLER2D;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-2359.53,-838.5478;Float;False;splatColor0;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;134;-1923.133,-1016.12;Inherit;False;993.3678;323.2437;;6;125;124;123;122;120;121;Blend Addictive;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;58;-2645.673,-2373.899;Float;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;117;-2372.176,-284.4217;Float;False;splatColor1;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;-1869.316,-915.3834;Inherit;False;55;splatColor0;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;121;-1870.723,-819.7946;Inherit;False;117;splatColor1;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;59;-2313.72,-2375.534;Float;False;_Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;123;-1615.276,-966.1201;Inherit;False;59;_Color;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;122;-1562.87,-874.6176;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;-1378.276,-882.1201;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;-1172.276,-887.1201;Float;False;albedoColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;-1506.905,-528.8466;Inherit;False;125;albedoColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;135;-1217.387,-533.661;Half;False;True;-1;2;;0;0;Lambert;Griffin/BuiltinRP/Terrain/Lambert_8Splats;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;26;0;31;0
WireConnection;12;0;13;0
WireConnection;6;0;7;0
WireConnection;9;0;8;0
WireConnection;11;0;10;0
WireConnection;70;0;68;0
WireConnection;84;0;83;0
WireConnection;71;0;66;0
WireConnection;73;0;67;0
WireConnection;72;0;69;0
WireConnection;137;54;110;0
WireConnection;137;55;114;0
WireConnection;137;56;113;0
WireConnection;137;57;115;0
WireConnection;137;58;112;0
WireConnection;136;54;38;0
WireConnection;136;55;39;0
WireConnection;136;56;40;0
WireConnection;136;57;41;0
WireConnection;136;58;42;0
WireConnection;55;0;136;0
WireConnection;117;0;137;0
WireConnection;59;0;58;0
WireConnection;122;0;120;0
WireConnection;122;1;121;0
WireConnection;124;0;123;0
WireConnection;124;1;122;0
WireConnection;125;0;124;0
WireConnection;135;0;62;0
ASEEND*/
//CHKSM=62B2B2E92A74D4333E78E1071D54270F5EBD6F3D