using System.Collections.Generic;
using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    public class AstralBody : MonoBehaviour
    {
        readonly List<AstralComponent> components = new List<AstralComponent>();

        /// <summary>
        /// float 1 = dt | float 2 = udt
        /// </summary>
        public System.Action<float, float> OnUpdate;

        void Update() => OnUpdate?.Invoke(Time.deltaTime, Time.unscaledDeltaTime);

        public void AddAstralComponent(AstralComponent component)
        {
            component.Set(this);
            components.Add(component);
        }
    }
}
