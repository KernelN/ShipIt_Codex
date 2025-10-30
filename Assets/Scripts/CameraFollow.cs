using System;
using UnityEngine;
using UnityEngine.Animations;

namespace ShipIt.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Ship ship;
        [SerializeField] PositionConstraint positionConstraint;
        [SerializeField] AnimationCurve followCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] float resetWeightsLerpDuration = .5f;
        bool isShipJumping;
        
        void Start()
        {
            ship.OnIsJumping += OnShipIsJumping;
        }
        void Update()
        {
            if(!isShipJumping) return;
            if (!ship || !positionConstraint) return;

            float weight = followCurve.Evaluate(ship.JumpPer);

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
        void OnShipIsJumping(bool isJumping)
        {
            isShipJumping = isJumping;
            
            if(isJumping) return;
            
            var source = positionConstraint.GetSource(1);
            source.sourceTransform = ship.CurrentPlanet;
            positionConstraint.SetSource(1, source);

            StartCoroutine(ResetWeights());
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
