using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace SpriteTools
{
    /// <summary>
    /// works out which sprites to load (from the project window selection, a folder, a drag and drop or the whole project)
    /// it's the first service anything calls, the window and the right-click menu both use it to turn paths into a list of SpriteEntry
    /// </summary>
    public static class SpriteAssetCollector
    {
        public static List<SpriteEntry> FromSelection()
        {
            return FromPaths(Selection.assetGUIDs.Select(AssetDatabase.GUIDToAssetPath));
        }

        public static List<SpriteEntry> FromFolders(params string[] folders)
        {
            return FromPaths(folders);
        }

        public static List<SpriteEntry> FromProject()
        {
            return FromPaths(new[] { "Assets" });
        }

        // requesting AssetDatabase for textures, folders get searched and loose files will get checked one by one
        public static List<SpriteEntry> FromPaths(IEnumerable<string> paths)
        {
            var guids = new HashSet<string>();
            var folders = new List<string>();

            foreach (var path in paths)
            {
                if (AssetDatabase.IsValidFolder(path))
                    folders.Add(path);
                else if (IsTexture(path))
                    guids.Add(AssetDatabase.AssetPathToGUID(path));
            }

            if (folders.Count > 0)
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", folders.ToArray()))
                {
                    if (IsTexture(AssetDatabase.GUIDToAssetPath(guid)))
                        guids.Add(guid);
                }
            }

            return guids.Select(guid => new SpriteEntry(guid)).OrderBy(entry => entry.path).ToList();
        }

        public static bool IsTexture(string path)
        {
            return AssetImporter.GetAtPath(path) is TextureImporter;
        }

        public static bool IsSprite(string path)
        {
            return AssetImporter.GetAtPath(path) is TextureImporter importer && importer.textureType == TextureImporterType.Sprite;
        }
    }
}
