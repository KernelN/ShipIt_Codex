using System;
using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [Serializable]
    public struct AstralBodyData
    {
        public Vector3Int gridPos;
        public Vector3 up;
        public AstralComponentType[] componentTypes;
    }
}
