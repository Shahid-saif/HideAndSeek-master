using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{

    List<Transform> points = new List<Transform>();
    Transform linkPoint;
    [SerializeField] float teleportCooldown = 0.5f;
    public float teleportTimer;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < transform.parent.childCount; i++) {
            points.Add(transform.parent.GetChild(i));
        }
        foreach (var point in points)
            {
            if(point.name != transform.name && int.Parse(point.name)/2 == int.Parse(transform.name) / 2)
            {
                linkPoint = point;
            }
            }}

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player" && teleportTimer < Time.time)
        {
            other.transform.position = linkPoint.position + Vector3.up ;
            teleportTimer = Time.time + teleportCooldown;
            linkPoint.GetComponent<Teleporter>().teleportTimer = Time.time + 2 * teleportCooldown;
        }
    }
}
