using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    public abstract class MapFactory : ScriptableObject
    {
        [SerializeField] internal AstralBodyFactory planetFactory;
        [SerializeField] internal int planetQuantity = 1;
        [SerializeField] internal float minDistanceBetweenPlanets = 1f;
        [SerializeField] internal float maxDistanceBetweenPlanets = 5f;
        [SerializeField] internal AstralComponentBuilder[] componentBuilders;
        public int LastSeed { get; protected set; }

        public abstract int SpawnMap(Transform anchor, int seed);
    }
}
