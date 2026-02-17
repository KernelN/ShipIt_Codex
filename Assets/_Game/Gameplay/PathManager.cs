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

        public IReadOnlyList<Vector3> CompletedPathPoints => completedPathPoints;

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

            if (path == null)
                return;

            for (int i = 0; i < path.Count; i++)
            {
                Transform planet = path[i];
                if (planet)
                    completedPathPoints.Add(planet.position);
            }
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
