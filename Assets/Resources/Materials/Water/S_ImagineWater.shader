Shader "KJK/S_ImagineWater"
{
    Properties
    {
        [Header(Textures)]
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _BumpMap("NormalMap", 2D) = "bump" {}

        [Space, Header(Rim)]
        _RimColor("RimColor", Color) = (1, 1, 1, 1)
        _RimPower("RimPower", Range(0.1, 10)) = 3

        [Space, Header(Wave)]
        _waveSpeed("WaveSpeed", Range(0, 10)) = 2
        _wavePower("WavePower", Range(0 ,1)) = 0.3
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "AlphaTest"}

            GrabPass{}

            CGPROGRAM
            #pragma surface surf MY vertex:vert alpha:fade

            sampler2D _MainTex;
            sampler2D _BumpMap;
            sampler2D _GrabTexture;
            float4 _RimColor;
            float _RimPower;
            float _waveSpeed;
            float _wavePower;

            struct Input
            {
                float2 uv_MainTex;
                float2 uv_BumpMap;

                float3 viewDir;
                float3 worldRefl;
                float4 screenPos;
                float4 color : COLOR;
                INTERNAL_DATA
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORDO;
                float4 color : COLOR;
            };

            v2f vert(inout appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = float4(v.texcoord.xy, 0, 0);
                float wave;
                wave = cos(abs(40 * o.uv.x) * 3 + _Time.y * _waveSpeed) * _wavePower;
                wave += cos(abs(20 * o.uv.y) * 10 + _Time.y * _waveSpeed) * _wavePower;
                wave += sin(abs(20 * o.uv.x) * 5 + _Time.y * _waveSpeed) * _wavePower;
                if (v.color.r >= 0.5)
                    v.vertex.y = wave * 0.5;
                else
                    v.vertex.y = wave;
                o.color = v.color;
                return o;
            }

            //=============================================================================================================
            float4 surf(Input IN, inout SurfaceOutput o)
            {
                fixed4 c = tex2D(_MainTex, float2(IN.uv_MainTex.x - _Time.x * _waveSpeed, IN.uv_MainTex.y));
                
                float2 ScreenUV = float2(0, 0);
                if(IN.screenPos.w != 0)
                    ScreenUV = IN.screenPos.xyz / IN.screenPos.w;
                
                //Normal Wave
                float3 normal1 = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap + _Time.y * 0.05));
                float3 normal2 = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap - _Time.y * 0.05));
                o.Normal = (normal1 + normal2) * 0.5;
                o.Normal *= float3(0.8, 0.8, 1);
                //Rim
                float rim = saturate(dot(o.Normal, IN.viewDir));
                //Reflection

                if (IN.color.b >= 0.5)
                {

                }


                o.Albedo = c;
                o.Emission = pow(rim, _RimPower) * _RimColor.rgb * tex2D(_GrabTexture, ScreenUV + o.Normal.xy * 0.09) * 0.8; // 굴절강도, Grab알파적용
                return float4(o.Emission, c.a);
            }

            float4 LightingMY(SurfaceOutput s, float3 lightDir, float3 viewDir, float atten)
            {
                float4 c;
                c.rgb = s.Albedo * _LightColor0.rgb;
                if (c.a != 0)
                    c.a = s.Alpha;
                //Specular
                float3 H = normalize(lightDir + viewDir);
                float spec = saturate(dot(s.Normal, H)); //스펙큘러 공식
                spec = pow(spec, 800) * 10; // 스펙큘러 범위와 블룸
                //Rim
                float rim = saturate(dot(s.Normal, viewDir));
                float rim1 = pow(1 - rim, 10); //기울어지면 밝아짐
                float rim2 = pow(1 - rim, 2); // 프레넬 마스킹용(알파)

                float4 final = saturate(rim1 + spec); // 라이트 받아오기
                return float4(c * final.rgb, saturate(rim2 + spec)); //흐려지지 않게하려면 rim2 + spec
            }

            ENDCG
        }
            FallBack "Diffuse"

}