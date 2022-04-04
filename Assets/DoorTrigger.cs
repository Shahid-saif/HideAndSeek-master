using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class DoorTrigger : MonoBehaviourPun
{

    [SerializeField] Transform doorsParent;
    [SerializeField] int doorIndex;
    Animator linkDoor;
    private const byte OPEN_DOOR_EVENT = 20;
    private void Start()
    {
        for (int i = 0; i < doorsParent.childCount; i++)
        {
            if(doorsParent.GetChild(i).name == doorIndex.ToString())
            {
                linkDoor = doorsParent.GetChild(i).GetComponent<Animator>();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (other.gameObject.GetPhotonView().IsMine)
            {
                object[] data = new object[] { doorIndex };
                PhotonNetwork.RaiseEvent(OPEN_DOOR_EVENT, data, RaiseEventOptions.Default, SendOptions.SendReliable);
                linkDoor.SetTrigger("Trigger");

            }
        }
    }
}
