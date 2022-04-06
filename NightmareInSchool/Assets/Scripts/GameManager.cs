using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        // Start is called before the first frame update

        public static GameManager instance;
        public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

        public GameObject localPlayerPrefab;
        public GameObject playerPrefab;

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

        public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
        {
            GameObject _player;

            _position.x = (float)328.3397;
            _position.y = (float)-15.8;
            _position.z = (float)75.46176;

            if (_id == Client.instance.myId)
            {
                _player = Instantiate(localPlayerPrefab, _position, _rotation);
            }
            else
            {
                _player = Instantiate(playerPrefab, _position, _rotation);
            }

            _player.GetComponent<PlayerManager>().id = _id;
            _player.GetComponent<PlayerManager>().username = _username;
            players.Add(_id, _player.GetComponent<PlayerManager>());
        }
    }
}
