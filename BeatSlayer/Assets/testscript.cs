using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testscript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject pointer;

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("123");
        //pointer.SetActive(true);
        //Vector3 mid = Vector3.zero;
        //for (int i = 0; i < other.collision.contactCount; i++)
        //{
        //    mid += collision.contacts[i].point;
        //}
        //mid = new Vector3(mid.x / collision.contactCount, mid.y / collision.contactCount, mid.z / collision.contactCount);
        //pointer.transform.position = mid;
    }
    private void OnCollisionStay(Collision collision)
    {
        Debug.Log("123");
        pointer.SetActive(true);
        Vector3 mid = Vector3.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            mid += collision.contacts[i].point;
        }
        mid = new Vector3(mid.x / collision.contactCount, mid.y / collision.contactCount, mid.z / collision.contactCount);
        pointer.transform.position = mid;
    }
    private void OnCollisionExit(Collision collision)
    {
        pointer.SetActive(false);
    }
    private void OnTriggerExit(Collider other)
    {
        //pointer.SetActive(false);
    }
}
