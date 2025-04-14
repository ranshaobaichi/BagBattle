#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnemyTypeMapping))]
public class EnemyTypeMappingDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // 使用更好看的显示方式
        SerializedProperty enemyTypeProperty = property.FindPropertyRelative("enemyType");
        SerializedProperty prefabProperty = property.FindPropertyRelative("prefab");

        // 计算绘制区域
        float lineHeight = EditorGUIUtility.singleLineHeight;
        Rect typeRect = new Rect(position.x, position.y, position.width * 0.4f, lineHeight);
        Rect prefabRect = new Rect(position.x + position.width * 0.4f + 5, position.y, position.width * 0.6f - 5, lineHeight);

        // 绘制敌人类型和预制体字段
        EditorGUI.PropertyField(typeRect, enemyTypeProperty, GUIContent.none);
        EditorGUI.PropertyField(prefabRect, prefabProperty, GUIContent.none);

        EditorGUI.EndProperty();
    }
}
#endif