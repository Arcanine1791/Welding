using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Vector3 screenPoint;
    Vector3 offSet;

    public GameObject plate;

    public float setDist;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 differenceDirection = Vector3.right;

        float difference = Vector3.Dot(differenceDirection,
                this.transform.position - plate.transform.position);
        //float dist = Vector3.Distance(this.transform.position, plate.transform.position);
        Debug.Log(difference);
        //if (dist < setDist)
        //{
        //    Debug.Log("Start");            
        //}
    }

    private void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(transform.position);

        offSet = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        Cursor.visible = false;
    }

    private void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offSet;

        transform.position = curPosition;
    }
}
