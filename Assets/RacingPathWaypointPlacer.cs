using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using NaughtyAttributes;
using Shapes2D;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Soulspace
{
    public class RacingPathWaypointPlacer : MonoBehaviour
    {
        [Range(1, 100)]
        public int numWaypoints = 10;
        public CinemachineSmoothPath path;
        public GameObject waypointPrefab;

        [ReadOnly] public GameObject[] waypoints;

        public Transform waypointParent;
    }

#if UNITY_EDITOR

[CustomEditor(typeof(RacingPathWaypointPlacer))]
public class RacingPathWaypointPlacerEditor : Editor {

    RacingPathWaypointPlacer placer;
    public override void OnInspectorGUI()
    {
        placer = target as RacingPathWaypointPlacer;

        EditorGUI.BeginDisabledGroup(true); 
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((RacingPathWaypointPlacer) target), typeof(RacingPathWaypointPlacer), false);
        EditorGUI.EndDisabledGroup();

        if(GUILayout.Button($"Place Waypoints for {placer.path.gameObject.name}")){
            if(DoesPathExist()){
                RemoveAllWaypoints();
            }
            PlaceWaypoints();
        }
        EditorGUI.BeginDisabledGroup(!DoesPathExist());
        if(GUILayout.Button("Clear Waypoints")){
            RemoveAllWaypoints();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.numWaypoints)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.path)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.waypointPrefab)));

        EditorGUI.BeginDisabledGroup(true); 
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(placer.waypoints)));
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
    
    private void PlaceWaypoints(){
        int undoIndex = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName($"Place {placer.numWaypoints} waypoints");
        Undo.RecordObject(placer, "Record object");
        
        placer.waypoints = new GameObject[placer.numWaypoints];

        int finalIndex = placer.numWaypoints - (placer.path.Looped ? 0 : 1);

        float waypointIntervalNormalized = 1f / finalIndex;

        placer.waypointParent = new GameObject("Waypoints").transform;
        placer.waypointParent.SetParent(placer.gameObject.transform);
        Undo.RegisterCreatedObjectUndo(placer.waypointParent.gameObject, "Created waypoint parent");

        for(int i = 0; i < finalIndex; i++){
            float positionNormalized = i * waypointIntervalNormalized;

            CreateWaypointWithUndo(i, positionNormalized);
        }

        if(!placer.path.Looped){
            CreateWaypointWithUndo(finalIndex, 1);
        }

        Undo.CollapseUndoOperations(undoIndex);
    }

    void CreateWaypointWithUndo(int index, float positionNormalized){
        Vector3 position = placer.path.EvaluatePositionAtUnit(positionNormalized, CinemachinePathBase.PositionUnits.Normalized);
        Quaternion rotation = placer.path.EvaluateOrientationAtUnit(positionNormalized, CinemachinePathBase.PositionUnits.Normalized);

        Undo.RecordObject(placer, "Array");
        placer.waypoints[index] = PrefabUtility.InstantiatePrefab(placer.waypointPrefab, placer.waypointParent) as GameObject;
        placer.waypoints[index].transform.position = position;
        placer.waypoints[index].transform.rotation = rotation;
        placer.waypoints[index].name = $"{placer.waypointPrefab.name.Substring(0, 8)}...{index.ToString("0000")}";
        // placer.waypoints[i].hideFlags = HideFlags.HideInHierarchy;
        Undo.RegisterCreatedObjectUndo(placer.waypoints[index], $"Created new waypoint {index}");
    }

    private void RemoveAllWaypoints(){
        int undoIndex = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName($"Remove Waypoints");

        for(int i = 0; i < placer.waypoints.Length; i++){
            if(placer.waypoints[i] == null) continue;
            Undo.DestroyObjectImmediate(placer.waypoints[i]);
        }

        Undo.RecordObject(placer, "Clear waypoint array");
        placer.waypoints = null;

        Undo.DestroyObjectImmediate(placer.waypointParent.gameObject);

        Undo.CollapseUndoOperations(undoIndex);
    }

    private bool DoesPathExist(){
        return placer.waypoints != null && placer.waypoints.Length > 0;
    }
}

#endif

}