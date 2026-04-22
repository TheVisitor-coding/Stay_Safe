Shader "Custom/CRTMonitor"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _PixelSize ("Pixel Size", Float) = 150
        _ScanlineCount ("Scanline Count", Float) = 80
        _ScanlineStrength ("Scanline Strength", Float) = 0.85
        _GlitchIntensity ("Glitch Intensity", Float) = 0
        _GlitchSpeed ("Glitch Speed", Float) = 10
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };
            
            float _GlitchIntensity;
            float _GlitchSpeed;
            sampler2D _MainTex;
            fixed4 _Color;
            float _PixelSize;
            float _ScanlineCount;
            float _ScanlineStrength;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Glitch — décalage horizontal aléatoire
                float glitchOffset = sin(_Time.y * _GlitchSpeed + IN.texcoord.y * 20.0);
                glitchOffset *= _GlitchIntensity * 0.05;
                float2 pixelUV = floor((IN.texcoord + float2(glitchOffset, 0)) * _PixelSize) / _PixelSize;

                // Couleur de base
                fixed4 col = tex2D(_MainTex, pixelUV);
                col *= IN.color;

                // Scanlines
                float scanline = sin(IN.texcoord.y * _ScanlineCount * 3.14159);
                scanline = lerp(_ScanlineStrength, 1.0, scanline * 0.5 + 0.5);
                col.rgb *= scanline;

                // Alpha premultiplié (obligatoire pour Unity UI)
                col.rgb *= col.a;

                return col;
            }
            ENDCG
        }
    }
}