Shader "KJK/S_Leaf"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _FireTex ("FireTexure", 2D) = "white" {}
        _BumpMap("Normal", 2D) = "bump"{}
        _Noise("Noise", 2D) = "white"{}
        _NP("NomalPower", float) = 1
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
    }
    SubShader
    {
        
            Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest"}
            cull back

            CGPROGRAM
            #pragma surface surf Lambert vertex:vert alphatest:_Cutoff
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _FireTex;
            sampler2D _BumpMap;
            sampler2D _Noise;

            half _Smoothness;
            float4 _ShadowColor;
            float _NP;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            v2f vert(inout appdata_full v)
            {
                float4 vPosWorld = mul(unity_ObjectToWorld, v.vertex);
                float4 lightDirection = -normalize(_WorldSpaceLightPos0);
                float opposite = vPosWorld.y;
                float cosTheta = -lightDirection.y;
                float hypotenuse = opposite / cosTheta;
                float3 vPos = vPosWorld.xyz + (lightDirection * hypotenuse);
                v2f o;
                o.pos = mul(UNITY_MATRIX_VP, float4(vPos.x, 0, vPos.z, 1));
                //o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = float4(v.texcoord.xy, 0, 0);
                float wave;
                wave = cos(abs(2 * o.uv.x - 1) + _Time.y * 0.5);
                v.vertex.x += wave * 0.02;
                v.vertex.z -= wave * 0.02;
                return o;
            }

            struct Input
            {
                float2 uv_MainTex;
                float2 uv_BumpMap;
                float2 uv_FireTex;
                float2 uv_Noise;
                float4 color:COLOR;
            };


            void surf(Input IN, inout SurfaceOutput o)
            {
                float4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
                float4 bumpMap = tex2D(_BumpMap, float2(IN.uv_BumpMap.x, IN.uv_BumpMap.y));
                float4 noise = tex2D(_Noise, float2(IN.uv_Noise.x + _Time.y * 0.1, IN.uv_Noise.y));
                float4 fireTex = tex2D(_FireTex, float2(IN.uv_FireTex.x, IN.uv_FireTex.y - _Time.y));

                float3 Nor = UnpackNormal(tex2D(_BumpMap,float2( IN.uv_BumpMap.x, IN.uv_BumpMap.y)));
                _NP *= 10 * cos(_Time.y);
                Nor = float3(Nor.r * _NP, Nor.g * _NP, Nor.b);
                if (IN.color.r >= 0.5)
                {
                    o.Albedo = fireTex * mainTex;
                    o.Albedo = float3(fireTex.r * 0.5 + 0.4, o.Albedo.g, o.Albedo.b);
                }
                else
                    o.Albedo = mainTex.rgb * noise * 3;
                o.Normal = Nor;
                o.Alpha = mainTex.a;
            }
            ENDCG
        
    }
    FallBack "Transparent/Cutout/Diffuse"
}
