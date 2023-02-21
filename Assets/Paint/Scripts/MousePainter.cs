using UnityEngine;

public class MousePainter : MonoBehaviour
{
    //public Camera cam;
    [Space]
    public bool mouseSingleClick;
    [Space]
    public Color paintColor;

    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;
    public Paintable p;


    public bool click;

    void Update()
    {
        // click = Player.readyWelding; //mouseSingleClick ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0);


        {
            Debug.Log(click);
            //Vector3 position = this.transform.position;
            //Ray ray = cam.ScreenPointToRay(position);
            RaycastHit hit;
            Debug.Log("Ready");

            if (Physics.Raycast(this.transform.position, transform.TransformDirection(-Vector3.right), out hit, 100.0f))
            {
                Debug.DrawRay(this.transform.position, hit.point - this.transform.position, Color.red);
                //transform.position = hit.point;
                Paintable p = hit.collider.GetComponent<Paintable>();
                Debug.Log("Hit");
                 if(p != null)
                {
                    //if (Player.instance.i)
                        PaintManager.instance.paint(p, hit.point, radius, hardness, strength, paintColor);
                }
            }
        }
    }
}
