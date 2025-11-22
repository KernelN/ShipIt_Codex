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
        Vector3 RayOrigin => cPlanet ? cPlanet.position : transform.position;
        public Transform CurrentPlanet => cPlanet;
        public Transform DetectedPlanet { get; private set; }
        
        [Header("Launch")]
        [SerializeField, Min(0)] float launchSpeed = 50f;
        [SerializeField, Min(0)] float failTravelDistance = 5f;
        [SerializeField, Min(0)] float failTravelSpeed = 15f;
        [SerializeField] FuelBank fuelBank;
        float sqrJumpSpeed;
        bool isJumping;
        bool isFailLaunching;
        float jumpElapsed;
        float jumpDuration;
        Vector3 jumpStartPosition;
        Vector3 jumpTargetPosition;
        Vector3 landTargetPos;
        Vector3 jumpInitialUp;
        Vector3 jumpFinalUp;
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

        [SerializeField, Range(0.05f, 1f)] float finalApproachDurationFactor = 0.35f;
        
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
            JumpPer = jumpElapsed / jumpDuration;
            transform.position = Vector3.Lerp(jumpStartPosition, jumpTargetPosition, JumpPer);

            transform.up = Vector3.Slerp(jumpInitialUp, jumpFinalUp, JumpPer);

            if(JumpPer >= 1f)
            {
                if (jumpPhase == JumpPhase.ToPlanet)
                {
                    BeginFinalApproach();
                    return;
                }

                Land();
            }
        }
        void SlowUpdate()
        {
            if(isJumping) return;
            Ray ray = new Ray(RayOrigin, transform.up);
            // throw ray in ship direction

            bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, checkDistance, planetMask,
                QueryTriggerInteraction.Ignore);

            HasPlanetAbove = hitSomething;

            // Update visual
            if (hitSomething)
            {
                DetectedPlanet = hit.transform;
                detectedTargetPoint = hit.point;
                UpdateTargetOutline(DetectedPlanet);
                UpdateLine(ray.origin, hit.point, Color.white);
            }
            else
            {
                DetectedPlanet = null;
                detectedTargetPoint = Vector3.zero;
                UpdateTargetOutline(null);
                UpdateLine(ray.origin, ray.origin + ray.direction * checkDistance, Color.red);
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

            Vector3 displacementToTarget = detectedTargetPoint - jumpStartPosition;

            if(displacementToTarget.sqrMagnitude <= Mathf.Epsilon)
            {
                JumpPer = 1f;
                return;
            }

            Vector3 pathDirection = displacementToTarget.normalized;
            Vector3 upReference = Vector3.up;
            if (Camera.main) upReference = Camera.main.transform.up;

            Vector3 rightDirection = Vector3.Cross(upReference, pathDirection);

            if(rightDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                upReference = Vector3.up;
                rightDirection = Vector3.Cross(upReference, pathDirection);
            }

            float planetRadius = DetectedPlanet.lossyScale.x;

            Vector3 oppositeDirection = pathDirection;
            if (cPlanet)
            {
                Vector3 toCurrentPlanet = cPlanet.position - DetectedPlanet.position;
                oppositeDirection = -toCurrentPlanet.normalized;
            }

            landTargetPos = DetectedPlanet.position + oppositeDirection * planetRadius;
            Vector3 offsetTarget = DetectedPlanet.position + rightDirection * planetRadius;

            bool startedPhase = StartLaunchPhase(offsetTarget, JumpPhase.ToPlanet);

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

                if (phase == JumpPhase.ToPlanet)
                {
                    BeginFinalApproach();
                }
                else
                {
                    Land();
                }

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
        void BeginFinalApproach()
        {
            isJumping = false;
            StartLaunchPhase(landTargetPos, JumpPhase.Land, finalApproachDurationFactor);
        }
        void Land()
        {
            //Get planet surface
            Vector3 up = (jumpTargetPosition - DetectedPlanet.position).normalized;
            up = Vector3.ProjectOnPlane(up, DetectedPlanet.up);
            Vector3 pos = DetectedPlanet.position + up * (DetectedPlanet.lossyScale.x / 2);
            
            //Set transform
            transform.position = pos;
            transform.up = up;

            //Update planet
            cPlanet = DetectedPlanet;
            transform.parent = cPlanet;
            NotifyPlanetEntered(cPlanet);
            
            //Reset jump values
            isJumping = false;
            jumpPhase = JumpPhase.None;
            landTargetPos = Vector3.zero;
            jumpFinalUp = Vector3.zero;
            jumpInitialUp = Vector3.zero;
            jumpElapsed = jumpDuration;
            JumpPer = 1f;
            OnJump?.Invoke(JumpPhase.None);
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
