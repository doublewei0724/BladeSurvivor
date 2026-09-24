// 可調數值【材質】_Color=RGBA 色調；_Metallic 金屬度0～1；_Glossiness 光滑度0～1。
// 設定優先序：程式 MaterialPropertyBlock > 材質屬性 > 此處預設；IronGhost 的透明度由 RecoveryFlash 控制。
Shader "BladeSurvivor/IronGhost" {
Properties { _Color("Tint",Color)=(1,1,1,1) _Metallic("Metal",Range(0,1))=.25 _Glossiness("Smoothness",Range(0,1))=.32 }
SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} LOD 200
CGPROGRAM
#pragma surface surf Standard alpha:fade
#pragma target 3.0
struct Input { float4 color:COLOR; };
fixed4 _Color; half _Metallic; half _Glossiness;
void surf(Input IN,inout SurfaceOutputStandard o){o.Albedo=IN.color.rgb*_Color.rgb;o.Metallic=_Metallic;o.Smoothness=_Glossiness;o.Alpha=_Color.a;}
ENDCG
} Fallback "Transparent/Diffuse"
}
