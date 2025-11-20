using UnityEngine;
using Universal;

namespace ShipIt.Gameplay.Astral
{
    public class AstralManager : Singleton<AstralManager>
    {
        [SerializeField] Transform mapRoot;
        [SerializeField] MapFactory mapFactory;

        public int MapSeed { get; private set; }

        internal override bool DoNotDestroyOnLoad => false;

        internal override void Awake()
        {
            base.Awake();

            if (inst != this)
            {
                return;
            }

            if (mapFactory == null || mapRoot == null)
            {
                return;
            }

            MapSeed = mapFactory.SpawnMap(mapRoot);
        }
    }
}
