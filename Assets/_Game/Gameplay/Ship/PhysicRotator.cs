using UnityEngine;
using UnityEngine.InputSystem;

namespace ShipIt.Gameplay
{
    public class PhysicRotator : MonoBehaviour
    {
        [SerializeField] Rigidbody rb;
        [SerializeField] float speed;
        bool enableRotation;

        void Start()
        {
            InputHolder inputs = InputHolder.inst;
            
            if(!inputs) return;
            
            inputs.actions.Player.EnableLook.performed += EnableRotation;
            inputs.actions.Player.EnableLook.canceled += EnableRotation;
            inputs.actions.Player.Look.performed += Rotate;
            inputs.actions.Player.Look.canceled += Rotate;
        }
        void OnDestroy()
        {
            InputHolder inputs = InputHolder.inst;
            
            if(!inputs) return;
            
            inputs.actions.Player.EnableLook.performed -= EnableRotation;
            inputs.actions.Player.EnableLook.canceled -= EnableRotation;
            inputs.actions.Player.Look.performed -= Rotate;
            inputs.actions.Player.Look.canceled -= Rotate;
        }
        void EnableRotation(InputAction.CallbackContext ctx)
        {
            //Debug.Log("EnableRotation: " + (ctx.performed ? "true" : "false"));
            enableRotation = ctx.performed;
            if(enableRotation) rb.angularVelocity = Vector3.zero;
        }
        void Rotate(InputAction.CallbackContext ctx)
        {
            if(!enableRotation) return;
            Vector2 input = ctx.ReadValue<Vector2>();
            Vector3 rot = new Vector3(input.y * speed, input.x * speed, 0);
            rb.angularVelocity = Vector3.zero;
            rb.AddRelativeTorque(rot, ForceMode.Impulse);
        }
    }
}