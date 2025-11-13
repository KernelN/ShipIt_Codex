using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [CreateAssetMenu(fileName = "SimpleMapFactory", menuName = "ShipIt/Astral/Simple Map Factory")]
    public class SimpleMapFactory : MapFactory
    {
        public override int SpawnMap(Transform anchor)
        {
            if (anchor == null || PlanetPrefab == null || PlanetQuantity <= 0)
            {
                LastSeed = 0;
                return LastSeed;
            }

            int seed = Random.Range(int.MinValue, int.MaxValue);
            var previousState = Random.state;
            Random.InitState(seed);

            float minDistance = Mathf.Min(MinDistanceBetweenPlanets, MaxDistanceBetweenPlanets);
            float maxDistance = Mathf.Max(MinDistanceBetweenPlanets, MaxDistanceBetweenPlanets);

            Quaternion firstRotation = Random.rotation;
            GameObject planetInstance = Instantiate(PlanetPrefab, anchor.position, firstRotation, anchor);
            Transform previousPlanet = planetInstance.transform;

            for (int i = 1; i < PlanetQuantity; i++)
            {
                float distance = Random.Range(minDistance, maxDistance);
                Vector3 spawnPosition = previousPlanet.position + previousPlanet.forward * distance;
                Quaternion spawnRotation = Random.rotation;

                planetInstance = Instantiate(PlanetPrefab, spawnPosition, spawnRotation, anchor);
                previousPlanet = planetInstance.transform;
            }

            LastSeed = seed;
            Random.state = previousState;

            return LastSeed;
        }
    }
}
