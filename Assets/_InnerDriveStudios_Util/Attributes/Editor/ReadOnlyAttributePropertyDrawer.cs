using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyAttributePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        bool guiEnabled = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField(position, prop, label);
        GUI.enabled = guiEnabled;
    }
}