using UnityEngine;
using System.IO;

public static class SaveHelper
{
    public static void SaveTextureAsPNG(Texture2D texture, string methodName)
    {
        string folder = Application.dataPath + "/../Screenshots";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string path = Path.Combine(folder, $"{methodName}_{timestamp}.png");
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Saved: {path}");
    }
}