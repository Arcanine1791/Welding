using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class directionCheck : MonoBehaviour
{
    public float dir;
    public Transform temp;
    void Update()
    {
        Vector3 differenceDirection = Vector3.right;
        //dir=Vector3.Distance(transform.position, temp.position);    
        dir =Vector3.Dot(differenceDirection, transform.position- temp.position);
    }
}
