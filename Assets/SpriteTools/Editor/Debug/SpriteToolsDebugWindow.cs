using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpriteTools
{
    /// <summary>
    /// a read-only test window built with UI Toolkit, two buttons that check the setup and list every sprite in the sandbox folder
    /// it doesn't use any of the tool's services on purpose, it's here to prove the uxml, callbacks and TextureImporter reading all work before anything writes
    /// </summary>
    public class SpriteToolsDebugWindow : EditorWindow
    {
        const string SandboxFolder = "Assets/SpriteToolsTest";

        [SerializeField] VisualTreeAsset m_VisualTreeAsset;

        Label statusLabel;
        ScrollView resultsList;

        [MenuItem("Tools/Sprite Tools/Debug/Debug Window")]
        public static void Open()
        {
            var window = GetWindow<SpriteToolsDebugWindow>();
            window.titleContent = new GUIContent("Sprite Tools Debug");
        }

        // this is test mode, CreateGUI runs once and the elements would stick around, compared to the main window's OnGUI redrawing every event
        public void CreateGUI()
        {
            var root = rootVisualElement;

            if (m_VisualTreeAsset == null)
            {
                root.Add(new Label("uxml not assigned"));
                return;
            }

            root.Add(m_VisualTreeAsset.Instantiate());
            statusLabel = root.Q<Label>("StatusLabel");
            resultsList = root.Q<ScrollView>("ResultsList");

            // I find each button by name instead of Query<Button>().First(), which returns null and throws if there's no button..
            root.Q<Button>("HelloButton")?.RegisterCallback<ClickEvent>(Clicked);
            root.Q<Button>("LogSandboxButton")?.RegisterCallback<ClickEvent>(Clicked);
        }

        // one handler for both buttons, currentTarget is the button I registered on (target could be a child element inside it)
        void Clicked(ClickEvent ev)
        {
            if (ev.currentTarget is not Button b)
                return;

            switch (b.name)
            {
                case "HelloButton":
                    Hello();
                    break;
                case "LogSandboxButton":
                    LogSandboxSprites();
                    break;
            }
        }

        void Hello()
        {
            Debug.Log("sprite tools: editor code is compiling and running");
            statusLabel.text = "hello works, uxml, button and callback are all hooked up";
        }

        // reads import settings straight off each TextureImporter
        void LogSandboxSprites()
        {
            resultsList.Clear();

            if (!AssetDatabase.IsValidFolder(SandboxFolder))
            {
                statusLabel.text = $"no sandbox folder yet, make {SandboxFolder} and drop some images in";
                Debug.LogWarning($"debug: {SandboxFolder} doesn't exist");
                return;
            }
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SandboxFolder });
            statusLabel.text = $"found {guids.Length} textures in {SandboxFolder}, click a row to highlight it";

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                    continue;
                var name = Path.GetFileNameWithoutExtension(path);
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                var info = $"{name} | type: {importer.textureType} | mode: {importer.spriteImportMode} | ppu: {importer.spritePixelsPerUnit} | filter: {importer.filterMode}";
                // rows are made in code and added to the retained tree, they stay there until the next Clear
                var row = new Label(info);
                row.RegisterCallback<ClickEvent>(_ => EditorGUIUtility.PingObject(asset));
                resultsList.Add(row);

                Debug.Log($"{info}\n{path}", asset);
            }
        }
    }
}
