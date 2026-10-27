using UnityEditor;
using UnityEngine;

namespace SpriteTools
{
    /// <summary>
    /// all the menu entries, Tools > Sprite Tools > Open Window plus the right-click shortcuts in the project window
    /// </summary>
    public static class SpriteToolsMenu
    {
        // this used to be just "Tools/Sprite Tools", which clashed with the Debug submenu and hid the window,
        // a menu path can be an item or a submenu but not both, so it lives under Open Window now
        [MenuItem("Tools/Sprite Tools/Open Window", false, 0)]
        static void OpenWindow()
        {
            SpriteToolsWindow.Open();
        }

        [MenuItem("Assets/Sprite Tools/Open With Selection")]
        static void OpenWithSelection()
        {
            SpriteToolsWindow.Open().SetEntries(SpriteAssetCollector.FromSelection());
        }

        // a shortcut that skips the window entirely, which only works because the service doesn't need it
        [MenuItem("Assets/Sprite Tools/Apply Last Preset")]
        static void ApplyLastPreset()
        {
            var preset = SpriteToolSettings.instance.LastPreset;

            if (preset == null)
            {
                Debug.LogWarning("sprite tools: no preset picked yet, pick one in the window first");
                return;
            }

            var count = SpriteImportService.Apply(SpriteAssetCollector.FromSelection(), preset);
            Debug.Log($"sprite tools: applied '{preset.name}' to {count} textures");
        }

        // the second MenuItem with true is a validator, it greys the entries out when nothing is selected
        [MenuItem("Assets/Sprite Tools/Open With Selection", true)]
        [MenuItem("Assets/Sprite Tools/Apply Last Preset", true)]
        static bool HasSelection()
        {
            return Selection.assetGUIDs.Length > 0;
        }
    }
}
