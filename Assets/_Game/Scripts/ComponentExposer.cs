using ShipIt.Gameplay.Astral;
using UnityEngine;

namespace ShipIt.Gameplay
{
    public class ComponentExposer : MonoBehaviour
    {
        [SerializeField] AstralBody astralBodybody;
        [SerializeField] Renderer renderer;
        
        public AstralBody AstralBody => astralBodybody;
        public Renderer Renderer => renderer;
    }
}