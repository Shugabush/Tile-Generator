#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TilePalette))]
public class TilePaletteEditor : Editor
{
    TilePalette palette;

    // Target y index for display
    int targetYIndex = 0;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (palette == null)
        {
            palette = (TilePalette)target;
        }

        EditorGUI.BeginChangeCheck();

        foreach (var set in palette.objectSets)
        {
            int objectsXLength = EditorGUILayout.IntField(set.objects.GetLength(0));
            
            int objectsYLength = EditorGUILayout.IntField(set.objects.GetLength(1));

            int objectsZLength = EditorGUILayout.IntField(set.objects.GetLength(2));

            if (set.ChangeObjectDimensions(objectsXLength, objectsYLength, objectsZLength))
            {
                EditorUtility.SetDirty(palette);
                set.Save();
            }

            set.Load();

            for (int x = 0; x < set.objects.GetLength(0); x++)
            {
                for (int z = 0; z < set.objects.GetLength(2); z++)
                {
                    set.objects[x, targetYIndex,z ] =
                    (GameObject)EditorGUI.ObjectField(new Rect(x * 50, 100 + z * 50, 25, 50),
                    set.objects[x, targetYIndex, z], typeof(GameObject), false);
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            foreach (var set in palette.objectSets)
            {
                set.Save();
            }
            EditorUtility.SetDirty(palette);
        }
    }
}
#endif