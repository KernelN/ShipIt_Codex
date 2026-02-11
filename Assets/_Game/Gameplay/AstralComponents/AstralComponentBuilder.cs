using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    public abstract class AstralComponentBuilder : ScriptableObject
    {
        public new abstract AstralComponentType GetType { get; }

        public abstract AstralComponent GetComponent();
    }
}
