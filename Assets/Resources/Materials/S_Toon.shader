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

            cull front    //! 1Pass�� �ո��� �׸��� �ʴ´�.
            Pass
            {
                CGPROGRAM
                #pragma vertex _VertexFuc
                #pragma fragment _FragmentFuc
                #include "UnityCG.cginc"

                    struct ST_VertexInput    //! ���ؽ� ���̴� Input
                    {
                        float4 vertex : POSITION;
                        float2 texcoord : TEXCOORD0;
                        float3 normal : NORMAL;
                    };

                    struct ST_VertexOutput    //! ���ؽ� ���̴� Output
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

                        //�Ÿ��� ���� ����ġ�� ��
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

            cull back    //! 2Pass�� �޸��� �׸��� �ʴ´�.
            CGPROGRAM

            #pragma surface surf _BandedLighting//! Ŀ���� ����Ʈ ���

            struct Input
            {
                float2 uv_MainTex;
                float2 uv_Band_Tex;
                float2 uv_BumpMap;
            };

            struct SurfaceOutputCustom        //! Custom SurfaceOutput ����ü, BandLUT �ؽ�ó�� �ֱ� ���� ����
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

            //! Ŀ���� ����Ʈ �Լ�
            float4 Lighting_BandedLighting(SurfaceOutputCustom s, float3 lightDir, float3 viewDir, float atten)
            {
                //! BandedDiffuse ���� ó�� ����
                float3 fBandedDiffuse;
                float fNDotL = dot(s.Normal, lightDir) * 0.5f + 0.5f;    //! Half Lambert ����

                //! 0~1�� �̷���� fNDotL���� 3���� ������ ������ <- Banded Lighting �۾�
                //float fBandNum = 3.0f;
                //fBandedDiffuse = ceil(fNDotL * fBandNum) / fBandNum;             
                //! BandLUT �ؽ�ó�� UV ��ǥ�� 0~1�� �̷���� NDotL���� �־ ���� ���� �����´�.
                fBandedDiffuse = tex2D(_Band_Tex, float2(fNDotL, 0.5f)).rgb;

                float3 SpecularColor;
                float3 HalfVector = normalize(lightDir + viewDir);
                float HDotN = saturate(dot(HalfVector, s.Normal));
                float PowedHDotN = pow(HDotN, _SpecPower);

                //! smoothstep
                float SpecularSmooth = smoothstep(0.005, 0.01f, PowedHDotN);
                SpecularColor = SpecularSmooth * 1.0f;

                //! ���� �÷� ���
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