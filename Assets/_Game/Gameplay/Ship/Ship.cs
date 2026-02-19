using UnityEngine;
using UnityEngine.InputSystem;
using ShipIt;
using ShipIt.TickManaging;
using ShipIt.Gameplay.Astral;

namespace ShipIt.Gameplay
{
    public class Ship : MonoBehaviour
    {
        [Header("Planet Check")]
        [SerializeField] float checkDistance = 20f;
        [SerializeField] LayerMask planetMask;
        [SerializeField] LineRenderer planetLine;
        [SerializeField] GameObject targetPlanetOutline;
        Transform cPlanet;
        AstralBody cPlanetBody;
        Vector3 detectedTargetPoint;
        public bool HasPlanetAbove { get; private set; }
        public float MaxJumpDistance => checkDistance;
        Vector3 RayOrigin => cPlanet ? cPlanet.position : transform.position;
        public Transform CurrentPlanet => cPlanet;
        public Transform DetectedPlanet { get; private set; }

        [Header("Launch")]
        [SerializeField, Min(0)] float launchSpeed = 50f;
        [SerializeField, Min(0)] float failTravelDistance = 5f;
        [SerializeField, Min(0)] float failTravelSpeed = 15f;
        [SerializeField] FuelBank fuelBank;
        [SerializeField] AnimationCurve turnCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField, Range(0f, 1f)] float straightTravelPoint = 0.35f;
        [SerializeField] ParticleSystem landVfx;
        float sqrJumpSpeed;
        bool isJumping;
        bool isFailLaunching;
        float jumpElapsed;
        float jumpDuration;
        Vector3 jumpStartPosition;
        Vector3 jumpTargetPosition;
        Vector3 jumpLaunchForward;
        Vector3 jumpTargetForward;
        bool landPhaseTriggered;
        public System.Action<JumpPhase> OnJump;
        public float JumpPer { get; private set; }
        public bool IsFailLaunching => isFailLaunching;

        public enum JumpPhase
        {
            None,
            ToPlanet,
            Land
        }

        JumpPhase jumpPhase = JumpPhase.None;

        const int SlowUpdateTime = 2;

        void Awake()
        {
            CacheSqrJumpSpeed();
            cPlanet = transform.parent;
            if (cPlanet)
                cPlanetBody = cPlanet.GetComponent<AstralBody>();
        }
        void Start()
        {
            UpdateManager.inst.SuscribeToLateScaled(SlowUpdateTime, SlowUpdate);

            InputHolder inputs = InputHolder.inst;

            if(!inputs) return;

            inputs.actions.Player.Launch.performed += Launch;
        }
        void OnDestroy()
        {
            if (UpdateManager.inst != null)
                UpdateManager.inst.RemoveFromLateScaled(SlowUpdateTime, SlowUpdate);

            InputHolder inputs = InputHolder.inst;

            if(!inputs) return;

            inputs.actions.Player.Launch.performed -= Launch;
        }
        void Update()
        {
            if (isFailLaunching)
            {
                UpdateFailLaunch();
                return;
            }

            if(!isJumping) return;

            jumpElapsed += Time.deltaTime;
            JumpPer = jumpDuration > 0f ? Mathf.Clamp01(jumpElapsed / jumpDuration) : 1f;
            transform.position = Vector3.Lerp(jumpStartPosition, jumpTargetPosition, JumpPer);

            UpdateJumpRotation(JumpPer);

            if(JumpPer >= 1f)
                Land();
        }
        void SlowUpdate()
        {
            if(isJumping) return;

            Transform targetPlanet = GetNextPathPlanet();
            if (!targetPlanet)
            {
                HasPlanetAbove = false;
                DetectedPlanet = null;
                detectedTargetPoint = Vector3.zero;
                UpdateTargetOutline(null);
                Vector3 rayEnd = RayOrigin + transform.up * checkDistance;
                UpdateLine(RayOrigin, rayEnd, Color.red);
                return;
            }

            Vector3 toTarget = targetPlanet.position - RayOrigin;
            float targetDistance = toTarget.magnitude;
            if (targetDistance <= Mathf.Epsilon)
            {
                HasPlanetAbove = false;
                return;
            }

            Vector3 targetDirection = toTarget / targetDistance;
            Ray ray = new Ray(RayOrigin, targetDirection);

            bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, targetDistance + checkDistance, planetMask,
                QueryTriggerInteraction.Ignore);

