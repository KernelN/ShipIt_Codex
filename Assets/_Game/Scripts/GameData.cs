using System;
using System.Collections.Generic;

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
        public int credits;
        public int fuel = -1;
        public List<ItemData> items = new List<ItemData>();
        public string selectedSkinId;
    }
}
