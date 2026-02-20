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
        [SerializeField, Min(0.01f)] float followSwitchRadius = 5f;

        Vector3 followOffset;
        AstralBody lookPlanet;

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

            bool shouldFollowShip = ShouldFollowShip();
            Transform followTarget = shouldFollowShip ? ship.transform : GetCurrentPlanetTransform();
            if (!followTarget)
                followTarget = ship.transform;

            Vector3 targetPosition = followTarget.position + followOffset;
            float posT = 1f - Mathf.Exp(-followLerpSpeed * Time.deltaTime);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, posT);

            UpdateLookPlanet();
            UpdateLookRotation();
        }

        bool ShouldFollowShip()
        {
            AstralBody currentPlanet = ship.CurrentPlanet;
            if (!currentPlanet)
                return true;

            float radius = Mathf.Max(followSwitchRadius, GetPlanetRadius(currentPlanet));
            float radiusSqr = radius * radius;
            float distanceSqr = (ship.transform.position - currentPlanet.transform.position).sqrMagnitude;
            return distanceSqr > radiusSqr;
        }

        Transform GetCurrentPlanetTransform()
        {
            return ship.CurrentPlanet ? ship.CurrentPlanet.transform : null;
        }

        void UpdateLookPlanet()
        {
            if (ship.TargetPlanet)
            {
                lookPlanet = ship.TargetPlanet;
                return;
            }

            if (!lookPlanet || lookPlanet == ship.CurrentPlanet)
            {
                PathManager pathManager = PathManager.inst;
                if (pathManager && ship.CurrentPlanet)
                    lookPlanet = pathManager.GetNextOnPath(ship.CurrentPlanet);
            }
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
