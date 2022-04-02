using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerTransformAntiLag : MonoBehaviourPun, IPunObservable
{

    ThirdPersonCharacter player;
    ThirdPersonUserControl playerJumpController;
    protected Vector3 RemotePlayerPosition;
    protected Vector3 RemotePlayerVelocity;
    protected Vector3 RemotePlayerRotation;


    Rigidbody rb;
    private void Awake()
    {
        player = GetComponent<ThirdPersonCharacter>();
        playerJumpController = GetComponent<ThirdPersonUserControl>();
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        if (photonView.IsMine) return;
        rb.velocity = RemotePlayerVelocity;
        transform.rotation = Quaternion.Euler(RemotePlayerRotation);

        var lagDistance = RemotePlayerPosition - transform.position;
        if (lagDistance.magnitude > 1f)
        {
            lagDistance = Vector3.zero;
            transform.position = RemotePlayerPosition;
        }
        else
        {
            player.h = lagDistance.normalized.x;
            player.v = lagDistance.normalized.z;
        }
        playerJumpController.jump = RemotePlayerPosition.y - transform.position.y > .1f;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation.eulerAngles);
            stream.SendNext(rb.velocity);
        }
        else
        {
            RemotePlayerPosition = (Vector3)stream.ReceiveNext();
            RemotePlayerRotation = (Vector3)stream.ReceiveNext();
            RemotePlayerVelocity = (Vector3)stream.ReceiveNext();
        }
    }
}