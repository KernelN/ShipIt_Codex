using System;
using UnityEngine;
using UnityEngine.Animations;

namespace ShipIt.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Ship ship;
        [SerializeField] PositionConstraint positionConstraint;
        [SerializeField] AnimationCurve toPlanetCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] AnimationCurve landCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] float resetWeightsLerpDuration = .5f;
        bool isShipJumping;
        AnimationCurve activeCurve;

        void Start()
        {
            if (!ship) return;

            ship.OnJump += OnShipJumpPhase;
        }
        void OnDestroy()
        {
            if (!ship) return;

            ship.OnJump -= OnShipJumpPhase;
        }
        void Update()
        {
            if(!isShipJumping) return;
            if (!ship || !positionConstraint || activeCurve == null) return;

            float weight = activeCurve.Evaluate(ship.JumpPer);

            SetConstraintWeight(0, weight);
            SetConstraintWeight(1, 1f - weight);
        }
        void SetConstraintWeight(int index, float weight)
        {
            if (index < 0 || index >= positionConstraint.sourceCount)
                return;

            var source = positionConstraint.GetSource(index);
            source.weight = weight;
            positionConstraint.SetSource(index, source);
        }
        void OnShipJumpPhase(Ship.JumpPhase phase)
        {
            if (ship != null && ship.IsFailLaunching)
            {
                activeCurve = null;
                isShipJumping = false;
                return;
            }

            switch (phase)
            {
                case Ship.JumpPhase.ToPlanet:
                    activeCurve = toPlanetCurve;
                    isShipJumping = true;
                    break;
                case Ship.JumpPhase.Land:
                    activeCurve = landCurve;
                    isShipJumping = true;
                    OverrideConstraintSource(ship.DetectedPlanet);
                    break;
                case Ship.JumpPhase.None:
                    activeCurve = null;
                    isShipJumping = false;
                    OverrideConstraintSource(ship.CurrentPlanet);
                    StartCoroutine(ResetWeights());
                    break;
                default:
                    activeCurve = null;
                    isShipJumping = false;
                    break;
            }
        }

        void OverrideConstraintSource(Transform newSource)
        {
            if (!positionConstraint || newSource == null) return;

            if (positionConstraint.sourceCount <= 1) return;

            var source = positionConstraint.GetSource(1);
            source.sourceTransform = newSource;
            positionConstraint.SetSource(1, source);
        }
        System.Collections.IEnumerator ResetWeights()
        {
            float timer = 0;
            float t;
            
            while (timer < resetWeightsLerpDuration)
            {
                timer += Time.deltaTime;
                t = timer / resetWeightsLerpDuration;
                SetConstraintWeight(0, 1f - t);
                SetConstraintWeight(1, t);
                yield return null;
            }
            
            //Reset weights
            SetConstraintWeight(0, 0);
            SetConstraintWeight(1, 1);
        }
    }
}
