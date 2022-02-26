//using System;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private GameObject[] players;
        private PlayerStat player1;
        private PlayerStat player2;

        private float health1;
        private float health2;
        private bool first;

        private int currentCardNumber;
        private int oldCardNumber;

        private float damageTurn;
        private float healTurn;

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

        [SerializeField] private GameObject victoryP1;
        [SerializeField] private GameObject victoryP2;


        private void Start()
        {
            #region Connection Management
            Instance = this;
            if (playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if (PlayerManager.LocalPlayerInstance == null)
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
            turn = 1;
            isPlaying = true;
            state = gameState.drawPhase;
            isEndTurn = true;
            first = true;
            currentCardNumber = 0;
            oldCardNumber = 0;
            #endregion
        }

        private void Update()
        {
            if (isPlaying)
            {
                MainGame();
                Debug.Log( "tour numéro : " + turn);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
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
            players = GameObject.FindGameObjectsWithTag("Player");
            if (first && players.Length > 1)
            {
                player1 = new PlayerStat(100, players[0].GetInstanceID());
                player2 = new PlayerStat(100, players[1].GetInstanceID());
                first = false;
            }
            if (SceneManager.GetActiveScene().name == "Room for 2" && players.Length > 1)
            {
                if (turn % 2 == 0)
                {
                    //Tour 1 P2
                    GameLoop(player2, player1);
                }
                else
                {
                    //Tour 2 P1
                    GameLoop(player1, player2);
                }
                CheckVictory(player1, player2);
            }
        }

        //private void GameLoop(PlayerManager playerTurn, PlayerManager otherPlayer)
        private void GameLoop(PlayerStat playerTurn, PlayerStat otherPlayer)
        {
            //foreach(var p in players)
            //{
            //    if(p.GetInstanceID() == playerTurn.GetInstanceID())
            //    {

            //    }
            //}
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
                        if (Input.GetKeyDown(KeyCode.A))
                        {
                            CardNumber(true);

                        }
                        if (currentCardNumber != oldCardNumber)
                        {
                            //Have to add the card to the player playing hands
                            oldCardNumber = currentCardNumber;
                            //player1.GetComponent<PlayerManager>().DamagesCalculation(10);
                            //otherPlayer.DamagesCalculation(50);
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
                        damageTurn = 0;
                        healTurn = 0;
                        battleText.SetActive(true);
                        if (Input.GetKeyDown(KeyCode.Z))
                        {
                            if(playerTurn.GetCardsOnField() != null)
                            {
                                foreach(var c in playerTurn.GetCardsOnField())
                                {
                                    if (c.CompareTag("DPS") || c.CompareTag("tank"))
                                    {
                                        damageTurn += c.damage;
                                        c.Attack();
                                    }
                                    else if (c.CompareTag("healer"))
                                    {
                                        healTurn += c.damage;
                                        playerTurn.SetHealth(playerTurn.GetHealth() + healTurn);
                                    }
                                }
                                if (otherPlayer.GetCardsOnField() != null)
                                {
                                    foreach(var c in otherPlayer.GetCardsOnField())
                                    {
                                        if(c.life + c.shield < damageTurn)
                                        {
                                            damageTurn -= c.life;
                                            CardNumber(false);
                                            oldCardNumber = currentCardNumber;
                                            c.Die();
                                        }
                                        else
                                        {
                                            c.life -= damageTurn;
                                            c.GetDamage();
                                        }
                                    }
                                }
                                else
                                {
                                    otherPlayer.SetHealth(otherPlayer.GetHealth() - damageTurn);
                                }

                            }

                            battleText.SetActive(false);
                            StartCoroutine(WaitChangePhase(timeBetweenBoards));
                        }
                        break;
                    }
                case gameState.endphase:
                    {
                        if (isEndTurn)
                        {
                            StartCoroutine(WaitChangePhase(timeBetweenBoards));
                            isEndTurn = true;
                        }
                        isEndTurn = false;
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

        private void ResetUI()
        {
            drawBoard.SetActive(false);
            mainBoard.SetActive(false);
            battleBoard.SetActive(false);
            endBoard.SetActive(false);

            drawText.SetActive(false);
            playText.SetActive(false);
            battleText.SetActive(false);
            endText.SetActive(false);
        }

        public int CheckVictory(PlayerStat p1, PlayerStat p2)
        {
            if(p1.GetHealth() <= 0)
            {
                ResetUI();
                isPlaying = false;
                return 1;
            }
            if(p2.GetHealth() <= 0)
            {
                ResetUI();
                isPlaying = false;
                return 2;
            }
            return 0;
        }

        public void CardNumber(bool isUp)
        {
            if (isUp)
            {
                currentCardNumber = oldCardNumber + 1;
            }
            else
            {
                currentCardNumber = oldCardNumber - 1;
            }
        }

        #region test to delete maybe?
        //private void VictoryCondition(int player)
        //{
        //    if(player == 1)
        //    {
        //        victoryP1.SetActive(true);
        //    }
        //    else
        //    {
        //        victoryP2.SetActive(true);
        //    }
        //}

        //private void OneIsDead(float health1, float health2)
        //{
        //    if(health1 <= 0)
        //    {
        //        VictoryCondition(2);
        //    }
        //    else if(health2 <= 0)
        //    {
        //        VictoryCondition(1);
        //    }
        //}
        #endregion

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