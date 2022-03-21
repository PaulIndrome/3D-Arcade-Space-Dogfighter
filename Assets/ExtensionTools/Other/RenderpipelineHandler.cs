using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
#if UNITY_EDITOR
namespace ExtensionTools
{
    [InitializeOnLoad]
    /// <summary>
    /// Adds Define symbols based on the current renderpipeline
    /// </summary>
    public static class RenderpipelineHandler
    {
        enum RenderPipeline { Custom,BuiltIn,HDRP,URP};

        static RenderpipelineHandler()
        {
            RenderPipeline renderPipeline = GetRenderPipeline();

            switch (renderPipeline)
            {
                case RenderPipeline.Custom:
                    SetDefine("RENDERPIPELINE_CUSTOM");
                    break;
                case RenderPipeline.BuiltIn:
                    SetDefine("RENDERPIPELINE_BUILTIN");
                    break;
                case RenderPipeline.HDRP:
                    SetDefine("RENDERPIPELINE_HDRP");
                    break;
                case RenderPipeline.URP:
                    SetDefine("RENDERPIPELINE_URP");
                    break;
            }
        }

        static void SetDefine(string define)
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);

            string[] defines;
            PlayerSettings.GetScriptingDefineSymbolsForGroup(group, out defines);
            List<string> defineList = new List<string>(defines);
            defineList.Remove("RENDERPIPELINE_CUSTOM");
            defineList.Remove("RENDERPIPELINE_BUILTIN");
            defineList.Remove("RENDERPIPELINE_HDRP");
            defineList.Remove("RENDERPIPELINE_URP");

            defineList.Add(define);

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defineList.ToArray());
        }
        static RenderPipeline GetRenderPipeline() {
#if UNITY_2019_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                // SRP
                var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
                if (srpType.Contains("HDRenderPipelineAsset"))
                {
                    return RenderPipeline.HDRP;
                }
                else if (srpType.Contains("UniversalRenderPipelineAsset") || srpType.Contains("LightweightRenderPipelineAsset"))
                {
                    return RenderPipeline.URP;
                }
                else return RenderPipeline.Custom;
            }
#elif UNITY_2017_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null) {
                // SRP not supported before 2019
                return RenderPipeline.Custom;
            }
#endif
            // no SRP
            return RenderPipeline.BuiltIn;
        }
    }
}
#endif
