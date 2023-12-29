using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Soulspace
{
    public class GridPlacer : MonoBehaviour
    {
        [System.Serializable]
        public struct GridDimension {
            public GridLineScaler[] dimensionObjects;
        }

        public Vector3 gridLinesPerDimension;
        public Vector3 gapBetweenGridLines;
        public GridLineScaler GridObjectX, GridObjectY, GridObjectZ;
        public GridDimension[] gridDimensions;
        public Transform gridParent;

        public GridLineScaler GetGridObjectForDimension(int dimension){
            return dimension switch
            {
                0 => GridObjectX,
                1 => GridObjectY,
                2 => GridObjectZ,
                _ => null,
            };
        }

        public Vector3 GetLocalDirectionForDimension(int dimension){
            return dimension switch
            {
                0 => transform.right,
                1 => transform.up,
                2 => transform.forward,
                _ => -transform.forward,
            };
        }

        public Quaternion GetLocalRotationForDimension(int dimension){
            return dimension switch
            {
                0 => transform.rotation * Quaternion.Euler(0, -90, 0),
                1 => transform.rotation,
                2 => transform.rotation,
                _ => Quaternion.Euler(Random.insideUnitSphere),
            };
        }
    }

#if UNITY_EDITOR
  
    [CustomEditor(typeof(GridPlacer))]
    public class GridPlacerEditor : Editor {

        GridPlacer placer;
        public override void OnInspectorGUI() {
            placer = target as GridPlacer;

            EditorGUI.BeginDisabledGroup(true); 
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((GridPlacer) target), typeof(GridPlacer), false);
            EditorGUI.EndDisabledGroup();

            bool doesGridExist = DoesGridExist();
            bool validGridSize = placer.gridLinesPerDimension.CanCreateGrid();

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

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.gridLinesPerDimension)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.gapBetweenGridLines)));

            EditorGUI.BeginDisabledGroup(true);
            Vector3 resultingSize = placer.gridLinesPerDimension.MultiplyPerElement(placer.gapBetweenGridLines);
            EditorGUILayout.Vector3Field("Gridsize", resultingSize);
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.GridObjectX)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.GridObjectY)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.GridObjectZ)));

            if(placer.gridDimensions != null && placer.gridDimensions.Length > 0){
                EditorGUI.BeginDisabledGroup(true);
                SerializedProperty gridDimensions = serializedObject.FindProperty(nameof(placer.gridDimensions));
                for(int i = 0; i < gridDimensions.arraySize; i++){
                    EditorGUILayout.PropertyField(gridDimensions.GetArrayElementAtIndex(i));
                }
                EditorGUI.EndDisabledGroup();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void PlaceGrid(){
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Place Grid of size {placer.gridLinesPerDimension}");
            Undo.RecordObject(placer, "Record object");
            
            placer.gridParent = new GameObject("Grid").transform;
            placer.gridParent.SetParent(placer.gameObject.transform);
            Undo.RegisterCreatedObjectUndo(placer.gridParent.gameObject, "Created grid parent");
           
            placer.gridDimensions = new GridPlacer.GridDimension[3];
            for(int e = 0; e < placer.gridDimensions.Length; e++){
                // we add 1 to dimensionSize because we want a closed grid
                int numGridLines = Mathf.FloorToInt(placer.gridLinesPerDimension[e]) + 1;
                if(numGridLines <= 1) continue;

                placer.gridDimensions[e].dimensionObjects = new GridLineScaler[numGridLines];

                for(int i = 0; i < numGridLines; i++){
                    CreateGridObjectWithUndo(e, i, placer.GetLocalDirectionForDimension(e));
                }
            }

            Undo.CollapseUndoOperations(undoIndex);
        }

        void CreateGridObjectWithUndo(int dimension, int index, Vector3 direction){
            Vector3 position = placer.transform.position + direction * index * placer.gapBetweenGridLines[dimension];
            Quaternion rotation = placer.GetLocalRotationForDimension(dimension);

            Undo.RecordObject(placer, "Array");
            GridLineScaler gridObject = placer.GetGridObjectForDimension(dimension);
            placer.gridDimensions[dimension].dimensionObjects[index] = PrefabUtility.InstantiatePrefab(gridObject, placer.gridParent) as GridLineScaler;
            placer.gridDimensions[dimension].dimensionObjects[index].transform.position = position;
            placer.gridDimensions[dimension].dimensionObjects[index].transform.rotation = rotation;
            placer.gridDimensions[dimension].dimensionObjects[index].Scale(placer.gridLinesPerDimension[dimension] * placer.gapBetweenGridLines[dimension]);
            placer.gridDimensions[dimension].dimensionObjects[index].name = $"{gridObject.name.Substring(0, 8)}...{index.ToString("0000")}";
            // placer.waypoints[i].hideFlags = HideFlags.HideInHierarchy;
            Undo.RegisterCreatedObjectUndo(placer.gridDimensions[dimension].dimensionObjects[index], $"Created new waypoint {index}");
        }

        bool DoesGridExist(){
            return placer.gridDimensions != null && placer.gridDimensions.Length > 0;
        }

        void RemoveGrid(){
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Remove Grid");

            for(int e = 0; e < placer.gridDimensions.Length; e++){
                if(placer.gridDimensions[e].dimensionObjects == null) continue;
                for(int i = 0; i < placer.gridDimensions[e].dimensionObjects.Length; i++){
                    if(placer.gridDimensions[e].dimensionObjects[i] == null) continue;
                    Undo.DestroyObjectImmediate(placer.gridDimensions[e].dimensionObjects[i]);
                }
            }

            Undo.RecordObject(placer, "Clear waypoint array");
            placer.gridDimensions = null;

            Undo.DestroyObjectImmediate(placer.gridParent.gameObject);

            Undo.CollapseUndoOperations(undoIndex);
        }
    }
#endif
}
