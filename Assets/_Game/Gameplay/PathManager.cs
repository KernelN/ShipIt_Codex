using System;
using System.Collections.Generic;
using UnityEngine;
using Universal;

namespace ShipIt.Gameplay.Astral
{
    public class PathManager : Singleton<PathManager>
    {
        [SerializeField] List<GameObject> disableOnPathCompleted = new List<GameObject>();
        [SerializeField] List<GameObject> enableOnPathCompleted = new List<GameObject>();

        readonly List<Vector3> completedPathPoints = new List<Vector3>();
        readonly List<AstralBody> completedPathBodies = new List<AstralBody>();

        public IReadOnlyList<Vector3> CompletedPathPoints => completedPathPoints;

        public event Action<AstralBody, IReadOnlyList<AstralBody>> SelectionContextUpdated;

        internal override bool DoNotDestroyOnLoad => true;

        public bool IsTargetPlanet(AstralBody planet)
        {
            AstralManager astralManager = AstralManager.inst;
            return astralManager && astralManager.IsTargetPlanet(planet);
        }


        public void NotifySelectionContext(AstralBody selectedPlanet, IReadOnlyList<AstralBody> selectablePlanets)
        {
            SelectionContextUpdated?.Invoke(selectedPlanet, selectablePlanets);
        }

        public void CompletePath(IReadOnlyList<AstralBody> path)
        {
            SavePath(path);
            SetObjectsActive(disableOnPathCompleted, false);
            SetObjectsActive(enableOnPathCompleted, true);
        }

        void SavePath(IReadOnlyList<AstralBody> path)
        {
            completedPathPoints.Clear();
            completedPathBodies.Clear();

            if (path == null)
                return;

            for (int i = 0; i < path.Count; i++)
            {
                AstralBody planet = path[i];
                if (!planet)
                    continue;

                completedPathBodies.Add(planet);
                completedPathPoints.Add(planet.transform.position);
            }
        }


        public AstralBody GetNextOnPath(AstralBody current)
        {
            if (!current || completedPathBodies.Count == 0)
                return null;

            for (int i = 0; i < completedPathBodies.Count - 1; i++)
            {
                if (completedPathBodies[i] == current)
                    return completedPathBodies[i + 1];
            }

            return null;
        }

        public AstralBody GetFirstOnPath()
        {
            if (completedPathBodies.Count == 0)
                return null;

            return completedPathBodies[0];
        }

        static void SetObjectsActive(IReadOnlyList<GameObject> objects, bool active)
        {
            if (objects == null)
                return;

            for (int i = 0; i < objects.Count; i++)
            {
                GameObject current = objects[i];
                if (current)
                    current.SetActive(active);
            }
        }
    }
}
