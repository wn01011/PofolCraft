Shader "KJK/S_Toon"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("Main Tex Color", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0,0,0,0)
        _BumpMap("NormalMap", 2D) = "bump" {}

        _Outline_Bold("Outline Bold", Range(0, 3)) = 0.1

        _Band_Tex("Band LUT", 2D) = "white" {}
        _SpecPower("SpeCular Power", float) = 5000
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" "Quque" = "Geometry"}

            cull front    //! 1Pass는 앞면을 그리지 않는다.
            Pass
            {
                CGPROGRAM
                #pragma vertex _VertexFuc
                #pragma fragment _FragmentFuc
                #include "UnityCG.cginc"

                    struct ST_VertexInput    //! 버텍스 쉐이더 Input
                    {
                        float4 vertex : POSITION;
                        float2 texcoord : TEXCOORD0;
                        float3 normal : NORMAL;
                    };

                    struct ST_VertexOutput    //! 버텍스 쉐이더 Output
                    {
                        float4 vertex : SV_POSITION;
                        float2 UV : TEXCOORD0;
                    };

                    float _Outline_Bold;
                    float4 _OutlineColor;

                    ST_VertexOutput _VertexFuc(ST_VertexInput stInput)
                    {
                        ST_VertexOutput stOutput;

                        float4 projPos = UnityObjectToClipPos(stInput.vertex);

                        //거리에 따라서 보정치를 줌
                        float distanceToCamera = 0.03 * projPos.z;
                        float normalScale = _Outline_Bold * lerp(0.03, 0.3, distanceToCamera);
                        float3 fNormalized_Normal = normalize(stInput.normal);      
                        float3 fOutline_Position = stInput.vertex + fNormalized_Normal * normalScale; 

                        stOutput.vertex = UnityObjectToClipPos(fOutline_Position);    
                        stOutput.UV = stInput.texcoord.xy;
                        return stOutput;
                    }


                    float4 _FragmentFuc(ST_VertexOutput i) : SV_Target
                    {
                        return float4(_OutlineColor.xyzw);
                    }

                ENDCG
            }

            cull back    //! 2Pass는 뒷면을 그리지 않는다.
            CGPROGRAM

            #pragma surface surf _BandedLighting//! 커스텀 라이트 사용

            struct Input
            {
                float2 uv_MainTex;
                float2 uv_Band_Tex;
                float2 uv_BumpMap;
            };

            struct SurfaceOutputCustom        //! Custom SurfaceOutput 구조체, BandLUT 텍스처를 넣기 위해 만듬
            {
                fixed3 Albedo;
                fixed3 Normal;
                fixed3 Emission;
                half Specular;
                fixed Gloss;
                fixed Alpha;

                float3 BandLUT;
            };

            sampler2D _MainTex;
            sampler2D _Band_Tex;
            sampler2D _BumpMap;

            float4 _Color;
            float _SpecPower;

            void surf(Input IN, inout SurfaceOutputCustom o)
            {
                float4 fMainTex = tex2D(_MainTex, IN.uv_MainTex);
                o.Albedo = fMainTex.rgb;
                o.Alpha = 1.0f;

                float4 fBandLUT = tex2D(_Band_Tex, IN.uv_Band_Tex);
                o.BandLUT = fBandLUT.rgb;

                float3 fNormalTex = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
                o.Normal = fNormalTex;
            }

            //! 커스텀 라이트 함수
            float4 Lighting_BandedLighting(SurfaceOutputCustom s, float3 lightDir, float3 viewDir, float atten)
            {
                //! BandedDiffuse 조명 처리 연산
                float3 fBandedDiffuse;
                float fNDotL = dot(s.Normal, lightDir) * 0.5f + 0.5f;    //! Half Lambert 공식

                //! 0~1로 이루어진 fNDotL값을 3개의 값으로 고정함 <- Banded Lighting 작업
                //float fBandNum = 3.0f;
                //fBandedDiffuse = ceil(fNDotL * fBandNum) / fBandNum;             
                //! BandLUT 텍스처의 UV 좌표에 0~1로 이루어진 NDotL값을 넣어서 음영 색을 가져온다.
                fBandedDiffuse = tex2D(_Band_Tex, float2(fNDotL, 0.5f)).rgb;

                float3 SpecularColor;
                float3 HalfVector = normalize(lightDir + viewDir);
                float HDotN = saturate(dot(HalfVector, s.Normal));
                float PowedHDotN = pow(HDotN, _SpecPower);

                //! smoothstep
                float SpecularSmooth = smoothstep(0.005, 0.01f, PowedHDotN);
                SpecularColor = SpecularSmooth * 1.0f;

                //! 최종 컬러 출력
                float4 FinalColor;
                FinalColor.rgb = ((s.Albedo * _Color) + SpecularColor) *
                                     fBandedDiffuse * _LightColor0.rgb * atten;
                FinalColor.a = s.Alpha;
                return FinalColor;
            }

            ENDCG
        }

            Fallback "VertexLit"
}