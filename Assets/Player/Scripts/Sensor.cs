using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;


public class Sensor : MonoBehaviour
{

    public Transform player;
    [SerializeField] bool inTrigger;
    [SerializeField] public List<Collider> Other;
    private const byte UPDATE_SEEKER_EVENT = 2;

    private void Update()
    {


        if (inTrigger && GetComponentInParent<PlayerStatus>().isSeeker && GetComponentInParent<PhotonView>().IsMine)
        {
            if (Other.Count <= 0)
                return;

            foreach (var item in Other)
            {

                RaycastHit hit;
                // Does the ray intersect any objects excluding the player layer
                if (Physics.Raycast(player.position, item.transform.position - player.transform.position, out hit, 100))
                {
                    if (hit.collider.tag == "Player")
                    {
                        hit.collider.gameObject.GetComponent<PlayerStatus>().isTargeted = true;

                        if (Input.GetKey(KeyCode.Mouse0))
                        {
                            int seekerId = hit.collider.gameObject.GetPhotonView().OwnerActorNr;
                            object[] data = new object[] { seekerId };

                            Debug.Log(seekerId);
                            PhotonNetwork.RaiseEvent(UPDATE_SEEKER_EVENT, data, RaiseEventOptions.Default, SendOptions.SendReliable);
                            GameRulesManager.gameRulesManager.UpdateSeeker(seekerId);
                        }
                    }

                }

            }

        }
    }



    private void OnTriggerStay(Collider other)
    {
        if (GetComponentInParent<PlayerStatus>().isSeeker && GetComponentInParent<PhotonView>().IsMine)
        {

            if (other.tag == "Player" && other.gameObject != this)
            {
                foreach (var collider in Other)
                {
                    if (collider == other)
                        return;

                }
                Other.Add(other);
                inTrigger = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (GetComponentInParent<PlayerStatus>().isSeeker && GetComponentInParent<PhotonView>().IsMine)
        {
            if (other.tag == "Player" && other.gameObject != this)
            {


                foreach (var collider in Other)
                {
                    if (collider == other)
                    {
                        collider.gameObject.GetComponent<PlayerStatus>().isTargeted = false;
                        Other.Remove(collider);
                    }
                }
                if (Other.Count <= 0)
                {
                    inTrigger = false;
                }
            }

        }
    }

}





