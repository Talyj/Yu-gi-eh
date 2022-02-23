//using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;

namespace Com.MyCompany.MyGame
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        //THIS CLASS CONTAIN THE GAME RULES
        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;
        public static GameManager Instance;
        private enum gameState
        {
            //Draw a card
            drawPhase = 0,
            //Here is when we put cards
            mainPhase = 1,
            //Here is when we order attacks
            battlePhase = 2,
            endphase = 3
        };

        private float timeBetweenBoards = 2;

        private int turn;
        private gameState state;
        private bool isEndTurn;
        private int playerPlaying;
        private PlayerManager[] players;
        private int player1;
        private int player2;
        private bool isPlaying;
        //[SerializeField] private GameObject[] boards;
        [SerializeField] private GameObject drawBoard;
        [SerializeField] private GameObject mainBoard;
        [SerializeField] private GameObject battleBoard;
        [SerializeField] private GameObject endBoard;

        //[SerializeField] private GameObject[] texts;
        [SerializeField] private GameObject drawText;
        [SerializeField] private GameObject playText;
        [SerializeField] private GameObject battleText;
        [SerializeField] private GameObject endText;

        private void Start()
        {
            #region Connection Management
            Instance = this;
            if(playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if(PlayerManager.LocalPlayerInstance == null)
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", Application.loadedLevelName);
                    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                    PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }
            #endregion

            #region game rules
            turn = 0;
            isPlaying = true;
            state = gameState.drawPhase;
            var cpt = 10;
            isEndTurn = true;

            //if (PhotonNetwork.PlayerList.Length >= 1)
            //{
            //    MainGame();
            //}
            #endregion
        }

        private void Update()
        {
            if (isPlaying)
            {
                MainGame();
            }
        }

        #region Connection Management

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
        }

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting


            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


                LoadArena();
            }
        }


        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


                LoadArena();
            }
        }
    #endregion

        #region game rules
        private void MainGame()
        {
            //players = FindObjectsOfType<PlayerManager>();
            //if (SceneManager.GetActiveScene().name == "Room for 2" && players.Length > 1)
            if (SceneManager.GetActiveScene().name == "Room for 1"/* && players.Length > 0*/)
            {
                //var res = new List<PlayerManager>();
                //foreach(var p in players)
                //{
                //    res.Add(p);
                //}
                //player1 = players[0].GetInstanceID();
                //player2 = players[1].GetInstanceID();
                //if (turn % 2 == 0)
                //{
                //    GameLoop(res.Any(x => x.GetInstanceID == player1), player2);
                //}
                //else
                //{
                //    GameLoop(player2, player1);
                //}
                GameLoop(/*players[0]*/);
            }
        }

        private void GameLoop(/*PlayerManager playerTurn, PlayerManager otherPlayer*/)
        {
            //If otherPlayer block the commands 
            switch (state)
            {
                case gameState.drawPhase:
                    {
                        //setActive text -> Piochez une carte !
                        drawText.SetActive(true);
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            drawText.SetActive(false);
                            ChangePhase(state);
                        }
                        break;
                    }
                case gameState.mainPhase:
                    {
                        playText.SetActive(true);
                        //Modify the If statement -> if a new imageTarget is detected
                        if (Input.GetKeyDown(KeyCode.A))
                        {
                            //Add the type of card to the player space
                            //Start invoke animation
                            playText.SetActive(false);
                            ChangePhase(state);

                        }
                        break;
                    }
                case gameState.battlePhase:
                    {
                        //Need image target to determine what is being played
                        //Loop trought the list cardsOnField to get damage/effects ...
                        //Let player choose what is the target and caculate damages
                        battleText.SetActive(true);
                        if (Input.GetKeyDown(KeyCode.Z))
                        {
                            //otherPlayer.Health -= 20;
                            battleText.SetActive(false);
                            ChangePhase(state);
                        }
                        break;
                    }
                case gameState.endphase:
                    {
                        //endText.SetActive(true);
                        //if (Input.GetKeyDown(KeyCode.E))
                        //{
                        //endText.SetActive(false);
                        if (isEndTurn)
                        {
                            StartCoroutine(WaitChangePhase(timeBetweenBoards));
                            isEndTurn = true;
                        }
                        isEndTurn = false;
                        //}
                        break;
                    }
            }
        }

        private IEnumerator WaitChangePhase(float time)
        {
            yield return new WaitForSeconds(time);
            ChangePhase(state);
        }

        #region actions
        private void Draw()
        {
            //TODO
        }

        private void MainPhase()
        {
            //TODO
        }

        private void BattlePhase()
        {
            //TODO
        }

        private void EndPhase()
        {
            //TODO
        }
        #endregion

        private bool OneIsDead(float health1, float health2)
        {
            if(health1 <= 0 || health2 <= 0)
            {
                return true;
            }
            return false;
        }

        private IEnumerator ChangeBoard(GameObject boardToDisplay, GameObject boardToHide1, GameObject boardToHide2, GameObject boardToHide3)
        {
            boardToHide1.SetActive(false);
            boardToHide2.SetActive(false);
            boardToHide3.SetActive(false);
            boardToDisplay.SetActive(true);
            yield return new WaitForSeconds(timeBetweenBoards);
            boardToDisplay.SetActive(false);
        }

        private void ChangePhase(gameState gameStatus)
        {
            switch (gameStatus)
            {
                case gameState.drawPhase:
                {
                    state = gameState.mainPhase;
                    StartCoroutine(ChangeBoard(mainBoard, endBoard, drawBoard, battleBoard));
                    break;
                }
                case gameState.mainPhase:
                {
                    state = gameState.battlePhase;
                    StartCoroutine(ChangeBoard(battleBoard, endBoard, drawBoard, mainBoard));
                    break;
                }
                case gameState.battlePhase:
                {
                    state = gameState.endphase;
                    StartCoroutine(ChangeBoard(endBoard, mainBoard, drawBoard, battleBoard));
                    break;
                }
                case gameState.endphase:
                {
                    state = gameState.drawPhase;
                    StartCoroutine(ChangeBoard(drawBoard, endBoard, mainBoard, battleBoard));
                    turn++;
                    break;
                }

            }
        }
        #endregion
    }
}

