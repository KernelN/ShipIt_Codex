using System.Collections.Generic;
using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    public class ManualPathTracer : MonoBehaviour
    {
        [SerializeField] PathFinder pathFinder;
        [SerializeField] List<GameObject> highlightersPool = new List<GameObject>();

        readonly List<GameObject> activeHighlights = new List<GameObject>();

        void Start()
        {
            Transform originPlanet = AstralManager.inst ? AstralManager.inst.OriginPlanet : null;
            HighlightPaths(originPlanet);
        }

        public void HighlightPaths(Transform originPlanet)
        {
            ClearHighlights();

            if (!pathFinder || !originPlanet)
            {
                return;
            }

            List<Transform> targets = pathFinder.GetPaths(originPlanet);
            if (targets == null || targets.Count == 0)
            {
                return;
            }

            foreach (Transform target in targets)
            {
                if (!target)
                {
                    continue;
                }

                GameObject highlighter = GetHighlighterFromPool();
                if (!highlighter)
                {
                    break;
                }

                Transform highlighterTransform = highlighter.transform;
                highlighterTransform.SetParent(target);
                highlighterTransform.localScale = Vector3.one;
                highlighterTransform.localPosition = Vector3.zero;
                highlighterTransform.localRotation = Quaternion.identity;
                highlighter.SetActive(true);

                activeHighlights.Add(highlighter);
            }
        }

        void ClearHighlights()
        {
            for (int i = activeHighlights.Count - 1; i >= 0; i--)
            {
                GameObject highlighter = activeHighlights[i];
                if (!highlighter)
                {
                    activeHighlights.RemoveAt(i);
                    continue;
                }

                highlighter.SetActive(false);
                if (!highlightersPool.Contains(highlighter))
                {
                    highlightersPool.Add(highlighter);
                }

                activeHighlights.RemoveAt(i);
            }
        }

        GameObject GetHighlighterFromPool()
        {
            for (int i = highlightersPool.Count - 1; i >= 0; i--)
            {
                GameObject candidate = highlightersPool[i];
                highlightersPool.RemoveAt(i);
                if (candidate)
                {
                    candidate.SetActive(false);
                    return candidate;
                }
            }

            return null;
        }
    }
}
