using ShipIt.Gameplay.Astral;
using UnityEngine;

namespace ShipIt.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Ship ship;
        [SerializeField] Transform cameraTransform;

        [Header("Follow")]
        [SerializeField, Min(0.01f)] float followLerpSpeed = 4f;
        [SerializeField, Min(0.01f)] float lookLerpSpeed = 6f;
        [SerializeField, Min(0.01f)] float shipFollowRadius = 6f;
        [SerializeField, Min(0.01f)] float planetFollowRadius = 4f;
        [SerializeField, Min(0.01f)] float aimSwitchRadius = 5f;

        Vector3 followOffset;
        AstralBody lookPlanet;
        bool followingShip;

        void Awake()
        {
            if (!ship)
                ship = FindAnyObjectByType<Ship>();

            if (!cameraTransform)
                cameraTransform = transform;

            if (ship)
                followOffset = cameraTransform.position - ship.transform.position;
        }

        void LateUpdate()
        {
            if (!ship || !cameraTransform)
                return;

            UpdateFollowMode();

            Transform followTarget = followingShip ? ship.transform : GetCurrentPlanetTransform();
            if (!followTarget)
                followTarget = ship.transform;

            Vector3 targetPosition = followTarget.position + followOffset;
            float posT = 1f - Mathf.Exp(-followLerpSpeed * Time.deltaTime);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, posT);

            UpdateLookPlanet();
            UpdateLookRotation();
        }

        void UpdateFollowMode()
        {
            AstralBody currentPlanet = ship.CurrentPlanet;
            if (!currentPlanet)
            {
                followingShip = true;
                return;
            }

            float distanceSqr = (ship.transform.position - currentPlanet.transform.position).sqrMagnitude;
            if (!followingShip)
            {
                float engageRadius = Mathf.Max(shipFollowRadius, GetPlanetRadius(currentPlanet));
                followingShip = distanceSqr > engageRadius * engageRadius;
                return;
            }

            float disengageRadius = Mathf.Max(planetFollowRadius, GetPlanetRadius(currentPlanet));
            if (distanceSqr <= disengageRadius * disengageRadius)
                followingShip = false;
        }

        Transform GetCurrentPlanetTransform()
        {
            return ship.CurrentPlanet ? ship.CurrentPlanet.transform : null;
        }

        void UpdateLookPlanet()
        {
            PathManager pathManager = PathManager.inst;
            AstralBody currentPlanet = ship.CurrentPlanet;
            AstralBody targetPlanet = ship.TargetPlanet;

            if (targetPlanet)
            {
                float switchDistanceSqr = aimSwitchRadius * aimSwitchRadius;
                float toTargetSqr = (ship.transform.position - targetPlanet.transform.position).sqrMagnitude;
                if (toTargetSqr <= switchDistanceSqr && pathManager)
                {
                    AstralBody nextAfterTarget = pathManager.GetNextOnPath(targetPlanet);
                    lookPlanet = nextAfterTarget ? nextAfterTarget : targetPlanet;
                }
                else
                {
                    lookPlanet = targetPlanet;
                }

                return;
            }

            if (pathManager && currentPlanet)
                lookPlanet = pathManager.GetNextOnPath(currentPlanet);
        }

        void UpdateLookRotation()
        {
            Vector3 lookDirection;
            if (lookPlanet)
                lookDirection = lookPlanet.transform.position - cameraTransform.position;
            else
                lookDirection = ship.transform.position - cameraTransform.position;

            if (lookDirection.sqrMagnitude <= 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            float rotT = 1f - Mathf.Exp(-lookLerpSpeed * Time.deltaTime);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation, rotT);
        }

        static float GetPlanetRadius(AstralBody planet)
        {
            Vector3 scale = planet.transform.lossyScale;
            float maxDiameter = Mathf.Max(scale.x, scale.y, scale.z);
            return Mathf.Max(0.01f, maxDiameter * 0.5f);
        }
    }
}
