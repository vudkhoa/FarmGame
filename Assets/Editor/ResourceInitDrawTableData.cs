using UnityEditor;
using UnityEngine;
using Data.Product;

[CustomEditor(typeof(ResourcesInitSO))]
public class ResourceInitDrawTableData : Editor
{
    private SerializedProperty _resourceByNameProp;
    private SerializedProperty _resourceListProp;

    private SerializedProperty _productConfigProp;
    private SerializedObject _productConfigSO;
    private SerializedProperty _productTypeListProp;

    private Vector2 _scroll;
    private readonly string[] _headerResourceProduct = { "Product Type", "Amount"};
    private readonly float[] _widthResourceProduct = { 180f, 100f };

    private readonly string[] _headerResourceByName = { "Name", "Amount" };
    private readonly float[] _widthResourceByName = { 100f, 100f };

    private void OnEnable()
    {
        _resourceListProp = serializedObject.FindProperty("ListResourceInit");
        _productConfigProp = serializedObject.FindProperty("ProductConfig");
        RefreshProductConfigBindings();
        _resourceByNameProp = serializedObject.FindProperty("ListResourceByName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Chọn/hiển thị tham chiếu ProductSO
        EditorGUILayout.PropertyField(_productConfigProp, new GUIContent("Product Config (ProductSO)"));
        if (GUI.changed) RefreshProductConfigBindings();

        // Cảnh báo nếu chưa trỏ tới ProductSO
        if (_productConfigSO == null || _productTypeListProp == null)
        {
            EditorGUILayout.HelpBox("Hãy gán 'ProductSO' vào trường Product Config để lấy danh sách ProductType.", MessageType.Warning);
        }

        EditorGUILayout.Space(6);
        this.DrawResourceInitTable();
        this.DrawResourceByNameTable();

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private void RefreshProductConfigBindings()
    {
        if (_productConfigProp != null && _productConfigProp.objectReferenceValue != null)
        {
            _productConfigSO = new SerializedObject(_productConfigProp.objectReferenceValue);
            _productConfigSO.Update();
            _productTypeListProp = _productConfigSO.FindProperty("ListProductType");
        }
        else
        {
            _productConfigSO = null;
            _productTypeListProp = null;
        }
    }

    private void DrawResourceInitTable()
    {
        EditorGUILayout.LabelField("Resource Init Data", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // Header
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < _headerResourceProduct.Length; i++)
            GUILayout.Label(_headerResourceProduct[i], EditorStyles.miniBoldLabel, GUILayout.Width(_widthResourceProduct[i]));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Chuẩn bị options cho popup Type
        string[] typeOptions = BuildTypeOptions(out int typeCount);

        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(130));
        for (int i = 0; i < _resourceListProp.arraySize; i++)
        {
            var row = _resourceListProp.GetArrayElementAtIndex(i);
            var typeProp = row.FindPropertyRelative("ProductType");
            var typeId = typeProp != null ? typeProp.FindPropertyRelative("Id") : null;
            var typeName = typeProp != null ? typeProp.FindPropertyRelative("Name") : null;
            var amount = row.FindPropertyRelative("Amount");

            EditorGUILayout.BeginHorizontal();

            // Popup Type
            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(_widthResourceProduct[0])))
            {
                if (typeCount > 0 && typeProp != null && typeId != null && typeName != null)
                {
                    int currentIndex = GetTypeIndexById(typeId.intValue);
                    if (currentIndex < 0) currentIndex = 0;

                    int picked = EditorGUILayout.Popup(currentIndex, typeOptions);
                    if (picked != currentIndex && picked >= 0 && picked < typeCount)
                    {
                        var pick = _productTypeListProp.GetArrayElementAtIndex(picked);
                        var pickId = pick.FindPropertyRelative("Id").intValue;
                        var pickName = pick.FindPropertyRelative("Name").stringValue;
                        typeId.intValue = pickId;
                        typeName.stringValue = pickName;
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("— (chưa có ProductSO)");
                }
            }

            // Amount
            amount.intValue = EditorGUILayout.IntField(amount.intValue, GUILayout.Width(_widthResourceProduct[1]));

            // Delete
            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
                _resourceListProp.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
                continue;
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);
        if (GUILayout.Button("+ Add New Resource", GUILayout.Height(24)))
        {
            int newIndex = _resourceListProp.arraySize;
            _resourceListProp.InsertArrayElementAtIndex(newIndex);

            var newRow = _resourceListProp.GetArrayElementAtIndex(newIndex);
            //newRow.FindPropertyRelative("Id").intValue = newIndex + 1;

            // Nếu có ít nhất 1 ProductType thì set mặc định theo index 0
            if (_productTypeListProp != null && _productTypeListProp.arraySize > 0)
            {
                var pick = _productTypeListProp.GetArrayElementAtIndex(0);
                var tp = newRow.FindPropertyRelative("ProductType");
                tp.FindPropertyRelative("Id").intValue = pick.FindPropertyRelative("Id").intValue;
                tp.FindPropertyRelative("Name").stringValue = pick.FindPropertyRelative("Name").stringValue;
            }

            newRow.FindPropertyRelative("Amount").intValue = 0;
        }
    }

    private void DrawResourceByNameTable()
    {
        EditorGUILayout.LabelField("Resource By Name Data", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // Header
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < _headerResourceByName.Length; i++)
            GUILayout.Label(_headerResourceByName[i], EditorStyles.miniBoldLabel, GUILayout.Width(_widthResourceByName[i]));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        for (int i = 0; i < _resourceByNameProp.arraySize; i++)
        {
                var row = _resourceByNameProp.GetArrayElementAtIndex(i);
            var name = row.FindPropertyRelative("Name");
            var amount = row.FindPropertyRelative("Amount");

            EditorGUILayout.BeginHorizontal();

            name.stringValue = EditorGUILayout.TextField(name.stringValue, GUILayout.Width(_widthResourceByName[0]));
            amount.intValue = EditorGUILayout.IntField(amount.intValue, GUILayout.Width(_widthResourceByName[1]));

            // Delete
            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
                _resourceByNameProp.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
                continue;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(6);

        if (GUILayout.Button("+ Add New Resource By Name", GUILayout.Height(24)))
        {
            int newIndex = _resourceByNameProp.arraySize;
            _resourceByNameProp.InsertArrayElementAtIndex(newIndex);

            var newRow = _resourceByNameProp.GetArrayElementAtIndex(newIndex);

            newRow.FindPropertyRelative("Name").stringValue = "Name";
            newRow.FindPropertyRelative("Amount").intValue = 0;
        }
    }

    private int GetTypeIndexById(int id)
    {
        if (_productTypeListProp == null) return -1;
        for (int i = 0; i < _productTypeListProp.arraySize; i++)
        {
            var tp = _productTypeListProp.GetArrayElementAtIndex(i);
            if (tp.FindPropertyRelative("Id").intValue == id) return i;
        }
        return -1;
    }

    private string[] BuildTypeOptions(out int typeCount)
    {
        if (_productTypeListProp == null) { typeCount = 0; return System.Array.Empty<string>(); }

        typeCount = _productTypeListProp.arraySize;
        var options = new string[typeCount];
        for (int i = 0; i < typeCount; i++)
        {
            var tp = _productTypeListProp.GetArrayElementAtIndex(i);
            int id = tp.FindPropertyRelative("Id").intValue;
            string nm = tp.FindPropertyRelative("Name").stringValue;
            options[i] = $"{id} - {nm}";
        }
        return options;
    }
}
