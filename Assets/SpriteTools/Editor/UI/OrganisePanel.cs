using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpriteTools
{
    /// <summary>
    /// the organise tab, pick a target folder, make it and move the ticked sprites in, optionally sorted into subfolders by prefix
    /// all the folder making and moving happens in SpriteFolderService, this just finds the path and confirms
    /// </summary>
    public class OrganisePanel : SpriteToolPanel
    {
        string targetFolder;
        bool groupByPrefix;
        char separator = '_';

        public OrganisePanel(SpriteToolsWindow window) : base(window)
        {
            targetFolder = SpriteToolSettings.instance.lastFolder;
        }

        public override void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            targetFolder = EditorGUILayout.TextField("Target Folder", targetFolder);

            if (GUILayout.Button("Browse", GUILayout.Width(70)))
                Browse();

            EditorGUILayout.EndHorizontal();

            groupByPrefix = EditorGUILayout.Toggle("Group By Prefix", groupByPrefix);

            if (groupByPrefix)
            {
                var text = EditorGUILayout.TextField("Separator", separator.ToString());
                if (text.Length > 0)
                    separator = text[0];

                DrawGroupPreview();
            }

            EditorGUILayout.Space();
            Window.DrawEntryList(DrawRow);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create Folder"))
                CreateFolder();

            using (new EditorGUI.DisabledScope(!Window.SelectedEntries.Any()))
            {
                if (GUILayout.Button("Move Selected", GUILayout.Height(28)))
                    MoveSelected();
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawRow(SpriteEntry entry)
        {
            EditorGUILayout.LabelField(entry.HasError ? entry.error : entry.Folder, EditorStyles.miniLabel);
        }

        // the grouping is calculated every repaint just for the preview box, nothing moves until the button press
        void DrawGroupPreview()
        {
            var groups = SpriteFolderService.GroupByPrefix(Window.SelectedEntries, separator);
            var summary = string.Join("\n", groups.Select(group => $"{targetFolder}/{group.Key}  ({group.Value.Count})"));
            EditorGUILayout.HelpBox(summary.Length > 0 ? summary : "nothing ticked", MessageType.None);
        }

        void Browse()
        {
            var absolute = EditorUtility.OpenFolderPanel("Target Folder", Application.dataPath, "");
            var folder = SpriteFolderService.ToProjectPath(absolute);

            if (!string.IsNullOrEmpty(folder))
            {
                targetFolder = folder;
                GUI.FocusControl(null);
            }
        }

        void CreateFolder()
        {
            var created = SpriteFolderService.EnsureFolder(targetFolder);

            if (created == null)
                Window.ShowNotification(new GUIContent("folder has to be inside Assets"));
            else
                EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(created));

            Remember();
        }

        // move sprite, then tell the window to refresh since every moved sprite now has a new path
        void MoveSelected()
        {
            var selected = Window.SelectedEntries.ToList();

            if (groupByPrefix)
            {
                var groups = SpriteFolderService.GroupByPrefix(selected, separator);
                if (!EditorUtility.DisplayDialog("Move Sprites", $"Move {selected.Count} sprites into {groups.Count} folders under {targetFolder}?", "Move", "Cancel"))
                    return;

                foreach (var group in groups)
                    SpriteFolderService.Move(group.Value, targetFolder + "/" + group.Key);
            }
            else
            {
                var conflicts = SpriteFolderService.FindConflicts(selected, targetFolder);
                var message = $"Move {selected.Count} sprites into {targetFolder}?";

                if (conflicts.Count > 0)
                    message += $"\n\n{conflicts.Count} will be skipped, a file with the same name is already there.";

                if (!EditorUtility.DisplayDialog("Move Sprites", message, "Move", "Cancel"))
                    return;

                SpriteFolderService.Move(selected.Except(conflicts), targetFolder);
            }

            Remember();
            Window.RefreshEntries();
            GUIUtility.ExitGUI();
        }

        void Remember()
        {
            SpriteToolSettings.instance.lastFolder = targetFolder;
        }
    }
}
