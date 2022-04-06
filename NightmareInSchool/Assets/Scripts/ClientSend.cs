using UnityEngine;

namespace Assets.Scripts
{
    public class ClientSend : MonoBehaviour
    {
        // Start is called before the first frame update
        public static void SendTcpData(Packet packet)
        {
            packet.WriteLength();
            Client.instance.tcp.SendData(packet);
        }

        public static void SendUdpData(Packet packet)
        {
            packet.WriteLength();
            Client.instance.udp.SendData(packet);
        }

        #region
        public static void WelcomeReceived()
        {

            try
            {
                Packet packet = new Packet((int)ClientPackets.welcomeReceived);
                packet.Write(Client.instance.myId);
                packet.Write(UIManager.instance.username.text);

                SendTcpData(packet);

            }
            catch (System.Exception)
            {

                throw;
            }

        }



        public static void PlayerMovement(bool[] _inputs)
        {
            Packet _packet = new Packet((int)ClientPackets.playerMovement);

            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs)
            {
                _packet.Write(_input);
            }
            _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);

            SendUdpData(_packet);

        }
        #endregion
    }
}
