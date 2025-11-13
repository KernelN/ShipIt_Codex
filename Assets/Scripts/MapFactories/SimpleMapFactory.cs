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

            float minDistance = minDistanceBetweenPlanets;
            float maxDistance = maxDistanceBetweenPlanets;

            ComponentExposer inst = Instantiate(planetPrefab, anchor.position, anchor.rotation);
            inst.AstralBody.AddAstralComponent(componentBuilders[0].GetComponent());
            inst.gameObject.name = "Planet 1";
            Transform prevPlanet = inst.transform;

            for (int i = 1; i < planetQuantity; i++)
            {
                float distance = Random.Range(minDistance, maxDistance);
                Vector3 spawnPos = prevPlanet.position + prevPlanet.forward * distance;
                Quaternion spawnRot = Random.rotation;

                inst = Instantiate(planetPrefab, spawnPos, spawnRot);
                inst.AstralBody.AddAstralComponent(componentBuilders[0].GetComponent());
                inst.gameObject.name = $"Planet {i + 1}";
                prevPlanet = inst.transform;
            }

            LastSeed = seed;
            Random.state = previousState;

            return LastSeed;
        }
    }
}
