#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class CreatePanelSettings
{
    [MenuItem("Tools/Create Mobile Panel Settings")]
    public static void Create()
    {
        var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        
        // Настройки для телефона (портрет)
        panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
        panelSettings.referenceDpi = 160;
        panelSettings.fallbackDpi = 160;
        
        AssetDatabase.CreateAsset(panelSettings, "Assets/MobilePanelSettings.asset");
        AssetDatabase.SaveAssets();
        
        Debug.Log("Panel Settings создан: Assets/MobilePanelSettings.asset");
        
        // Выделяем в Project окне
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = panelSettings;
    }
}
#endif
