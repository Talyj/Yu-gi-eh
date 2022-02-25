using Photon.Pun;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField] public GameObject PlayerUiPrefab;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        [Tooltip("The current Health of our player")]
        public float Health;

        //public bool myTurn = false;
        public static PlayerManager Instance;

        public static GameObject[] cardsOnField;

        public void Start()
        {
            Instance = this;
            if (PlayerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(PlayerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }
        }

        public void Awake()
        {
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
            {
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }
            // CRITICAL
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);
        }

        public void Update()
        {
            if (photonView.IsMine)
            {
                if (Health <= 0f)
                {
                    LeaveRoom();
                }
            }

            //if (myTurn)
            //{
            //Movement();
            //Game
            //}
            Debug.Log(" health" + Health);
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void DamagesCalculation(float damage)
        {
            //If regen the float has to be negative
            Health -= damage;
        }

        private void EndTurn()
        {
            //myTurn = false;            
        }

        private void DeclareAttack()
        {
            //TODO GetHealth 
        }

        private void Movement()
        {
            var speed = Random.Range(1, 20);

            transform.RotateAround(new Vector3(0, 1, 0), Vector3.up, 20 * Time.deltaTime * speed);

            if (Input.GetKeyDown(KeyCode.K))
            {
                transform.position += new Vector3(0, 3, 0);
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                transform.position -= new Vector3(0, 3, 0);
            }

        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(Health);
            }
            else
            {
                // Network player, receive data
                this.Health = (float)stream.ReceiveNext();
            }
        }
    }
}