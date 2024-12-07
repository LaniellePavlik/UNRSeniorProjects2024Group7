using UnityEngine;
using UnityEditor;
using System.IO;

namespace ColorStudio {

    public static class TextureExtensions {

        public static void EnsureTextureIsReadable(this Texture texture) {
#if UNITY_EDITOR
            if (Application.isPlaying) return;
            string path = AssetDatabase.GetAssetPath(texture);
            if (!string.IsNullOrEmpty(path)) {
                TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp != null && !imp.isReadable) {
                    imp.isReadable = true;
                    imp.SaveAndReimport();
                }
            }
#endif
        }

        public static bool EnsureTextureCanBeEdited(this Texture texture) {
#if UNITY_EDITOR
            if (Application.isPlaying) return true;
            string path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) {
                return false;
            } else {
                TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
                if (imp != null && (!imp.isReadable || imp.textureCompression != TextureImporterCompression.Uncompressed)) {
                    imp.isReadable = true;
                    imp.textureCompression = TextureImporterCompression.Uncompressed;
                    imp.SaveAndReimport();
                }
            }
#endif
            return true;
        }

    }
}