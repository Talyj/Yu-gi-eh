using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagesBehaviour : MonoBehaviourPunCallbacks
{

    public static GameObject LocalDamageInstance;


    public void Start()
    {
        Destroy(gameObject, 1);

    }

    public void Awake()
    {
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            DamagesBehaviour.LocalDamageInstance = this.gameObject;
        }
        // CRITICAL
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }
}
