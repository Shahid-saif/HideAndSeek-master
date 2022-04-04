using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Transporter : MonoBehaviour
{
    [SerializeField] Transform reciever;
    [SerializeField] float movingSpeed;
    Transform player;
    bool startMoving;

    private void Update()
    {
        if (startMoving)
        {
            player.GetComponent<Rigidbody>().MovePosition(Vector3.MoveTowards(player.position, reciever.position + Vector3.up * 2, movingSpeed));
            if((int)player.position.x == (int)reciever.position.x && (int)player.position.z == (int)reciever.position.z)
            {
                startMoving = false;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (other.gameObject.GetPhotonView().IsMine)
            {
                startMoving = true;
                player = other.transform;
            }
        }
    }
}
