using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Training
{
    public class PlayerController:MonoBehaviour
    {
        void Update()
        {
            ReadInput();
        }

        private void ReadInput()
        {
            var keyboard = Keyboard.current;

            if(keyboard.dKey.isPressed)
            {
                Debug.Log("dKey pressed!");
            }
            else if(keyboard.aKey.isPressed)
            {
                Debug.Log("aKey pressed!");
            }
        }
    }
}
