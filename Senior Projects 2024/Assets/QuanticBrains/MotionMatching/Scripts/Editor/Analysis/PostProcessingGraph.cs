using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor.Analysis
{
    public class PostProcessingGraph : CustomChartEditor
    {
        private float _yMax = 5, _xMax = 210, _xMargin = 10;
        private const float YMargin = 50, SquareSize = 2f;
        private float _height, _width;
        
        private void OnEnable()
        {
            material = new Material(
                Shader.Find("Hidden/Internal-Colored")
            );

            rect = Rect.zero;
        }

        /// <summary>
        /// Draws the axes lines
        /// </summary>
        private void DrawAxes()
        {
            DrawXAxis(RectPosition(-_xMax, 0), RectPosition(_xMax, 0), Color.red);
            DrawYAxis(RectPosition(0, 0), RectPosition(0, _yMax), Color.green);
        }

        /// <summary>
        /// Draws the labels for the axes
        /// </summary>
        /// <param name="size">Size for the labels</param>
        public void DrawLabels(int size)
        {
            DrawNumber(RectPosition(0, (float)(_yMax * -0.025)), 0f, size);
            
            // x axis
            for (var i = _xMax * 0.2f; i <= _xMax ; i = i + _xMax * 0.2f)
            {
                DrawNumber(RectPosition(Mathf.Round(i), (float)(_yMax * -0.025)), i, size);
                DrawNumber(RectPosition(-Mathf.Round(i),(float)(_yMax * -0.025)), -i, size);
            }
            
            // y axis
            for (var i = 1; i <= _yMax; i++)
            {
                DrawNumber(RectPosition(-_xMax - _xMargin / 2, i), i, size);
            }
        }
        
        /// <summary>
        /// Converts all linear and angular speed coordinates into positions in the graph
        /// </summary>
        /// <param name="x">Angular speed coordinate</param>
        /// <param name="y">Linear speed coordinate</param>
        /// <returns></returns>
        public Vector2 RectPosition(float x, float y)
        {
            var heightAdjustedToZero = _height - 2 * YMargin;

            return new Vector2(
                (_xMax + _xMargin + x) / (_xMax + _xMargin) * _width / 2, 
                heightAdjustedToZero - y * heightAdjustedToZero / _yMax + YMargin
                );
        }

        /// <summary>
        /// Draws a click from the user
        /// </summary>
        /// <param name="x">X coordinate on the window</param>
        /// <param name="y">Y coordinate on the window</param>
        public void DrawClick(float x, float y)
        {
            DrawPoint(RectPosition(x, y), 2, Color.magenta);
        }
        
        /// <summary>
        /// Maps a position from the window into angular and linear speeds
        /// </summary>
        /// <param name="x">X coordinate on the window</param>
        /// <param name="y">Y coordinate on the window</param>
        /// <returns></returns>
        public Vector2 RectPositionToSpeed(float x, float y)
        {
            var heightAdjustedToZero = _height - 2 * YMargin;

            var speed = _yMax * ((heightAdjustedToZero - (y - YMargin - offsetFromTop)) / heightAdjustedToZero);

            return new Vector2(
                (_xMax + _xMargin) * (2 * x - _width) / _width,
                speed
                //(1 + _yMax) - (y + offsetFromTop) * _yMax / heightAdjustedToZero
            );
        }
        
        /// <summary>
        /// Initializes GL and draw the background, grid and axes
        /// </summary>
        /// <param name="minWidth">Minimum width for the graph</param>
        /// <param name="maxWidth">Maximum width for the graph</param>
        /// <param name="minHeight">Minimum height for the graph</param>
        /// <param name="maxHeight">Maximum height for the graph</param>
        public override void InitializePlot()
        {
            base.InitializePlot();

            if (rect.width > 1)
            {
                _width = rect.width;
                _height = rect.height;
            }
            
            DrawBgAndGrid();
            DrawAxes();
        }

        /// <summary>
        /// Displays a colored square to represent the amount of redundancy present in the dataset poses
        /// </summary>
        /// <param name="speedPair">Linear and angular speed coordinates relative to the score</param>
        /// <param name="score">How redundant the poses are</param>
        public void DisplayRedundancy((Vector2, Vector2) speedPair, float score=1f)
        {
            var alpha = 0.3f;

            if (score < 0)
            {
                alpha = 0;
            }
            
            var color = new Color(1-score, score, 0, alpha);

            var lowLeft = speedPair.Item1;
            var highRight = speedPair.Item2;
            
            DrawSquare(
                RectPosition(lowLeft.x, lowLeft.y), 
                RectPosition(highRight.x, highRight.y), 
                color
                );
        }
        
        /// <summary>
        /// Displays coverage by plotting every linear and angular speed present in the dataset poses
        /// </summary>
        /// <param name="linearVelocities">Array of linear speeds</param>
        /// <param name="angularVelocities">Array of angular speeds</param>
        /// <param name="size">Size of the representative point</param>
        public void GraphDatasetCoverage(ref List<List<float>> linearVelocities, ref List<List<float>> angularVelocities, float size=SquareSize)
        {
            if (linearVelocities.Count == 0)
            {
                return;
            }
         
            // draw points   
            _yMax = (float)Math.Ceiling(linearVelocities.SelectMany(i => i).Max());
            _xMax = (float)Math.Ceiling(angularVelocities.SelectMany(i => i).Max());
            _xMargin = _xMax * 0.05f;
            for (var i = 0; i < linearVelocities.Count; i++)
            {
                for (var j = 0; j < linearVelocities[i].Count; j++)
                {
                    DrawPoint(RectPosition(angularVelocities[i][j], linearVelocities[i][j]), size, Color.cyan);
                }
            }
        }
    }
}
