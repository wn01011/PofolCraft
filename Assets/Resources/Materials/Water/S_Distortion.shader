Shader "KJK/S_Distortion"
{
    Properties
    {
        _Strength("Distort Strength", float) = 1.0
        _Speed("Distort Speed", float) = 1.0
        _Color("Color(rgb)", color) = (1,1,1,1)

        _Noise("Noise Texture", 2D) = "white"{}
        _StrengthFilter("Strength Filter", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "DisableBatching" = "True"}
        GrabPass {}

        Pass
        {
        ZTest Always

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        sampler2D _GrabTexture;
        sampler2D _Noise;
        sampler2D _StrengthFilter;
        float _Strength;
        float _Speed;
        float4 _Color;

        struct vertexInput
        {
            float4 vertex : POSITION;
            float3 texCoord : TEXCOORD0;
        };

        struct vertexOutput
        {
            float4 pos : SV_POSITION;
            float4 grabPos : TEXCOORD0;
        };

        vertexOutput vert(vertexInput input)
        {
            vertexOutput output;

            float4 pos = input.vertex;
            pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(0, 0, 0, 0.8)) + float4(pos.x * 0.2f, pos.z * 0.2f, 0, 0));

            //transform process
//          float4 pos = input.vertex;
//          // transform origin to view space
//          float4 originInViewSpace = mul(UNITY_MATRIX_MV, float4(0, 0, 0, 1);
//          // translate view space point by vertex position
//          float4 vertInViewSpace = originInViewSpace + float4(pos.x, pos.z, 0, 0);
//          // convert from view space to projection space
//          pos = mul(UNITY_MATRIX_P, vertInViewSpace);
            output.pos = pos;

            //grab coordinates
            output.grabPos = ComputeGrabScreenPos(output.pos);
            float noise = tex2Dlod(_Noise, float4(input.texCoord, 0)).rgb;
            float filter = tex2Dlod(_StrengthFilter, float4(input.texCoord, 0)).rgb;

            output.grabPos.x += cos(noise * _Time.y * _Speed) * _Strength * filter;
            output.grabPos.y += sin(noise * _Time.y * _Speed) * _Strength * filter;

            return output;

        }

        float4 frag(vertexOutput input) : COLOR
        {
            return tex2Dproj(_GrabTexture, input.grabPos) * _Color;
        }

        ENDCG
        }
    }
    FallBack "Diffuse"

}
