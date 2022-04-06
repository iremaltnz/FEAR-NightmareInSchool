using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerController1 : MonoBehaviour
    {
        // Start is called before the first frame update
        private void FixedUpdate()
        {
            SendInputToServer();
        }

        /// <summary>Sends player input to the server.</summary>
        private void SendInputToServer()
        {
            bool[] _inputs = new bool[]
            {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space)
            };

            ClientSend.PlayerMovement(_inputs);
        }
    }
}
