using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UIElements;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Soulspace
{
    public class ObeliskRandomizer : MonoBehaviour
    {
        [SerializeField, MinMaxSlider(-20, 20)] private Vector2 rotationRange;
        [SerializeField] private Vector3 randomizeAxisFactor = Vector3.one;

        private float RandomInRotationRange => Random.Range(rotationRange.x, rotationRange.y);
        
        public void RandomizeObelisk(){
            foreach(MeshRenderer meshRenderer in GetComponentsInChildren<MeshRenderer>()){
                meshRenderer.transform.localRotation = Quaternion.Euler(RandomInRotationRange * randomizeAxisFactor.x, RandomInRotationRange * randomizeAxisFactor.y, RandomInRotationRange * randomizeAxisFactor.z);
            }
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Soulspace.ObeliskRandomizer)), CanEditMultipleObjects]
public class ObeliskRandomizerEditor : Editor {
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUILayout.Button("Randomize Obelisk")){
            int UndoIndex = Undo.GetCurrentGroup();
            foreach(Object inspectedObject in serializedObject.targetObjects){
                Soulspace.ObeliskRandomizer randomizer = (Soulspace.ObeliskRandomizer) inspectedObject;
                if(randomizer == null) continue;

                List<Transform> transforms = new List<Transform>();
                
                foreach(MeshRenderer mr in randomizer.GetComponentsInChildren<MeshRenderer>()){
                    transforms.Add(mr.transform);
                }
                
                Undo.RecordObjects(transforms.ToArray(), $"Randomize Obelisk {randomizer.gameObject.name}");
                randomizer.RandomizeObelisk();
            }
            Undo.CollapseUndoOperations(UndoIndex);
            Undo.SetCurrentGroupName($"Randomize {serializedObject.targetObjects.Length} Obelisk{(serializedObject.targetObjects.Length > 1 ? "s" : "")}");
        }
    }
}

#endif