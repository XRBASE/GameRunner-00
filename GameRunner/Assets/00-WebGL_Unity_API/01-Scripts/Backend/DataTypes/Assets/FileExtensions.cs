using System;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Extension class for handy file things.
/// </summary>
public static class FileExtensions {
    /// <summary>
    /// Retrieve filetyp from filename, so we can convert it into
    /// an filetype enum.
    /// </summary>
    /// <param name="fileName">filename or path, extension included.</param>
    /// <returns>type of the file.</returns>
    public static FileType GetType(string fileName) {
        if (string.IsNullOrEmpty(fileName)) {
            return FileType.unknown;
        }

        string ext = "";
        if (fileName.Contains('.')) {
            ext = fileName.Split('.').Last().ToLower();
        }

        switch (ext) {
            case "":
                return FileType.none;
            case "pdf":
                return FileType.pdf;
            case "jpg":
            case "jpeg":
                return FileType.jpg;
            case "png":
                return FileType.png;
            case "mp4":
                return FileType.mp4;
            case "obj":
                return FileType.obj;
            case "fbx":
                return FileType.fbx;
            case "glb":
                return FileType.glb;
            case "txt":
                return FileType.txt;
            case "zip":
                return FileType.zip;
            default: {
                Debug.LogError($"Filetype {ext} was not recognized!");
                return FileType.unknown;
            }
        }
    }

    /// <summary>
    /// Finds the flags id of a given filetype, only supports one type at a time, not multiple.
    /// </summary>
    /// <param name="t">type of index that needs to be retrieved (one at a time)</param>
    /// <returns>index of enum.</returns>
    public static int GetEnumId(FileType t) {
        string[] types = Enum.GetNames(typeof(FileType));
        for (int i = 0; i < types.Length; i++) {
            if (t.ToString() == types[i]) {
                return i;
            }
        }

        Debug.LogError($"type {t} not found");
        return -1;
    }

    /// <summary>
    /// Tries to write a new file and returns true/false if it does not complete.
    /// File should be autodeleted at all times.
    /// </summary>
    /// <param name="directoryPath">path for which to check the permissions</param>
    public static bool HasFileWriteAccess(string directoryPath) {
        try {
            string path = Path.Combine(directoryPath, "TEMP-AccessFile.txt");
            if (File.Exists(path)) {
                File.Delete(path);
            }

            //try to write an actual file
            using (FileStream fs = File.Create(path, 1, FileOptions.DeleteOnClose)) { }

            return true;
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
            return false;
        }
    }

}

[Flags]
public enum FileType {
    none = 0,
    unknown = 1,
    pdf = 1 << 1,
    jpg = 1 << 2,
    png = 1 << 3,
    mp4 = 1 << 4,
    obj = 1 << 5,
    fbx = 1 << 6,
    glb = 1 << 7,
    txt = 1 << 8,
    zip = 1 << 9,
}