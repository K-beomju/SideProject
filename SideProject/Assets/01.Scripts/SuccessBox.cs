using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuccessBox : MonoBehaviour
{
    [SerializeField] private GameObject particle;
    private bool isHit = false;

    

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Ball") && !isHit)
        {
           Destroy(Instantiate(particle, col.transform.position, Utills.QI), 1);
            isHit = true;

            col.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }
    }
}
