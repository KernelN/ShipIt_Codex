using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    public class AstralManager : MonoBehaviour
    {
        [SerializeField] Transform mapRoot;
        [SerializeField] MapFactory mapFactory;

        public int MapSeed { get; private set; }

        void Awake()
        {
            if (mapFactory == null || mapRoot == null)
            {
                return;
            }

            MapSeed = mapFactory.SpawnMap(mapRoot);
        }
    }
}
