using UnityEngine;

namespace SpriteTools
{
    /// <summary>
    /// links one folder to one preset, so new art dropped in there sets itself up
    /// </summary>
    [CreateAssetMenu(menuName = "Sprite Tools/Folder Rule", fileName = "SpriteFolderRule")]
    public class SpriteFolderRule : ScriptableObject
    {
        public string folderPath = "Assets/Sprites";
        public bool includeSubfolders = true;
        public bool firstImportOnly = true;
        public SpriteImportPreset preset;
    }
}
