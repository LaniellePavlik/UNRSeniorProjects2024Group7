/* Pixel Painter by Ramiro Oliva (Kronnect)   /
/  Premium assets for Unity on kronnect.com */

using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ColorStudio {


    public partial class PPWindow : EditorWindow {

        Color floodFillRefereceColor;
        bool floodFillChanges;

        void CreateNew(int newWidth, int newHeight) {
            customWidth = width = newWidth;
            customHeight = height = newHeight;
            texture = null;
            sprite = null;
            canvasTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            canvasTexture.filterMode = FilterMode.Point;
            ReadColors();
            Color trans = new Color(0, 0, 0, 0);
            for (int k = 0; k < colors.Length; k++) {
                colors[k] = trans;
            }
            canvasTexture.SetPixels(colors);
            canvasTexture.Apply();
        }

        void SetSize(int newWidth, int newHeight) {
            customWidth = width = newWidth;
            customHeight = height = newHeight;
            Scale(canvasTexture, width, height, FilterMode.Point);
            ReadColors();
        }

        void Scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear) {

            if (tex.width == width && tex.height == height) return;

            RenderTexture currentActiveRT = RenderTexture.active;

            Rect texR = new Rect(0, 0, width, height);

            tex.filterMode = mode;
            tex.Apply(true);

            RenderTexture rtt = RenderTexture.GetTemporary(width, height, 0);

            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);
            Graphics.Blit(tex, rtt);

            // Update new texture
            tex.Reinitialize(width, height, TextureFormat.ARGB32, false);
            tex.ReadPixels(texR, 0, 0, true);
            tex.Apply(true);

            RenderTexture.active = currentActiveRT;
            RenderTexture.ReleaseTemporary(rtt);
        }


        void PaintPixel() {
            bool changes = PaintPixelOne(currentTexelPos.x, currentTexelPos.y, false);
            switch (_mirrorMode) {
                case MirrorMode.Horizontal:
                    changes = PaintPixelOne(width - 1 - currentTexelPos.x, currentTexelPos.y, changes);
                    break;
                case MirrorMode.Vertical:
                    changes = PaintPixelOne(currentTexelPos.x, height - 1 - currentTexelPos.y, changes);
                    break;
                case MirrorMode.Quad:
                    changes = PaintPixelOne(width - 1 - currentTexelPos.x, currentTexelPos.y, changes);
                    changes = PaintPixelOne(currentTexelPos.x, height - 1 - currentTexelPos.y, changes);
                    changes = PaintPixelOne(width - 1 - currentTexelPos.x, height - 1 - currentTexelPos.y, changes);
                    break;
            }
            if (changes) {
                UpdateCanvasTexture();
                PostPaintPixel();
            }
        }


        bool PaintPixelOne(int cursorPosX, int cursorPosY, bool changes) {
            int brushWidth = brushPixelWidth;
            int offset = -brushWidth / 2;
            for (int py = 0; py < brushWidth; py++) {
                int y = cursorPosY + offset + py;
                if (y < 0 || y >= height) continue;
                for (int px = 0; px < brushWidth; px++) {
                    int x = cursorPosX + offset + px;
                    if (x < 0 || x >= width) continue;
                    if (!BrushShapeTest(x, y, px, py)) continue;
                    int colorIndex = y * width + x;
                    Color newColor = GetTransformedColor(colors[colorIndex]);
                    if (colors[colorIndex] != newColor) {
                        if (!changes) {
                            changes = true;
                            Undo.RegisterCompleteObjectUndo(this, currentBrush.opDescription());
                        }
                        colors[colorIndex] = newColor;
                    }
                }
            }
            return changes;
        }

        bool BrushShapeTest(int x, int y, int px, int py) {
            switch (_brushShape) {
                case BRUSH_SHAPE_CIRCLE: {
                        int brushWidth = brushPixelWidth;
                        if (brushWidth < 3) return true;
                        float radius = brushWidth / 2f;
                        float fx = px + 0.5f;
                        float fy = py + 0.5f;
                        float distSqr = Mathf.Sqrt((fx - radius) * (fx - radius) + (fy - radius) * (fy - radius));
                        return distSqr < radius - 0.2f;
                    }
                case BRUSH_SHAPE_DITHER_1:
                    return (x + y) % 2 == 1;
                case BRUSH_SHAPE_DITHER_2:
                    return (x % 2) == 0 && (y % 2) == 0;
                case BRUSH_SHAPE_DITHER_3:
                    return ((x + y / 2) % 2) == 1 && (y % 2) == 0;
                case BRUSH_SHAPE_DITHER_4:
                    return ((x + y / 3) % 3) == 1 && (y % 3) == 0;
                case BRUSH_SHAPE_CUSTOM: {
                        int fx = x % brushTextureWidth;
                        int fy = y % brushTextureHeight;
                        int colorIndex = fy * brushTextureWidth + fx;
                        if (brushColors != null && colorIndex < brushColors.Length) {
                            return brushColors[colorIndex].a >= 128;
                        }
                    }
                    break;
            }
            return true;
        }

        Color GetTransformedColor(Color originalColor) {
            switch (currentBrush) {
                case Brush.Darken: {
                        if (originalColor.a == 0) return originalColor;
                        HSLColor hsl = ColorConversion.GetHSLFromRGB(originalColor.r, originalColor.g, originalColor.b);
                        hsl.l -= 0.05f;
                        if (hsl.l < 0.01f) hsl.l = 0.01f;
                        return ColorConversion.GetColorFromHSL(hsl.h, hsl.s, hsl.l);
                    }
                case Brush.Lighten: {
                        if (originalColor.a == 0) return originalColor;
                        HSLColor hsl = ColorConversion.GetHSLFromRGB(originalColor.r, originalColor.g, originalColor.b);
                        hsl.l += 0.05f;
                        if (hsl.l > 0.99f) hsl.l = 0.99f;
                        return ColorConversion.GetColorFromHSL(hsl.h, hsl.s, hsl.l);
                    }
                case Brush.Dry: {
                        if (originalColor.a == 0) return originalColor;
                        HSLColor hsl = ColorConversion.GetHSLFromRGB(originalColor.r, originalColor.g, originalColor.b);
                        hsl.s -= 0.05f;
                        if (hsl.s < 0.01f) hsl.s = 0.01f;
                        return ColorConversion.GetColorFromHSL(hsl.h, hsl.s, hsl.l);
                    }
                case Brush.Vivid: {
                        if (originalColor.a == 0) return originalColor;
                        HSLColor hsl = ColorConversion.GetHSLFromRGB(originalColor.r, originalColor.g, originalColor.b);
                        hsl.s += 0.05f;
                        if (hsl.s > 0.99f) hsl.s = 0.99f;
                        return ColorConversion.GetColorFromHSL(hsl.h, hsl.s, hsl.l);
                    }
                case Brush.Noise: {
                        if (originalColor.a == 0) return originalColor;
                        HSLColor hsl = ColorConversion.GetHSLFromRGB(originalColor.r, originalColor.g, originalColor.b);
                        hsl.l += Random.value * 0.1f - 0.05f;
                        if (hsl.l > 0.99f) hsl.l = 0.99f; else if (hsl.l < 0.01f) hsl.l = 0.01f;
                        return ColorConversion.GetColorFromHSL(hsl.h, hsl.s, hsl.l);
                    }
                case Brush.NoiseTone: {
                        if (originalColor.a == 0) return originalColor;
                        HSLColor hsl = ColorConversion.GetHSLFromRGB(originalColor.r, originalColor.g, originalColor.b);
                        hsl.h += Random.value * 0.1f - 0.05f;
                        if (hsl.h > 0.99f) hsl.l = 0.99f; else if (hsl.h < 0.01f) hsl.h = 0.01f;
                        return ColorConversion.GetColorFromHSL(hsl.h, hsl.s, hsl.l);
                    }
                case Brush.Gradient: {
                        HSLColor hsl = ColorConversion.GetHSLFromRGB(_brushColor.r, _brushColor.g, _brushColor.b);
                        int distance = Mathf.Max(Mathf.Abs(startTexelPos.x - currentTexelPos.x), Mathf.Abs(startTexelPos.y - currentTexelPos.y));
                        if (distance > 0) {
                            hsl.l -= 0.05f * distance;
                            if (hsl.l < 0.01f) hsl.l = 0.01f;
                            return ColorConversion.GetColorFromHSL(hsl.h, hsl.s, hsl.l);

                        }
                        return _brushColor;
                    }


            }
            return _brushColor;
        }


        void ClearAll() {
            Undo.RegisterCompleteObjectUndo(this, "Clear All");
            FillWithColor(new Color(0, 0, 0, 0));
            UpdateCanvasTexture();
        }

        void FillAll() {
            Undo.RegisterCompleteObjectUndo(this, "Fill All");
            FillWithColor(_brushColor);
            UpdateCanvasTexture();
        }

        void FillWithColor(Color color) {
            for (int k = 0; k < colors.Length; k++) {
                colors[k] = color;
            }
            UpdateCanvasTexture();
        }


        void ReplaceColors() {
            Color pixelColor = GetCursorColor();
            bool changes = false;
            int colorsLength = colors.Length;
            for (int k = 0; k < colorsLength; k++) {
                if (!changes) {
                    changes = true;
                    Undo.RegisterCompleteObjectUndo(this, "Replace color");
                }
                if (colors[k] == pixelColor) {
                    colors[k] = _brushColor;
                }
            }
            if (changes) {
                UpdateCanvasTexture();
            }
        }


        void FloodFill() {
            floodFillRefereceColor = GetCursorColor();
            if (_brushColor == floodFillRefereceColor) return;
            floodFillChanges = false;
            FloodFillIterative(currentTexelPos.x, currentTexelPos.y);
            if (floodFillChanges) {
                UpdateCanvasTexture();
            }
        }

        void FloodFillIterative(int startX, int startY) {
            Stack<(int, int)> stack = new Stack<(int, int)>();
            stack.Push((startX, startY));

            while (stack.Count > 0) {
                var (x, y) = stack.Pop();
                int colorIndex = y * width + x;

                if (EqualRGB(colors[colorIndex], floodFillRefereceColor)) {
                    if (!floodFillChanges) {
                        floodFillChanges = true;
                        Undo.RegisterCompleteObjectUndo(this, "Flood Fill");
                    }
                    colors[colorIndex] = _brushColor;
                    if (x > 0) stack.Push((x - 1, y));
                    if (x < width - 1) stack.Push((x + 1, y));
                    if (y > 0) stack.Push((x, y - 1));
                    if (y < height - 1) stack.Push((x, y + 1));
                }
            }
        }

        bool EqualRGB(Color32 color1, Color32 color2) {
            return color1.r == color2.r && color1.g == color2.g && color1.b == color2.b && color1.a == color2.a;
        }

        void ApplyPalette() {
            Undo.RegisterCompleteObjectUndo(this, "Apply Palette");
            CSPalette palette = CSWindow.GetPalette();
            if (palette == null) {
                if (EditorUtility.DisplayDialog("Apply Palette", "This option adapts the colors of the sprite to the current palette in the Palette Manager. Do you want to open the Palette Manager now?", "Ok", "Cancel")) {
                    CSWindow.ShowWindow();
                }
                return;
            }
            if (!EditorUtility.DisplayDialog("Apply Palette", "Replace the sprite colors with those from the Palette Manager?", "Yes", "No")) {
                return;
            }
            Color[] paletteColors = palette.BuildPaletteColors();
            for (int k = 0; k < colors.Length; k++) {
                if (colors[k].a > 0) {
                    colors[k] = palette.GetNearestColor(colors[k], paletteColors, ColorMatchMode.RGB, false);
                }
            }
            UpdateCanvasTexture();
        }


        void ExtractColors() {
            Undo.RegisterCompleteObjectUndo(this, "Extract Colors");
            CSPalette palette = CSWindow.GetPalette();
            if (palette == null) {
                if (EditorUtility.DisplayDialog("Extract Colors", "This option replaces the palette colors with the unique colors of current sprite. Do you want to open the Palette Manager now?", "Ok", "Cancel")) {
                    CSWindow.ShowWindow();
                }
                return;
            }
            if (!EditorUtility.DisplayDialog("Extract Colors", "Replaces palette colors with the unique colors of the current sprite?", "Yes", "No")) {
                return;
            }
            CSWindow cs = GetWindow(typeof(CSWindow)) as CSWindow;
            if (cs != null) {
                cs.ExtractColors(canvasTexture, 32);
            }
        }


        void UpdateCanvasTexture() {
            canvasTexture.SetPixels(colors);
            canvasTexture.Apply();
        }

        Color GetCursorColor() {
            int colorIndex = currentTexelPos.y * width + currentTexelPos.x;
            if (colorIndex < 0 || colorIndex >= colors.Length) {
                return _brushColor;
            }
            return colors[colorIndex];
        }

        void FlipHoriz() {
            Color[] newColors = new Color[width * height];
            for (int colorIndex = 0, y = 0; y < height; y++) {
                for (int x = 0; x < width; x++, colorIndex++) {
                    newColors[colorIndex] = colors[y * width + width - 1 - x];
                }
            }
            colors = newColors;
            UpdateCanvasTexture();
        }

        void FlipVert() {
            Color[] newColors = new Color[width * height];
            for (int colorIndex = 0, y = 0; y < height; y++) {
                for (int x = 0; x < width; x++, colorIndex++) {
                    newColors[colorIndex] = colors[(height - 1 - y) * width + x];
                }
            }
            colors = newColors;
            UpdateCanvasTexture();
        }

        void Displace(int dirX, int dirY) {
            Color[] newColors = new Color[width * height];
            Color trans = new Color(0, 0, 0, 0);
            for (int colorIndex = 0, y = 0; y < height; y++) {
                int y0 = y - dirY;
                for (int x = 0; x < width; x++, colorIndex++) {
                    int x0 = x - dirX;
                    if (y0 < 0 || y0 >= height || x0 < 0 || x0 >= width) {
                        newColors[colorIndex] = trans;
                    } else {
                        newColors[colorIndex] = colors[y0 * width + x0];
                    }
                }
            }
            colors = newColors;
            UpdateCanvasTexture();
        }

        void RotateLeft() {
            Color[] newColors = new Color[width * height];
            int aux = width;
            width = height;
            height = aux;
            for (int colorIndex = 0, y = 0; y < height; y++) {
                for (int x = 0; x < width; x++, colorIndex++) {
                    newColors[colorIndex] = colors[(height - x - 1) * height + y];
                }
            }
            colors = newColors;
            UpdateCanvasTexture();
        }

        void RotateRight() {
            Color[] newColors = new Color[width * height];
            int aux = width;
            width = height;
            height = aux;
            for (int colorIndex = 0, y = 0; y < height; y++) {
                for (int x = 0; x < width; x++, colorIndex++) {
                    newColors[colorIndex] = colors[x * height + (width - 1 - y)];
                }
            }
            colors = newColors;
            UpdateCanvasTexture();
        }

    }

}
