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
        [SerializeField] AnimationCurve turnTowardsTargetCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] ParticleSystem landingVfx;
        [SerializeField] FuelBank fuelBank;
        float sqrJumpSpeed;
        bool isJumping;
        bool isFailLaunching;
        float jumpElapsed;
        float jumpDuration;
        Vector3 jumpStartPosition;
        Vector3 jumpTargetPosition;
        Vector3 jumpStartForward;
        Vector3 jumpInitialUp;
        Vector3 jumpFinalUp;
        Transform jumpTargetPlanet;
        public System.Action<JumpPhase> OnJump;
        public float JumpPer { get; private set; }
        public bool IsFailLaunching => isFailLaunching;
        public Transform CurrentJumpTargetPlanet => jumpTargetPlanet;

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
            // Always unsubscribe when disabled/destroyed
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

            Vector3 toFinalTarget = jumpTargetPosition - jumpStartPosition;
            float totalDistance = toFinalTarget.magnitude;
            float traveledDistance = totalDistance * JumpPer;
            float turnPer = Mathf.Clamp01(turnTowardsTargetCurve.Evaluate(JumpPer));
            Vector3 targetDirection = totalDistance > Mathf.Epsilon ? toFinalTarget / totalDistance : jumpStartForward;
            Vector3 flightDirection = Vector3.Slerp(jumpStartForward, targetDirection, turnPer).normalized;

            transform.position = jumpStartPosition + flightDirection * traveledDistance;

            transform.up = Vector3.Slerp(jumpInitialUp, jumpFinalUp, JumpPer);

            if(JumpPer >= 1f)
            {
                if (jumpPhase != JumpPhase.Land)
                {
                    jumpPhase = JumpPhase.Land;
                    OnJump?.Invoke(JumpPhase.Land);
                }

                Land();
            }
        }
        void SlowUpdate()
        {
            if(isJumping) return;

            Transform nextPlanet = GetNextPlanetOnPath();
            DetectedPlanet = nextPlanet;
            HasPlanetAbove = DetectedPlanet;

            Vector3 rayEnd = RayOrigin + transform.up * checkDistance;

            if (DetectedPlanet)
            {
                detectedTargetPoint = GetPlanetSurfacePoint(DetectedPlanet, cPlanet ? cPlanet.position : transform.position);
                UpdateTargetOutline(DetectedPlanet);
                UpdateLine(RayOrigin, detectedTargetPoint, Color.white);
            }
            else
            {
                detectedTargetPoint = Vector3.zero;
                UpdateTargetOutline(null);
                UpdateLine(RayOrigin, rayEnd, Color.red);
            }
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

            if (!HasPlanetAbove || DetectedPlanet == null)
            {
                StartFailLaunch();
                return;
            }

            isFailLaunching = false;
            NotifyPlanetExit(cPlanet);
            transform.parent = null;

            jumpStartPosition = transform.position;
            jumpStartForward = transform.up.normalized;
            jumpTargetPlanet = DetectedPlanet;

            Vector3 displacementToTarget = detectedTargetPoint - jumpStartPosition;

            if(displacementToTarget.sqrMagnitude <= Mathf.Epsilon)
            {
                JumpPer = 1f;
                return;
            }

            bool startedPhase = StartLaunchPhase(detectedTargetPoint, JumpPhase.ToPlanet);

            if (!startedPhase && !isJumping)
                return;
        }
        void CacheSqrJumpSpeed() => sqrJumpSpeed = launchSpeed * launchSpeed;
        bool StartLaunchPhase(Vector3 targetPosition, JumpPhase phase, float durationMultiplier = 1f)
        {
            jumpPhase = phase;
            jumpStartPosition = transform.position;
            jumpTargetPosition = targetPosition;

            Vector3 displacement = jumpTargetPosition - jumpStartPosition;
            float sqrDistance = displacement.sqrMagnitude;

            jumpInitialUp = transform.up;
            jumpFinalUp = displacement.normalized;
            
            //Check if ship is already at target
            if (sqrDistance <= Mathf.Epsilon)
            {
                OnJump?.Invoke(phase);

                Land();

                return false;
            }

            float duration = sqrDistance / sqrJumpSpeed;
            float multiplier = durationMultiplier;

            jumpDuration = duration * multiplier;
            jumpElapsed = 0f;
            JumpPer = 0f;
            isJumping = true;

            OnJump?.Invoke(phase);

            return true;
        }
        void Land()
        {
            if (!jumpTargetPlanet)
            {
                isJumping = false;
                jumpPhase = JumpPhase.None;
                OnJump?.Invoke(JumpPhase.None);
                return;
            }

            //Get planet surface
            Vector3 planetNormal = (jumpTargetPosition - jumpTargetPlanet.position).normalized;
            Vector3 pos = GetPlanetSurfacePoint(jumpTargetPlanet, jumpStartPosition);
            
            //Set transform
            transform.position = pos;
            transform.up = -planetNormal;

            PlayLandingVfx(pos, planetNormal);

            //Update planet
            cPlanet = jumpTargetPlanet;
            transform.parent = cPlanet;
            NotifyPlanetEntered(cPlanet);
            
            //Reset jump values
            isJumping = false;
            jumpPhase = JumpPhase.None;
            jumpTargetPlanet = null;
            jumpFinalUp = Vector3.zero;
            jumpInitialUp = Vector3.zero;
            jumpElapsed = jumpDuration;
            JumpPer = 1f;
            OnJump?.Invoke(JumpPhase.None);
        }
        void PlayLandingVfx(Vector3 position, Vector3 normal)
        {
            if (!landingVfx)
                return;

            Quaternion vfxRotation = Quaternion.LookRotation(transform.forward, normal);
            ParticleSystem spawned = Instantiate(landingVfx, position, vfxRotation);
            spawned.Play();
            Destroy(spawned.gameObject, spawned.main.duration + spawned.main.startLifetime.constantMax + 0.1f);
        }
        Transform GetNextPlanetOnPath()
        {
            if (!cPlanetBody)
                return null;

            PathManager pathManager = PathManager.inst;
            AstralBody nextBody = pathManager ? pathManager.GetNextOnPath(cPlanetBody) : null;
            return nextBody ? nextBody.transform : null;
        }
        Vector3 GetPlanetSurfacePoint(Transform planet, Vector3 fromPosition)
        {
            if (!planet)
                return fromPosition;

            Vector3 normal = (fromPosition - planet.position).normalized;
            if (normal.sqrMagnitude <= Mathf.Epsilon)
                normal = planet.up;

            float radius = planet.lossyScale.x * 0.5f;
            return planet.position + normal * radius;
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
            jumpInitialUp = transform.up;

            if (jumpDuration <= 0f)
            {
                FinishFailLaunch();
            }
        }
        void UpdateFailLaunch()
        {
            if (!isFailLaunching)
                return;

            jumpElapsed += Time.deltaTime;
            float failPer = jumpDuration > 0f ? Mathf.Clamp01(jumpElapsed / jumpDuration) : 1f;

            transform.position = Vector3.Lerp(jumpStartPosition, jumpTargetPosition, failPer);

            if (jumpElapsed >= jumpDuration)
            {
                FinishFailLaunch();
            }
        }
        void FinishFailLaunch()
        {
            isFailLaunching = false;
            transform.position = jumpStartPosition;
            transform.up = jumpInitialUp;
            jumpElapsed = 0f;
        }
        #endregion
    }
}
