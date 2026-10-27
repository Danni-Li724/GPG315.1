using UnityEditor;

namespace SpriteTools
{
    /// <summary>
    /// remembers the last tab, preset and folder so the window doesn't forget everything when it closes or scripts recompile.
    /// it's saved under UserSettings so it stays per-person and doesn't end up in git with the rest of the project.
    /// </summary>
    [FilePath("UserSettings/SpriteToolSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class SpriteToolSettings : ScriptableSingleton<SpriteToolSettings>
    {
        public int lastTab;
        public string lastPresetGuid;
        public string lastFolder = "Assets/Sprites";

        // here I store the preset as a guid string instead of a direct reference so it survives the preset being renamed or moved
        public SpriteImportPreset LastPreset
        {
            get => AssetDatabase.LoadAssetAtPath<SpriteImportPreset>(AssetDatabase.GUIDToAssetPath(lastPresetGuid));
            set => lastPresetGuid = value == null ? "" : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(value));
        }

        public void Persist()
        {
            Save(true);
        }
    }
}
