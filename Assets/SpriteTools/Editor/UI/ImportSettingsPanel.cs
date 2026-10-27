using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpriteTools
{
    /// <summary>
    /// the import settings tab, pick a preset and apply it to the ticked sprites
    /// it talks to SpriteImportService for the counting and applying, and saves the chosen preset into SpriteToolSettings
    /// </summary>
    public class ImportSettingsPanel : SpriteToolPanel
    {
        SpriteImportPreset preset;
        Editor presetEditor;

        public ImportSettingsPanel(SpriteToolsWindow window) : base(window)
        {
            preset = SpriteToolSettings.instance.LastPreset;
        }

        public override void Draw()
        {
            var selected = (SpriteImportPreset)EditorGUILayout.ObjectField("Preset", preset, typeof(SpriteImportPreset), false);

            if (selected != preset)
            {
                preset = selected;
                SpriteToolSettings.instance.LastPreset = preset;
            }

            if (preset == null)
            {
                EditorGUILayout.HelpBox("no preset yet, make one via Assets > Create > Sprite Tools > Import Preset", MessageType.Info);
                return;
            }

            // here I draw the preset's own inspector inside the tab, edits save straight to the asset and get undo for free
            Editor.CreateCachedEditor(preset, null, ref presetEditor);
            presetEditor.OnInspectorGUI();

            EditorGUILayout.Space();
            Window.DrawEntryList(null);

            using (new EditorGUI.DisabledScope(!Window.SelectedEntries.Any()))
            {
                if (GUILayout.Button("Apply To Selected", GUILayout.Height(28)))
                    ApplyPreset();
            }
        }

        // count, confirm, then apply, the confirm is my stand-in for undo since reimports can't be rolled back
        void ApplyPreset()
        {
            var selected = Window.SelectedEntries.ToList();
            var changes = SpriteImportService.CountChanges(selected, preset);

            if (changes == 0)
            {
                Window.ShowNotification(new GUIContent("everything already matches"));
                return;
            }

            if (!EditorUtility.DisplayDialog("Apply Import Settings", $"Apply '{preset.name}' to {changes} textures?", "Apply", "Cancel"))
                return;

            SpriteImportService.Apply(selected, preset);

            // the reimport changes the asset database mid-OnGUI, ExitGUI bails out of this pass so IMGUI doesn't throw layout errors
            GUIUtility.ExitGUI();
        }

        public override void OnDisable()
        {
            if (presetEditor != null)
                Object.DestroyImmediate(presetEditor);
        }
    }
}
