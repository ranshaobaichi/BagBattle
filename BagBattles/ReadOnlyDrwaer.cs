using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Assets.Editor.ItemAttributeDrawer.ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 开始禁用组
        GUI.enabled = false;
        
        // 绘制属性（禁用状态）
        EditorGUI.PropertyField(position, property, label, true);
        
        // 结束禁用组
        GUI.enabled = true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 使用默认高度
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}