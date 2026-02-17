using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ShipIt.Gameplay;

namespace ShipIt.Gameplay.Astral
{
    public class ManualPathTracer : MonoBehaviour
    {
        [SerializeField] PathFinder pathFinder;
        [SerializeField] List<GameObject> highlightersPool = new List<GameObject>();
        [SerializeField] CameraSwapper camSwapper;
        [SerializeField] PathShower pathShower;

        readonly List<GameObject> activeHighlights = new List<GameObject>();
        readonly HashSet<Transform> highlightedTargets = new HashSet<Transform>();
        readonly List<Transform> selectionPath = new List<Transform>();
        
        void OnEnable()
        {
            SubscribeInput();
        }

        void OnDisable()
        {
            UnsubscribeInput();
        }

        void Start()
        {
            if (pathFinder)
            {
                pathFinder.SetGrid();
            }

            SubscribeInput();

            Transform originPlanet = AstralManager.inst ? AstralManager.inst.OriginPlanet : null;
            selectionPath.Clear();
            if (originPlanet)
            {
                selectionPath.Add(originPlanet);
            }

            ShowPath();
            HighlightPaths(originPlanet);
        }

        public void HighlightPaths(Transform originPlanet)
        {
            ClearHighlights();
            highlightedTargets.Clear();

            if (!pathFinder || !originPlanet)
            {
                return;
            }

            List<Transform> targets = pathFinder.GetPaths(originPlanet);
            if (targets == null || targets.Count == 0)
                return;

            for (int i = 0; i < targets.Count; i++)
            {
                Transform target = targets[i];
                if (!target)
                    continue;

                GameObject highlighter = GetHighlighterFromPool();
                if (!highlighter)
                    break;

                Transform highlighterTransform = highlighter.transform;
                highlighterTransform.SetParent(target);
                highlighterTransform.localScale = Vector3.one;
                highlighterTransform.localPosition = Vector3.zero;
                highlighterTransform.localRotation = Quaternion.identity;
                highlighter.SetActive(true);

                highlightedTargets.Add(target);
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
                    highlightersPool.Add(highlighter);

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

        void SubscribeInput()
        {
            InputHolder inputs = InputHolder.inst;
            if (!inputs)
                return;

            inputs.actions.UI.Click.performed += HandleClick;
        }

        void UnsubscribeInput()
        {
            InputHolder inputs = InputHolder.inst;
            if (!inputs)
                return;

            inputs.actions.UI.Click.performed -= HandleClick;
        }

        void HandleClick(InputAction.CallbackContext context)
        {
            if (highlightedTargets.Count == 0)
                return;

            InputHolder inputs = InputHolder.inst;
            if (!inputs)
                return;

            if (!camSwapper.cam)
                return;

            Vector2 screenPosition = inputs.actions.UI.Point.ReadValue<Vector2>();
            Ray ray = camSwapper.cam.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            Transform selectedPlanet = GetHighlightedPlanet(hit.transform);
            if (!selectedPlanet)
                return;

            SelectPlanet(selectedPlanet);
        }

        Transform GetHighlightedPlanet(Transform hitTransform)
        {
            Transform current = hitTransform;
            while (current)
            {
                if (highlightedTargets.Contains(current))
                    return current;

                current = current.parent;
            }

            return null;
        }

        void SelectPlanet(Transform selectedPlanet)
        {
            if (!selectedPlanet)
                return;

            if (selectionPath.Count == 0)
            {
                selectionPath.Add(selectedPlanet);
                ShowPath();
                if (TryCompletePath(selectedPlanet))
                    return;
                HighlightPaths(selectedPlanet);
                return;
            }

            Transform currentPlanet = selectionPath[selectionPath.Count - 1];
            if (selectedPlanet == currentPlanet)
                return;

            if (selectionPath.Count >= 2 && selectedPlanet == selectionPath[selectionPath.Count - 2])
            {
                selectionPath.RemoveAt(selectionPath.Count - 1);
                ShowPath();
                if (TryCompletePath(selectedPlanet))
                    return;
                HighlightPaths(selectedPlanet);
                return;
            }

            selectionPath.Add(selectedPlanet);
            ShowPath();
            if (TryCompletePath(selectedPlanet))
                return;
            HighlightPaths(selectedPlanet);
        }

        bool TryCompletePath(Transform selectedPlanet)
        {
            PathManager pathManager = PathManager.inst;
            if (!pathManager || !pathManager.IsTargetPlanet(selectedPlanet))
                return false;

            pathManager.CompletePath(selectionPath);
            highlightedTargets.Clear();
            ClearHighlights();
            return true;
        }

        void ShowPath() => pathShower.ShowPath(selectionPath);
    }
}
