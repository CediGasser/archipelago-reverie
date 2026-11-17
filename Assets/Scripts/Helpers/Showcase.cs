using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
    // Orbits a camera around a target while keeping all of the target's renderers in frame
    public class Showcase : MonoBehaviour
    {
        [Tooltip("Object to orbit around; its renderers (including children) are kept in frame.")]
        [SerializeField] private Transform target;

        [Tooltip("Camera to orbit; defaults to the main camera.")]
        [SerializeField] private Camera targetCamera;

        [Header("Camera")]
        [SerializeField, Range(-89f, 89f)] private float viewPitch = 35f;
        [Tooltip("Angle around the target the orbit starts at.")]
        [SerializeField] private float viewYaw = 45f;
        [Tooltip("Scales the camera distance.")]
        [SerializeField, Min(0.1f)] private float framingPadding = 1f;

        [Header("Showcase")]
        [Tooltip("Degrees per second around the vertical axis.")]
        [SerializeField] private float spinSpeed = 20f;

        private readonly List<Renderer> _renderers = new();
        private float _yaw;

        private void Start()
        {
            if (this.targetCamera == null) this.targetCamera = Camera.main;

            if (this.target == null || this.targetCamera == null)
            {
                Debug.LogError($"{this.name}: assign a target and a camera.", this);
                this.enabled = false;
                return;
            }

            this._yaw = this.viewYaw;
        }

        // LateUpdate so the camera frames the target where it ended up this frame
        private void LateUpdate()
        {
            if (!this.TryGetTargetBounds(out Bounds bounds)) return;

            this._yaw = Mathf.Repeat(this._yaw + this.spinSpeed * Time.deltaTime, 360f);

            // Fit the bounding sphere, so the target stays in view from every angle
            float radius = bounds.extents.magnitude * this.framingPadding;
            float verticalHalfFov = this.targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float horizontalHalfFov = Mathf.Atan(Mathf.Tan(verticalHalfFov) * this.targetCamera.aspect);
            float distance = radius / Mathf.Sin(Mathf.Min(verticalHalfFov, horizontalHalfFov));

            Quaternion rotation = Quaternion.Euler(this.viewPitch, this._yaw, 0f);
            this.targetCamera.transform.SetPositionAndRotation(bounds.center - rotation * Vector3.forward * distance, rotation);
        }

        // Gathered every frame because the target's meshes can be created or regenerated after Start
        private bool TryGetTargetBounds(out Bounds bounds)
        {
            bounds = default;
            this.target.GetComponentsInChildren(this._renderers);
            if (this._renderers.Count == 0) return false;

            bounds = this._renderers[0].bounds;
            for (int i = 1; i < this._renderers.Count; i++)
                bounds.Encapsulate(this._renderers[i].bounds);

            return true;
        }
    }
}
