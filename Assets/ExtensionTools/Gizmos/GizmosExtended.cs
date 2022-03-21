using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using ExtensionTools.Text;
using UnityEngine.Rendering;
using System.Reflection;

namespace ExtensionTools.Gizmos
{

    public class GizmosExtended
    {

        static GizmosExtended _instance;

        static GizmosExtended INSTANCE
        {
            get
            {
                if (_instance == null)
                    _instance = new GizmosExtended();

                return _instance;
            }
        }

        /*Buffers*/
        ComputeBuffer m_ComputeBufferQuad;
        CommandBuffer m_CommandBuffer;

        Material m_GizmoMaterialURP;
        Material GizmoMaterial;
        Material BackgroundMaterial;

        List<(Vector3, Texture, float)> m_TexturesToDraw = new List<(Vector3, Texture, float)>();
        List<(Vector3, string, Color, float, bool)> m_TextsToDraw = new List<(Vector3, string, Color, float, bool)>();

        public GizmosExtended()
        {
#if UNITY_EDITOR
            Application.quitting += CleanUp;
            Application.unloading += CleanUp;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += CleanUp;


            Initialize();

#if RENDERPIPELINE_BUILTIN
                UnityEngine.Camera.onPostRender -= OnPostRender;
                UnityEngine.Camera.onPostRender += OnPostRender;
#elif RENDERPIPELINE_HDRP
            RenderPipelineManager.endFrameRendering -= RenderPipelineManager_endFrameRendering;
            RenderPipelineManager.endFrameRendering += RenderPipelineManager_endFrameRendering;
#endif
#endif
        }

        void Initialize() {
            m_GizmoMaterialURP = new Material(Shader.Find("ExtensionTools/GizmoShaderURP"));
            GizmoMaterial = new Material(Shader.Find("ExtensionTools/Gizmo"));
            BackgroundMaterial = new Material(Shader.Find("ExtensionTools/Gizmo"));


            if(m_ComputeBufferQuad!=null)
                m_ComputeBufferQuad?.Dispose();

            m_ComputeBufferQuad = new ComputeBuffer(4, sizeof(float) * 3);
            m_ComputeBufferQuad.SetData(new Vector3[] { new Vector3(-0.5f, -0.5f), new Vector3(-0.5f, 0.5f), new Vector3(0.5f, 0.5f), new Vector3(0.5f, -0.5f) });

        }
        void CleanUp()
        {
            m_ComputeBufferQuad?.Dispose();
        }

        ~GizmosExtended()
        {
            CleanUp();
        }

        public static void DrawCircle(Vector3 position, Vector3 up, float radius, float stepsize = 5f)
        {
            Vector3 left = Vector3.Cross(Vector3.right, up).normalized;
            Vector3 forward = Vector3.Cross(left, up).normalized;

            List<Vector3> positions = new List<Vector3>();

            for (float degrees = 0.0f; degrees < 360.0f; degrees += stepsize)
            {
                float x = Mathf.Sin(degrees * Mathf.Deg2Rad);
                float y = Mathf.Cos(degrees * Mathf.Deg2Rad);

                Vector3 PosOnCircle = (left * x + forward * y) * radius + position;
                positions.Add(PosOnCircle);
            }

            DrawPath(positions);
        }

        public static void DrawArrow(Vector3 from, Vector3 to)
        {
            Vector3 Direction = (to - from).normalized;
            Vector3 Cross = Vector3.Cross(Direction, (UnityEngine.Camera.current.transform.position - from).normalized).normalized;
            Vector3 CrossForward = Vector3.Cross(Cross, Direction).normalized;

            float ArrowSize = 0.5f;
            float ArrowLength = ArrowSize / 2f;

            UnityEngine.Gizmos.DrawLine(from, to);

            UnityEngine.Gizmos.DrawLine(to - (Direction * ArrowSize) + (Cross * ArrowLength), to);
            UnityEngine.Gizmos.DrawLine(to - (Direction * ArrowSize) - (Cross * ArrowLength), to);

            UnityEngine.Gizmos.DrawLine(to - (Direction * ArrowSize) + (CrossForward * ArrowLength), to);
            UnityEngine.Gizmos.DrawLine(to - (Direction * ArrowSize) - (CrossForward * ArrowLength), to);

            DrawCircle(to - (Direction * ArrowSize), Direction, ArrowLength);
        }
        public static void DrawArrow(Line3D line)
        {
            DrawArrow(line.start, line.end);
        }
        public static void DrawPath(IList<Vector3> path)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                UnityEngine.Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }
        public static void DrawPath(params Vector3[] path)
        {
            DrawPath(path);
        }



