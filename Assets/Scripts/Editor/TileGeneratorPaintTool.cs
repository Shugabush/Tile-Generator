#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor;

[EditorTool("Paint Tool", typeof(TileGenerator))]
public class TileGeneratorPaintTool : EditorTool
{
    void BeforeSceneGUI(SceneView sceneView)
    {
        if (!ToolManager.IsActiveTool(this)) return;

        if (Event.current.type == EventType.MouseDrag)
        {
            Event.current.Use();
        }

        if (Event.current.type == EventType.MouseUp)
        {
            Event.current.Use();
        }
    }

    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView)) return;

        if (!ToolManager.IsActiveTool(this)) return;

        if (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent<TileGenerator>(out _)) return;
    }

    public override void OnActivated()
    {
        base.OnActivated();

        SceneView.beforeSceneGui += BeforeSceneGUI;
    }

    public override void OnWillBeDeactivated()
    {
        base.OnWillBeDeactivated();

        SceneView.beforeSceneGui -= BeforeSceneGUI;
    }
}
#endif