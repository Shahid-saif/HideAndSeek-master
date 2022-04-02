using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class VelocityAntiLag : MonoBehaviourPun, IPunObservable
{

    Rigidbody rb;
    protected Vector3 RemotePlayerVelocity;
    private void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine) return;
        rb.velocity = RemotePlayerVelocity;

    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.velocity);
        }
        else
        {
            RemotePlayerVelocity = (Vector3)stream.ReceiveNext();
        }
    }
}
