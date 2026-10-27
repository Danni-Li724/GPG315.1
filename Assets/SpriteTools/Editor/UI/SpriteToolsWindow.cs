using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpriteTools
{
    /// <summary>
    /// the main window, it owns the sprite list, the toolbar, the drop area and the three tabs.
    /// it's the hub of the ui layer, the tabs read the list through it and it calls SpriteAssetCollector whenever sprites get loaded.
    /// </summary>
    public class SpriteToolsWindow : EditorWindow
    {
        static readonly string[] Tabs = { "Import Settings", "Rename", "Organise" };

        // it's a SerializeField so the list survives script recompiles, and so Undo can record changes to it
        [SerializeField] List<SpriteEntry> entries = new List<SpriteEntry>();

        SpriteToolPanel[] panels;
        int tab;
        Vector2 listScroll;

        public List<SpriteEntry> Entries => entries;
        public IEnumerable<SpriteEntry> SelectedEntries => entries.Where(entry => entry.selected);

        public static SpriteToolsWindow Open()
        {
            var window = GetWindow<SpriteToolsWindow>();
            window.titleContent = new GUIContent("Sprite Tools");
            window.minSize = new Vector2(420, 320);
            window.Show();
            return window;
        }

        void OnEnable()
        {
            panels = new SpriteToolPanel[]
            {
                new ImportSettingsPanel(this),
                new RenamePanel(this),
                new OrganisePanel(this)
            };

            tab = Mathf.Clamp(SpriteToolSettings.instance.lastTab, 0, Tabs.Length - 1);
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;

            foreach (var panel in panels)
                panel.OnDisable();

            SpriteToolSettings.instance.lastTab = tab;
            SpriteToolSettings.instance.Persist();
        }

        // went immediate mode (IMGUI) for this window, so OnGUI rebuilds the whole thing every event (layout/repaint/each click)
        // it was the quickest way to get something working, the debug window and DragDropSpriteList are the retained-mode (of UI Toolkit) versions
        void OnGUI()
        {
            DrawToolbar();
            DrawDropArea();

            EditorGUILayout.Space();
            tab = GUILayout.Toolbar(tab, Tabs);
            EditorGUILayout.Space();

            panels[tab].Draw();
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // every load button just asks the collector for a fresh list of entries
            if (GUILayout.Button("Load Selection", EditorStyles.toolbarButton))
                SetEntries(SpriteAssetCollector.FromSelection());

            if (GUILayout.Button("Add Selection", EditorStyles.toolbarButton))
                AddEntries(SpriteAssetCollector.FromSelection());

            if (GUILayout.Button("Load Folder", EditorStyles.toolbarButton))
                LoadFolder();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("All", EditorStyles.toolbarButton))
                SetAllSelected(true);

            if (GUILayout.Button("None", EditorStyles.toolbarButton))
                SetAllSelected(false);

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                SetEntries(new List<SpriteEntry>());

            GUILayout.Label($"{SelectedEntries.Count()} / {entries.Count}", EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal();
        }

        // in IMGUI drag and drop is just events, so I check the mouse is over the box and react to DragUpdated and DragPerform
        void DrawDropArea()
        {
            var rect = GUILayoutUtility.GetRect(0, 36, GUILayout.ExpandWidth(true));
            GUI.Box(rect, "Drop sprites or folders here", EditorStyles.helpBox);

            var evt = Event.current;
            if (!rect.Contains(evt.mousePosition))
                return;

            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                AddEntries(SpriteAssetCollector.FromPaths(DragAndDrop.paths));
            }

            evt.Use();
        }

        // the list is shared, each tab passes in drawExtra to add its own columns on the right
        public void DrawEntryList(Action<SpriteEntry> drawExtra)
        {
            listScroll = EditorGUILayout.BeginScrollView(listScroll, GUILayout.ExpandHeight(true));

            var nameWidth = position.width * 0.35f;
            var selectionChanged = false;

            foreach (var entry in entries)
            {
                EditorGUILayout.BeginHorizontal();

                var selected = EditorGUILayout.Toggle(entry.selected, GUILayout.Width(18));
                if (selected != entry.selected)
                {
                    // here I record the window before changing a tick so ctrl+z can bring it back
                    Undo.RecordObject(this, "Toggle Sprite");
                    entry.selected = selected;
                    selectionChanged = true;
                }

                if (GUILayout.Button(entry.currentName, EditorStyles.label, GUILayout.Width(nameWidth)))
                    EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(entry.path));

                drawExtra?.Invoke(entry);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (selectionChanged)
                NotifyEntriesChanged();
        }

        public void SetEntries(List<SpriteEntry> list)
        {
            entries = list;
            NotifyEntriesChanged();
        }

        public void AddEntries(IEnumerable<SpriteEntry> list)
        {
            var existing = new HashSet<string>(entries.Select(entry => entry.guid));
            entries.AddRange(list.Where(entry => existing.Add(entry.guid)));
            NotifyEntriesChanged();
        }

        // re-read every entry from its guid
        public void RefreshEntries()
        {
            foreach (var entry in entries)
                entry.Refresh();

            entries.RemoveAll(entry => string.IsNullOrEmpty(entry.path));
            NotifyEntriesChanged();
        }

        void SetAllSelected(bool selected)
        {
            Undo.RecordObject(this, selected ? "Select All Sprites" : "Select No Sprites");

            foreach (var entry in entries)
                entry.selected = selected;

            NotifyEntriesChanged();
        }

        void LoadFolder()
        {
            var absolute = EditorUtility.OpenFolderPanel("Load Sprites From Folder", Application.dataPath, "");
            var folder = SpriteFolderService.ToProjectPath(absolute);

            if (!string.IsNullOrEmpty(folder))
                SetEntries(SpriteAssetCollector.FromFolders(folder));
        }

        // undo only covers stuff inside the window (ticks and hand-typed names), real renames and moves can't be undone this way
        void OnUndoRedo()
        {
            NotifyEntriesChanged();
        }

        void NotifyEntriesChanged()
        {
            foreach (var panel in panels)
                panel.OnEntriesChanged();

            Repaint();
        }
    }
}
