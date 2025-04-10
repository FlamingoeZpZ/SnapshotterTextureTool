using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SnapshotSpawner : MonoBehaviour
{
#if UNITY_EDITOR
    private string saveFolder;
    private Camera _sceneCam;
    private List<GameObject> childObjects = new List<GameObject>();

    private void Awake()
    {
        _sceneCam = Camera.main;

        // Get all direct child objects
        foreach (Transform child in transform)
        {
            childObjects.Add(child.gameObject);
            child.gameObject.SetActive(false); // Ensure they start disabled
        }
    }

    [ContextMenu("StartSnapshotting")]
    public void StartSnapshotting()
    {
        if (childObjects.Count == 0)
        {
            Debug.LogError("No child objects found!");
            return;
        }

        saveFolder = Path.Combine(Application.persistentDataPath, "Snapshots");
        Directory.CreateDirectory(saveFolder);
        Debug.Log($"Saving snapshots to: {saveFolder}");

        StartCoroutine(SnapshotChildren());
    }

    private IEnumerator SnapshotChildren()
    {
        foreach (var obj in childObjects)
        {
            obj.SetActive(true);
            yield return new WaitForEndOfFrame(); // Wait for frame to render

            CaptureSnapshot(obj.name);
                
            obj.SetActive(false);
            yield return new WaitForSeconds(0.1f); // Small delay to avoid frame issues
        }
    }

    private void CaptureSnapshot(string objectName)
    {
        if (_sceneCam == null)
        {
            Debug.LogError("No main camera found!");
            return;
        }

        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        _sceneCam.targetTexture = rt;
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        _sceneCam.Render();
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        _sceneCam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenshot.EncodeToPNG();
        string filePath = Path.Combine(saveFolder, $"{objectName}.png");
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Saved snapshot: {filePath}");
    }
#endif
}