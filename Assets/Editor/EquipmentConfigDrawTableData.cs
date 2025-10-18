using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EquipmentSO))]
public class EquipmentConfigDrawTableData : Editor
{
    private SerializedProperty _equipmentProp;

    private Vector2 _scroll;
    private readonly string[] _headers = { "Percent", "Limit Level", "Cost", "Pack Size" };
    private readonly float[] _widths = { 100f, 100f, 100f, 100f };

    private void OnEnable()
    {
        _equipmentProp = serializedObject.FindProperty("EquipmentConfig");
    }

    public override void OnInspectorGUI()
    {
        if (_equipmentProp == null)
        {
            EditorGUILayout.HelpBox("Not found 'EquipmentConfig' in EquipmentSO.", MessageType.Error);
            return;
        }

        serializedObject.Update();
        EnsureSingleRow();
        EditorGUILayout.Space(6);
        this.DrawEquipmentData();

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private void EnsureSingleRow()
    {
        if (_equipmentProp.isArray)
        {
            if (_equipmentProp.arraySize == 0)
                _equipmentProp.InsertArrayElementAtIndex(0);

            while (_equipmentProp.arraySize > 1)
                _equipmentProp.DeleteArrayElementAtIndex(_equipmentProp.arraySize - 1);
        }
    }

    private void DrawEquipmentData()
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

        SerializedProperty row = _equipmentProp;

        var percent = row.FindPropertyRelative("Percent");
        var limitLevel = row.FindPropertyRelative("LimitLevel");
        var cost = row.FindPropertyRelative("Cost");
        var packSize = row.FindPropertyRelative("PackSize");

        using (var cc = new EditorGUI.ChangeCheckScope())
        {
            EditorGUILayout.BeginHorizontal();
            percent.intValue = EditorGUILayout.IntField(percent.intValue, GUILayout.Width(_widths[0]));
            limitLevel.intValue = EditorGUILayout.IntField(limitLevel.intValue, GUILayout.Width(_widths[1]));
            cost.intValue = EditorGUILayout.IntField(cost.intValue, GUILayout.Width(_widths[2]));
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
