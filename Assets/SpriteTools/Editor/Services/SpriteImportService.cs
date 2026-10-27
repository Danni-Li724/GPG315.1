using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace SpriteTools
{
    /// <summary>
    /// takes a preset and pushes its settings onto every sprite's TextureImporter in one batch
    /// the import tab; right-click "apply last preset" menu and the automatic folder rules all end up here, this is the core of the tool
    /// </summary>
    public static class SpriteImportService
    {
        // here I count first so the confirm dialog can say how many will actually change, sprites that already match get skipped
        public static int CountChanges(IEnumerable<SpriteEntry> entries, SpriteImportPreset preset)
        {
            return entries.Count(entry => GetImporter(entry) is TextureImporter importer && !Matches(importer, preset));
        }
        public static int Apply(IEnumerable<SpriteEntry> entries, SpriteImportPreset preset)
        {
            var list = entries.ToList();
            var changed = 0;

            try
            {
                AssetDatabase.StartAssetEditing();

                for (var i = 0; i < list.Count; i++)
                {
                    EditorUtility.DisplayProgressBar("Applying Import Settings", list[i].currentName, (float)i / list.Count);

                    var importer = GetImporter(list[i]);
                    if (importer == null || Matches(importer, preset))
                        continue;
                    ApplyTo(importer, preset);
                    importer.SaveAndReimport();
                    changed++;
                }
            }
            finally
            {
                // learned this one the hard way from the docs...if something throws and StopAssetEditing never runs the editor stays paused
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            return changed;
        }

        // split this out so SpriteRuleProcessor can reuse it mid-import without the batching and progress bar
        public static void ApplyTo(TextureImporter importer, SpriteImportPreset preset)
        {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = preset.meshType;
            settings.spriteAlignment = (int)preset.alignment;
            settings.spritePivot = preset.customPivot;
            importer.SetTextureSettings(settings);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = preset.spriteMode;
            importer.spritePixelsPerUnit = preset.pixelsPerUnit;
            importer.filterMode = preset.filterMode;
            importer.textureCompression = preset.compression;
            importer.wrapMode = preset.wrapMode;
            importer.maxTextureSize = preset.maxSize;
            importer.mipmapEnabled = preset.generateMipMaps;
        }

        public static bool Matches(TextureImporter importer, SpriteImportPreset preset)
        {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);

            return importer.textureType == TextureImporterType.Sprite
                && importer.spriteImportMode == preset.spriteMode
                && importer.spritePixelsPerUnit == preset.pixelsPerUnit
                && importer.filterMode == preset.filterMode
                && importer.textureCompression == preset.compression
                && importer.wrapMode == preset.wrapMode
                && importer.maxTextureSize == preset.maxSize
                && importer.mipmapEnabled == preset.generateMipMaps
                && settings.spriteMeshType == preset.meshType
                && settings.spriteAlignment == (int)preset.alignment
                && settings.spritePivot == preset.customPivot;
        }

        static TextureImporter GetImporter(SpriteEntry entry)
        {
            return AssetImporter.GetAtPath(entry.path) as TextureImporter;
        }
    }
}
