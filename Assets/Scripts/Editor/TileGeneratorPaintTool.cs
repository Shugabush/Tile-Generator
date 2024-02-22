#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor;

namespace TileGeneration
{
    [EditorTool("Paint Tool", typeof(TileGenerator))]
    public class TileGeneratorPaintTool : EditorTool
    {
        bool isHoldingMouseButton = false;
        bool mouseUp = false;

        TileGenerator tileGenerator;

        void BeforeSceneGUI(SceneView sceneView)
        {
            if (!ToolManager.IsActiveTool(this)) return;
            mouseUp = false;
            if (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent<TileGenerator>(out _)) return;

            if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && Event.current.button == 0)
            {
                isHoldingMouseButton = true;
                Event.current.Use();
            }
            else
            {
                isHoldingMouseButton = false;
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
                mouseUp = true;
                Event.current.Use();
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView)) return;

            if (!ToolManager.IsActiveTool(this)) return;

            if (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent<TileGenerator>(out _)) return;

            Camera currentCamera = Camera.current;
            if (currentCamera != null)
            {
                GUIContent labelContent = new GUIContent($"Click to {(tileGenerator.shouldPaint ? "draw" : "erase")} a tile");
                GUIStyle labelStyle = new GUIStyle();
                labelStyle.alignment = TextAnchor.UpperCenter;
                labelStyle.fontSize = 18;
                Handles.Label(currentCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.975f, 1)), labelContent, labelStyle);

                labelContent.text = "Shift click to toggle whether a tile will evaluate their rule or just use their default game object.";
                Handles.Label(currentCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.925f, 1)), labelContent, labelStyle);
            }

            if (tileGenerator.GetSelectedPoint(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out Vector3 point))
            {
                Handles.matrix = Matrix4x4.TRS(point,
                    tileGenerator.transform.rotation,
                    tileGenerator.transform.localScale * tileGenerator.PaintRadius);

                Handles.DrawWireDisc(Vector3.zero, Vector3.up, 1);
            }
            else
            {
                Handles.DrawWireDisc(GetCurrentMousePositionInScene(), Vector3.up, 0.5f);
            }

            Handles.matrix = Matrix4x4.identity;

            TileGenerator.PaintMode paintMode = tileGenerator.paintMode;

            // Determine whether we should switch the tile's status of whether it is ignoring the rule or not
            Event currentEvent = Event.current;
            do
            {
                if (currentEvent.shift)
                {
                    tileGenerator.paintMode = TileGenerator.PaintMode.ChangeRuleStatus;
                    break;
                }
            }
            while (Event.PopEvent(currentEvent));

            if (isHoldingMouseButton)
            {
                tileGenerator.ChangeTile();
            }

            if (mouseUp)
            {
                tileGenerator.tilesBeingEdited.Clear();
            }

            tileGenerator.paintMode = paintMode;

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