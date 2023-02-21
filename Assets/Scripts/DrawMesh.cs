using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMesh : MonoBehaviour
{
    Camera cam;
    void Start()
    {
       // cam = GetComponent<PlayerInput>().camera;
    }

    public void OnMouseDrag() //(InputAction.CallbackContext ctx)
    {
        //if(!ctx.performed)
        //    return;

        Debug.Log("StartDraw");

        StartCoroutine(Draw());
    }
    private IEnumerator Draw()
    {
        GameObject drawing = new GameObject("Drawing");
        drawing.AddComponent<MeshFilter>();
        drawing.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>(new Vector3[8]);

        //Start Draw
        Vector3 startPosition = this.transform.position; //cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,10));
        Vector3 temp = new Vector3(startPosition.x, startPosition.y, 0.5f);

        for (int i = 0;i < vertices.Count; i++)
        {
            vertices[i] = temp;
        }

        List<int> triangles = new List<int>( new int[36]);

        //Front Face
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        //triangles[3] = 0;
        //triangles[4] = 3;
        //triangles[5] = 2;
        
        //Top Face
        //triangles[6] = 2;
        //triangles[7] = 3;
        //triangles[8] = 4;
        //triangles[9] = 2;
        //triangles[10] = 4;
        //triangles[11] = 5;
        
        ////Right Face
        //triangles[12] = 1;
        //triangles[13] = 2;
        //triangles[14] = 5;
        //triangles[15] = 1;
        //triangles[16] = 5;
        //triangles[17] = 6;

        ////Left Face
        //triangles[18] = 0;
        //triangles[19] = 7;
        //triangles[20] = 4;
        //triangles[21] = 0;
        //triangles[22] = 4;
        //triangles[23] = 3;

        ////Back Face
        //triangles[24] = 5;
        //triangles[25] = 4;
        //triangles[26] = 7;
        //triangles[27] = 5;
        //triangles[28] = 7;
        //triangles[29] = 6;

        ////Bottom Face
        //triangles[30] = 0;
        //triangles[31] = 6;
        //triangles[32] = 7;
        //triangles[33] = 0;
        //triangles[34] = 1;
        //triangles[35] = 6;

        mesh.vertices = vertices.ToArray(); 
        mesh.triangles = triangles.ToArray();

        drawing.GetComponent<MeshFilter>().mesh = mesh;
        drawing.GetComponent<Renderer>().material.color = Color.red;

        Vector3 lastMousePosition = startPosition;
        while(true)
        {
            float minDistance = 0.1f;

            float distance = Vector3.Distance(cam.ScreenToWorldPoint(new Vector3( Input.mousePosition.x, Input.mousePosition.y, 10)), lastMousePosition);

            while(distance < minDistance)
            {
                distance = Vector3.Distance(cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)), lastMousePosition);
                yield return null;
            }

            vertices.AddRange(new Vector3[4]);
            triangles.AddRange(new int[30]);

            int vIndex = vertices.Count - 8;
            //Previous Vertices Indices
            int vINdex0 = vIndex + 3;
            int vINdex1 = vIndex + 2;
            int vINdex2 = vIndex + 1;
            int vINdex3 = vIndex + 0;
            //New Vertices Indices
            int vINdex4 = vIndex + 4;
            int vINdex5 = vIndex + 5;
            int vINdex6 = vIndex + 6;
            int vINdex7 = vIndex + 7;

            Vector3 currentMousePosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
            Vector3 mouseForwardVector = (currentMousePosition- lastMousePosition).normalized;

            float lineThickness = 0.25f;
            Vector3 topRightVertex = currentMousePosition + Vector3.Cross(mouseForwardVector, Vector3.back) * lineThickness;
            Vector3 bottomRightVertex = currentMousePosition + Vector3.Cross(mouseForwardVector, Vector3.forward) * lineThickness;
            Vector3 topLeftVertex = new Vector3(topRightVertex.x, topRightVertex.y, 1);
            Vector3 bottomLeftVertex = new Vector3(bottomRightVertex.x, bottomRightVertex.y, 1);

            vertices[vINdex4] = topLeftVertex;
            vertices[vINdex5] = topRightVertex;
            vertices[vINdex6] = bottomRightVertex;
            vertices[vINdex7] = bottomLeftVertex;

            int tIndex = triangles.Count - 30;

            //New Top Face
            triangles[tIndex + 0] = vINdex2;
            triangles[tIndex + 1] = vINdex3;
            triangles[tIndex + 2] = vINdex4;
            triangles[tIndex + 3] = vINdex2;
            triangles[tIndex + 4] = vINdex4;
            triangles[tIndex + 5] = vINdex5;

            //New Right Face
            triangles[tIndex + 6] = vINdex1;
            triangles[tIndex + 7] = vINdex2;
            triangles[tIndex + 8] = vINdex5;
            triangles[tIndex + 9] = vINdex1;
            triangles[tIndex + 10] = vINdex5;
            triangles[tIndex + 11] = vINdex6;

            //New Left Face
            triangles[tIndex + 12] = vINdex0;
            triangles[tIndex + 13] = vINdex7;
            triangles[tIndex + 14] = vINdex4;
            triangles[tIndex + 15] = vINdex0;
            triangles[tIndex + 16] = vINdex4;
            triangles[tIndex + 17] = vINdex3;

            ////New Back Face
            //triangles[tIndex + 18] = vINdex5;
            //triangles[tIndex + 19] = vINdex4;
            //triangles[tIndex + 20] = vINdex7;
            //triangles[tIndex + 21] = vINdex0;
            //triangles[tIndex + 22] = vINdex4;
            //triangles[tIndex + 23] = vINdex3;

            ////New Bottom Face
            //triangles[tIndex + 24] = vINdex0;
            //triangles[tIndex + 25] = vINdex6;
            //triangles[tIndex + 26] = vINdex7;
            //triangles[tIndex + 27] = vINdex0;
            //triangles[tIndex + 28] = vINdex1;
            //triangles[tIndex + 29] = vINdex6;

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            lastMousePosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));

            yield return null;
        }        
    }
    public void OnMouseUp() //(InputAction.CallbackContext ctx)
    {
        //if (!ctx.performed)
        //    return;

        Debug.Log("EndDraw");

        StopAllCoroutines();
    }
}
