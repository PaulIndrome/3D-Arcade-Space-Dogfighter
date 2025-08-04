using System;
using System.Collections.Generic;
using Drawing;
using UnityEngine;

namespace Soulspace
{
[ExecuteAlways]
public class CurvingTestGizmos : MonoBehaviour
{
    [Serializable]
    public struct SphereGizmo {
        public float radius;
        public Color color;
        public Mesh radiusMesh;
    }

    public Camera renderToCam;
    public List<SphereGizmo> sphereGizmos;

    private void LateUpdate() {
        DrawCircleLines();
    }

    void DrawCircleLines(){
        CommandBuilder builder = DrawingManager.GetBuilder(true);
        builder.cameraTargets = new Camera[] { renderToCam };
        using(builder){
            foreach(SphereGizmo gizmo in sphereGizmos){
                Draw.ingame.SolidCircle(transform.position, transform.up, gizmo.radius, gizmo.color);
            }
        }
    }

    void DrawWireSphereGuides(){
        CommandBuilder builder = DrawingManager.GetBuilder(true);
        builder.cameraTargets = new Camera[] { renderToCam };
        using(builder){
            float rotationOffset = 0;
            foreach(SphereGizmo gizmo in sphereGizmos){
                if(gizmo.radiusMesh == null) continue;
                using   (   Draw.ingame.WithMatrix(transform.localToWorldMatrix 
                            * Matrix4x4.Scale(Vector3.one * gizmo.radius)  
                            * Matrix4x4.Rotate(Quaternion.Euler(rotationOffset, rotationOffset, rotationOffset)))
                        ) 
                {
                    Draw.ingame.WireMesh(gizmo.radiusMesh, gizmo.color);
                }
                rotationOffset += 23;
            }
        }
    }
}
}
