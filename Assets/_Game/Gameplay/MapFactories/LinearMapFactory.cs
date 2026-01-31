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
        Transform[,,] planetGrid;

        public override int SpawnMap(Transform anchor, int seed)
        {
            if (anchor == null || planetFactory == null || planetQuantity <= 0)
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

            Vector3 anchorPosition = new Vector3(gridSize.x * 0.5f, 0f, 0f);
            float cellSpacing = minDistanceBetweenPlanets;
            int totalSlots = gridSize.x * gridSize.y * gridSize.z;
            int totalPlanets = Mathf.Min(planetQuantity, totalSlots);
            planetGrid = new Transform[gridSize.x, gridSize.y, gridSize.z];

            AstralBody firstPlanet = planetFactory.SpawnBody(anchorPosition, anchor.rotation);
            if (firstPlanet == null)
            {
                LastSeed = 0;
                Random.state = previousState;
                return LastSeed;
            }

            firstPlanet.AddAstralComponent(componentBuilders[0].GetComponent());
            firstPlanet.gameObject.name = "Planet 1";
            Quaternion referenceRotation = firstPlanet.transform.rotation;
            AstralBody lastPlanet = firstPlanet;
            planetGrid[0, 0, 0] = firstPlanet.transform;

            int planetIndex = 1;
            for (int x = 0; x < gridSize.x && planetIndex < totalPlanets; x++)
            {
                for (int y = 0; y < gridSize.y && planetIndex < totalPlanets; y++)
                {
                    for (int z = 0; z < gridSize.z && planetIndex < totalPlanets; z++)
                    {
                        if (x == 0 && y == 0 && z == 0)
                        {
                            continue;
                        }

                        Vector3 spawnPos = anchorPosition + new Vector3(x * cellSpacing, y * cellSpacing, z * cellSpacing);
                        float rotationLimit = Mathf.Max(k_MinRotationOffset, maxRotationAngle);
                        Vector3 targetEuler = Random.rotation.eulerAngles;
                        Vector3 constrainedEuler = Vector3.Scale(targetEuler, rotationAxisMultiplier);
                        Quaternion targetRotation = Quaternion.Euler(constrainedEuler);
                        float rotationStep = Random.Range(k_MinRotationOffset, rotationLimit);
                        Quaternion spawnRot = Quaternion.RotateTowards(referenceRotation, targetRotation, rotationStep);

                        AstralBody planet = planetFactory.SpawnBody(spawnPos, spawnRot);
                        if (planet == null)
                        {
                            continue;
                        }

                        planet.AddAstralComponent(componentBuilders[0].GetComponent());
                        planet.gameObject.name = $"Planet {planetIndex + 1}";
                        planetGrid[x, y, z] = planet.transform;
                        lastPlanet = planet;
                        planetIndex++;
                    }
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
