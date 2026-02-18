using System.Collections.Generic;
using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    public class PathFinder : MonoBehaviour
    {
        [SerializeField] Ship ship;

        AstralBody[,,] planetMatrix;
        Vector3 cellSize = Vector3.one;
        readonly Dictionary<AstralBody, Vector3Int> planetIndexLookup = new Dictionary<AstralBody, Vector3Int>();

        public void SetGrid()
        {
            if (planetMatrix != null)
                return;

            AstralManager astralManager = AstralManager.inst;
            if (!astralManager)
                return;

            Transform[,,] mapGrid = astralManager.MapGrid;
            if (mapGrid == null)
                return;

            int sizeX = mapGrid.GetLength(0);
            int sizeY = mapGrid.GetLength(1);
            int sizeZ = mapGrid.GetLength(2);
            planetMatrix = new AstralBody[sizeX, sizeY, sizeZ];

            for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
            for (int z = 0; z < sizeZ; z++)
            {
                Transform planetTransform = mapGrid[x, y, z];
                if (planetTransform && planetTransform.TryGetComponent(out AstralBody body))
                    planetMatrix[x, y, z] = body;
            }

            cellSize = astralManager.MapCellSize;
            CachePlanetIndices();
        }

        public List<AstralBody> GetPaths(AstralBody planet)
        {
            var jumpablePlanets = new List<AstralBody>();

            if (!planet || planetMatrix == null)
                return jumpablePlanets;

            float maxJumpDistance = ship ? ship.MaxJumpDistance : 0f;
            if (maxJumpDistance <= 0f)
                return jumpablePlanets;

            float sqrMaxJumpDistance = maxJumpDistance * maxJumpDistance;

            if (!planetIndexLookup.TryGetValue(planet, out Vector3Int planetIndex))
                return jumpablePlanets;

            int sizeX = planetMatrix.GetLength(0);
            int sizeY = planetMatrix.GetLength(1);
            int sizeZ = planetMatrix.GetLength(2);

            int maxCellDeltaX = GetMaxCellDelta(cellSize.x, maxJumpDistance, sizeX);
            int maxCellDeltaY = GetMaxCellDelta(cellSize.y, maxJumpDistance, sizeY);
            int maxCellDeltaZ = GetMaxCellDelta(cellSize.z, maxJumpDistance, sizeZ);

            int minX = Mathf.Clamp(planetIndex.x - maxCellDeltaX, 0, sizeX - 1);
            int maxX = Mathf.Clamp(planetIndex.x + maxCellDeltaX, 0, sizeX - 1);
            int minY = Mathf.Clamp(planetIndex.y - maxCellDeltaY, 0, sizeY - 1);
            int maxY = Mathf.Clamp(planetIndex.y + maxCellDeltaY, 0, sizeY - 1);
            int minZ = Mathf.Clamp(planetIndex.z - maxCellDeltaZ, 0, sizeZ - 1);
            int maxZ = Mathf.Clamp(planetIndex.z + maxCellDeltaZ, 0, sizeZ - 1);

            for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            for (int z = minZ; z <= maxZ; z++)
            {
                AstralBody candidate = planetMatrix[x, y, z];
                if (!candidate || candidate == planet)
                    continue;

                Vector3 toCandidate = candidate.transform.position - planet.transform.position;
                float sqrDistance = toCandidate.sqrMagnitude;
                if (sqrDistance > sqrMaxJumpDistance || sqrDistance <= Mathf.Epsilon)
                    continue;
                
                jumpablePlanets.Add(candidate);
            }

            return jumpablePlanets;
        }

        void CachePlanetIndices()
        {
            if (planetMatrix == null)
                return;

            planetIndexLookup.Clear();

            int sizeX = planetMatrix.GetLength(0);
            int sizeY = planetMatrix.GetLength(1);
            int sizeZ = planetMatrix.GetLength(2);

            for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
            for (int z = 0; z < sizeZ; z++)
            {
                AstralBody planet = planetMatrix[x, y, z];
                if (planet) 
                    planetIndexLookup[planet] = new Vector3Int(x, y, z);
            }
        }

        static int GetMaxCellDelta(float axisCellSize, float maxJumpDistance, int axisSize)
        {
            if (axisSize <= 0)
                return 0;

            if (axisCellSize <= Mathf.Epsilon)
                return axisSize;

            return Mathf.Max(0, Mathf.CeilToInt(maxJumpDistance / axisCellSize));
        }
    }
}
