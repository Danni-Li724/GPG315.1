using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace SpriteTools
{
    /// <summary>
    /// builds the new names, checks them for problems and then does the actual renaming.
    /// the RenamePanel calls GeneratePreview every time an option changes, nothing writes to the actual disk until Apply()
    /// </summary>
    public static class SpriteRenameService
    {
        // this part is pure strings with no files involved, which is why the rename tab can show a live preview
        public static void GeneratePreview(IList<SpriteEntry> entries, RenameOptions options)
        {
            var number = options.startNumber;

            foreach (var entry in entries)
            {
                entry.pendingName = entry.selected ? BuildName(entry.currentName, options, number++) : entry.currentName;
            }

            Validate(entries);
        }

        public static string BuildName(string original, RenameOptions options, int number)
        {
            var name = options.numbering && !string.IsNullOrEmpty(options.baseName) ? options.baseName : original;

            if (!string.IsNullOrEmpty(options.find))
                name = name.Replace(options.find, options.replace ?? "");

            if (options.replaceSpaces)
                name = name.Replace(' ', '_');

            name = options.nameCase switch
            {
                NameCase.Lower => name.ToLowerInvariant(),
                NameCase.Upper => name.ToUpperInvariant(),
                _ => name
            };

            if (options.numbering)
                name += options.separator + number.ToString("D" + options.padding);

            return options.prefix + name + options.suffix;
        }
        
        public static void Validate(IList<SpriteEntry> entries)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var duplicates = new HashSet<SpriteEntry>(entries
                .Where(entry => entry.selected)
                .GroupBy(TargetPath, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .SelectMany(group => group));

            foreach (var entry in entries)
            {
                entry.error = null;

                if (!entry.selected || !entry.IsRenamed)
                    continue;
                // added error msgs in UI after making lots of mistakes myself xD
                if (string.IsNullOrWhiteSpace(entry.pendingName))
                    entry.error = "name is empty";
                else if (entry.pendingName.IndexOfAny(invalid) >= 0)
                    entry.error = "has invalid characters";
                else if (duplicates.Contains(entry))
                    entry.error = "duplicate name in this batch";
                else if (!string.Equals(TargetPath(entry), entry.path, StringComparison.OrdinalIgnoreCase) && File.Exists(TargetPath(entry)))
                    entry.error = "a file with this name already exists";
            }
        }

        public static bool CanApply(IList<SpriteEntry> entries)
        {
            return entries.Any(entry => entry.selected && entry.IsRenamed)
                && !entries.Any(entry => entry.selected && entry.HasError);
        }

        // RenameAsset keeps the .meta and guid so nothing referencing the sprite breaks,
        // but it sits outside unity's undo system, hence why the panel asks the user before calling this
        public static int Apply(IList<SpriteEntry> entries)
        {
            var targets = entries.Where(entry => entry.selected && entry.IsRenamed && !entry.HasError).ToList();
            var renamed = new List<SpriteEntry>();

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var entry in targets)
                {
                    var result = AssetDatabase.RenameAsset(entry.path, entry.pendingName);

                    if (string.IsNullOrEmpty(result))
                        renamed.Add(entry);
                    else
                        entry.error = result;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            foreach (var entry in renamed)
                entry.Refresh();

            return renamed.Count;
        }

        static string TargetPath(SpriteEntry entry)
        {
            return entry.Folder + "/" + entry.pendingName + entry.Extension;
        }
    }
}
