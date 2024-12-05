using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Editor.Analysis;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor.DatasetConfiguration
{
    public class TimelineGraph : CustomChartEditor
    {
        private int _minuteHeight, _secondsHeight;
        private float _timeLimit, _width, _height;
        private List<float> _tickPositions;
        
        public readonly int spaceBetweenTextFields = 2, tagFieldVerticalSize = 19, styleFieldVerticalSize = 18, timeRulerLimit = 30;
        
        private void OnEnable()
        {
            material = new Material(
                Shader.Find("Hidden/Internal-Colored")
            );
            
            _secondsHeight = 5;
            _minuteHeight = 10;
            rect = Rect.zero;
        }
        
        public void DrawBackground(int offset)
        {
            DrawSquare(new Vector2(0, offset), new Vector2(_width, timeRulerLimit), new Color(0.2f, 0.2f,  0.2f));
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

            _width = rect.width;
            _height = rect.height;
        }

        public float NavPosToTime(float navPos, float totalFrames, float framesRate)
        {
            return NavPosToFrame(navPos, totalFrames) / framesRate;
        }
        
        public float NavPosToFrame(float navPos, float totalFrames)
        {
            if (navPos <= 0)
            {
                return 0;
            }

            if (navPos >= _width)
            {
                return totalFrames;
            }

            return (navPos * totalFrames) / _width;
        }
        
        public float NavFrameToPos(int currentFrame, float totalFrames)
        {
            if (currentFrame <= 0)
            {
                return 0;
            }

            return (currentFrame * _width) / totalFrames;
        }

        private float TimeToNavPos(float time)
        {
            if (time < 0)
            {
                return 0;
            }

            return (_width / _timeLimit) * time;
        }

        private void DrawTicks(float frames)
        {
            var framesSeparation = _width / frames;
            var minLimit = (int)(frames / (frames > 30 ? 30 : frames));
            var numberLimit = minLimit * 3;
            var redLimit = minLimit * 6;

            _tickPositions = new List<float>();
            
            for (var i = 0; i <= frames; i++)
            {
                var start = new Vector2(i * framesSeparation, timeRulerLimit);

                _tickPositions.Add(start.x);
                
                var end = new Vector2(i * framesSeparation, timeRulerLimit - _secondsHeight );
                if (redLimit != 0 && i % redLimit == 0)
                {
                    end.y -= _minuteHeight - _secondsHeight;
                    DrawLine(start, end, Color.red);
                    DrawNumber(new Vector2(end.x, end.y - 10), i, 6);
                    continue;
                }
                
                if (numberLimit != 0 && i % numberLimit == 0)
                {
                    DrawNumber(new Vector2(end.x, end.y - 10), i, 5);
                    DrawLine(start, end, Color.white);
                    continue;
                }
                
                if (minLimit != 0 && i % minLimit == 0)
                {
                    DrawLine(start, end, Color.white);
                }
            }
        }

        public void DrawNavigator(float x)
        {
            if (x < 0)
            {
                x = 0;
            }

            if (x > _width)
            {
                x = _width;
            }

            GL.Begin(GL.QUADS);
            GL.Color(Color.red);

            GL.Vertex3(x, timeRulerLimit, 0);
            GL.Vertex3(x, timeRulerLimit - _minuteHeight, 0);
            GL.Vertex3(x - _secondsHeight, timeRulerLimit - (_minuteHeight + _secondsHeight), 0);
            GL.Vertex3(x - _secondsHeight, timeRulerLimit - _secondsHeight, 0);

            GL.Vertex3(x, timeRulerLimit, 0);
            GL.Vertex3(x, timeRulerLimit - _minuteHeight, 0);
            GL.Vertex3(x + _secondsHeight - 1, timeRulerLimit - (_minuteHeight + _secondsHeight), 0);
            GL.Vertex3(x + _secondsHeight - 1, timeRulerLimit - _secondsHeight, 0);
            
            GL.End();
            
            DrawLine(new Vector2(x - 1, timeRulerLimit), new Vector2(x - 1, _height), Color.red);
        }

        public void DrawHelpers(float x, float y, Color col)
        {
            DrawLine(new Vector2(x, y), new Vector2(x, 0), col);
            DrawLine(new Vector2(x, y), new Vector2(0, y), col);
        }

        public void DrawTagInterval(int start, int end, int tagNumber, Color color, int offset, float framerate, int verticalSizeModifier=0)
        {
            var startPos = _tickPositions[start];
            var endPos = _tickPositions[end];

            if (tagNumber < 0)
            {
                return;
            }
            
            var lowerLeft = new Vector2(startPos,
                offset + (tagFieldVerticalSize + verticalSizeModifier) * (tagNumber + 1) + spaceBetweenTextFields * tagNumber);

            var highRight = new Vector2(endPos,
                offset + (tagFieldVerticalSize + verticalSizeModifier) * tagNumber + spaceBetweenTextFields * (tagNumber + 1) + spaceBetweenTextFields);

            DrawSquare(lowerLeft, highRight, color);
        }

        /// <summary>
        /// Receives the time limit and draws the upper time ruler for the timeline
        /// </summary>
        /// <param name="timeLimit">Time limit in seconds</param>
        public void DrawTime(float timeLimit, float totalFrames)
        {
            _timeLimit = timeLimit;
            DrawTicks(totalFrames);
        }
    }
}
