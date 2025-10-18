using UnityEditor;
using UnityEngine;
using Data.Product;

[CustomEditor(typeof(WorkerSO))]
public class WorkerConfigDrawTableData : Editor
{
    private SerializedProperty _workerProp;

    private Vector2 _scroll;
    private readonly string[] _headers = { "Name", "Cost", "Time Task", "Pack Size" };
    private readonly float[] _widths = { 100f, 100f, 100f, 100f };

    private void OnEnable()
    {
        _workerProp = serializedObject.FindProperty("WorkerConfig");
    }

    public override void OnInspectorGUI()
    {
        if (_workerProp == null)
        {
            EditorGUILayout.HelpBox("Not found 'WorkerConfig' in WorkerSO.", MessageType.Error);
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
        if (_workerProp.isArray)
        {
            if (_workerProp.arraySize == 0)
                _workerProp.InsertArrayElementAtIndex(0);

            while (_workerProp.arraySize > 1)
                _workerProp.DeleteArrayElementAtIndex(_workerProp.arraySize - 1);
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

        SerializedProperty row = _workerProp;

        var name = row.FindPropertyRelative("Name");
        var cose = row.FindPropertyRelative("Cost");
        var timeTask = row.FindPropertyRelative("TimeTask");
        var packSize = row.FindPropertyRelative("PackSize");

        using (var cc = new EditorGUI.ChangeCheckScope())
        {
            EditorGUILayout.BeginHorizontal();
            name.stringValue = EditorGUILayout.TextField(name.stringValue, GUILayout.Width(_widths[0]));
            cose.intValue = EditorGUILayout.IntField(cose.intValue, GUILayout.Width(_widths[1]));
            timeTask.intValue = EditorGUILayout.IntField(timeTask.intValue, GUILayout.Width(_widths[2]));
            packSize.intValue = EditorGUILayout.IntField(packSize.intValue, GUILayout.Width(_widths[3]));
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
