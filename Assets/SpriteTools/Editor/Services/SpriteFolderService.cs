using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpriteTools
{
    /// <summary>
    /// makes folders and moves sprites into them, plus the group-by-prefix sorting
    /// the OrganisePanel is the only caller for moving, but SpriteRuleProcessor borrows Normalise so paths get compared the same way everywhere
    /// </summary>
    public static class SpriteFolderService
    {
        // unity's CreateFolder only makes one level at a time, so here I walk the path and make each missing piece
        public static string EnsureFolder(string folderPath)
        {
            folderPath = Normalise(folderPath);

            if (AssetDatabase.IsValidFolder(folderPath))
                return folderPath;

            var parts = folderPath.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
                return null;

            var current = "Assets";

            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];

                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);

                current = next;
            }

            return current;
        }

        public static List<SpriteEntry> FindConflicts(IEnumerable<SpriteEntry> entries, string folderPath)
        {
            folderPath = Normalise(folderPath);

            return entries
                .Where(entry => entry.Folder != folderPath && File.Exists(folderPath + "/" + Path.GetFileName(entry.path)))
                .ToList();
        }

        // I use MoveAsset instead of System.IO so the .meta comes along and scenes and prefabs keep their refs
        public static int Move(IEnumerable<SpriteEntry> entries, string folderPath)
        {
            var target = EnsureFolder(folderPath);
            if (target == null)
                return 0;

            var list = entries.ToList();
            var moved = new List<SpriteEntry>();

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var entry in list)
                {
                    if (entry.Folder == target)
                        continue;

                    var destination = target + "/" + Path.GetFileName(entry.path);
                    var result = AssetDatabase.MoveAsset(entry.path, destination);

                    if (string.IsNullOrEmpty(result))
                        moved.Add(entry);
                    else
                        entry.error = result;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            foreach (var entry in moved)
                entry.Refresh();

            return moved.Count;
        }

        // everything before the first separator becomes the folder name, so enemy_slime would end up in enemy
        public static Dictionary<string, List<SpriteEntry>> GroupByPrefix(IEnumerable<SpriteEntry> entries, char separator)
        {
            var groups = new Dictionary<string, List<SpriteEntry>>();

            foreach (var entry in entries)
            {
                var index = entry.currentName.IndexOf(separator);
                var key = index > 0 ? entry.currentName.Substring(0, index) : "Misc";

                if (!groups.TryGetValue(key, out var list))
                    groups[key] = list = new List<SpriteEntry>();

                list.Add(entry);
            }

            return groups;
        }

        // the folder picker gives back a full windows path, here I turn it back into an Assets/... path unity understands
        public static string ToProjectPath(string absolutePath)
        {
            absolutePath = Normalise(absolutePath);
            var dataPath = Normalise(Application.dataPath);
            if (absolutePath == dataPath)
                return "Assets";
            return absolutePath.StartsWith(dataPath + "/") ? "Assets" + absolutePath.Substring(dataPath.Length) : null;
        }

        public static string Normalise(string path)
        {
            return string.IsNullOrEmpty(path) ? "" : path.Replace('\\', '/').Trim().TrimEnd('/');
        }
    }
}
