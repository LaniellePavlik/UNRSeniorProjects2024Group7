using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor.Analysis
{
    public class CustomChartEditor : EditorWindow
    {
        public Material material;
        public Rect rect;
        public float offsetFromTop;
        public List<byte[]> loadedDigits;
        public byte[] dot, trace;
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");

        private void OnEnable()
        {
            material = new Material(
                Shader.Find("Hidden/Internal-Colored")
            );

            rect = Rect.zero;
        }

        /// <summary>
        /// Draws the X axis line
        /// </summary>
        protected static void DrawXAxis(Vector2 left, Vector2 right, Color color)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);

            GL.Vertex3(left.x, left.y, 0);
            GL.Vertex3(right.x, right.y, 0);
            GL.End();
        }
        
        /// <summary>
        /// Draws the Y axis line
        /// </summary>
        protected static void DrawYAxis(Vector2 bottom, Vector2 top, Color color)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);
            
            GL.Vertex3(bottom.x, bottom.y, 0);
            GL.Vertex3(top.x, top.y, 0);
            
            GL.End();
        }
        
        /// <summary>
        /// Draws black background and grey grid
        /// </summary>
        protected void DrawBgAndGrid()
        {
            // background
            GL.Begin(GL.QUADS);
            GL.Color(Color.black);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(rect.width, 0, 0);
            GL.Vertex3(rect.width, rect.height, 0);
            GL.Vertex3(0, rect.height, 0);
            GL.End();
            
            // draw grid
            GL.Begin(GL.LINES);
            var count = (int)(rect.width / 10) + 20;
            for (var i = 0; i < count; i++)
            {
                var f = (i % 5 == 0) ? 0.5f : 0.2f;
                GL.Color(new Color(f, f, f, 1));
                float x = i * 10;
                if (x >= 0 && x < rect.width)
                {
                    GL.Vertex3(x, 0, 0);
                    GL.Vertex3(x, rect.height, 0);
                }
                if (i < rect.height / 10)
                {
                    GL.Vertex3(0, i * 10, 0);
                    GL.Vertex3(rect.width, i * 10, 0);
                }
            }
            GL.End();
        }

        /// <summary>
        /// Draws a number texture in a coordinate based on the input
        /// </summary>
        /// <param name="pos">Angular and linear speed coordinate</param>
        /// <param name="number">Number to draw, with sign and points if needed</param>
        /// <param name="size">Size of the label</param>
        protected void DrawNumber(Vector2 pos, float number, float size)
        {
            var digits = number.ToString("#.0", new CultureInfo("en-US").NumberFormat);
            if (Mathf.Abs(number - Mathf.RoundToInt(number)) < .001f)
            {
                digits = Mathf.RoundToInt(number).ToString();
            }
            
            material = new Material(Shader.Find("Hidden/Internal-GUITexture"));
            var padding = size / 10;
            
            foreach(var it in digits.Select((digit, idx) => new {digit, idx}))
            {
                // separate each number
                var horizontalPlacement = size * (it.idx - digits.Length / 2);
                
                GL.Begin(GL.QUADS);
                
                // load digit texture
                Texture2D tex = new Texture2D(2, 2); // Create an empty Texture; size doesn't matter
                
                if (string.Equals(".", it.digit.ToString()))
                {
                    tex = new Texture2D(2, 2); // Create an empty Texture; size doesn't matter
                    tex.LoadImage(dot);
                }
                else if (string.Equals("-", it.digit.ToString()))
                {
                    tex = new Texture2D(2, 2); // Create an empty Texture; size doesn't matter
                    tex.LoadImage(trace);
                }
                else
                {
                    var intDigit = int.Parse(it.digit.ToString());
                    tex.LoadImage(loadedDigits[intDigit]);
                }

                material.SetTexture(MainTex, tex);
                material.SetPass(0);

                // place digit texture
                GL.TexCoord2(0, 0);
                GL.Vertex3(pos.x - size / 2 + horizontalPlacement + padding, pos.y + size - padding, 0);
                GL.TexCoord2(0, 1);
                GL.Vertex3(pos.x - size / 2 + horizontalPlacement + padding, pos.y - size + padding, 0);
                GL.TexCoord2(1, 1);
                GL.Vertex3(pos.x + size / 2 + horizontalPlacement - padding, pos.y - size + padding, 0);
                GL.TexCoord2(1, 0);
                GL.Vertex3(pos.x + size / 2 + horizontalPlacement - padding, pos.y + size - padding, 0);
                
                GL.End();
            }

            material = new Material(Shader.Find("Hidden/Internal-Colored"));
            material.SetPass(0);
        }

        /// <summary>
        /// Draws a colored square in the graph based on two coordinates
        /// </summary>
        /// <param name="lowLeft">Lower left coordinate for the square</param>
        /// <param name="highRight">Upper right coordinate for the square</param>
        /// <param name="color">RGBA-styled color for the square</param>
        protected static void DrawSquare(Vector2 lowLeft, Vector2 highRight, Color color)
        {
            var highLeft = (lowLeft.x, highRight.y);
            var lowRight = (highRight.x, lowLeft.y);
            
            var rLL = new Vector2(lowLeft.x, lowLeft.y);
            var rHL = new Vector2(highLeft.x, highLeft.y);
            var rHR = new Vector2(highRight.x, highRight.y);
            var rLR = new Vector2(lowRight.x, lowRight.y);
            
            GL.Begin(GL.QUADS);
            GL.Color(color);
            GL.Vertex3(rLL.x, rLL.y, 0);
            GL.Vertex3(rHL.x, rHL.y, 0);
            GL.Vertex3(rHR.x, rHR.y, 0);
            GL.Vertex3(rLR.x, rLR.y, 0);
            GL.End();
        }
        
        /// <summary>
        /// Draws a colored hollow square in the graph based on two coordinates
        /// </summary>
        /// <param name="lowLeft">Lower left coordinate for the square</param>
        /// <param name="highRight">Upper right coordinate for the square</param>
        /// <param name="color">RGBA-styled color for the square</param>
        protected static void DrawHollowSquare(Vector2 lowLeft, Vector2 highRight, Color color)
        {
            var highLeft = (lowLeft.x, highRight.y);
            var lowRight = (highRight.x, lowLeft.y);
            
            var rLL = new Vector2(lowLeft.x, lowLeft.y);
            var rHL = new Vector2(highLeft.x, highLeft.y);
            var rHR = new Vector2(highRight.x, highRight.y);
            var rLR = new Vector2(lowRight.x, lowRight.y);
            
            DrawLine(rLL, rHL, color);
            DrawLine(rHL, rHR, color);
            DrawLine(rHR, rLR, color);
            DrawLine(rLR, rLL, color);
        }
        
        /// <summary>
        /// Draws a colored line in the graph based on two coordinates
        /// </summary>
        /// <param name="start">Start coordinate for the line</param>
        /// <param name="end">End coordinate for the line</param>
        /// <param name="color">RGBA-styled color for the line</param>
        protected static void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex3(start.x, start.y, 0);
            GL.Vertex3(end.x, end.y, 0);
            GL.End();
        }
        
        /// <summary>
        /// Draws a square around a coordinate that emulates a point
        /// </summary>
        /// <param name="pos">Angular and linear speed coordinate</param>
        /// <param name="squareSize">Size of the point</param>
        /// <param name="color">Color of the point</param>
        protected static void DrawPoint(Vector2 pos, float squareSize, Color color)
        {
            GL.Begin(GL.QUADS);
            GL.Color(color);
            
            GL.Vertex3(pos.x + squareSize, pos.y + squareSize, 0);
            GL.Vertex3(pos.x + squareSize, pos.y - squareSize, 0);
            GL.Vertex3(pos.x - squareSize, pos.y - squareSize, 0);
            GL.Vertex3(pos.x - squareSize, pos.y + squareSize, 0);
            
            GL.End();
        }
        

        /// <summary>
        /// Initializes the plot and draws background
        /// </summary>
        /// <param name="minWidth">Minimum width for the graph</param>
        /// <param name="maxWidth">Maximum width for the graph</param>
        /// <param name="minHeight">Minimum height for the graph</param>
        /// <param name="maxHeight">Maximum height for the graph</param>
        public virtual void InitializePlot()
        {
            if (rect.position.y > 0)
            {
                offsetFromTop = rect.position.y;
            }

            GL.PushMatrix();

            GL.Clear(true, false, Color.black);

            if (material != null)
            {
                material = new Material(
                    Shader.Find("Hidden/Internal-Colored")
                );
                material.SetPass(0);
            }

            loadedDigits = new List<byte[]>();
            
            dot = System.IO.File.ReadAllBytes("Assets/QuanticBrains/MotionMatching/Materials/Numbers/White/dot.png");
            trace = System.IO.File.ReadAllBytes("Assets/QuanticBrains/MotionMatching/Materials/Numbers/White/-.png");
            
            for (var i = 0; i < 10; i++)
            {
                var filename = "Assets/QuanticBrains/MotionMatching/Materials/Numbers/White/" + i + ".png";
                loadedDigits.Add(System.IO.File.ReadAllBytes(filename));
            }
        }
        
        /// <summary>
        /// Closes GL and pops the matrix to draw
        /// </summary>
        public void FinalizePlot()
        {
            GL.PopMatrix();
        }
    }
}
