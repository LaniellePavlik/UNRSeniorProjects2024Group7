using UnityEngine;

namespace ColorStudio {

    public static class Cube {

        public enum Side {
            Top,
            Bottom,
            Forward,
            Back,
            Left,
            Right
        }

        public static readonly Vector3[] faceVerticesForward = {
            new Vector3 (0.5f, -0.5f, 0.5f),
            new Vector3 (0.5f, 0.5f, 0.5f),
            new Vector3 (-0.5f, -0.5f, 0.5f),
            new Vector3 (-0.5f, 0.5f, 0.5f)
        };
        public static readonly Vector3[] faceVerticesBack ={
            new Vector3 (-0.5f, -0.5f, -0.5f),
            new Vector3 (-0.5f, 0.5f, -0.5f),
            new Vector3 (0.5f, -0.5f, -0.5f),
            new Vector3 (0.5f, 0.5f, -0.5f)
        };
        public static readonly Vector3[] faceVerticesLeft = {
            new Vector3 (-0.5f, -0.5f, 0.5f),
            new Vector3 (-0.5f, 0.5f, 0.5f),
            new Vector3 (-0.5f, -0.5f, -0.5f),
            new Vector3 (-0.5f, 0.5f, -0.5f)
        };
        public static readonly Vector3[] faceVerticesRight = {
            new Vector3 (0.5f, -0.5f, -0.5f),
            new Vector3 (0.5f, 0.5f, -0.5f),
            new Vector3 (0.5f, -0.5f, 0.5f),
            new Vector3 (0.5f, 0.5f, 0.5f)
        };
        public static readonly Vector3[] faceVerticesTop =  {
            new Vector3 (-0.5f, 0.5f, -0.5f),
            new Vector3 (-0.5f, 0.5f, 0.5f),
            new Vector3 (0.5f, 0.5f, -0.5f),
            new Vector3 (0.5f, 0.5f, 0.5f)
        };
        public static readonly Vector3[] faceVerticesTopFlipped =  {
            new Vector3 (-0.5f, 0.5f, 0.5f),
            new Vector3 (-0.5f, 0.5f, -0.5f),
            new Vector3 (0.5f, 0.5f, 0.5f),
            new Vector3 (0.5f, 0.5f, -0.5f)
        };
        public static readonly Vector3[] faceVerticesBottom = {
            new Vector3 (0.5f, -0.5f, -0.5f),
            new Vector3 (0.5f, -0.5f, 0.5f),
            new Vector3 (-0.5f, -0.5f, -0.5f),
            new Vector3 (-0.5f, -0.5f, 0.5f)
        };

        public static readonly Vector3[] normalsBack = {
            Vector3.back, Vector3.back, Vector3.back,Vector3.back
        };
        public static readonly Vector3[] normalsForward = {
            Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward
        };
        public static readonly Vector3[] normalsLeft = {
            Vector3.left, Vector3.left, Vector3.left, Vector3.left
        };
        public static readonly Vector3[] normalsRight = {
            Vector3.right, Vector3.right, Vector3.right, Vector3.right
        };
        public static readonly Vector3[] normalsUp = {
            Vector3.up, Vector3.up, Vector3.up, Vector3.up
        };
        public static readonly Vector3[] normalsDown =  {
            Vector3.down, Vector3.down, Vector3.down,Vector3.down
        };

    }
}
