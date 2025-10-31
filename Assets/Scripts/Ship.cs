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
        bool isJumping;
        float jumpElapsed;
        float jumpDuration;
        Vector3 jumpStartPosition;
        Vector3 jumpTargetPosition;
        Vector3 landTargetPos;
        Vector3 jumpInitialUp;
        Vector3 jumpFinalUp;
        public System.Action<bool> OnIsJumping;
        public float JumpPer { get; private set; }

        enum JumpPhase
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
#if UNITY_EDITOR
        void OnValidate() => CacheSqrJumpSpeed();
#endif
        
        void Launch(InputAction.CallbackContext ctx)
        {
            if(!HasPlanetAbove || detectedPlanet == null || isJumping)
                return;

            if(sqrJumpSpeed <= Mathf.Epsilon)
                return;

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

            float planetRadius = detectedPlanet.lossyScale.x;

            Vector3 oppositeDirection = pathDirection;
            if (cPlanet)
            {
                Vector3 toCurrentPlanet = cPlanet.position - detectedPlanet.position;
                oppositeDirection = -toCurrentPlanet.normalized;
            }

            landTargetPos = detectedPlanet.position + oppositeDirection * planetRadius;
            Vector3 offsetTarget = detectedPlanet.position + rightDirection * planetRadius;

            bool startedPhase = StartLaunchPhase(offsetTarget, JumpPhase.ToPlanet);

            if (!startedPhase && !isJumping)
                return;

            OnIsJumping?.Invoke(true);
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
            Vector3 up = (jumpTargetPosition - detectedPlanet.position).normalized;
            Vector3 pos = detectedPlanet.position + up * (detectedPlanet.lossyScale.x / 2);
            
            //Set transform
            transform.position = pos;
            transform.up = up;

            //Update planet
            cPlanet = detectedPlanet;
            transform.parent = cPlanet;
            
            //Reset jump values
            isJumping = false;
            jumpPhase = JumpPhase.None;
            landTargetPos = Vector3.zero;
            jumpFinalUp = Vector3.zero;
            jumpInitialUp = Vector3.zero;
            jumpElapsed = jumpDuration;
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