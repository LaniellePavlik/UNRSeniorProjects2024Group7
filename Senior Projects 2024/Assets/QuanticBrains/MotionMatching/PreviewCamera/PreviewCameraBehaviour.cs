using UnityEngine;

namespace QuanticBrains.MotionMatching.PreviewCamera
{
    public class PreviewCameraBehaviour : MonoBehaviour
    {
        private Transform _camera;
        private Transform _target;
        

        public void SetCamera(Transform cam)
        {
            _camera = cam;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void UpdateCameraView(CameraView view, float depth, float vertical, float horizontal)
        {
            if (!_camera) return;
            if (!_target) return;
        
            var right = _target.right;
            var forward = _target.forward;
            var up = _target.up;

            Vector3 position = Vector3.zero;
        
            switch (view)
            {
                case CameraView.Left:
                    position = -right * depth - forward * horizontal + up * vertical;
                    break;
                case CameraView.Right:
                    position = right * depth + forward * horizontal + up * vertical;
                    break;
                case CameraView.Front:
                    position = forward * depth + up * vertical - right * horizontal;
                    break;
                case CameraView.Back:
                    position = -forward * depth + up * vertical + right * horizontal;
                    break;
            }

            position += up;
            _camera.transform.position = _target.TransformPoint(position);
            _camera.forward = Vector3.ProjectOnPlane((_target.position - _camera.position).normalized, up);
        }
    }

    public enum CameraView
    {
        Left,
        Right,
        Front,
        Back
    }
}
