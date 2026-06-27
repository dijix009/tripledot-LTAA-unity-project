Shader "Hidden/UI/KawaseBlur"
{
    // Cheap multi-pass Kawase blur intended for Graphics.Blit (pipeline-agnostic full-screen blit).
    // Used by PopupBlurBackground to frost a captured snapshot of whatever is behind a popup.
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Offset ("Blur Offset", Float) = 1.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Offset;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 4 diagonal taps — classic Kawase kernel; iterate with growing _Offset for a wide blur.
                float2 o = _MainTex_TexelSize.xy * _Offset;
                fixed4 col  = tex2D(_MainTex, i.uv + float2( o.x,  o.y));
                col        += tex2D(_MainTex, i.uv + float2(-o.x,  o.y));
                col        += tex2D(_MainTex, i.uv + float2( o.x, -o.y));
                col        += tex2D(_MainTex, i.uv + float2(-o.x, -o.y));
                return col * 0.25;
            }
            ENDCG
        }
    }
}
