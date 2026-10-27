using System;
using UnityEditor;

namespace SpriteTools
{
    /// <summary>
    /// the automatic side of the tool, unity calls this on its own whenever a texture gets imported
    /// if the file landed in a folder with a SpriteFolderRule, it runs the same ApplyTo the window uses
    /// </summary>
    public class SpriteRuleProcessor : AssetPostprocessor
    {
        // nothing in my code calls this, unity does, it's the one place the engine reaches into the tool by itself
        // I looked at custom ScriptedImporter during research but those are for teaching unity a brand new file type,
        // pngs already have the TextureImporter so a postprocessor that tweaks it before it runs fits better
        void OnPreprocessTexture()
        {
            var rule = FindRule(assetPath);
            if (rule == null || rule.preset == null)
                return;

            // importSettingsMissing means there's no .meta yet, so this only fires the first time unless the rule says otherwise
            if (rule.firstImportOnly && !assetImporter.importSettingsMissing)
                return;

            // reusing the import service so the rule and the window can't be separated
            SpriteImportService.ApplyTo((TextureImporter)assetImporter, rule.preset);
        }

        // when rules overlap I let the deepest folder win, i.e. Art/Characters/Enemies beats Art/Characters
        static SpriteFolderRule FindRule(string path)
        {
            SpriteFolderRule best = null;

            foreach (var guid in AssetDatabase.FindAssets("t:SpriteFolderRule"))
            {
                var rule = AssetDatabase.LoadAssetAtPath<SpriteFolderRule>(AssetDatabase.GUIDToAssetPath(guid));
                if (rule == null || !Matches(rule, path))
                    continue;

                if (best == null || rule.folderPath.Length > best.folderPath.Length)
                    best = rule;
            }

            return best;
        }

        static bool Matches(SpriteFolderRule rule, string path)
        {
            var folder = SpriteFolderService.Normalise(rule.folderPath) + "/";

            if (!path.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
                return false;

            return rule.includeSubfolders || path.IndexOf('/', folder.Length) < 0;
        }
    }
}