#region comment
//switch (state)
//{
//    case gameState.drawPhase:
//        {
//            //setActive text -> Piochez une carte !
//            drawText.SetActive(true);
//            if (Input.GetKeyDown(KeyCode.Space))
//            {
//                drawText.SetActive(false);
//                ChangePhase(state);
//            }
//            break;
//        }
//    case gameState.mainPhase:
//        {
//                playText.SetActive(true);
//                //Modify the If statement -> if a new imageTarget is detected
//                if (Input.GetKeyDown(KeyCode.A))
//                {
//                    //Add the type of card to the player space
//                    //Start invoke animation
//                    playText.SetActive(false);
//                    ChangePhase(state);

//                }
//            break;
//        }
//    case gameState.battlePhase:
//        {
//                //Need image target to determine what is being played
//                //Loop trought the list cardsOnField to get damage/effects ...
//                //Let player choose what is the target and caculate damages
//                battleText.SetActive(true);
//                if (Input.GetKeyDown(KeyCode.Z))
//                {
//                    battleText.SetActive(false);
//                    ChangePhase(state);
//                }
//                break;
//        }
//    case gameState.endphase:
//        {
//                //endText.SetActive(true);
//                //if (Input.GetKeyDown(KeyCode.E))
//                //{
//                //endText.SetActive(false);
//                if (isEndTurn)
//                {
//                    StartCoroutine(WaitChangePhase(3));
//                    isEndTurn = true;
//                }
//                isEndTurn = false;
//                //}
//            break;
//        }
//}
#endregion