using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Soulspace {
public abstract class WeaponSettingsBase : ScriptableObject {

#region public accessors
    public WeaponDamageModifiers WeaponDamage => weaponDamage;
    public float ProjectileExitVelocity => projectileExitVelocity;
    public float ProjectileTimeoutDelay => projectileTimeoutDelay;
    public float TargetLockRadius => targetLockRadius;
#endregion

    public abstract float MaxWeaponRange { get; }
    public abstract ProjectileBase ProjectileBase { get; }

    [Header("Game Settings")]
    [SerializeField] protected int projectilePoolSize = 50;
    [SerializeField] protected float targetLockRadius = 1f;

    [Header("Base Projectile Settings")]
    [SerializeField] private WeaponDamageModifiers weaponDamage;
    [SerializeField] private float projectileExitVelocity;
    [SerializeField] private float projectileTimeoutDelay;

    [Header("Asset references")]
    [SerializeField] protected ProjectileBase projectilePrefab;

    protected abstract void OnValidate();
}


#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(WeaponSettingsBase), true)]
public class WeaponSettingsBasePropertyDrawer : PropertyDrawer {

    // Cached scriptable object editor
    private Editor editor = null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, label, true);

        // Draw foldout arrow
        if (property.objectReferenceValue != null)
        {
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
        }
 
        // Draw foldout properties
        if (property.isExpanded)
        {
            // Make child fields be indented
            EditorGUI.indentLevel++;
         
            // Draw object properties
            if (!editor)
                Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);
            
            if(editor)
                editor.OnInspectorGUI();
         
            // Set indent back to what it was
            EditorGUI.indentLevel--;
        }
    }
}

#endif

}