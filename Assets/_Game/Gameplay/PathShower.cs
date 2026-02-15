using System.Collections.Generic;
using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [System.Serializable]
    public class PathShower
    {
        [SerializeField] LineRenderer pathRenderer;
        [SerializeField] Color pathColor = Color.cyan;

        public void ShowPath(IReadOnlyList<Transform> path)
        {
            if (!pathRenderer)
                return;

            pathRenderer.useWorldSpace = true;
            pathRenderer.startColor = pathColor;
            pathRenderer.endColor = pathColor;

            int pointCount = path != null ? path.Count : 0;
            pathRenderer.positionCount = pointCount;
            for (int i = 0; i < pointCount; i++)
            {
                Transform planet = path[i];
                Vector3 point = planet ? planet.position : Vector3.zero;
                pathRenderer.SetPosition(i, point);
            }
        }
    }
}
