using System.Collections.Generic;
using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    public class CameraAligner : MonoBehaviour
    {
        public enum AxisDirection
        {
            XPositive,
            XNegative,
            YPositive,
            YNegative,
            ZPositive,
            ZNegative
        }

        [SerializeField] Camera targetCamera;
        [SerializeField] AxisDirection viewAxis = AxisDirection.ZNegative;
        [SerializeField, Min(0f)] float framingPadding = 2f;
        [SerializeField, Min(0.01f)] float depthPadding = 5f;
        [SerializeField, Min(0.01f)] float minimumOrthographicSize = 1f;
        [SerializeField] float rotationOffset;

        public void AlignToMap(MapData mapData)
        {
            if (mapData == null)
                return;

            List<Transform> planets = new List<Transform>();
            Transform[,,] grid = mapData.grid;
            if (grid != null)
            {
                foreach (Transform planet in grid)
                {
                    if (planet)
                        planets.Add(planet);
                }
            }

            AlignToPlanets(planets);
        }

        public void AlignToPlanets(IReadOnlyList<Transform> planets)
        {
            if (!targetCamera || planets == null || planets.Count == 0)
                return;

            targetCamera.orthographic = true;

            Vector3 forward = GetAxisVector(viewAxis);
            Vector3 up = GetUpVector(forward);
            Vector3 right = Vector3.Cross(up, forward).normalized;
            up = Vector3.Cross(forward, right).normalized;

            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            float minZ = float.PositiveInfinity;
            float maxZ = float.NegativeInfinity;

            for (int i = 0; i < planets.Count; i++)
            {
                Transform planet = planets[i];
                if (!planet)
                    continue;

                float radius = Mathf.Max(planet.lossyScale.x, planet.lossyScale.y, planet.lossyScale.z) * 0.5f;
                Vector3 position = planet.position;

                float x = Vector3.Dot(position, right);
                float y = Vector3.Dot(position, up);
                float z = Vector3.Dot(position, forward);

                minX = Mathf.Min(minX, x - radius);
                maxX = Mathf.Max(maxX, x + radius);
                minY = Mathf.Min(minY, y - radius);
                maxY = Mathf.Max(maxY, y + radius);
                minZ = Mathf.Min(minZ, z - radius);
                maxZ = Mathf.Max(maxZ, z + radius);
            }

            if (float.IsInfinity(minX))
                return;

            minX -= framingPadding;
            maxX += framingPadding;
            minY -= framingPadding;
            maxY += framingPadding;

            float centerX = (minX + maxX) * 0.5f;
            float centerY = (minY + maxY) * 0.5f;
            float cameraDepth = minZ - depthPadding;

            float requiredHalfWidth = (maxX - minX) * 0.5f;
            float requiredHalfHeight = (maxY - minY) * 0.5f;
            float aspect = Mathf.Max(0.0001f, targetCamera.aspect);

            float orthographicSize = Mathf.Max(requiredHalfHeight, requiredHalfWidth / aspect, minimumOrthographicSize);

            Quaternion baseRotation = Quaternion.LookRotation(forward, up);
            Quaternion offsetRotation = Quaternion.AngleAxis(rotationOffset, Vector3.forward);
            targetCamera.transform.rotation = baseRotation * offsetRotation;
            targetCamera.transform.position = (right * centerX) + (up * centerY) + (forward * cameraDepth);
            targetCamera.orthographicSize = orthographicSize;
            targetCamera.nearClipPlane = 0.01f;
            targetCamera.farClipPlane = Mathf.Max(100f, (maxZ - cameraDepth) + depthPadding);
        }

        static Vector3 GetAxisVector(AxisDirection axis)
        {
            switch (axis)
            {
                case AxisDirection.XPositive: return Vector3.right;
                case AxisDirection.XNegative: return Vector3.left;
                case AxisDirection.YPositive: return Vector3.up;
                case AxisDirection.YNegative: return Vector3.down;
                case AxisDirection.ZPositive: return Vector3.forward;
                default: return Vector3.back;
            }
        }

        static Vector3 GetUpVector(Vector3 forward)
        {
            if (Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.99f)
                return Vector3.forward;

            return Vector3.up;
        }
    }
}
