using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject rodOffsetObj;
    public bool readyWelding;
    public GameObject plate;
    public GameObject weldParticle;
    public GameObject electrodeFilling;

    public float meltingSize;

    public float setDist = 0.05f;//0.05f;// 0.3222525f;//0.008498728f;//-0.2075334f;

    public GameObject temp;

    public bool direction;

    public GameObject burningPoint;
    public GameObject weldDecal;
    public static bool isHot;

    public static bool isReached;

    public static Player instance;
    public Color paintColor;
    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        weldParticle.SetActive(false);
        burningPoint.SetActive(false);
    }

    void Update()
    {
        Vector3 difDir = Vector3.up;
        float dist = Vector3.Dot(difDir, this.transform.position - plate.transform.position);
      //  Debug.Log(dist);    

      //  if (readyWelding)
        //{
            
            RaycastHit hit;
            if (Physics.Raycast(this.transform.position, transform.TransformDirection(-Vector3.right), out hit, 100.0f))
            {
                Debug.DrawRay(this.transform.position, hit.point - this.transform.position, Color.red);
                Paintable p = hit.collider.GetComponent<Paintable>();
                if (p != null)
                {
                    weldParticle.SetActive(true);
                    PaintManager.instance.paint(p, this.transform.position, radius, hardness, strength, paintColor);
                    rodOffsetObj.transform.localScale -= new Vector3(rodOffsetObj.transform.localScale.x * Time.deltaTime * meltingSize, 0, 0);
                }
            }
        //}

        else
        {
            weldParticle.SetActive(false);
        }

        if(readyWelding)
        {
            burningPoint.SetActive(true);
        }

        else { burningPoint.SetActive(false); }



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

    public Vector3 offset, decalOffset;
    private void OnTriggerEnter(Collider other)
    {


        if (other.gameObject.tag == "Plate")
        {
            readyWelding = true;
        }
    }
    private void OnTriggeStay(Collider other)
    {


        if (other.gameObject.tag == "Plate")
        {
            readyWelding = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Plate")
        {
            readyWelding = false;
            isHot = false;
        }
    }

}