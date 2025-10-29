using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    public class AstralBody : MonoBehaviour
    {
        readonly List<AstralComponent> components = new List<AstralComponent>();

        public void AddAstralComponent(AstralComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            component.Set(this);
            components.Add(component);
        }
    }
}
