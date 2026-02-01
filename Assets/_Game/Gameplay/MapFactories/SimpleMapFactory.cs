using System.Collections.Generic;
using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [CreateAssetMenu(fileName = "SimpleMapFactory", menuName = "ShipIt/MapFactory/Simple Map Factory")]
    public class SimpleMapFactory : MapFactory
    {
        [SerializeField] AstralTargetBuilder targetBuilder;
        [SerializeField, Range(0f, 180f)] float maxRotationAngle = 45f;

        const float k_MinRotationOffset = 0.1f;

        public override MapData SpawnMap(Transform anchor, int seed)
        {
            if (anchor == null || planetFactory == null || planetQuantity <= 0)
            {
                originPlanet = null;
                LastSeed = 0;
                return new MapData(LastSeed, new List<AstralBodyData>(), new Transform[0, 0, 0]);
            }

            var previousState = Random.state;
            Random.InitState(seed);

            float minDistance = minDistanceBetweenPlanets;
            float maxDistance = maxDistanceBetweenPlanets;
            var bodyData = new List<AstralBodyData>();
            var baseComponents = new List<AstralComponentType>();
            if (componentBuilders != null && componentBuilders.Length > 0 && componentBuilders[0] != null)
            {
                baseComponents.Add(componentBuilders[0].GetType);
            }
            Transform[,,] planetGrid = new Transform[planetQuantity, 1, 1];
            int placedIndex = 0;
            int lastBodyIndex = -1;

            AstralBody firstPlanet = planetFactory.SpawnBody(anchor.position, anchor.rotation);
            if (firstPlanet == null)
            {
                originPlanet = null;
                LastSeed = 0;
                Random.state = previousState;
                return new MapData(LastSeed, bodyData, planetGrid);
            }

            firstPlanet.AddAstralComponent(componentBuilders[0].GetComponent());
            firstPlanet.gameObject.name = "Planet 1";
            Transform prevPlanet = firstPlanet.transform;
            AstralBody lastPlanet = firstPlanet;
            originPlanet = firstPlanet.transform;
            planetGrid[placedIndex, 0, 0] = firstPlanet.transform;
            bodyData.Add(new AstralBodyData
            {
                gridPos = new Vector3Int(placedIndex, 0, 0),
                up = firstPlanet.transform.up,
                componentTypes = baseComponents.ToArray()
            });
            lastBodyIndex = bodyData.Count - 1;
            placedIndex++;

            for (int i = 1; i < planetQuantity; i++)
            {
                float distance = Random.Range(minDistance, maxDistance);
                Vector3 spawnPos = prevPlanet.position + prevPlanet.forward * distance;
                float rotationLimit = Mathf.Max(k_MinRotationOffset, maxRotationAngle);
                Quaternion targetRotation = Random.rotation;
                float rotationStep = Random.Range(k_MinRotationOffset, rotationLimit);
                Quaternion spawnRot = Quaternion.RotateTowards(prevPlanet.rotation, targetRotation, rotationStep);

                AstralBody planet = planetFactory.SpawnBody(spawnPos, spawnRot);
                if (planet == null)
                {
                    continue;
                }

                planet.AddAstralComponent(componentBuilders[0].GetComponent());
                planet.gameObject.name = $"Planet {i + 1}";
                prevPlanet = planet.transform;
                lastPlanet = planet;
                if (placedIndex < planetGrid.GetLength(0))
                {
                    planetGrid[placedIndex, 0, 0] = planet.transform;
                }
                bodyData.Add(new AstralBodyData
                {
                    gridPos = new Vector3Int(placedIndex, 0, 0),
                    up = planet.transform.up,
                    componentTypes = baseComponents.ToArray()
                });
                lastBodyIndex = bodyData.Count - 1;
                placedIndex++;
            }

            if (lastPlanet && targetBuilder)
            {
                targetBuilder.Build(lastPlanet.transform);
                lastPlanet.AddAstralComponent(targetBuilder.GetComponent());
                if (lastBodyIndex >= 0)
                {
                    var updatedData = bodyData[lastBodyIndex];
                    var updatedComponents = new List<AstralComponentType>(updatedData.componentTypes);
                    updatedComponents.Add(targetBuilder.GetType);
                    updatedData.componentTypes = updatedComponents.ToArray();
                    bodyData[lastBodyIndex] = updatedData;
                }
            }

            LastSeed = seed;
            Random.state = previousState;

            return new MapData(LastSeed, bodyData, planetGrid);
        }
    }
}
