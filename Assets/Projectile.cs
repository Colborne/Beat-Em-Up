using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;

    void Update()
    {
        GetComponent<Rigidbody>().AddForce(new Vector3(speed, 0, 0));
    }
}
