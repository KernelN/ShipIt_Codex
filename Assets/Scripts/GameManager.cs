using UnityEngine;

namespace ShipIt
{
    public class GameManager : Universal.Singleton<GameManager>
    {
        //Methods
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
