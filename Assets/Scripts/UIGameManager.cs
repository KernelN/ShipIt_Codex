using System;
using UnityEngine;

namespace ShipIt
{
    public class UIGameManager : MonoBehaviour
    {
        //[Header("Set Values")]
        //[Header("Runtime Values")]
        GameManager inst;

        //Unity Events
        void Start()
        {
            inst = GameManager.inst;
        }

        //Methods
        public void QuitGame() => inst.QuitGame();
    }
}
