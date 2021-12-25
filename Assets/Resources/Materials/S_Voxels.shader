Shader "KJK/S_Voxels"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MainTex2("Albedo (RGB)", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,10)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 4.0

        sampler2D _MainTex;
        sampler2D _MainTex2;
        half _HeightMapScale;
        half _NormalPower;
        half _Smoothness;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_MainTex2;
            float4 pos : SV_POSITION;
            fixed4 color : COLOR;
            float3 worldPos;
            float3 viewDir;
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float uv : TEXCOORDO;
        };
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float4 heightMap = tex2Dlod(_MainTex2, float4(v.texcoord.x * o.pos.x, v.texcoord.y * o.pos.y, 0, 0));
            v.vertex.y +=  cos(heightMap.r * _Time.y);
        }

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 color = IN.color;
            
            fixed4 c = tex2D (_MainTex, float2(IN.uv_MainTex.x, IN.uv_MainTex.y - _Time.y));
            fixed4 noise = tex2D(_MainTex2, float2(IN.uv_MainTex.x * 4 , IN.uv_MainTex.y * 4));
            o.Smoothness = _Smoothness;
            if (color.r >= 0.5)
            {
                o.Albedo = c * noise;
            }
            else if (color.b >= 0.5)
            {
                o.Albedo = c;
            }
            else
                o.Albedo = (c * 20 + noise)/21;
            //o.Normal = UnpackNormal(tex2D(_MainTex2, IN.uv_MainTex2)) * 0.05;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
