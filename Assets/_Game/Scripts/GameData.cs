using System;
using System.Collections.Generic;
using ShipIt.Gameplay.Astral;

namespace ShipIt
{
    [Serializable]
    public class ItemData
    {
        public string id;
        public int quantity;
    }

    [Serializable]
    public class GameData
    {
        public int randomSeed = -1;
        public AstralBodyData[] astralBodies = Array.Empty<AstralBodyData>();
        public int credits;
        public int fuel = -1;
        public List<ItemData> items = new List<ItemData>();
        public string selectedSkinId;
    }
}
