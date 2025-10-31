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
        Vector3 finalApproachTargetPosition;
        Vector3 finalApproachInitialUp;
        Vector3 finalApproachDesiredUp;
        public System.Action<bool> OnIsJumping;
        public float JumpPer { get; private set; }

        enum LaunchPhase
        {
            None,
            ToOffset,
            ToOpposite
        }

        LaunchPhase launchPhase = LaunchPhase.None;

        [SerializeField, Range(0.05f, 1f)] float finalApproachDurationFactor = 0.35f;
        
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

            if (launchPhase == LaunchPhase.ToOpposite && finalApproachDesiredUp.sqrMagnitude > Mathf.Epsilon)
            {
                transform.up = Vector3.Slerp(finalApproachInitialUp, finalApproachDesiredUp, JumpPer);
            }

            if(JumpPer >= 1f)
            {
                if (launchPhase == LaunchPhase.ToOffset)
                {
                    BeginFinalApproach();
                    return;
                }

                CompleteLaunch();
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

            Vector3 targetPoint = detectedTargetPoint;
            Vector3 displacementToTarget = targetPoint - launchStartPosition;

            if(displacementToTarget.sqrMagnitude <= Mathf.Epsilon)
            {
                JumpPer = 1f;
                return;
            }

            Vector3 pathDirection = displacementToTarget.normalized;
            Vector3 upReference = Vector3.up;
            Camera mainCam = Camera.main;

            if (mainCam)
                upReference = mainCam.transform.up;

            Vector3 rightDirection = Vector3.Cross(upReference, pathDirection);

            if(rightDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                upReference = transform.up;
                rightDirection = Vector3.Cross(upReference, pathDirection);
            }

            if(rightDirection.sqrMagnitude > Mathf.Epsilon)
                rightDirection.Normalize();
            else
                rightDirection = transform.right;

            float planetRadius = Mathf.Abs(detectedPlanet.lossyScale.x) * 0.5f;
            if(planetRadius <= Mathf.Epsilon)
                planetRadius = Vector3.Distance(targetPoint, detectedPlanet.position);

            Vector3 oppositeDirection = pathDirection;
            if (cPlanet)
            {
                Vector3 toCurrentPlanet = cPlanet.position - detectedPlanet.position;
                if (toCurrentPlanet.sqrMagnitude > Mathf.Epsilon)
                    oppositeDirection = -toCurrentPlanet.normalized;
            }

            if (oppositeDirection.sqrMagnitude <= Mathf.Epsilon)
                oppositeDirection = pathDirection;

            finalApproachTargetPosition = detectedPlanet.position + oppositeDirection * planetRadius;
            Vector3 offsetTarget = detectedPlanet.position + rightDirection * planetRadius;

            bool startedPhase = StartLaunchPhase(offsetTarget, LaunchPhase.ToOffset);

            if (!startedPhase && !isLaunching)
                return;

            OnIsJumping?.Invoke(true);
        }
        void CacheSqrJumpSpeed() => sqrJumpSpeed = launchSpeed * launchSpeed;

        bool StartLaunchPhase(Vector3 targetPosition, LaunchPhase phase, float durationMultiplier = 1f)
        {
            launchPhase = phase;
            launchStartPosition = transform.position;
            launchTargetPosition = targetPosition;

            Vector3 displacement = launchTargetPosition - launchStartPosition;
            float sqrDistance = displacement.sqrMagnitude;

            if (sqrDistance <= Mathf.Epsilon)
            {
                if (phase == LaunchPhase.ToOffset)
                {
                    BeginFinalApproach();
                }
                else
                {
                    CompleteLaunch();
                }

                return false;
            }

            float duration = sqrDistance / sqrJumpSpeed;
            float multiplier = Mathf.Max(durationMultiplier, 0.0001f);
            if (phase == LaunchPhase.ToOpposite)
                multiplier = Mathf.Min(multiplier, 1f);

            launchDuration = Mathf.Max(duration * multiplier, 0.0001f);
            launchElapsed = 0f;
            JumpPer = 0f;
            isLaunching = true;

            if (phase == LaunchPhase.ToOpposite)
            {
                finalApproachInitialUp = transform.up;
                finalApproachDesiredUp = displacement.normalized;
            }

            return true;
        }

        void BeginFinalApproach()
        {
            isLaunching = false;
            StartLaunchPhase(finalApproachTargetPosition, LaunchPhase.ToOpposite, finalApproachDurationFactor);
        }

        void CompleteLaunch()
        {
            transform.position = launchTargetPosition;

            Vector3 finalDirection = finalApproachDesiredUp;

            if (finalDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                Vector3 displacement = launchTargetPosition - launchStartPosition;
                if (displacement.sqrMagnitude > Mathf.Epsilon)
                    finalDirection = displacement.normalized;
            }

            if (finalDirection.sqrMagnitude > Mathf.Epsilon)
                transform.up = finalDirection;

            cPlanet = detectedPlanet;
            isLaunching = false;
            launchPhase = LaunchPhase.None;
            finalApproachTargetPosition = Vector3.zero;
            finalApproachDesiredUp = Vector3.zero;
            finalApproachInitialUp = Vector3.zero;
            launchElapsed = launchDuration;
            JumpPer = 1f;
            OnIsJumping?.Invoke(false);
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