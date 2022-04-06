using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;

        public GameObject startMenu;
        public InputField username;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;

            }
            else if (instance != this)
            {
                Debug.Log("Zaten var");
                Destroy(this);
            }



        }

        public void ConnectServer()
        {
            startMenu.SetActive(false);
            username.interactable = false;
            Client.instance.ConnectToServer();
        }
    }
}
