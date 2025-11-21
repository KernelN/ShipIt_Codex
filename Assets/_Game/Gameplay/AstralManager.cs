using UnityEngine;
using Universal;
using ShipIt;

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

            int seed = -1;
            GameData data = GameManager.inst?.Data;
            if (data != null)
            {
                seed = data.randomSeed;
            }

            if (seed < 0)
            {
                seed = Random.Range(0, int.MaxValue);
            }

            MapSeed = mapFactory.SpawnMap(mapRoot, seed);

            if (data != null)
            {
                data.randomSeed = MapSeed;
                GameManager.inst.SaveGameData();
            }
        }
    }
}
