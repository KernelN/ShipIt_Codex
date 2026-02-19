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
        readonly List<Transform> completedPathPlanets = new List<Transform>();

        public IReadOnlyList<Vector3> CompletedPathPoints => completedPathPoints;
        public IReadOnlyList<Transform> CompletedPathPlanets => completedPathPlanets;

        internal override bool DoNotDestroyOnLoad => true;

        public bool IsTargetPlanet(Transform planet)
        {
            AstralManager astralManager = AstralManager.inst;
            return astralManager && astralManager.IsTargetPlanet(planet);
        }

        public void CompletePath(IReadOnlyList<Transform> path)
        {
            SavePath(path);
            SetObjectsActive(disableOnPathCompleted, false);
            SetObjectsActive(enableOnPathCompleted, true);
        }

        void SavePath(IReadOnlyList<Transform> path)
        {
            completedPathPoints.Clear();
            completedPathPlanets.Clear();

            if (path == null)
                return;

            for (int i = 0; i < path.Count; i++)
            {
                Transform planet = path[i];
                if (planet)
                {
                    completedPathPlanets.Add(planet);
                    completedPathPoints.Add(planet.position);
                }
            }
        }

        public Transform GetNextPlanet(Transform currentPlanet)
        {
            if (completedPathPlanets.Count == 0)
                return null;

            if (!currentPlanet)
                return completedPathPlanets[0];

            int currentIndex = completedPathPlanets.IndexOf(currentPlanet);
            if (currentIndex < 0)
                return completedPathPlanets[0];

            int nextIndex = currentIndex + 1;
            if (nextIndex >= completedPathPlanets.Count)
                return null;

            return completedPathPlanets[nextIndex];
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
