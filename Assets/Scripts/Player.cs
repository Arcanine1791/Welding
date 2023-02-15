using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject rodOffsetObj;

    public bool readyWelding;

    Vector3 screenPoint;
    Vector3 offSet;

    public GameObject plate;
    public GameObject weldParticle;
    public GameObject electrodeFilling;

    public float meltingSize;

    public float setDist = 0.3222525f;//0.008498728f;//-0.2075334f;

    public GameObject temp;

    public bool direction;


    // Start is called before the first frame update
    void Start()
    {
        weldParticle.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 differenceDirection = Vector3.up;

        float difference = Vector3.Dot(differenceDirection,
                this.transform.position - plate.transform.position);
        //float dist = Vector3.Distance(this.transform.position, plate.transform.position);
        Debug.Log(difference);

        Vector3 difDir = Vector3.forward;
        if (temp != null)
        {
            float dist = Vector3.Dot(difDir, this.transform.position - temp.transform.position);

            if (dist < 0) { direction = false; }
            else { direction = true; }
        }

        if (difference < setDist)
        {
            weldParticle.SetActive(true);
            rodOffsetObj.transform.localScale -= new Vector3(rodOffsetObj.transform.localScale.x * Time.deltaTime * meltingSize, 0, 0);
            //  Invoke("InstantiateBlob", 2f);
        }

        else
            weldParticle.SetActive(false);

        if (readyWelding)
        {
            Debug.Log("Scaling");
            if (direction && rb.velocity.z>0)
            {
                temp.transform.localRotation = Quaternion.Euler(0, 0, 0);
                // temp.transform.localScale += new Vector3(0, 0, temp.transform.localScale.z * Time.deltaTime * meltingSize * 10);
                Vector3 dirNorm = (this.transform.position - temp.transform.position).normalized;
                temp.transform.localScale += new Vector3(0, 0, dirNorm.z * Time.deltaTime * meltingSize / 100);
            }

            if (!direction)
            {
                temp.transform.localRotation = Quaternion.Euler(0, 180, 0);
                Vector3 dirNorm = (this.transform.position - temp.transform.position).normalized;
                //Vector3 temp1 = new Vector3(0, 0, temp.transform.localScale.z * Time.deltaTime * meltingSize*50);
                temp.transform.localScale -= new Vector3(0, 0, dirNorm.z * Time.deltaTime * meltingSize / 100);
            }

            else
                return;


        }
    }

    public Rigidbody rb;
    public bool cantgoback;
    public void InstantiateBlob()
    {
        //Debug.Log("ho");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, setDist);
    }

    public Vector3 offset;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Plate")
        {
            temp = Instantiate(electrodeFilling, this.transform.position + offset, Quaternion.identity);

            Debug.Log(transform.position);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        readyWelding = true;
    }

    private void OnTriggerExit(Collider other)
    {

        readyWelding = false;
    }

}