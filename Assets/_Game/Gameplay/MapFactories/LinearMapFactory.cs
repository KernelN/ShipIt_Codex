using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [CreateAssetMenu(fileName = "LinearMapFactory", menuName = "ShipIt/MapFactory/Linear Map Factory")]
    public class LinearMapFactory : MapFactory
    {
        [SerializeField] AstralTargetBuilder targetBuilder;
        [SerializeField, Range(0f, 180f)] float maxRotationAngle = 45f;
        [SerializeField] Vector3 rotationAxisMultiplier = Vector3.one;
        [SerializeField] Vector3Int gridSize = new Vector3Int(1, 1, 1);

        const float k_MinRotationOffset = 0.1f;
        const int k_MaxPlacementAttempts = 25;
        Transform[,,] planetGrid;

        public override int SpawnMap(Transform anchor, int seed)
        {
            if (!anchor || !planetFactory || planetQuantity <= 0)
            {
                LastSeed = 0;
                return LastSeed;
            }

            var previousState = Random.state;
            Random.InitState(seed);

            if (gridSize.x <= 0 || gridSize.y <= 0 || gridSize.z <= 0)
            {
                LastSeed = 0;
                Random.state = previousState;
                return LastSeed;
            }

            Vector3 anchorPosition = anchor.position;
            int totalCells = gridSize.x * gridSize.y * gridSize.z;
            int totalPlanets = Mathf.Min(planetQuantity, totalCells);
            Vector3 cellSize = planetFactory.MaxScale;
            planetGrid = new Transform[gridSize.x, gridSize.y, gridSize.z];

            AstralBody firstPlanet = planetFactory.SpawnBody(anchorPosition, anchor.rotation);
            if (!firstPlanet)
            {
                LastSeed = 0;
                Random.state = previousState;
                return LastSeed;
            }

            firstPlanet.AddAstralComponent(componentBuilders[0].GetComponent());
            firstPlanet.gameObject.name = "Planet (0, 0, 0)";
            Quaternion referenceRotation = firstPlanet.transform.rotation;
            Transform prevPlanet = firstPlanet.transform;
            AstralBody lastPlanet = firstPlanet;
            planetGrid[0, 0, 0] = firstPlanet.transform;

            for (int i = 1; i < totalPlanets; i++)
            {
                bool placed = false;
                for (int attempt = 0; attempt < k_MaxPlacementAttempts; attempt++)
                {
                    float distance = Random.Range(minDistanceBetweenPlanets, maxDistanceBetweenPlanets);
                    float rotationLimit = Mathf.Max(k_MinRotationOffset, maxRotationAngle);
                    Vector3 targetEuler = Random.rotation.eulerAngles;
                    Vector3 constrainedEuler = Vector3.Scale(targetEuler, rotationAxisMultiplier);
                    Quaternion targetRotation = Quaternion.Euler(constrainedEuler);
                    float rotationStep = Random.Range(k_MinRotationOffset, rotationLimit);
                    Quaternion spawnRot = Quaternion.RotateTowards(referenceRotation, targetRotation, rotationStep);
                    Vector3 direction = spawnRot * Vector3.forward;
                    Vector3 candidatePos = prevPlanet.position + direction * distance;
                    Vector3 relativePos = candidatePos - anchorPosition;
                    int gridX = Mathf.Clamp(Mathf.RoundToInt(relativePos.x / cellSize.x), 0, gridSize.x - 1);
                    int gridY = Mathf.Clamp(Mathf.RoundToInt(relativePos.y / cellSize.y), 0, gridSize.y - 1);
                    int gridZ = Mathf.Clamp(Mathf.RoundToInt(relativePos.z / cellSize.z), 0, gridSize.z - 1);

                    if (planetGrid[gridX, gridY, gridZ])
                        continue;

                    Vector3 gridPos = new Vector3(gridX * cellSize.x, gridY * cellSize.y, gridZ * cellSize.z);
                    Vector3 spawnPos = anchorPosition + gridPos;
                    AstralBody planet = planetFactory.SpawnBody(spawnPos, spawnRot);
                    if (!planet)
                        continue;

                    planet.AddAstralComponent(componentBuilders[0].GetComponent());
                    planet.gameObject.name = $"Planet ({gridX}, {gridY}, {gridZ})";
                    planetGrid[gridX, gridY, gridZ] = planet.transform;

                    prevPlanet = planet.transform;
                    lastPlanet = planet;
                    placed = true;
                    break;
                }

                if (!placed)
                {
                    continue;
                }
            }

            if (lastPlanet && targetBuilder)
            {
                targetBuilder.Build(lastPlanet.transform);
                lastPlanet.AddAstralComponent(targetBuilder.GetComponent());
            }

            LastSeed = seed;
            Random.state = previousState;

            return LastSeed;
        }
    }
}
