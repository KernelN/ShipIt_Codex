using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [Serializable]
    public class MapData
    {
        public int seed;
        public List<AstralBodyData> astralBodies = new List<AstralBodyData>();
        public Transform[,,] grid;

        public MapData()
        {
        }

        public MapData(int seed, List<AstralBodyData> astralBodies, Transform[,,] grid)
        {
            this.seed = seed;
            this.astralBodies = astralBodies ?? new List<AstralBodyData>();
            this.grid = grid;
        }
    }
}
