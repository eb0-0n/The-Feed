using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class AutoRebuildMayaBedroom
{
    private const string MarkerName = ".codex_rebuild_bedroom";

    static AutoRebuildMayaBedroom()
    {
        string marker = Path.Combine(Directory.GetCurrentDirectory(), MarkerName);
        if (!File.Exists(marker)) return;

        EditorApplication.delayCall += () =>
        {
            try
            {
                File.Delete(marker);
                BuildMayaBedroom.RebuildAndCapture();
                Debug.Log("The Feed automatic bedroom rebuild completed.");
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
            }
        };
    }
}
