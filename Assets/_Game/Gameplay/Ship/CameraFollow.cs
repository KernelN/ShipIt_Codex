using ShipIt.Gameplay.Astral;
using UnityEngine;
using UnityEngine.Animations;

namespace ShipIt.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Ship ship;
        [SerializeField] PositionConstraint positionConstraint;
        [SerializeField] AnimationCurve toPlanetCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] AnimationCurve landCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        [SerializeField, Min(0.01f)] float resetWeightsLerpDuration = 0.1f;

        [Header("Follow")]
        [SerializeField, Min(0.01f)] float switchRadius = 6f;

        [Header("Rotation")]
        [SerializeField, Min(0.01f)] float lookLerpSpeed = 4f;

        AstralBody landedPlanet;
        AstralBody departurePlanet;
        AstralBody targetPlanet;
        bool wasLanded;
        bool useTargetPlanetAsAnchor;

        float shipWeight;

        Quaternion landingFromRotation;
        float landingBlendTime;

        float SwitchRadiusSqr => switchRadius * switchRadius;

        void Awake()
        {
            if (!ship)
                ship = FindAnyObjectByType<Ship>();

            if (!positionConstraint)
                positionConstraint = GetComponent<PositionConstraint>();
        }

        void Start()
        {
            EnsureConstraintSources();
            RefreshLandingState(force: true);
        }

        void LateUpdate()
        {
            if (!ship || !positionConstraint)
                return;

            RefreshLandingState(force: false);
            UpdateFollowWeights(Time.deltaTime);
            UpdateRotation(Time.deltaTime);
        }

        void RefreshLandingState(bool force)
        {
            bool isLanded = ship.transform.parent && ship.transform.parent.TryGetComponent(out AstralBody parentBody);

            if (!force && isLanded == wasLanded)
                return;

            if (isLanded)
            {
                landedPlanet = parentBody;
                departurePlanet = landedPlanet;
                targetPlanet = PathManager.inst ? PathManager.inst.GetNextOnPath(landedPlanet) : null;
                useTargetPlanetAsAnchor = false;
                shipWeight = 0f;
                SetPlanetAnchor(landedPlanet ? landedPlanet.transform : null);

                landingFromRotation = transform.rotation;
                landingBlendTime = 0f;
            }
            else
            {
                departurePlanet = landedPlanet;
                targetPlanet = departurePlanet && PathManager.inst ? PathManager.inst.GetNextOnPath(departurePlanet) : targetPlanet;
                useTargetPlanetAsAnchor = false;
                SetPlanetAnchor(departurePlanet ? departurePlanet.transform : null);
            }

            wasLanded = isLanded;
        }

        void UpdateFollowWeights(float dt)
        {
            float targetShipWeight = 0f;

            if (!wasLanded)
            {
                if (departurePlanet && targetPlanet)
                {
                    float fromDepartureSqr = (ship.transform.position - departurePlanet.transform.position).sqrMagnitude;
                    float toTargetSqr = (ship.transform.position - targetPlanet.transform.position).sqrMagnitude;

                    if (!useTargetPlanetAsAnchor && fromDepartureSqr > SwitchRadiusSqr)
                        targetShipWeight = 1f;

                    if (toTargetSqr <= SwitchRadiusSqr)
                    {
                        useTargetPlanetAsAnchor = true;
                        SetPlanetAnchor(targetPlanet.transform);
                        targetShipWeight = 0f;
                    }
                }
                else
                {
                    targetShipWeight = 1f;
                }
            }

            float lerpSpeed = resetWeightsLerpDuration > 0.0001f ? 1f / resetWeightsLerpDuration : 1f;
            shipWeight = Mathf.MoveTowards(shipWeight, targetShipWeight, lerpSpeed * dt);

            float curvedShipWeight = Mathf.Clamp01(toPlanetCurve.Evaluate(shipWeight));
            SetConstraintWeights(1f - curvedShipWeight, curvedShipWeight);
        }

        void UpdateRotation(float dt)
        {
            Transform lookTarget = targetPlanet ? targetPlanet.transform : (landedPlanet ? landedPlanet.transform : null);
            if (!lookTarget)
                return;

            Vector3 forward = lookTarget.position - transform.position;
            if (forward.sqrMagnitude <= 0.0001f)
                return;

            Quaternion desired = Quaternion.LookRotation(forward.normalized, Vector3.up);

            if (wasLanded)
            {
                landingBlendTime += dt;
                float t = resetWeightsLerpDuration > 0.0001f
                    ? Mathf.Clamp01(landingBlendTime / resetWeightsLerpDuration)
                    : 1f;
                float curve = Mathf.Clamp01(landCurve.Evaluate(t));
                transform.rotation = Quaternion.Slerp(landingFromRotation, desired, 1f - curve);
                return;
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, desired, dt * lookLerpSpeed);
        }

        void EnsureConstraintSources()
        {
            ConstraintSource planetSource = new ConstraintSource { sourceTransform = null, weight = 1f };
            ConstraintSource shipSource = new ConstraintSource { sourceTransform = ship ? ship.transform : null, weight = 0f };

            int sourceCount = positionConstraint.sourceCount;
            if (sourceCount == 0)
            {
                positionConstraint.AddSource(planetSource);
                positionConstraint.AddSource(shipSource);
                return;
            }

            positionConstraint.SetSource(0, planetSource);
            if (sourceCount == 1)
                positionConstraint.AddSource(shipSource);
            else
                positionConstraint.SetSource(1, shipSource);
        }

        void SetPlanetAnchor(Transform planet)
        {
            ConstraintSource source = positionConstraint.GetSource(0);
            source.sourceTransform = planet;
            positionConstraint.SetSource(0, source);
        }

        void SetConstraintWeights(float planetWeight, float followShipWeight)
        {
            ConstraintSource planetSource = positionConstraint.GetSource(0);
            planetSource.weight = Mathf.Clamp01(planetWeight);
            positionConstraint.SetSource(0, planetSource);

            if (positionConstraint.sourceCount < 2)
                return;

            ConstraintSource shipSource = positionConstraint.GetSource(1);
            shipSource.sourceTransform = ship ? ship.transform : shipSource.sourceTransform;
            shipSource.weight = Mathf.Clamp01(followShipWeight);
            positionConstraint.SetSource(1, shipSource);
        }
    }
}
