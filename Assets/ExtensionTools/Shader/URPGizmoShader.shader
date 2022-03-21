Shader "ExtensionTools/GizmoShaderURP" {
   Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color",COLOR) = (1,1,1,1)
    }
     SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
        LOD 100
                 
         Pass {
         CGPROGRAM
         #include "UnityCG.cginc"
         #pragma target 5.0  
         #pragma vertex vertex_shader
         #pragma fragment fragment_shader
       

         float4x4 _ObjectMatrix;

         sampler2D _MainTex;
         float4 _MainTex_ST;
         uniform fixed4 _LightColor0;
         float4 _Color;
               
         struct Point{
             float3         vertex;
         };      
 

         float2 uvOffset;
         float2 uvScale;
         StructuredBuffer<Point> points;
 
         struct v2f {
             float4 pos : SV_POSITION;
             float2 uv: TEXCOORD0;
         };
 
         v2f vertex_shader (uint id : SV_VertexID, uint inst : SV_InstanceID)
         {
             v2f o;
             float4 vertex_position =  float4(points[id].vertex,1.0f);
             vertex_position=mul(_ObjectMatrix,vertex_position);
             o.pos = mul (UNITY_MATRIX_VP, vertex_position);
             o.uv=(points[id].vertex.xy+float2(0.5,0.5))*uvScale+uvOffset;
             return o;
         }
 
         fixed4 fragment_shader (v2f i) : SV_Target
         {
            return tex2D(_MainTex,i.uv)*_Color;
         }
       
         ENDCG
     }
     }
}