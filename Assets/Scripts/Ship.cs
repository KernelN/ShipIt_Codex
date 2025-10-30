using UnityEngine;
using ShipIt.TickManaging;

namespace ShipIt.Gameplay
{
    public class Ship : MonoBehaviour
    {
        // ——— Config ———
        [Header("Planet Check")] [SerializeField]
        float checkDistance = 200f;

        [SerializeField] LayerMask planetMask;
        [SerializeField] LineRenderer planetLine;
        Transform cPlanet;

        // ——— Runtime ———
        public bool HasPlanetAbove { get; private set; }
        public float JumpPer;

        const int UpdateTime = 2;
        Vector3 RayOrigin => cPlanet ? cPlanet.position : transform.position;

        void Awake()
        {
            if (!planetLine)
            {
                planetLine = gameObject.AddComponent<LineRenderer>();

                // pre set
                planetLine.positionCount = 2;
                planetLine.useWorldSpace = true;
                planetLine.widthMultiplier = 0.05f;
                if (planetLine.material == null)
                    planetLine.material = new Material(Shader.Find("Sprites/Default")); // simple unlit shader}
            }
        }

        void Start()
        {
            UpdateManager.inst.SuscribeToLateScaled(UpdateTime, _Update);
        }

        void OnDestroy()
        {
            // Always unsubscribe when disabled/destroyed
            if (UpdateManager.inst != null)
                UpdateManager.inst.RemoveFromLateScaled(UpdateTime, _Update);
        }

        void _Update()
        {
            Ray ray = new Ray(RayOrigin, transform.up);
            // throw ray in ship direction

            bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, checkDistance, planetMask,
                QueryTriggerInteraction.Ignore);

            HasPlanetAbove = hitSomething;
            planetLine.SetPosition(0, ray.origin);

            // Update visual
            if (hitSomething)
            {
                planetLine.SetPosition(1, hit.point);
                SetLineColor(Color.white);
            }
            else
            {
                planetLine.SetPosition(1, ray.origin + ray.direction * checkDistance);
                SetLineColor(Color.red);
            }
        }

        void SetLineColor(Color c)
        {
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                new[] { new GradientAlphaKey(c.a, 0f), new GradientAlphaKey(c.a, 1f) }
            );
            planetLine.colorGradient = grad;
        }
    }
}