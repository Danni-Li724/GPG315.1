using UnityEditor;
using UnityEngine;

namespace SpriteTools
{
    /// <summary>
    /// a saved bundle of import settings made from the create menu.
    /// SpriteImportService reads it and pushes the values onto each TextureImporter
    /// </summary>
    [CreateAssetMenu(menuName = "Sprite Tools/Import Preset", fileName = "SpriteImportPreset")]
    public class SpriteImportPreset : ScriptableObject
    {
        public SpriteImportMode spriteMode = SpriteImportMode.Single;
        public float pixelsPerUnit = 100f;
        public FilterMode filterMode = FilterMode.Point;
        public TextureImporterCompression compression = TextureImporterCompression.Uncompressed;
        public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
        public int maxSize = 2048;
        public bool generateMipMaps;
        public SpriteMeshType meshType = SpriteMeshType.FullRect;
        public SpriteAlignment alignment = SpriteAlignment.Center;
        public Vector2 customPivot = new Vector2(0.5f, 0.5f);
    }
}
