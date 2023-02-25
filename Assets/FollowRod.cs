using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowRod : MonoBehaviour
{
    public GameObject Rod;
    Material mt;

    public float maxCDTime;
    public float cdTime;

    // Start is called before the first frame update
    void Start()
    {
        mt = GetComponent<MeshRenderer>().material;
      
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = new Vector3(Rod.transform.position.x, Rod.transform.position.y/* + 0.008f*/, Rod.transform.position.z);

        if(Player.isHot)
        {
            mt.color = Color.red;
            cdTime = 0;
        }

        else
        {
            cdTime += Time.deltaTime;
            float t = cdTime / maxCDTime;

            mt.color = Color.Lerp(Color.red, Color.gray, t);
            
        }
    }
            
}
