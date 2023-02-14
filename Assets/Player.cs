using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject rodOffsetObj;

    Vector3 screenPoint;
    Vector3 offSet;

    public GameObject plate;
    public GameObject weldParticle;

    public float meltingSize;

    public float setDist = 0.3222525f;//0.008498728f;//-0.2075334f;

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

        if (difference < setDist)
        {
            weldParticle.SetActive(true);
            rodOffsetObj.transform.localScale -= new Vector3(rodOffsetObj.transform.localScale.x* Time.deltaTime*meltingSize,0, 0);
        }

        else
            weldParticle.SetActive(false);
    }

    //private void OnMouseDown()
    //{
    //    screenPoint = Camera.main.WorldToScreenPoint(transform.position);

    //    offSet = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    //    Cursor.visible = false;
    //}

    //private void OnMouseDrag()
    //{
    //    Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
    //    Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offSet;

    //    transform.position = curPosition;
    //}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, setDist);
    }
}