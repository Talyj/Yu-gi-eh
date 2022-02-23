using UnityEngine;
using UnityEngine.EventSystems;

using Photon.Pun;

using System.Collections;
using Photon.Realtime;

namespace Com.MyCompany.MyGame
{
    public class PlayerManager : MonoBehaviourPunCallbacks
    {
        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField] public GameObject PlayerUiPrefab;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        [Tooltip("The current Health of our player")]
        public float Health = 100f;

        public static bool myTurn = false;
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
                if(Health <= 0f)
                {
                    GameManager.Instance.LeaveRoom();
                }
            }

            if (myTurn)
            {
                //Game
            }
        }

        private void EndTurn()
        {
            myTurn = false;            
        }

        private void DeclareAttack()
        {
            //TODO GetHealth 
        }

        public static void LooseHealth(float damages)
        {
            Instance.Health -= damages;
        }
    }
}