using System;
using System.Collections.Generic;

namespace ShipIt
{
    [Serializable]
    public class PurchasedItemData
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
        public List<PurchasedItemData> purchases = new List<PurchasedItemData>();
    }
}
