using UnityEditor;

namespace Ashbound.Editor
{
    /// <summary>
    /// First-import defaults for textures under Art/, so dropped-in pixel art is crisp and
    /// to scale. Runs once per file, so a manual change in the Inspector is kept.
    /// </summary>
    public sealed class ArtImportDefaults : AssetPostprocessor
    {
        private const string ArtFolder = "Assets/_Project/Art/";
        private const float PixelsPerUnit = 16f;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtFolder) || !assetImporter.importSettingsMissing)
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = UnityEngine.FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
        }
    }
}
