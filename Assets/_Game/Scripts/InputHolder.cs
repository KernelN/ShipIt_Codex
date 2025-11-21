using UnityEngine;
using Universal;

namespace ShipIt.Gameplay
{
    public class InputHolder : Singleton<InputHolder>
    {
        public InputActions actions { get; private set; }

        internal override void Awake()
        {
            base.Awake();
            if (inst != this) return;

            actions = new InputActions();
            actions.Enable();
        }

        internal override void OnDestroy()
        {
            if (inst != this) return;
            base.OnDestroy();

            actions.Disable();
        }
    }
}