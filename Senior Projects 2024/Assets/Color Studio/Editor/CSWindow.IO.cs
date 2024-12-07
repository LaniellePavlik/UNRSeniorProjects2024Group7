/* Color Studio by Ramiro Oliva (Kronnect)   /
/  Premium assets for Unity on kronnect.com */


using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace ColorStudio {

    public partial class CSWindow : EditorWindow {

        void NewPalette() {
            CSPalette newPalette = CreateInstance<CSPalette>();
            LoadPalette(newPalette);
            otherPalette = null;
        }

        bool SaveAsNewPalette(CSPalette palette) {
            string basePath = GetExportsPath("Palettes");
            string path = basePath + "/Palette.asset";
            int counter = 2;
            while (File.Exists(path)) {
                path = basePath + "/Palette" + counter + ".asset";
                counter++;
                if (counter > 1000) return false;
            }
            otherPalette = Instantiate(palette);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(otherPalette, path);
            EditorGUIUtility.PingObject(otherPalette);
            UpdateCWMaterial();
            palette.UpdateMaterial();
            return true;
        }

        void SavePalette() {
            otherPalette.Load(palette);
            EditorUtility.SetDirty(otherPalette);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(otherPalette);
            UpdateCWMaterial();
            palette.UpdateMaterial();
            // Notify recolor scripts
#if UNITY_2023_3_OR_NEWER
            Recolor[] recolors = FindObjectsByType<Recolor>(FindObjectsSortMode.None);
#else
            Recolor[] recolors = FindObjectsOfType<Recolor>();
#endif
            foreach (Recolor recolor in recolors) {
                if (recolor.enabled && recolor.palette == otherPalette) {
                    recolor.Refresh(updateOptimization: true);
                }
            }
        }

        public void LoadPalette(CSPalette otherPalette) {
            this.otherPalette = otherPalette;
            if (palette == otherPalette) {
                // this should not happen, safety check
                palette = Instantiate(palette);
            }
            palette.Load(otherPalette);
            UpdateCWMaterial();
            SetColorKeys();
        }

        bool DeletePalette(CSPalette otherPalette) {
            string path = AssetDatabase.GetAssetPath(otherPalette);
            if (!string.IsNullOrEmpty(path)) {
                if (EditorUtility.DisplayDialog("Confirmation", "Delete palette?", "Yes", "No")) {
                    AssetDatabase.DeleteAsset(path);
                    return true;
                }
            }
            return false;
        }

        bool DuplicatePalette(CSPalette otherPalette) {
            Selection.activeObject = otherPalette;
            return SaveAsNewPalette(otherPalette);
        }

        void ExportLUT() {
            Texture2D tex = palette.ExportLUT();
            string path = GetExportsPath("Exports") + "/LUT.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.Refresh();
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            tex.EnsureTextureIsReadable();
            EditorMisc.HighlightAsset(tex);
        }


        void ExportTexture() {
            Texture2D tex = palette.ExportTexture();
            string path = GetExportsPath("Exports") + "/ColorTexture.png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.Refresh();
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp != null) {
                imp.isReadable = true;
                imp.npotScale = TextureImporterNPOTScale.None;
                imp.mipmapEnabled = false;
                imp.wrapMode = TextureWrapMode.Clamp;
                imp.filterMode = FilterMode.Point;
                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.SaveAndReimport();
            }
            EditorMisc.HighlightAsset(tex);
        }


        void ExportCode() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Color[] colors = {");
            Color[] colors = palette.BuildPaletteColors();
            for (int k = 0; k < colors.Length; k++) {
                Color c = colors[k];
                sb.Append(string.Format(CultureInfo.InvariantCulture, "new Color({0}f, {1}f, {2}f)", c.r, c.g, c.b));
                if (k < colors.Length - 1) {
                    sb.Append(", ");
                }
                sb.AppendLine();
            }
            sb.AppendLine("};");
            Debug.Log(sb.ToString());
            EditorUtility.DisplayDialog("Generate Code", "C# code written to Unity console. You can copy the code from there.", "Ok");
        }


        void ExportColoredTexture() {
            string path = AssetDatabase.GetAssetPath(referenceTexture);
            if (!string.IsNullOrEmpty(path)) {
                path = Path.GetDirectoryName(path);
            }
            if (string.IsNullOrEmpty(path)) {
                path = "Assets/Color Studio/Exports";
                Directory.CreateDirectory(path);
            }
            path += "/" + nearestTexture.name + ".png";
            File.WriteAllBytes(path, nearestTexture.EncodeToPNG());
            AssetDatabase.Refresh();
            nearestTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Selection.activeObject = nearestTexture;
            EditorMisc.HighlightAsset(nearestTexture);
            EditorUtility.DisplayDialog("Texture Export", "Texture saved to:\n" + path, "Ok");
        }

        void ImportASE() {
            string path = EditorUtility.OpenFilePanel("Pick ASE file", ".", "ase");
            if (!string.IsNullOrEmpty(path)) {
                AdobeSwatchExchangeLoader.SwatchExchangeData aseLoader = new AdobeSwatchExchangeLoader.SwatchExchangeData();
                aseLoader.Load(path);
                palette.Clear();
                palette.scheme = ColorScheme.Custom;
                palette.shades = 1;
                Color[] colors = aseLoader.GetColors();
                for (int k = 0; k < colors.Length; k++) {
                    AddCustomColor(colors[k]);
                }
                UpdateCWMaterial();
                SetColorKeys();
            }
        }

        public void ExtractColors(Texture2D texture, int maxColors) {
            palette.Clear();
            palette.scheme = ColorScheme.Custom;
            palette.shades = 1;
            Color32[] inputColors = texture.GetPixels32();
            int inputColorsLength = inputColors.Length;

            Dictionary<Color32, int> colorFrequency = new Dictionary<Color32, int>();

            // Count the frequency of each color
            for (int k = 0; k < inputColorsLength; k++) {
                Color32 color = inputColors[k];
                if (color.a < 128) continue;
                color.a = 255;
                if (colorFrequency.ContainsKey(color)) {
                    colorFrequency[color]++;
                } else {
                    colorFrequency[color] = 1;
                }
            }

            // Sort colors by frequency and take the top N most frequent
            var mostFrequentColors = colorFrequency
                .OrderByDescending(c => c.Value)
                .Take(maxColors)
                .Select(c => c.Key)
                .ToList();

            ClearCustomColors();
            for (int k = 0; k < mostFrequentColors.Count; k++) {
                AddCustomColor(mostFrequentColors[k]);
            }
            UpdateCWMaterial();
            SetColorKeys();
        }

    }

}
