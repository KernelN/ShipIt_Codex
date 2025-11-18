using System.Collections.Generic;
using UnityEngine;
using ShipIt.Gameplay;

namespace ShipIt.Gameplay.Astral
{
    public class AstralBody : MonoBehaviour
    {
        readonly List<AstralComponent> components = new List<AstralComponent>();

        /// <summary>
        /// float 1 = dt | float 2 = udt
        /// </summary>
        public System.Action<float, float> OnUpdate;
        public System.Action<Ship> onShipEntered;
        public System.Action<Ship> onShipExit;

        void Update() => OnUpdate?.Invoke(Time.deltaTime, Time.unscaledDeltaTime);

        public void AddAstralComponent(AstralComponent component)
        {
            component.Set(this);
            components.Add(component);
        }

        public void OnShipEntered(Ship ship) => onShipEntered?.Invoke(ship);

        public void OnShipExit(Ship ship) => onShipExit?.Invoke(ship);
    }
}
