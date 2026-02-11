using System.Collections.Generic;
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

        Vector3 GetWorldPositionForCell(Vector3 anchorPosition, Vector3 cellSize, int centerX, int centerY, Vector3Int cell)
        {
            Vector3 gridPos = new Vector3(
                (cell.x - centerX) * cellSize.x,
                (cell.y - centerY) * cellSize.y,
                cell.z * cellSize.z);

            return anchorPosition + gridPos;
        }

        bool TryMoveCellWithinMaxDistance(
            Vector3 previousPlanetPosition,
            Vector3 anchorPosition,
            Vector3 cellSize,
            int centerX,
            int centerY,
            Vector3Int startCell,
            out Vector3Int adjustedCell)
        {
            adjustedCell = startCell;
            Vector3 adjustedCellWorldPos = GetWorldPositionForCell(anchorPosition, cellSize, centerX, centerY, adjustedCell);
            float currentDistance = Vector3.Distance(previousPlanetPosition, adjustedCellWorldPos);
            if (currentDistance <= maxDistanceBetweenPlanets)
                return true;

            int maxMoves = Mathf.Max(gridSize.x, 1) * Mathf.Max(gridSize.y, 1) * Mathf.Max(gridSize.z, 1);
            for (int move = 0; move < maxMoves; move++)
            {
                Vector3Int bestCell = adjustedCell;
                float bestDistance = currentDistance;

                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
                        {
                            if (offsetX == 0 && offsetY == 0 && offsetZ == 0)
                                continue;

                            Vector3Int neighbourCell = new Vector3Int(
                                Mathf.Clamp(adjustedCell.x + offsetX, 0, gridSize.x - 1),
                                Mathf.Clamp(adjustedCell.y + offsetY, 0, gridSize.y - 1),
                                Mathf.Clamp(adjustedCell.z + offsetZ, 0, gridSize.z - 1));

                            if (neighbourCell == adjustedCell)
                                continue;

                            Vector3 neighbourWorldPos = GetWorldPositionForCell(anchorPosition, cellSize, centerX, centerY, neighbourCell);
                            float neighbourDistance = Vector3.Distance(previousPlanetPosition, neighbourWorldPos);
                            if (neighbourDistance < bestDistance)
                            {
                                bestCell = neighbourCell;
                                bestDistance = neighbourDistance;
                            }
                        }
                    }
                }

                if (bestCell == adjustedCell)
                    return false;

                adjustedCell = bestCell;
                currentDistance = bestDistance;
                if (currentDistance <= maxDistanceBetweenPlanets)
                    return true;
            }

            return false;
        }

        public override MapData SpawnMap(Transform anchor, int seed)
        {
            if (!anchor || !planetFactory || planetQuantity <= 0)
            {
                originPlanet = null;
                LastSeed = 0;
                return new MapData(LastSeed, new List<AstralBodyData>(), new Transform[0, 0, 0]);
            }

            var previousState = Random.state;
            Random.InitState(seed);

            if (gridSize.x <= 0 || gridSize.y <= 0 || gridSize.z <= 0)
            {
                originPlanet = null;
                LastSeed = 0;
                Random.state = previousState;
                return new MapData(LastSeed, new List<AstralBodyData>(), new Transform[0, 0, 0]);
            }

            Vector3 anchorPosition = anchor.position;
            int totalCells = gridSize.x * gridSize.y * gridSize.z;
            int totalPlanets = Mathf.Min(planetQuantity, totalCells);
            Vector3 cellSize = planetFactory.MaxScale;
            planetGrid = new Transform[gridSize.x, gridSize.y, gridSize.z];
            var bodyData = new List<AstralBodyData>();
            var baseComponents = new List<AstralComponentType>();
            if (componentBuilders != null && componentBuilders.Length > 0 && componentBuilders[0] != null)
            {
                baseComponents.Add(componentBuilders[0].GetType);
            }
            int lastBodyIndex = -1;

            int centerX = Mathf.Clamp(gridSize.x / 2, 0, gridSize.x - 1);
            int centerY = Mathf.Clamp(gridSize.y / 2, 0, gridSize.y - 1);

            AstralBody firstPlanet = planetFactory.SpawnBody(anchorPosition, anchor.rotation);
            if (!firstPlanet)
            {
                originPlanet = null;
                LastSeed = 0;
                Random.state = previousState;
                return new MapData(LastSeed, bodyData, planetGrid);
            }

            firstPlanet.AddAstralComponent(componentBuilders[0].GetComponent());
            firstPlanet.gameObject.name = $"Planet ({centerX}, {centerY}, 0)";
            Quaternion referenceRotation = firstPlanet.transform.rotation;
            Transform prevPlanet = firstPlanet.transform;
            AstralBody lastPlanet = firstPlanet;
            planetGrid[centerX, centerY, 0] = firstPlanet.transform;
            originPlanet = firstPlanet.transform;
            bodyData.Add(new AstralBodyData
            {
                gridPos = Vector3Int.zero,
                up = firstPlanet.transform.up,
                componentTypes = baseComponents.ToArray()
            });
            lastBodyIndex = bodyData.Count - 1;

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
                    Vector3Int clampedCell = new Vector3Int(
                        Mathf.Clamp(Mathf.RoundToInt(relativePos.x / cellSize.x) + centerX, 0, gridSize.x - 1),
                        Mathf.Clamp(Mathf.RoundToInt(relativePos.y / cellSize.y) + centerY, 0, gridSize.y - 1),
                        Mathf.Clamp(Mathf.RoundToInt(relativePos.z / cellSize.z), 0, gridSize.z - 1));

                    if (!TryMoveCellWithinMaxDistance(
                        prevPlanet.position,
                        anchorPosition,
                        cellSize,
                        centerX,
                        centerY,
                        clampedCell,
                        out Vector3Int selectedCell))
                    {
                        continue;
                    }

                    if (planetGrid[selectedCell.x, selectedCell.y, selectedCell.z])
                        continue;

                    Vector3 spawnPos = GetWorldPositionForCell(anchorPosition, cellSize, centerX, centerY, selectedCell);
                    AstralBody planet = planetFactory.SpawnBody(spawnPos, spawnRot);
                    if (!planet)
                        continue;

                    planet.AddAstralComponent(componentBuilders[0].GetComponent());
                    planet.gameObject.name = $"Planet ({selectedCell.x}, {selectedCell.y}, {selectedCell.z})";
                    planetGrid[selectedCell.x, selectedCell.y, selectedCell.z] = planet.transform;
                    bodyData.Add(new AstralBodyData
                    {
                        gridPos = selectedCell,
                        up = planet.transform.up,
                        componentTypes = baseComponents.ToArray()
                    });
                    lastBodyIndex = bodyData.Count - 1;

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
