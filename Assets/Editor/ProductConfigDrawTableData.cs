using Data.Product;
using UnityEditor;
using UnityEngine;
using Utils.DesignPattern.Singleton;

[CustomEditor(typeof(ProductSO))]
public class ProductConfigDrawTableData : Editor
{
    private SerializedProperty productProp;      // ListProductConf
    public SerializedProperty productTypeProp;  // ListProductType

    // Ghi nhớ loại vừa thêm để set mặc định cho Product mới
    private int _lastAddedTypeIndex = -1;

    private Vector2 scrollTypes;
    private Vector2 scrollProducts;

    private readonly string[] headerProductTypes = { "Id", "Name" };
    private readonly float[] widthProductTypes = { 40, 160 };

    private readonly string[] headerProducts = {
        "Id", "Type", "Interval", "Lifetime", "Cost", "Price", "Pack Size", "Deadline"
    };
    private readonly float[] widthProducts = { 40, 100, 80, 80, 80, 80, 80, 80 };

    private void OnEnable()
    {
        productTypeProp = serializedObject.FindProperty("ListProductType");
        productProp = serializedObject.FindProperty("ListProductConf");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (productTypeProp == null || productProp == null)
        {
            EditorGUILayout.HelpBox("Không tìm thấy ListProductType hoặc ListProductConf.", MessageType.Error);
            return;
        }

        DrawTableForProductType();
        EditorGUILayout.Space(10);
        DrawTableForProduct();

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    // ========== PRODUCT TYPE TABLE ==========

    private void DrawTableForProductType()
    {
        EditorGUILayout.LabelField("Product Type Config Data", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // Header
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < headerProductTypes.Length; i++)
            GUILayout.Label(headerProductTypes[i], EditorStyles.miniBoldLabel, GUILayout.Width(widthProductTypes[i]));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Body
        scrollTypes = EditorGUILayout.BeginScrollView(scrollTypes, GUILayout.Height(120));
        for (int i = 0; i < productTypeProp.arraySize; i++)
        {
            var row = productTypeProp.GetArrayElementAtIndex(i);
            var idProp = row.FindPropertyRelative("Id");
            var nameProp = row.FindPropertyRelative("Name");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(widthProductTypes[0]));

            // Name
            nameProp.stringValue = EditorGUILayout.TextField(nameProp.stringValue, GUILayout.Width(widthProductTypes[1]));

            // Delete
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                productTypeProp.DeleteArrayElementAtIndex(i);
                if (_lastAddedTypeIndex == i) _lastAddedTypeIndex = -1;
                EditorGUILayout.EndHorizontal();
                continue;
            }

            EditorGUILayout.EndHorizontal();
            idProp.intValue = i + 1;
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(5);
        if (GUILayout.Button("+ Add New Type", GUILayout.Height(24)))
        {
            int newIndex = productTypeProp.arraySize;
            productTypeProp.InsertArrayElementAtIndex(newIndex);

            var newRow = productTypeProp.GetArrayElementAtIndex(newIndex);
            newRow.FindPropertyRelative("Id").intValue = newIndex + 1;
            newRow.FindPropertyRelative("Name").stringValue = $"Type {newIndex + 1}";
            _lastAddedTypeIndex = newIndex;
        }
    }

    // ========== PRODUCT TABLE ==========

    private void DrawTableForProduct()
    {
        EditorGUILayout.LabelField("Product Config Data", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // Header
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < headerProducts.Length; i++)
            GUILayout.Label(headerProducts[i], EditorStyles.miniBoldLabel, GUILayout.Width(widthProducts[i]));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        int typeCount = productTypeProp.arraySize;
        string[] typeOptions = new string[typeCount];
        for (int t = 0; t < typeCount; t++)
        {
            var tp = productTypeProp.GetArrayElementAtIndex(t);
            int id = tp.FindPropertyRelative("Id").intValue;
            string nm = tp.FindPropertyRelative("Name").stringValue;
            typeOptions[t] = $"{id} - {nm}";
        }

        // Body
        scrollProducts = EditorGUILayout.BeginScrollView(scrollProducts, GUILayout.Height(120));
        for (int i = 0; i < productProp.arraySize; i++)
        {
            var row = productProp.GetArrayElementAtIndex(i);

            var idProp = row.FindPropertyRelative("Id");

            var typeNested = row.FindPropertyRelative("ProductType");
            var typeIdProp = typeNested.FindPropertyRelative("Id");
            var typeNameProp = typeNested.FindPropertyRelative("Name");

            var intervalProp = row.FindPropertyRelative("Interval");
            var lifetimeProp = row.FindPropertyRelative("Lifetime");
            var costProp = row.FindPropertyRelative("Cost");
            var priceProp = row.FindPropertyRelative("Price");
            var packProp = row.FindPropertyRelative("PackSize");
            var deadlineProp = row.FindPropertyRelative("Deadline");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(widthProducts[0]));
            idProp.intValue = i + 1;

            // Type popup
            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(widthProducts[1])))
            {
                if (typeCount > 0)
                {
                    int currentIdx = GetTypeIndexById(typeIdProp.intValue);
                    if (currentIdx < 0) currentIdx = 0;

                    int newIdx = EditorGUILayout.Popup(currentIdx, typeOptions);
                    if (newIdx != currentIdx && newIdx >= 0 && newIdx < typeCount)
                    {
                        var pick = productTypeProp.GetArrayElementAtIndex(newIdx);
                        typeIdProp.intValue = pick.FindPropertyRelative("Id").intValue;
                        typeNameProp.stringValue = pick.FindPropertyRelative("Name").stringValue;
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("—");
                }
            }

            IntField(intervalProp, widthProducts[2]);
            IntField(lifetimeProp, widthProducts[3]);
            IntField(costProp, widthProducts[4]);
            IntField(priceProp, widthProducts[5]);
            IntField(packProp, widthProducts[6]);
            IntField(deadlineProp, widthProducts[7]);

            // Delete
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                productProp.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
                continue;
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(5);
        if (GUILayout.Button("+ Add New Product", GUILayout.Height(24)))
        {
            int newIndex = productProp.arraySize;
            productProp.InsertArrayElementAtIndex(newIndex);

            var newRow = productProp.GetArrayElementAtIndex(newIndex);
            newRow.FindPropertyRelative("Id").intValue = newIndex + 1;

            int defaultTypeIndex = -1;
            defaultTypeIndex = productProp.arraySize - 1;
            if (defaultTypeIndex < 0 || defaultTypeIndex > productTypeProp.arraySize - 1) { return; }

            newRow.FindPropertyRelative("Interval").intValue = 0;
            newRow.FindPropertyRelative("Lifetime").intValue = 0;
            newRow.FindPropertyRelative("Cost").intValue = 0;
            newRow.FindPropertyRelative("Price").intValue = 0;
            newRow.FindPropertyRelative("PackSize").intValue = 0;
            newRow.FindPropertyRelative("Deadline").intValue = 0;
        }
    }

    // ========== HELPERS ==========

    private int GetTypeIndexById(int id)
    {
        for (int i = 0; i < productTypeProp.arraySize; i++)
        {
            var tp = productTypeProp.GetArrayElementAtIndex(i);
            if (tp.FindPropertyRelative("Id").intValue == id) return i;
        }
        return -1;
    }

    private static void IntField(SerializedProperty prop, float width)
    {
        prop.intValue = EditorGUILayout.IntField(prop.intValue, GUILayout.Width(width));
    }
}
