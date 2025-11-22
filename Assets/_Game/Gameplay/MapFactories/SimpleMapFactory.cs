using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [CreateAssetMenu(fileName = "SimpleMapFactory", menuName = "ShipIt/MapFactory/Simple Map Factory")]
    public class SimpleMapFactory : MapFactory
    {
        [SerializeField] AstralTargetBuilder targetBuilder;

        public override int SpawnMap(Transform anchor, int seed)
        {
            if (anchor == null || planetFactory == null || planetQuantity <= 0)
            {
                LastSeed = 0;
                return LastSeed;
            }

            var previousState = Random.state;
            Random.InitState(seed);

            float minDistance = minDistanceBetweenPlanets;
            float maxDistance = maxDistanceBetweenPlanets;

            AstralBody firstPlanet = planetFactory.SpawnBody(anchor.position, anchor.rotation);
            if (firstPlanet == null)
            {
                LastSeed = 0;
                Random.state = previousState;
                return LastSeed;
            }

            firstPlanet.AddAstralComponent(componentBuilders[0].GetComponent());
            firstPlanet.gameObject.name = "Planet 1";
            Transform prevPlanet = firstPlanet.transform;
            AstralBody lastPlanet = firstPlanet;

            for (int i = 1; i < planetQuantity; i++)
            {
                float distance = Random.Range(minDistance, maxDistance);
                Vector3 spawnPos = prevPlanet.position + prevPlanet.forward * distance;
                Quaternion spawnRot = Random.rotation;

                AstralBody planet = planetFactory.SpawnBody(spawnPos, spawnRot);
                if (planet == null)
                {
                    continue;
                }

                planet.AddAstralComponent(componentBuilders[0].GetComponent());
                planet.gameObject.name = $"Planet {i + 1}";
                prevPlanet = planet.transform;
                lastPlanet = planet;
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
