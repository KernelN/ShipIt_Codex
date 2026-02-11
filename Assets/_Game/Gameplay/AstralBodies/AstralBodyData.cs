using System;
using UnityEngine;
using Universal.FileManaging.Types;

namespace ShipIt.Gameplay.Astral
{
    [Serializable]
    public struct AstralBodyData
    {
        public Vec3 gridPos;
        public Vec3 up;
        public AstralComponentType[] componentTypes;
    }
}
