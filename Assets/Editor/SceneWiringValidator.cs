using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Editor-only script that validates all serialized ObjectReference fields on key
/// scene components (SceneBootstrapper, PlayfieldRenderer, UIDocuments) are non-null.
/// Flags missing references as warnings.
/// Run via: Unity -batchmode -executeMethod SceneWiringValidator.ValidateAndExit
/// Or via menu: Tools/Validate Scene Wiring
/// </summary>
public static class SceneWiringValidator
{
    [MenuItem("Tools/Validate Scene Wiring")]
    public static void Validate()
    {
        int warnings = 0;

        // Validate SceneBootstrapper serialized references
        warnings += ValidateComponentReferences<SceneBootstrapper>();

        // Validate PlayfieldRenderer serialized references
        warnings += ValidateComponentReferences<PlayfieldRenderer>();

        // Validate UIDocument sourceAsset and m_PanelSettings on all UI screen GameObjects
        warnings += ValidateUIDocuments();

        if (warnings == 0)
        {
            Debug.Log("[SceneWiringValidator] All scene wiring references are valid. No warnings found.");
        }
        else
        {
            Debug.LogWarning("[SceneWiringValidator] Found " + warnings + " missing reference warning(s). See above for details.");
        }
    }

    /// <summary>
    /// Command-line entry point for batch mode validation.
    /// </summary>
    [MenuItem("Tools/Validate Scene Wiring and Exit")]
    public static void ValidateAndExit()
    {
        Validate();
        EditorApplication.Exit(0);
    }

    /// <summary>
    /// Validates all serialized ObjectReference fields on every instance of the given
    /// MonoBehaviour type found in the current scene. Returns the number of warnings.
    /// </summary>
    private static int ValidateComponentReferences<T>() where T : MonoBehaviour
    {
        int warnings = 0;
        T[] components = Object.FindObjectsByType<T>(FindObjectsSortMode.None);

        foreach (T component in components)
        {
            if (component == null || component.gameObject == null)
                continue;

            string componentName = component.gameObject.name + " (" + typeof(T).Name + ")";
            var so = new SerializedObject(component);
            var iterator = so.GetIterator();

            // Traverse all properties
            if (iterator.Next(true))
            {
                do
                {
                    // Check ObjectReference properties for null values
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference &&
                        iterator.objectReferenceValue == null)
                    {
                        // Skip m_ParentUI and m_WorldSpaceCollider on UIDocument - these are only for WorldSpace mode
                        if (typeof(T) == typeof(UIDocument) &&
                            (iterator.name == "m_ParentUI" || iterator.name == "m_WorldSpaceCollider"))
                        {
                            continue;
                        }

                        Debug.LogWarning("[SceneWiringValidator] Missing reference: " + componentName + "." + iterator.name + " is null");
                        warnings++;
                    }
                }
                while (iterator.Next(false));
            }
        }

        return warnings;
    }

    /// <summary>
    /// Validates UIDocument sourceAsset and m_PanelSettings assignments on all UI
    /// screen GameObjects in the current scene.
    /// </summary>
    private static int ValidateUIDocuments()
    {
        int warnings = 0;
        UIDocument[] documents = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);

        foreach (UIDocument doc in documents)
        {
            if (doc == null || doc.gameObject == null)
                continue;

            string componentName = doc.gameObject.name + " (UIDocument)";
            var so = new SerializedObject(doc);

            // Check sourceAsset
            var sourceAssetProp = so.FindProperty("sourceAsset");
            if (sourceAssetProp != null && sourceAssetProp.objectReferenceValue == null)
            {
                Debug.LogWarning("[SceneWiringValidator] Missing reference: " + componentName + ".sourceAsset is null");
                warnings++;
            }

            // Check m_PanelSettings
            var panelSettingsProp = so.FindProperty("m_PanelSettings");
            if (panelSettingsProp != null && panelSettingsProp.objectReferenceValue == null)
            {
                Debug.LogWarning("[SceneWiringValidator] Missing reference: " + componentName + ".m_PanelSettings is null");
                warnings++;
            }
        }

        return warnings;
    }
}
