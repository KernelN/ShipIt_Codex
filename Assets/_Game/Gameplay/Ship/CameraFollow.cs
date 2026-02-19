using UnityEngine;

namespace ShipIt.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Ship ship;
        [SerializeField] Vector3 followOffset = new(0f, 3f, -5f);
        [SerializeField, Min(0f)] float positionLerpSpeed = 10f;
        [SerializeField, Min(0f)] float rotationLerpSpeed = 10f;
        [SerializeField, Min(0.01f)] float forwardLookDistance = 8f;

        void LateUpdate()
        {
            if (!ship)
                return;

            float deltaTime = Time.deltaTime;
            float positionT = 1f - Mathf.Exp(-positionLerpSpeed * deltaTime);
            float rotationT = 1f - Mathf.Exp(-rotationLerpSpeed * deltaTime);

            Vector3 desiredPosition = ship.transform.TransformPoint(followOffset);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, positionT);

            Vector3 lookTarget = GetLookTarget();
            Vector3 forward = lookTarget - transform.position;

            if (forward.sqrMagnitude <= Mathf.Epsilon)
                forward = ship.transform.forward;

            Quaternion targetRotation = Quaternion.LookRotation(forward.normalized, ship.transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationT);
        }

        Vector3 GetLookTarget()
        {
            bool isLanded = ship.CurrentPlanet != null && ship.CurrentJumpTargetPlanet == null && !ship.IsFailLaunching;

            if (isLanded)
                return ship.CurrentPlanet.position;

            return ship.transform.position + ship.transform.forward * forwardLookDistance;
        }
    }
}
