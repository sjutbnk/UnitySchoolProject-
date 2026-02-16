using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Генерация сетки", EditorStyles.boldLabel);

        GameManager manager = (GameManager)target;

        if (GUILayout.Button("Сгенерировать сетку", GUILayout.Height(30)))
        {
            GenerateGridInEditor(manager);
        }

        if (GUILayout.Button("Удалить сетку", GUILayout.Height(30)))
        {
            ClearGridInEditor(manager);
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "Сетка будет создана в редакторе и сохранена в сцене. " +
            "Препятствия расставляются автоматически при запуске игры.", 
            MessageType.Info);
    }

    private void GenerateGridInEditor(GameManager manager)
    {
        if (manager.levelContainer == null)
        {
            GameObject container = GameObject.Find("LevelContainer");
            if (container == null)
            {
                container = new GameObject("LevelContainer");
            }
            manager.levelContainer = container.transform;
        }

        // Очищаем старую сетку если есть
        ClearGridInEditor(manager);

        // Создаем контейнер для сетки
        GameObject gridContainer = new GameObject("Grid");
        gridContainer.transform.SetParent(manager.levelContainer);
        gridContainer.transform.SetAsFirstSibling();

        int offset = manager.gridSize / 2;
        
        for (int x = 0; x < manager.gridSize; x++)
        {
            for (int z = 0; z < manager.gridSize; z++)
            {
                Vector3 pos = new Vector3(x - offset, 0, z - offset);
                
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.name = $"Floor_{x}_{z}";
                tile.transform.position = pos;
                tile.transform.rotation = Quaternion.Euler(90, 0, 0);
                tile.transform.localScale = Vector3.one * manager.cellSize * 0.95f;
                
                Renderer renderer = tile.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Создаем материал для тайла
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = (x + z) % 2 == 0
                        ? new Color(0.18f, 0.18f, 0.20f)
                        : new Color(0.22f, 0.22f, 0.24f);
                    renderer.material = mat;
                }

                // Удаляем коллайдер
                Collider col = tile.GetComponent<Collider>();
                if (col != null)
                {
                    DestroyImmediate(col);
                }

                tile.transform.SetParent(gridContainer.transform);
                tile.isStatic = true;
            }
        }

        Undo.RegisterCreatedObjectUndo(gridContainer, "Generate Grid");
        EditorUtility.SetDirty(manager.levelContainer.gameObject);
        Debug.Log($"Сетка {manager.gridSize}x{manager.gridSize} сгенерирована!");
    }

    private void ClearGridInEditor(GameManager manager)
    {
        if (manager.levelContainer == null)
        {
            GameObject container = GameObject.Find("LevelContainer");
            if (container != null)
            {
                manager.levelContainer = container.transform;
            }
            else
            {
                return;
            }
        }

        // Находим и удаляем сетку
        Transform gridTransform = manager.levelContainer.Find("Grid");
        if (gridTransform != null)
        {
            Undo.DestroyObjectImmediate(gridTransform.gameObject);
            EditorUtility.SetDirty(manager.levelContainer.gameObject);
            Debug.Log("Сетка удалена!");
        }

        // Также удаляем старые тайлы если они были созданы по-другому
        for (int i = manager.levelContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = manager.levelContainer.GetChild(i);
            if (child.name.StartsWith("Floor") && child.name != "Floor")
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }
        }
    }
}
