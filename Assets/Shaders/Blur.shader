Shader "XImmersion/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurRadius ("Blur Radius", Range(0, 0.1)) = 0.02
        _Iteration ("Quality", Range(8, 32)) = 16
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        ZWrite Off Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _BlurRadius;
            float _Iteration;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            float Rand(float2 co) {
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = 0;
                float2 uv = i.uv;
                
                float noise = Rand(i.screenPos.xy / i.screenPos.w);
                float angle = noise * 6.2831; // 2 * PI
                
                for (int s = 0; s < _Iteration; s++)
                {
                    float dist = s / _Iteration;
                    float2 offset = float2(cos(angle + s), sin(angle + s)) * dist * _BlurRadius;
                    col += tex2D(_MainTex, uv + offset);
                }

                return col / _Iteration;
            }
            ENDCG
        }
    }
}