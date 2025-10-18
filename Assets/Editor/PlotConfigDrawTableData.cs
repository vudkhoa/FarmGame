using UnityEditor;
using UnityEngine;
using Data.Product;

[CustomEditor(typeof(PlotSO))]
public class PlotConfigDrawTableData : Editor
{
    private SerializedProperty _plotProp;

    private Vector2 _scroll;
    private readonly string[] _headers = { "Cost", "Pack Size" };
    private readonly float[] _widths = { 100f, 100f };

    private void OnEnable()
    {
        _plotProp = serializedObject.FindProperty("PlotConfig");
    }

    public override void OnInspectorGUI()
    {
        if (_plotProp == null)
        {
            EditorGUILayout.HelpBox("Not found 'PlotConfig' in PlotSO.", MessageType.Error);
            return;
        }

        serializedObject.Update();
        EnsureSingleRow();
        EditorGUILayout.Space(6);
        this.DrawWorkerData();

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private void EnsureSingleRow()
    {
        if (_plotProp.isArray)
        {
            if (_plotProp.arraySize == 0)
                _plotProp.InsertArrayElementAtIndex(0);

            while (_plotProp.arraySize > 1)
                _plotProp.DeleteArrayElementAtIndex(_plotProp.arraySize - 1);
        }
    }

    private void DrawWorkerData()
    {
        EditorGUILayout.LabelField("Equipment Config Data", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < _headers.Length; i++)
            GUILayout.Label(_headers[i], EditorStyles.miniBoldLabel, GUILayout.Width(_widths[i]));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(120));

        SerializedProperty row = _plotProp;

        var name = row.FindPropertyRelative("Cost");
        var packSize = row.FindPropertyRelative("PackSize");

        using (var cc = new EditorGUI.ChangeCheckScope())
        {
            EditorGUILayout.BeginHorizontal();
            name.intValue = EditorGUILayout.IntField(name.intValue, GUILayout.Width(_widths[0]));
            packSize.intValue = EditorGUILayout.IntField(packSize.intValue, GUILayout.Width(_widths[1]));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (cc.changed)
            {
                Undo.RecordObject(target, "Edit Equipment Config");
            }
        }

        EditorGUILayout.EndScrollView();
    }
}
