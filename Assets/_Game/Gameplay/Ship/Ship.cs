using UnityEngine;
using UnityEngine.InputSystem;
using ShipIt.Gameplay.Astral;

namespace ShipIt.Gameplay
{
    public class Ship : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0.1f)] float travelSpeed = 10f;
        [SerializeField] AnimationCurve forwardToTargetCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Landing")]
        [SerializeField] ParticleSystem landingVfxPrefab;

        [Header("References")]
        [SerializeField] FuelBank fuelBank;

        AstralBody currentPlanet;
        AstralBody targetPlanet;

        Vector3 launchForward;
        Vector3 targetLandingPoint;
        Vector3 targetLandingNormal;
        float travelDistance;
        float totalTravelDistance;
        bool isTraveling;

        void Awake()
        {
            if (!fuelBank)
                fuelBank = FindAnyObjectByType<FuelBank>();
        }

        void OnEnable() => SubscribeInput();

        void OnDisable() => UnsubscribeInput();

        void Start()
        {
            InitializeOnPathStart();
        }

        void Update()
        {
            if (!isTraveling || !targetPlanet)
                return;

            UpdateTravel(Time.deltaTime);
        }

        void SubscribeInput()
        {
            InputHolder inputs = InputHolder.inst;
            if (!inputs)
                return;

            inputs.actions.Player.Launch.performed += HandleLaunch;
        }

        void UnsubscribeInput()
        {
            InputHolder inputs = InputHolder.inst;
            if (!inputs)
                return;

            inputs.actions.Player.Launch.performed -= HandleLaunch;
        }

        void InitializeOnPathStart()
        {
            PathManager pathManager = PathManager.inst;
            if (!pathManager)
                return;

            currentPlanet = pathManager.GetFirstOnPath();
            if (!currentPlanet)
                return;

            LandOnPlanet(currentPlanet, transform.position - currentPlanet.transform.position, false);
        }

        void HandleLaunch(InputAction.CallbackContext context)
        {
            if (isTraveling || !currentPlanet)
                return;

            if (fuelBank && !fuelBank.TryConsumeForLaunch())
                return;

            PathManager pathManager = PathManager.inst;
            if (!pathManager)
                return;

            AstralBody nextPlanet = pathManager.GetNextOnPath(currentPlanet);
            if (!nextPlanet)
                return;

            LaunchTo(nextPlanet);
        }

        void LaunchTo(AstralBody nextPlanet)
        {
            targetPlanet = nextPlanet;
            launchForward = transform.forward.sqrMagnitude > 0.0001f ? transform.forward.normalized : (targetPlanet.transform.position - transform.position).normalized;

            Vector3 toPlanetCenter = targetPlanet.transform.position - transform.position;
            if (toPlanetCenter.sqrMagnitude <= 0.0001f)
                toPlanetCenter = transform.forward;

            float targetRadius = GetPlanetRadius(targetPlanet);
            Vector3 fromCenterToShip = (transform.position - targetPlanet.transform.position).normalized;
            if (fromCenterToShip.sqrMagnitude <= 0.0001f)
                fromCenterToShip = -toPlanetCenter.normalized;

            targetLandingNormal = fromCenterToShip;
            targetLandingPoint = targetPlanet.transform.position + targetLandingNormal * targetRadius;

            totalTravelDistance = Vector3.Distance(transform.position, targetLandingPoint);
            travelDistance = 0f;
            isTraveling = totalTravelDistance > 0.01f;

            if (!isTraveling)
                CompleteLanding();
        }

        void UpdateTravel(float dt)
        {
            Vector3 targetDirection = (targetLandingPoint - transform.position).normalized;
            float travelPercent = totalTravelDistance > 0.0001f ? Mathf.Clamp01(travelDistance / totalTravelDistance) : 1f;
            float blend = Mathf.Clamp01(forwardToTargetCurve.Evaluate(travelPercent));
            Vector3 travelDirection = Vector3.Slerp(launchForward, targetDirection, blend).normalized;

            float step = travelSpeed * dt;
            transform.position += travelDirection * step;
            travelDistance += step;

            if (travelDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(travelDirection, transform.up);
                transform.rotation = lookRotation;
            }

            if (travelDistance >= totalTravelDistance)
            {
                CompleteLanding();
            }
        }

        void CompleteLanding()
        {
            if (!targetPlanet)
                return;

            if (currentPlanet)
                currentPlanet.OnShipExit(this);

            currentPlanet = targetPlanet;
            targetPlanet = null;
            isTraveling = false;

            LandOnPlanet(currentPlanet, targetLandingNormal, true);
            currentPlanet.OnShipEntered(this);
        }

        void LandOnPlanet(AstralBody planet, Vector3 preferredNormal, bool spawnVfx)
        {
            if (!planet)
                return;

            Vector3 normal = preferredNormal.sqrMagnitude > 0.0001f
                ? preferredNormal.normalized
                : (transform.position - planet.transform.position).normalized;

            if (normal.sqrMagnitude <= 0.0001f)
                normal = Vector3.up;

            float radius = GetPlanetRadius(planet);
            Vector3 landingPoint = planet.transform.position + normal * radius;
            transform.position = landingPoint;

            Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, normal);
            if (projectedForward.sqrMagnitude <= 0.0001f)
                projectedForward = Vector3.ProjectOnPlane(transform.right, normal);
            if (projectedForward.sqrMagnitude <= 0.0001f)
                projectedForward = Vector3.Cross(normal, Vector3.forward);

            transform.rotation = Quaternion.LookRotation(projectedForward.normalized, normal);

            if (spawnVfx)
                SpawnLandingVfx(landingPoint, normal);
        }

        void SpawnLandingVfx(Vector3 position, Vector3 normal)
        {
            if (!landingVfxPrefab)
                return;

            Vector3 vfxForward = Vector3.ProjectOnPlane(transform.forward, normal);
            if (vfxForward.sqrMagnitude <= 0.0001f)
                vfxForward = Vector3.Cross(normal, Vector3.right);
            if (vfxForward.sqrMagnitude <= 0.0001f)
                vfxForward = Vector3.Cross(normal, Vector3.forward);

            Quaternion rotation = Quaternion.LookRotation(vfxForward.normalized, normal);
            ParticleSystem vfx = Instantiate(landingVfxPrefab, position, rotation);
            vfx.Play();
            Destroy(vfx.gameObject, vfx.main.duration + vfx.main.startLifetime.constantMax + 0.5f);
        }

        static float GetPlanetRadius(AstralBody planet)
        {
            Vector3 scale = planet.transform.lossyScale;
            float maxDiameter = Mathf.Max(scale.x, scale.y, scale.z);
            return Mathf.Max(0.01f, maxDiameter * 0.5f);
        }
    }
}
