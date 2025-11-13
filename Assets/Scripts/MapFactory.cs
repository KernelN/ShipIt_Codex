using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    public abstract class MapFactory : ScriptableObject
    {
        [SerializeField] GameObject planetPrefab;
        [SerializeField] int planetQuantity = 1;
        [SerializeField] float minDistanceBetweenPlanets = 1f;
        [SerializeField] float maxDistanceBetweenPlanets = 5f;

        public GameObject PlanetPrefab => planetPrefab;
        public int PlanetQuantity => planetQuantity;
        public float MinDistanceBetweenPlanets => minDistanceBetweenPlanets;
        public float MaxDistanceBetweenPlanets => maxDistanceBetweenPlanets;
        public int LastSeed { get; protected set; }

        public abstract int SpawnMap(Transform anchor);
    }
}
