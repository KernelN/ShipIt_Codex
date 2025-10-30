using UnityEngine;
using UnityEngine.InputSystem;
using ShipIt.TickManaging;

namespace ShipIt.Gameplay
{
    public class Ship : MonoBehaviour
    {
        [Header("Planet Check")]
        [SerializeField] float checkDistance = 200f;
        [SerializeField] LayerMask planetMask;
        [SerializeField] LineRenderer planetLine;
        Transform detectedPlanet;
        Transform cPlanet;
        Vector3 detectedTargetPoint;
        public bool HasPlanetAbove { get; private set; }
        Vector3 RayOrigin => cPlanet ? cPlanet.position : transform.position;
        public Transform CurrentPlanet => cPlanet;
        
        [Header("Launch")] 
        [SerializeField, Min(0)] float launchSpeed = 50f;
        float sqrJumpSpeed;
        bool isLaunching;
        float launchElapsed;
        float launchDuration;
        Vector3 launchStartPosition;
        Vector3 launchTargetPosition;
        public System.Action<bool> OnIsJumping;
        public float JumpPer { get; private set; }
        
        const int UpdateTime = 2;

        void Awake()
        {
            CacheSqrJumpSpeed();

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

            InputHolder inputs = InputHolder.inst;

            if(!inputs) return;

            inputs.actions.Player.Launch.performed += Launch;
        }
        void OnDestroy()
        {
            // Always unsubscribe when disabled/destroyed
            if (UpdateManager.inst != null)
                UpdateManager.inst.RemoveFromLateScaled(UpdateTime, _Update);

            InputHolder inputs = InputHolder.inst;

            if(!inputs) return;

            inputs.actions.Player.Launch.performed -= Launch;
        }
        void Update()
        {
            if(!isLaunching) return;

            launchElapsed += Time.deltaTime;
            JumpPer = launchDuration <= 0f ? 1f : Mathf.Clamp01(launchElapsed / launchDuration);
            transform.position = Vector3.Lerp(launchStartPosition, launchTargetPosition, JumpPer);

            if(JumpPer >= 1f)
            {
                transform.up = (launchStartPosition - launchTargetPosition).normalized;
                cPlanet = detectedPlanet;
                isLaunching = false;
                OnIsJumping?.Invoke(false);
            }
        }
#if UNITY_EDITOR
        void OnValidate() => CacheSqrJumpSpeed();
#endif

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
                detectedPlanet = hit.transform;
                detectedTargetPoint = hit.point;
            }
            else
            {
                planetLine.SetPosition(1, ray.origin + ray.direction * checkDistance);
                SetLineColor(Color.red);
                detectedPlanet = null;
                detectedTargetPoint = Vector3.zero;
            }
        }
        void Launch(InputAction.CallbackContext ctx)
        {
            if(!HasPlanetAbove || detectedPlanet == null || isLaunching)
                return;

            if(sqrJumpSpeed <= Mathf.Epsilon)
                return;

            launchStartPosition = transform.position;
            launchTargetPosition = detectedTargetPoint;

            Vector3 displacement = launchTargetPosition - launchStartPosition;
            float sqrDistance = displacement.sqrMagnitude;

            if(sqrDistance <= Mathf.Epsilon)
            {
                JumpPer = 1f;
                return;
            }

            launchDuration = sqrDistance / sqrJumpSpeed;
            launchElapsed = 0f;
            JumpPer = 0f;
            isLaunching = true;
            OnIsJumping?.Invoke(true);
        }
        void CacheSqrJumpSpeed() => sqrJumpSpeed = launchSpeed * launchSpeed;

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