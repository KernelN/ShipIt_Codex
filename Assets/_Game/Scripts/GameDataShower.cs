using TMPro;
using UnityEngine;

namespace ShipIt
{
    public class GameDataShower : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI text;
        
        public void Show(GameData data)
        {
            string dataString;
            dataString = "Credits: " + data.credits;
            dataString += "\n Fuel: " + data.fuel;
            dataString += "\n Highest Level Completed: " + data.highestLevelCompleted;
            dataString += "\n Skins Bought" + data.items.Count;
            text.text = dataString;
        }
    }
}
