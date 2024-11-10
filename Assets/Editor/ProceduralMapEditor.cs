using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(ProceduralMap))]
public class ProceduralMapEditor : Editor
{
    private ProceduralMap mapScript;
    private DualGridTilemap dualGridScript;
    private CameraMovement cameraMovement;
    private IEnumerator generateTextureCoroutine;

    private void OnEnable()
    {
        mapScript = (ProceduralMap)target;
        dualGridScript = mapScript.gameManager.dualGridTilemap;
        cameraMovement = mapScript.gameManager.cameraMovement;
    }

    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Generate Map button
        if (GUILayout.Button("Generate Map"))
        {
            dualGridScript.Start();
            mapScript.Start();
            StartEditorCoroutine(mapScript.GenerateTexture(mapScript.mapSize, cameraMovement.GetWorldPos2D(cameraMovement.gameManager.player.transform.position)));
        }

        // Clear Map button
        if (GUILayout.Button("Clear Map"))
        {
            dualGridScript.RemoveAllTilemaps(); // remove everything
        }
    }

    private void StartEditorCoroutine(IEnumerator coroutine)
    {
        generateTextureCoroutine = coroutine;
        EditorApplication.update += EditorCoroutineStep;
    }

    private void EditorCoroutineStep()
    {
        // Continue the coroutine to the next frame
        if (generateTextureCoroutine != null && generateTextureCoroutine.MoveNext() == false)
        {
            // Stop the coroutine if it's done
            generateTextureCoroutine = null;
            EditorApplication.update -= EditorCoroutineStep;
        }
    }
}
