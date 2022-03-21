Shader "ExtensionTools/WorldGridShader"
{
    Properties
    {
        _Color1 ("Color1", Color) = (1,1,1,1)
        _Color2 ("Color2", Color) = (0,0,0,1)
        _GridSize("GridSize",Range(0.1,10)) = 1.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows  vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input
        {
            float3 worldNormal;
            float3 worldBinormal;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color1;
        fixed4 _Color2;
        float _GridSize;


         void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            float3 worldNormal = UnityObjectToWorldNormal(v.normal);
            float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
            float3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w * unity_WorldTransformParams.w;

            o.worldNormal=worldNormal;
            o.worldBinormal =worldBinormal;
        }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 worldPos =IN.worldPos;
            
            float3 worldForward=IN.worldNormal;
            float3 worldRight=IN.worldBinormal;
            float3 worldUp=cross(worldRight,worldForward);

            float worldX=dot(worldPos,worldRight);
            float worldY=dot(worldPos,worldUp);

            float offset=0.500001;
            float x=worldX+offset;
            float y=worldY+offset;
 
            float GridSizeDouble=_GridSize*2.0;

            if(x<0)
                x-=_GridSize;
            x%=GridSizeDouble;

            if(y<0)
                y-=_GridSize;
            y%=GridSizeDouble;

            bool val;

            if(abs(x)<_GridSize)
                val=true;
            else
                val=false;
            if(abs(y)<_GridSize)
                val=!val;
            
            float3 color=(val)?_Color1:_Color2;
            o.Albedo = color.rgb;

            //o.Albedo=IN.worldBinormal;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