        public static void DrawTexture(Vector3 position, Texture texture, float scale = 1.0f)
        {
#if UNITY_EDITOR
            INSTANCE.CheckURPFeaturePresent();
            INSTANCE.m_TexturesToDraw.Add((position, texture, scale));
#endif
        }
        public static void DrawIcon(Vector3 position, Icon icon, float scale = 1.0f)
        {
#if UNITY_EDITOR
            INSTANCE.CheckURPFeaturePresent();
            DrawTexture(position, IconTextures.GetTexture(icon), scale);
#endif
        }

        public static void DrawText(Vector3 position, string text, bool Background = false)
        {
            INSTANCE.CheckURPFeaturePresent();
            DrawText(position, text, Color.white, 1f, Background);
        }

        public static void DrawText(Vector3 position, string text, float Size = 1f, bool Background = false)
        {
            INSTANCE.CheckURPFeaturePresent();
            DrawText(position, text, Color.white, Size, Background);
        }

        public static void DrawText(Vector3 position, string text, Color color, float Size = 1f, bool Background = false)
        {
#if UNITY_EDITOR
                        INSTANCE.CheckURPFeaturePresent();
            INSTANCE.m_TextsToDraw.Add((position, text, color, Size, Background));
#endif
        }


        private void RenderPipelineManager_endFrameRendering(ScriptableRenderContext context, UnityEngine.Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
                OnPostRender(camera);
            }
        }

        static internal void OnPostRenderURP(UnityEngine.Camera cam, CommandBuffer commandBuffer)
        {
#if UNITY_EDITOR
            INSTANCE.m_CommandBuffer = commandBuffer;
            INSTANCE.OnPostRender(cam);
#endif
        }

        void OnPostRender(UnityEngine.Camera cam)
        {
            if (!cam.IsEditorSceneCamera() && !cam.IsMainCamera())
            {
                return;
            }

            DrawAllTextures(cam);
            DrawAllText(cam);
        }

        bool hasCheckURP = false;
        void CheckURPFeaturePresent() {
            if (hasCheckURP)
                return;
#if RENDERPIPELINE_URP
            if (UnityEngine.Camera.current)
            {
                var pipeLineAsset = GraphicsSettings.renderPipelineAsset;
                var type = pipeLineAsset.GetType();
                var propertyInfo = type.GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);

                if (propertyInfo == null)
                {
                    return;
                }

                var scriptableRenderData = (UnityEngine.Rendering.Universal.ScriptableRendererData[])propertyInfo.GetValue(pipeLineAsset);

                if (scriptableRenderData != null && scriptableRenderData.Length > 0)
                {
                    foreach (var renderData in scriptableRenderData)
                    {
                        foreach (var rendererFeature in renderData.rendererFeatures)
                        {
                            if (rendererFeature is GizmosFeature)
                            {
                                return;
                            }
                        }
                    }
                }

                hasCheckURP = true;
                Debug.LogWarning("Gizmos Features is not added as a feature of the renderpipeline! Please add it to display advanced Gizmos.");
            }
