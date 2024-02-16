#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor;

namespace TileGeneration
{
    [EditorTool("Paint Tool", typeof(TileGenerator))]
    public class TileGeneratorPaintTool : EditorTool
    {
        bool isClicking = false;

        TileGenerator tileGenerator;

        void BeforeSceneGUI(SceneView sceneView)
        {
            if (!ToolManager.IsActiveTool(this)) return;
            if (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent<TileGenerator>(out _)) return;

            if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && Event.current.button == 0)
            {
                isClicking = true;
                Event.current.Use();
            }
            else
            {
                isClicking = false;
            }

            if (Event.current.type == EventType.ScrollWheel)
            {
                int scrollDir = System.Math.Sign(Event.current.delta.y);

                if (scrollDir == 1)
                {
                    tileGenerator.PaintRadius--;
                    Event.current.Use();
                }
                else if (scrollDir == -1)
                {
                    tileGenerator.PaintRadius++;
                    Event.current.Use();
                }
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                Event.current.Use();
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView)) return;

            if (!ToolManager.IsActiveTool(this)) return;

            if (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent<TileGenerator>(out _)) return;
            
            if (tileGenerator.GetSelectedPoint(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out Vector3 point))
            {
                Handles.matrix = Matrix4x4.TRS(point,
                    tileGenerator.transform.rotation,
                    Vector3.Scale(tileGenerator.GetGridScaleRatio(), tileGenerator.transform.localScale) * 
                    tileGenerator.PaintRadius);

                Handles.DrawWireDisc(Vector3.zero, Vector3.up, 1);
            }
            else
            {
                Handles.DrawWireDisc(GetCurrentMousePositionInScene(), Vector3.up, 0.5f);
            }

            Handles.matrix = Matrix4x4.identity;

            if (isClicking)
            {
                tileGenerator.ChangeTile();
            }

            base.OnToolGUI(window);

            window.Repaint();
        }

        public override void OnActivated()
        {
            base.OnActivated();
            if (tileGenerator == null)
            {
                tileGenerator = (TileGenerator)target;
            }

            SceneView.beforeSceneGui += BeforeSceneGUI;
        }

        public override void OnWillBeDeactivated()
        {
            base.OnWillBeDeactivated();

            SceneView.beforeSceneGui -= BeforeSceneGUI;
            tileGenerator.selectedTileIndex.x = -1;
            tileGenerator.selectedTileIndex.z = -1;
        }

        Vector3 GetCurrentMousePositionInScene()
        {
            Vector3 mousePosition = Event.current.mousePosition;
            var placeObject = HandleUtility.PlaceObject(mousePosition, out var newPosition, out var normal);

            return placeObject ? newPosition : HandleUtility.GUIPointToWorldRay(mousePosition).GetPoint(10);
        }
    }
}
#endif