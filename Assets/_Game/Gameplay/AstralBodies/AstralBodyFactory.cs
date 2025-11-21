using ShipIt.Gameplay;
using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    public abstract class AstralBodyFactory : ScriptableObject
    {
        [SerializeField] ComponentExposer bodyPrefab;
        [SerializeField] float minScaleMultiplier = 1f;
        [SerializeField] float maxScaleMultiplier = 1f;

        public virtual AstralBody SpawnBody(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            ComponentExposer exposer = CreateBody(position, rotation, parent);
            return exposer != null ? exposer.AstralBody : null;
        }

        protected ComponentExposer CreateBody(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (bodyPrefab == null)
            {
                return null;
            }

            ComponentExposer exposer = Instantiate(bodyPrefab, position, rotation, parent);
            ApplyRandomScale(exposer.transform);
            return exposer;
        }

        void ApplyRandomScale(Transform target)
        {
            float min = Mathf.Min(minScaleMultiplier, maxScaleMultiplier);
            float max = Mathf.Max(minScaleMultiplier, maxScaleMultiplier);

            if (Mathf.Approximately(min, max))
            {
                target.localScale *= min;
                return;
            }

            float multiplier = Random.Range(min, max);
            target.localScale *= multiplier;
        }
    }
}
