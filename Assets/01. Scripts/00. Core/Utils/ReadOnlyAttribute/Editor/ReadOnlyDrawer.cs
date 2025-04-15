#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// ReadOnlyAttribute를 인스펙터에서 처리하는 Drawer
/// </summary>
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif