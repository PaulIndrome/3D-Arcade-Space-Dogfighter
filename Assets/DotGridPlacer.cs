using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes2D;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Soulspace
{
    public class DotGridPlacer : MonoBehaviour
    {
        public Vector3 gridObjectsPerDimension;
        public Vector3 gapBetweenGridLines;
        public GameObject dotGridPrefab;
        public GameObject[] dotGridObjects;
        public Transform gridParent;
        public List<GameObject> groupParents;

        public Vector3 GetLocalDirectionForDimension(int dimension){
            return dimension switch
            {
                0 => transform.right,
                1 => transform.up,
                2 => transform.forward,
                _ => -transform.forward,
            };
        }
    }

#if UNITY_EDITOR
  
    [CustomEditor(typeof(DotGridPlacer))]
    public class DotGridPlacerEditor : Editor {

        DotGridPlacer placer;
        public override void OnInspectorGUI() {
            placer = target as DotGridPlacer;

            EditorGUI.BeginDisabledGroup(true); 
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((DotGridPlacer) target), typeof(DotGridPlacer), false);
            EditorGUI.EndDisabledGroup();

            bool doesGridExist = DoesGridExist();
            bool validGridSize = placer.gridObjectsPerDimension.CanCreateGrid();

            EditorGUI.BeginDisabledGroup(!validGridSize);
            if(GUILayout.Button($"Place Grid for \"{placer.gameObject.name}\"")){
                if(doesGridExist){
                    RemoveGrid();
                }
                PlaceGrid();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!doesGridExist);
            if(GUILayout.Button("Clear Waypoints")){
                RemoveGrid();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.gridObjectsPerDimension)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.gapBetweenGridLines)));

            EditorGUI.BeginDisabledGroup(true);
            Vector3 resultingSize = placer.gridObjectsPerDimension.MultiplyPerElement(placer.gapBetweenGridLines);
            EditorGUILayout.Vector3Field("Gridsize", resultingSize);
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(true);
            int resultingNum = (Mathf.FloorToInt(placer.gridObjectsPerDimension.x) + 1) 
                * (Mathf.FloorToInt(placer.gridObjectsPerDimension.y) + 1) 
                * (Mathf.FloorToInt(placer.gridObjectsPerDimension.z) + 1);
            EditorGUILayout.FloatField("Num objects", resultingNum);
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.dotGridPrefab)));

            if(placer.dotGridObjects != null && placer.dotGridObjects.Length > 0){
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.dotGridObjects)));
                EditorGUI.EndDisabledGroup();
            }

            if(placer.groupParents != null && placer.groupParents.Count > 0){
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.groupParents)));
                EditorGUI.EndDisabledGroup();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void PlaceGrid(){
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Place Grid of size {placer.gridObjectsPerDimension}");
            Undo.RecordObject(placer, "Record object");
            
            placer.gridParent = new GameObject("Grid").transform;
            placer.gridParent.position = placer.transform.position;
            placer.gridParent.SetParent(placer.gameObject.transform);
            Undo.RegisterCreatedObjectUndo(placer.gridParent.gameObject, "Created grid parent");
           
            int numObjects = 
                (Mathf.FloorToInt(placer.gridObjectsPerDimension.x) + 1) 
                * (Mathf.FloorToInt(placer.gridObjectsPerDimension.y) + 1) 
                * (Mathf.FloorToInt(placer.gridObjectsPerDimension.z) + 1);
            placer.dotGridObjects = new GameObject[numObjects];
            
            bool canceled = false;
            int index = 0;

            GameObject groupX = new GameObject("Group X");
            groupX.transform.position = placer.gridParent.position;
            groupX.transform.SetParent(placer.gridParent.transform);
            Undo.RegisterCreatedObjectUndo(groupX, "Created group X");

            for(int x = 0; x <= placer.gridObjectsPerDimension.x; x++){
                canceled = EditorUtility.DisplayCancelableProgressBar("Fill Group X", string.Format("Group X {0}/{1}", x, placer.gridObjectsPerDimension.x), x / placer.gridObjectsPerDimension.x);
                if(canceled) {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                Vector3 localPosition = new Vector3(
                    x * placer.gapBetweenGridLines.x,
                    0,
                    0
                );
                CreateGridObjectWithUndo(index, localPosition, groupX.transform);
                index++;
            }
            
            GameObject groupY = new GameObject("Group Y");
            groupY.transform.position = placer.gridParent.position;
            groupY.transform.SetParent(placer.gridParent.transform);
            Undo.RegisterCreatedObjectUndo(groupY, "Created group Y");

            groupX.transform.SetParent(groupY.transform);

            for(int y = 1; y < placer.gridObjectsPerDimension.y; y++){
                canceled = EditorUtility.DisplayCancelableProgressBar("Fill Group Y", string.Format("Group Y {0}/{1}", y, placer.gridObjectsPerDimension.y), y / placer.gridObjectsPerDimension.y);
                if(canceled) {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                Vector3 localPosition = new Vector3(
                    0,
                    y * placer.gapBetweenGridLines.y,
                    0
                );

                GameObject copiedGroupX = Instantiate(groupX, groupY.transform);
                copiedGroupX.transform.localPosition = localPosition;
                Undo.RegisterCreatedObjectUndo(copiedGroupX, "Copied group X");
            }

            GameObject groupZ = new GameObject("Group Z");
            groupZ.transform.position = placer.gridParent.position;
            groupZ.transform.SetParent(placer.gridParent.transform);
            Undo.RegisterCreatedObjectUndo(groupZ, "Created group Z");

            groupY.transform.SetParent(groupZ.transform);

            for(int z = 1; z < placer.gridObjectsPerDimension.z; z++){
                canceled = EditorUtility.DisplayCancelableProgressBar("Fill Group Z", string.Format("Group Z {0}/{1}", z, placer.gridObjectsPerDimension.z), z / placer.gridObjectsPerDimension.z);
                if(canceled) {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                Vector3 localPosition = new Vector3(
                    0,
                    0,
                    z * placer.gapBetweenGridLines.z
                );

                GameObject copiedGroupXY = Instantiate(groupY, groupZ.transform);
                copiedGroupXY.transform.localPosition = localPosition;
                Undo.RegisterCreatedObjectUndo(copiedGroupXY, "Copied group XY");
            }

            Undo.CollapseUndoOperations(undoIndex);

            EditorUtility.ClearProgressBar();
        }

        void CreateGridObjectWithUndo(int index, Vector3 localPosition, Transform group){

            Undo.RecordObject(placer, "Array");
            GameObject gridObject = placer.dotGridPrefab;
            placer.dotGridObjects[index] = PrefabUtility.InstantiatePrefab(gridObject, placer.gridParent) as GameObject;
            placer.dotGridObjects[index].transform.SetParent(group);
            placer.dotGridObjects[index].transform.localPosition = localPosition;
            placer.dotGridObjects[index].name = $"{gridObject.name.Substring(0, 10)}...{index.ToString("000000")}";
            // placer.waypoints[i].hideFlags = HideFlags.HideInHierarchy;
            Undo.RegisterCreatedObjectUndo(placer.dotGridObjects[index], $"Created new dotgridobject {index}");
        }

        bool DoesGridExist(){
            return placer.dotGridObjects != null && placer.dotGridObjects.Length > 0;
        }

        void RemoveGrid(){
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Remove Grid");

            for(int e = 0; e < placer.dotGridObjects.Length; e++){
                if(placer.dotGridObjects[e] == null) continue;
                Undo.DestroyObjectImmediate(placer.dotGridObjects[e]);
            }

            Undo.RecordObject(placer, "Clear waypoint array");
            placer.dotGridObjects = null;

            if(placer.gridParent != null){
                Undo.DestroyObjectImmediate(placer.gridParent.gameObject);
            }

            Undo.CollapseUndoOperations(undoIndex);
        }
    }
#endif
}
