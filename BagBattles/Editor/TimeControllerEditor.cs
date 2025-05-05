using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
[CustomEditor(typeof(TimeController))]
public class TimeControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("结束波次"))
        {
            TimeController.Instance.TimeUpEvent();
        }
    }
}