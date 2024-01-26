#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TilePalette))]
public class TilePaletteEditor : Editor
{
    TilePalette palette;

    // Target y index for display
    int targetYIndex = 1;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (palette == null)
        {
            palette = (TilePalette)target;
        }

        EditorGUI.BeginChangeCheck();

        // Make sure the objects field below are interactable
        // Hopefully there is a better way to do this,
        // but right now this is okay
        GUILayout.Space(500);

        float inspectorWidth = EditorGUIUtility.currentViewWidth;
        float inspectorWidthRatio = inspectorWidth / 3f;

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(palette);
        }
    }
}
#endif