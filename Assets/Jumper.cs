using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumper : MonoBehaviour
{
    public float jumpPower = 150;



    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponentInChildren<Rigidbody>();
            Animator animator = other.GetComponentInChildren<Animator>();
          
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(0, jumpPower, 0);
            }
        }
    }
}
