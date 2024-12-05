using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor.Analysis
{
    public class TransitionCostGraph : CustomChartEditor
    {
        private float _yMax = 10, _xMargin = 10, _yMargin = 25;

        private void OnEnable()
        {
            material = new Material(
                Shader.Find("Hidden/Internal-Colored")
            );

            rect = Rect.zero;
        }

        public void Reset()
        {
            _yMax = 10;
            _xMargin = 10;
            _yMargin = 25;
        }

        /// <summary>
        /// Draws the axes lines
        /// </summary>
        private void DrawAxes()
        {
            DrawXAxis(RectPosition(0, 0), RectPosition(rect.width, 0), Color.green);
        }

        /// <summary>
        /// Draws the labels for the axes
        /// </summary>
        /// <param name="size">Size for the labels</param>
        public void DrawLabels(int size)
        {            
            for (var i = 0f; i <= _yMax; i += (_yMax / 10f))
            {
                DrawNumber(RectPosition(2 * _xMargin, i), i, size);
            }
        }

        /// <summary>
        /// Draws a click from the user
        /// </summary>
        /// <param name="x">X coordinate on the window</param>
        /// <param name="cost">Y coordinate, interpreted as cost, on the window</param>
        public void DrawClick(float x, float cost)
        {
            DrawPoint(RectPosition(rect.width - x, cost), 2, Color.cyan);
        }

        /// <summary>
        /// Draws the transition cost along the time for the character
        /// </summary>
        /// <param name="costs">Transition costs</param>
        public void DrawTransitionCost(ref List<GraphPointInfo> costs)
        {
            if (costs.Count < 2)
            {
                return;
            }

            var oldValue = _yMax;
            _yMax = (int)Math.Ceiling(costs.Max().distance);

            if (_yMax == 0)
            {
                _yMax = oldValue;
            }

            for (var i = costs.Count - 1; i > 0; i--)
            {
                var start = RectPosition(rect.width - costs.Count + i, costs[i].distance);
                var end = RectPosition(rect.width - costs.Count + i - 1, costs[i - 1].distance);
                
                DrawLine(start, end, Color.magenta);
            }
        }
        
        /// <summary>
        /// Converts all linear and angular speed coordinates into positions in the graph
        /// </summary>
        /// <param name="x">Angular speed coordinate</param>
        /// <param name="y">Linear speed coordinate</param>
        /// <returns></returns>
        private Vector2 RectPosition(float x, float y)
        {
            return new Vector2(
                x, 
                (rect.height - _yMargin) - y * (rect.height - 2 * _yMargin) / _yMax
                );
        }

        /// <summary>
        /// Initializes the plot and draws background
        /// </summary>
        /// <param name="minWidth">Minimum width for the graph</param>
        /// <param name="maxWidth">Maximum width for the graph</param>
        /// <param name="minHeight">Minimum height for the graph</param>
        /// <param name="maxHeight">Maximum height for the graph</param>
        public override void InitializePlot()
        {
            base.InitializePlot();

            DrawBgAndGrid();
            DrawAxes();
        }
    }
}