            if (hitSomething && hit.transform == targetPlanet)
            {
                HasPlanetAbove = true;
                DetectedPlanet = hit.transform;
                detectedTargetPoint = hit.point;
                UpdateTargetOutline(DetectedPlanet);
                UpdateLine(ray.origin, hit.point, Color.white);
                return;
            }

            HasPlanetAbove = true;
            DetectedPlanet = targetPlanet;
            Vector3 planetDirection = (targetPlanet.position - RayOrigin).normalized;
            float planetRadius = Mathf.Max(targetPlanet.lossyScale.x, targetPlanet.lossyScale.y, targetPlanet.lossyScale.z) * 0.5f;
            detectedTargetPoint = targetPlanet.position - planetDirection * planetRadius;
            UpdateTargetOutline(DetectedPlanet);
            UpdateLine(ray.origin, detectedTargetPoint, Color.white);
        }
#if UNITY_EDITOR
        void OnValidate() => CacheSqrJumpSpeed();
#endif

        void Launch(InputAction.CallbackContext ctx)
        {
            if(isJumping || isFailLaunching)
                return;

            if(sqrJumpSpeed <= Mathf.Epsilon)
                return;

            if (fuelBank && !fuelBank.TryConsumeForLaunch())
                return;

            Transform targetPlanet = GetNextPathPlanet();
            if (!targetPlanet)
            {
                StartFailLaunch();
                return;
            }

            DetectedPlanet = targetPlanet;
            HasPlanetAbove = true;

            isFailLaunching = false;
            NotifyPlanetExit(cPlanet);
            transform.parent = null;

            jumpStartPosition = transform.position;
            jumpTargetPosition = GetLandingPosition(targetPlanet);

            Vector3 displacement = jumpTargetPosition - jumpStartPosition;
            if (displacement.sqrMagnitude <= Mathf.Epsilon)
            {
                Land();
                return;
            }

            jumpLaunchForward = transform.up.normalized;
            jumpTargetForward = displacement.normalized;
            landPhaseTriggered = false;

            float sqrDistance = displacement.sqrMagnitude;
            jumpDuration = sqrDistance / sqrJumpSpeed;
            jumpElapsed = 0f;
            JumpPer = 0f;
            isJumping = true;
            jumpPhase = JumpPhase.ToPlanet;
            OnJump?.Invoke(JumpPhase.ToPlanet);
        }

        Vector3 GetLandingPosition(Transform planet)
        {
            Vector3 normal = (planet.position - transform.position).normalized;
            float planetRadius = Mathf.Max(planet.lossyScale.x, planet.lossyScale.y, planet.lossyScale.z) * 0.5f;
            return planet.position - normal * planetRadius;
        }

        void UpdateJumpRotation(float jumpPer)
        {
            float turnStart = Mathf.Clamp01(straightTravelPoint);
            if (jumpPer <= turnStart)
            {
                transform.up = jumpLaunchForward;
                return;
            }

            if (!landPhaseTriggered)
            {
                landPhaseTriggered = true;
                jumpPhase = JumpPhase.Land;
                OnJump?.Invoke(JumpPhase.Land);
            }

            float turnPer = Mathf.InverseLerp(turnStart, 1f, jumpPer);
            float curvedTurnPer = turnCurve != null ? turnCurve.Evaluate(turnPer) : turnPer;
            Vector3 desiredForward = Vector3.Slerp(jumpLaunchForward, jumpTargetForward, curvedTurnPer);
            if (desiredForward.sqrMagnitude > Mathf.Epsilon)
                transform.up = desiredForward.normalized;
        }

        Transform GetNextPathPlanet()
        {
            PathManager pathManager = PathManager.inst;
            if (!pathManager)
                return null;

            return pathManager.GetNextPlanet(cPlanet);
        }

        void CacheSqrJumpSpeed() => sqrJumpSpeed = launchSpeed * launchSpeed;

        void Land()
        {
            if (!DetectedPlanet)
            {
                isJumping = false;
                jumpPhase = JumpPhase.None;
                OnJump?.Invoke(JumpPhase.None);
                return;
            }

            Vector3 planetNormal = (transform.position - DetectedPlanet.position).normalized;
            float planetRadius = Mathf.Max(DetectedPlanet.lossyScale.x, DetectedPlanet.lossyScale.y, DetectedPlanet.lossyScale.z) * 0.5f;
            Vector3 pos = DetectedPlanet.position + planetNormal * planetRadius;

            transform.position = pos;
            transform.up = -planetNormal;

            PlayLandVfx(pos, planetNormal);

            cPlanet = DetectedPlanet;
            transform.parent = cPlanet;
            NotifyPlanetEntered(cPlanet);

            isJumping = false;
            jumpPhase = JumpPhase.None;
            jumpElapsed = jumpDuration;
            JumpPer = 1f;
            landPhaseTriggered = false;
            OnJump?.Invoke(JumpPhase.None);
        }

        void PlayLandVfx(Vector3 position, Vector3 normal)
        {
            if (!landVfx)
                return;

            Transform vfxTransform = landVfx.transform;
            vfxTransform.position = position;
            Vector3 tangentForward = Vector3.ProjectOnPlane(transform.forward, normal);
            if (tangentForward.sqrMagnitude <= Mathf.Epsilon)
                tangentForward = Vector3.Cross(normal, transform.right);
            vfxTransform.rotation = Quaternion.LookRotation(tangentForward.normalized, normal);
            landVfx.Play();
        }

        void SetLineColor(Color c)
        {
            if (!planetLine)
                return;

            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                new[] { new GradientAlphaKey(c.a, 0f), new GradientAlphaKey(c.a, 1f) }
            );
            planetLine.colorGradient = grad;
        }
        void UpdateLine(Vector3 start, Vector3 end, Color color)
        {
            if (!planetLine)
                return;

            planetLine.positionCount = 2;
            planetLine.useWorldSpace = true;
            planetLine.SetPosition(0, start);
            planetLine.SetPosition(1, end);
            SetLineColor(color);
        }
        void NotifyPlanetEntered(Transform planet)
        {
            if (!planet)
            {
                cPlanetBody = null;
                return;
            }

            cPlanetBody = planet.GetComponent<AstralBody>();
            cPlanetBody?.OnShipEntered(this);
        }
        void NotifyPlanetExit(Transform planet)
        {
            if (!planet)
                return;

            cPlanetBody?.OnShipExit(this);
            cPlanetBody = null;
        }
        void UpdateTargetOutline(Transform targetPlanet)
        {
            if (!targetPlanetOutline)
                return;

            if (targetPlanet)
            {
                Transform outlineTransform = targetPlanetOutline.transform;
                outlineTransform.SetParent(targetPlanet);
                outlineTransform.localScale = Vector3.one;
                outlineTransform.localPosition = Vector3.zero;
                outlineTransform.localRotation = Quaternion.identity;
                targetPlanetOutline.SetActive(true);
            }
            else
            {
                targetPlanetOutline.SetActive(false);
            }
        }
        #region Fail Launch
        void StartFailLaunch()
        {
            if (failTravelDistance <= 0f)
                return;

            CandyCoded.HapticFeedback.HapticFeedback.LightFeedback();

            isFailLaunching = true;
            jumpElapsed = 0f;
            jumpDuration = failTravelSpeed > 0f ? failTravelDistance / failTravelSpeed : 0f;

            jumpStartPosition = transform.position;
            jumpTargetPosition = jumpStartPosition + transform.up * failTravelDistance;

            if (jumpDuration <= 0f)
                FinishFailLaunch();
        }
        void UpdateFailLaunch()
        {
            if (!isFailLaunching)
                return;

            jumpElapsed += Time.deltaTime;
            float failPer = jumpDuration > 0f ? Mathf.Clamp01(jumpElapsed / jumpDuration) : 1f;

            transform.position = Vector3.Lerp(jumpStartPosition, jumpTargetPosition, failPer);

            if (jumpElapsed >= jumpDuration)
                FinishFailLaunch();
        }
        void FinishFailLaunch()
        {
            isFailLaunching = false;
            transform.position = jumpStartPosition;
            jumpElapsed = 0f;
        }
        #endregion
    }
}
