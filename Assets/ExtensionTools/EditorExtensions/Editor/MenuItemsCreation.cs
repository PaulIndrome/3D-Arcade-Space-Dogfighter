using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ExtensionTools.Gameplay;
using ExtensionTools.Helpers;

#if UNITY_EDITOR
namespace ExtensionTools
{
	public class MenuItemsCreation : MonoBehaviour
	{
		[MenuItem("GameObject/Extension Tools/Flying Camera", false, 10)]
		static void CreateFlyingCamera(MenuCommand menuCommand)
		{
			// Create a custom game object
			GameObject go = new GameObject("Flying Camera");
			go.AddComponent(typeof(UnityEngine.Camera));
			go.AddComponent(typeof(FlyingCameraMovement));
			go.tag = "MainCamera";
			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
			Selection.activeObject = go;
		}

		[MenuItem("GameObject/Extension Tools/Create Note", false, 10)]
		static void CreateNote(MenuCommand menuCommand)
		{
			// Create a custom game object
			GameObject go = new GameObject("Note");
			go.AddComponent(typeof(Note));
			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
			Selection.activeObject = go;
		}

		[MenuItem("GameObject/Extension Tools/Character", false, 10)]
		static void CreateCharacter(MenuCommand menuCommand)
		{
			// Create a custom game object
			GameObject go = new GameObject("Character");
			go.AddComponent(typeof(UnityEngine.CharacterController));
			go.AddComponent(typeof(UnityEngine.CapsuleCollider));
			go.AddComponent(typeof(CharacterMovement));

			GameObject goCamera = new GameObject("Character Camera");
			goCamera.transform.parent = go.transform;
			goCamera.transform.localPosition = Vector3.zero;
			goCamera.transform.localRotation = Quaternion.identity;
			goCamera.transform.localScale = Vector3.one;
			goCamera.AddComponent(typeof(UnityEngine.Camera));
			goCamera.tag = "MainCamera";


			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
			Selection.activeObject = go;
		}
	}
}
#endif
