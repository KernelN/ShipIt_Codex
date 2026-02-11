using System;
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
        public Transform[,,] MapGrid { get; private set; }
        public Vector3 MapCellSize => mapFactory && mapFactory.planetFactory
            ? mapFactory.planetFactory.MaxScale
            : Vector3.one;
        public Transform OriginPlanet => mapFactory ? mapFactory.OriginPlanet : null;

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

            MapData mapData = mapFactory.SpawnMap(mapRoot, seed);
            MapSeed = mapData != null ? mapData.seed : 0;
            MapGrid = mapData != null ? mapData.grid : null;

            if (data != null)
            {
                data.randomSeed = MapSeed;
                data.astralBodies = mapData != null
                    ? mapData.astralBodies.ToArray()
                    : Array.Empty<AstralBodyData>();
                GameManager.inst.SaveGameData();
            }
        }
    }
}
