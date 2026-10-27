using UnityEditor;
using UnityEngine;

namespace SpriteTools
{
    /// <summary>
    /// the rename tab, options at the top and a live old-to-new preview underneath where names can also be typed by hand
    /// it fills in a RenameOptions and hands it to SpriteRenameService, the panel itself handles only the UI side
    /// </summary>
    public class RenamePanel : SpriteToolPanel
    {
        RenameOptions options = new RenameOptions();
        bool previewActive;
        GUIStyle errorStyle;

        public RenamePanel(SpriteToolsWindow window) : base(window)
        {
        }

        public override void Draw()
        {
            DrawOptions();

            EditorGUILayout.Space();
            Window.DrawEntryList(DrawRow);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset"))
                ResetPreview();

            using (new EditorGUI.DisabledScope(!SpriteRenameService.CanApply(Window.Entries)))
            {
                if (GUILayout.Button("Rename Selected", GUILayout.Height(28)))
                    ApplyRename();
            }

            EditorGUILayout.EndHorizontal();
        }

        // since OnGUI runs constantly I wrap the fields in a change check, so the preview only rebuilds when something actually changed
        void DrawOptions()
        {
            EditorGUI.BeginChangeCheck();

            options.find = EditorGUILayout.TextField("Find", options.find);
            options.replace = EditorGUILayout.TextField("Replace", options.replace);
            options.prefix = EditorGUILayout.TextField("Prefix", options.prefix);
            options.suffix = EditorGUILayout.TextField("Suffix", options.suffix);
            options.replaceSpaces = EditorGUILayout.Toggle("Spaces To Underscores", options.replaceSpaces);
            options.nameCase = (NameCase)EditorGUILayout.EnumPopup("Case", options.nameCase);
            options.numbering = EditorGUILayout.Toggle("Numbering", options.numbering);

            if (options.numbering)
            {
                EditorGUI.indentLevel++;
                options.baseName = EditorGUILayout.TextField("Base Name", options.baseName);
                options.startNumber = EditorGUILayout.IntField("Start Number", options.startNumber);
                options.padding = EditorGUILayout.IntSlider("Padding", options.padding, 1, 5);
                options.separator = EditorGUILayout.TextField("Separator", options.separator);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                previewActive = true;

                // options received via UI, preview names come back written onto each entry
                SpriteRenameService.GeneratePreview(Window.Entries, options);
            }
        }

        void DrawRow(SpriteEntry entry)
        {
            errorStyle ??= new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(1f, 0.4f, 0.4f) } };

            EditorGUILayout.LabelField("rename to", GUILayout.Width(64));

            using (new EditorGUI.DisabledScope(!entry.selected))
            {
                EditorGUI.BeginChangeCheck();
                var typed = EditorGUILayout.TextField(entry.pendingName);

                if (EditorGUI.EndChangeCheck())
                {
                    // hand-typed names go through undo too, so a typo can be ctrl+z'd before anything is renamed for real
                    Undo.RecordObject(Window, "Edit Sprite Name");
                    entry.pendingName = typed;
                    SpriteRenameService.Validate(Window.Entries);
                }
            }

            if (entry.HasError)
                EditorGUILayout.LabelField(entry.error, errorStyle, GUILayout.Width(180));
        }

        void ResetPreview()
        {
            options = new RenameOptions();
            previewActive = false;

            foreach (var entry in Window.Entries)
                entry.pendingName = entry.currentName;

            SpriteRenameService.Validate(Window.Entries);
            GUI.FocusControl(null);
        }

        // show warning/ask user for permission becuase RenameAsset isn't on the undo stack
        void ApplyRename()
        {
            if (!EditorUtility.DisplayDialog("Rename Sprites", "Rename the ticked sprites? This can't be undone with Ctrl+Z.", "Rename", "Cancel"))
                return;
            var count = SpriteRenameService.Apply(Window.Entries);
            previewActive = false;
            Window.ShowNotification(new GUIContent($"renamed {count} sprites"));
            GUIUtility.ExitGUI();
        }

        public override void OnEntriesChanged()
        {
            if (previewActive)
                SpriteRenameService.GeneratePreview(Window.Entries, options);
            else
                SpriteRenameService.Validate(Window.Entries);
        }
    }
}
