using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [CreateAssetMenu(fileName = "SimpleMapFactory", menuName = "ShipIt/Astral/Simple Map Factory")]
    public class SimpleMapFactory : MapFactory
    {
        public override int SpawnMap(Transform anchor)
        {
            if (anchor == null || planetPrefab == null || planetQuantity <= 0)
            {
                LastSeed = 0;
                return LastSeed;
            }

            int seed = Random.Range(int.MinValue, int.MaxValue);
            var previousState = Random.state;
            Random.InitState(seed);

            float minDistance = Mathf.Min(minDistanceBetweenPlanets, maxDistanceBetweenPlanets);
            float maxDistance = Mathf.Max(minDistanceBetweenPlanets, maxDistanceBetweenPlanets);

            AstralBody planetInstance = Instantiate(planetPrefab, anchor.position, anchor.rotation);
            planetInstance.AddAstralComponent(componentBuilders[0].GetComponent());
            planetInstance.gameObject.name = "Planet 1";
            Transform previousPlanet = planetInstance.transform;

            for (int i = 1; i < planetQuantity; i++)
            {
                float distance = Random.Range(minDistance, maxDistance);
                Vector3 spawnPos = previousPlanet.position + previousPlanet.forward * distance;
                Quaternion spawnRot = Random.rotation;

                planetInstance = Instantiate(planetPrefab, spawnPos, spawnRot);
                planetInstance.AddAstralComponent(componentBuilders[0].GetComponent());
                planetInstance.gameObject.name = $"Planet {i + 1}";
                previousPlanet = planetInstance.transform;
            }

            LastSeed = seed;
            Random.state = previousState;

            return LastSeed;
        }
    }
}