#endif
        }


        void DrawAllText(UnityEngine.Camera cam)
        {
#if UNITY_EDITOR
            if (!GizmoMaterial)
            {
                Initialize();
            }

            m_TextsToDraw = m_TextsToDraw.OrderBy(((Vector3, string, Color, float, bool) element) => { return -(cam.transform.position - element.Item1).sqrMagnitude; }).ToList();
            foreach (var texttodraw in m_TextsToDraw)
            {
                string text = texttodraw.Item2;
                Vector3 position = texttodraw.Item1;
                Color color = texttodraw.Item3;
                float fontsize = texttodraw.Item4;
                bool drawbackground = texttodraw.Item5;

                var textdata = FontToTexture.GenerateFromText(text, fontsize);

                if (textdata.Quads.Count == 0)
                    continue;

                Texture texture = FontToTexture.GetFontAtlas("DejaVuSans Mono");

                Vector3 forward = (cam.transform.position - position).normalized;
                Vector3 up = cam.transform.up;
                Vector3 left = Vector3.Cross(forward, up).normalized;

                foreach (var Quad in textdata.Quads)
                {

                    Vector2 Offset = Quad.LeftBottom;

                    Vector2 uvOffset = Quad.UVLB;
                    Vector2 uvScale = (Quad.UVRU - Quad.UVLB);
                    DrawQuad(-forward, up, forward.normalized * 0.5f + position + Offset.x * left + Offset.y * up, Vector3.one * fontsize, texture, color, uvOffset, uvScale);
                }
                if (drawbackground)
                {
                    float MinX = textdata.MinX;
                    float MaxX = textdata.MaxX;
                    float MinxY = textdata.MinY;
                    float MaxY = textdata.MaxY;


                    DrawQuad(-forward, up, position, new Vector3(MaxX - MinX + 0.5f, MaxY - MinxY + 0.5f), IconTextures.GetTexture(Icon.Label), Color.gray, BackgroundMaterial);

                }
            }

            m_TextsToDraw.Clear();
#endif
        }


        void DrawAllTextures(UnityEngine.Camera cam)
        {
#if UNITY_EDITOR

            if (!GizmoMaterial)
            {
                Initialize();
            }

            //Sort by distance
            m_TexturesToDraw = m_TexturesToDraw.OrderBy(((Vector3, Texture, float) element) => { return -(cam.transform.position - element.Item1).sqrMagnitude; }).ToList();

            foreach (var texturetodraw in m_TexturesToDraw)
            {

                Texture texture = texturetodraw.Item2;
                Vector3 position = texturetodraw.Item1;
                float scale = texturetodraw.Item3;

                Vector3 ForwardDirection = (cam.transform.position - position).normalized;
                Vector3 UpDirection = cam.transform.up;


                DrawQuad(-ForwardDirection, UpDirection, position, Vector3.one * scale, texture);
            }

            m_TexturesToDraw.Clear();
#endif
        }


        void DrawQuad(Vector3 forward, Vector3 upward, Vector3 position, Vector3 scale, Texture texture, Material material = null)
        {
            DrawQuad(forward, upward, position, scale, texture, Color.white, Vector2.zero, Vector2.one, material);
        }
        void DrawQuad(Vector3 forward, Vector3 upward, Vector3 position, Vector3 scale, Texture texture, Color color, Material material = null)
        {
            DrawQuad(forward, upward, position, scale, texture, color, Vector2.zero, Vector2.one, material);
        }
        void DrawQuad(Vector3 forward, Vector3 upward, Vector3 position, Vector3 scale, Texture texture, Vector2 uvOffset, Vector2 uvScale, Material material = null)
        {
            DrawQuad(forward, upward, position, scale, texture, Color.white, uvOffset, uvScale, material);
        }

        void DrawQuad(Vector3 forward, Vector3 upward, Vector3 position, Vector3 scale, Texture texture, Color color, Vector2 uvOffset, Vector2 uvScale, Material material = null)
        {
#if RENDERPIPELINE_URP
            DrawQuadURP(Quaternion.LookRotation(forward, upward), position, scale, texture, uvOffset, uvScale, color);
#else
            if (material == null)
                material = GizmoMaterial;

            material.SetTexture("_MainTex", texture);
            material.SetColor("_Color", color);
            material.SetPass(0);

            var matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(forward, upward),scale);
            //matrix = Matrix4x4.Translate(position + (position - cam.transform.position).normalized * 0.01f) * matrix;
            GL.PushMatrix();
            GL.MultMatrix(matrix);
            //GL.PushMatrix();


            Vector3 BottomLeft = new Vector3(-0.5f,-0.5f,0f);
            Vector3 BottomRight = new Vector3(0.5f, -0.5f, 0f); //position + (-left * halfScale.x - upward * halfScale.y);
            Vector3 UpperLeft = new Vector3(-0.5f, 0.5f, 0f); //position + (left * halfScale.x + upward * halfScale.y);
            Vector3 UpperRight = new Vector3(0.5f, 0.5f, 0f); //position + (-left * halfScale.x + upward * halfScale.y);

            GL.Begin(GL.QUADS);
                GL.TexCoord2(uvOffset.x, uvOffset.y);
                GL.Vertex3(BottomLeft.x, BottomLeft.y, BottomLeft.z);
                GL.TexCoord2(uvOffset.x, uvOffset.y+uvScale.y);
                GL.Vertex3(UpperLeft.x, UpperLeft.y, UpperLeft.z);
                GL.TexCoord2(uvOffset.x+uvScale.x, uvOffset.y + uvScale.y);
                GL.Vertex3(UpperRight.x, UpperRight.y, UpperRight.z);
                GL.TexCoord2(uvOffset.x + uvScale.x, uvOffset.y);
                GL.Vertex3(BottomRight.x, BottomRight.y, BottomRight.z);
           GL.End();

           GL.PopMatrix();
#endif
        }

        void DrawQuadURP(Quaternion rotation, Vector3 position, Vector3 scale, Texture texture, Vector2 uvOffset, Vector2 uvScale, Color color)
        {
            if (texture == null)
                return;

            m_GizmoMaterialURP.SetPass(0);
            m_GizmoMaterialURP.SetBuffer("points", m_ComputeBufferQuad);

            Matrix4x4 TRS = Matrix4x4.TRS(position, rotation, scale);

            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            properties.SetMatrix("_ObjectMatrix", TRS);
            properties.SetTexture("_MainTex", texture);
            properties.SetVector("uvOffset", uvOffset);
            properties.SetVector("uvScale", uvScale);
            properties.SetColor("_Color", color);

            m_CommandBuffer.DrawProcedural(Matrix4x4.identity, m_GizmoMaterialURP, 0, MeshTopology.Quads, 4, 1, properties);
        }
    }
}
