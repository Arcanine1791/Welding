using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeldBehaviour : MonoBehaviour
{
    public GameObject spark;
    private void OnCollisionEnter(Collision collision)
    {
        spark.SetActive(true);
    }

    private void OnCollisionExit(Collision collision)
    {
        spark.SetActive(false);
    }
}
