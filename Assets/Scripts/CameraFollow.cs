using UnityEngine;
using UnityEngine.Animations;

namespace ShipIt.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Ship ship;
        [SerializeField] PositionConstraint positionConstraint;
        [SerializeField] AnimationCurve followCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        void Reset()
        {
            if (!ship)
                ship = GetComponentInParent<Ship>();

            if (!positionConstraint)
                positionConstraint = GetComponent<PositionConstraint>();
        }

        void Update()
        {
            if (!ship || !positionConstraint || followCurve == null)
                return;

            float weight = Mathf.Clamp01(followCurve.Evaluate(ship.JumpPer));

            SetConstraintWeight(0, 1f - weight);
            SetConstraintWeight(1, weight);
        }

        void SetConstraintWeight(int index, float weight)
        {
            if (index < 0 || index >= positionConstraint.sourceCount)
                return;

            var source = positionConstraint.GetSource(index);
            source.weight = weight;
            positionConstraint.SetSource(index, source);
        }
    }
}
