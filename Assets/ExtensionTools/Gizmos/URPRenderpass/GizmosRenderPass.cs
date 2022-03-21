
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if RENDERPIPELINE_URP
namespace ExtensionTools.Gizmos
{
    public class GizmosRenderPass : ScriptableRenderPass
    {
        // The profiler tag that will show up in the frame debugger.
        const string ProfilerTag = "Gizmos Pass";

        // We will store our pass settings in this variable.
        GizmosFeature.PassSettings passSettings;

        // The constructor of the pass. Here you can set any material properties that do not need to be updated on a per-frame basis.
        public GizmosRenderPass(GizmosFeature.PassSettings passSettings)
        {
            this.passSettings = passSettings;

            // Set the render pass event.
            renderPassEvent = passSettings.renderPassEvent;

        }

        // The actual execution of the pass. This is where custom rendering occurs.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler(ProfilerTag)))
            {
                GizmosExtended.OnPostRenderURP(renderingData.cameraData.camera, cmd);
            }

            // Execute the command buffer and release it.
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }
}
#endif