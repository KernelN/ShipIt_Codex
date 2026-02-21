using System.Collections.Generic;
using UnityEngine;

namespace ShipIt
{
    public class LevelShower : MonoBehaviour
    {
        [SerializeField] List<GameObject> levelObjects = new List<GameObject>();

        void OnEnable() => Refresh();

        [ContextMenu("Refresh Levels")]
        public void Refresh()
        {
            int highestCompletedLevel = LevelManager.HighestCompletedLevel;

            for (int i = 0; i < levelObjects.Count; i++)
            {
                GameObject levelObject = levelObjects[i];
                if (!levelObject)
                    continue;

                bool shouldEnable = i == 0 || i <= highestCompletedLevel;
                levelObject.SetActive(shouldEnable);
            }
        }
    }
}