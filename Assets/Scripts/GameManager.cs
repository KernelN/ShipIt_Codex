using UnityEngine;

namespace ShipIt
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager inst;
        
        //[Header("Set Values")]
        //[Header("Runtime Values")]

        //Unity Events
        public void Awake()
        {
            if (inst != null)
            {
                Destroy(this);
                return;
            }
            
            inst = this;
            DontDestroyOnLoad(this);
        }
        void OnDestroy()
        {
            if (inst == this)
                inst = null;
        }

        //Methods
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
