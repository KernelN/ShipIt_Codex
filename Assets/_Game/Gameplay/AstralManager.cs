using UnityEngine;
using Universal;

namespace ShipIt.Gameplay.Astral
{
    public class AstralManager : Singleton<AstralManager>
    {
        [SerializeField] Transform mapRoot;
        [SerializeField] MapFactory mapFactory;

        public int MapSeed { get; private set; }
        public Transform[,,] MapGrid { get; private set; }
        public Vector3 MapCellSize => mapFactory && mapFactory.planetFactory
            ? mapFactory.planetFactory.MaxScale
            : Vector3.one;
        public AstralBody OriginPlanet => mapFactory ? mapFactory.OriginPlanet : null;
        public AstralBody TargetPlanet { get; private set; }

        internal override bool DoNotDestroyOnLoad => false;

        public bool IsTargetPlanet(AstralBody planet)
        {
            return planet && planet == TargetPlanet;
        }

        internal override void Awake()
        {
            base.Awake();

            if (inst != this)
                return;

            if (!mapFactory || !mapRoot)
                return;

            int seed = -1;
            GameData data = GameManager.inst?.Data;
            if (data != null)
                seed = data.randomSeed;

            if (seed < 0)
                seed = Random.Range(0, int.MaxValue);

            MapData mapData = mapFactory.SpawnMap(mapRoot, seed, out AstralBody targetPlanet);
            MapSeed = mapData?.seed ?? 0;
            MapGrid = mapData?.grid;
            TargetPlanet = targetPlanet;

            if (data != null)
            {
                data.randomSeed = MapSeed;
                data.astralBodies = mapData != null
                    ? mapData.astralBodies.ToArray()
                    : System.Array.Empty<AstralBodyData>();
                GameManager.inst.SaveGameData();
            }
        }
    }
}
