using System;
using System.IO;
using UnityEditor;

namespace SpriteTools
{
    /// <summary>
    /// one row in the window's list: which sprite it is, its name now, the name it's about to get and whether it's ticked
    /// this is the thing that gets passed between every layer: the collector makes them, the window shows them and the services work on them
    /// </summary>
    [Serializable]
    public class SpriteEntry
    {
        public string guid;
        public string path;
        public string currentName;
        public string pendingName;
        public bool selected = true;
        public string error;

        public bool HasError => !string.IsNullOrEmpty(error);
        public bool IsRenamed => pendingName != currentName;
        public string Folder => Path.GetDirectoryName(path)?.Replace('\\', '/');
        public string Extension => Path.GetExtension(path);

        public SpriteEntry()
        {
        }

        public SpriteEntry(string guid)
        {
            this.guid = guid;
            Refresh();
        }

        // here I keep the guid as the real id because paths change after a rename or move, the guid never does
        public void Refresh()
        {
            path = AssetDatabase.GUIDToAssetPath(guid);
            currentName = Path.GetFileNameWithoutExtension(path);
            pendingName = currentName;
            error = null;
        }
    }
}
