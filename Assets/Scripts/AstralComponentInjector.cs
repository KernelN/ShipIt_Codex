using UnityEngine;

public class AstralComponentInjector : MonoBehaviour
{
    [SerializeField] ShipIt.Gameplay.Astral.AstralBody astralBody;
    [SerializeField] ShipIt.Gameplay.Astral.AstralComponentBuilder[] componentList;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < componentList.Length; i++)
            astralBody.AddAstralComponent(componentList[i].GetComponent());
    }
}
