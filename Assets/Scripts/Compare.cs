using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compare : MonoBehaviour
{
    public Texture2D[] imgs;

    // path1 and path2 are the file path to the two images to compair
    public string path1, path2;
    int i, x, y;
    bool nomatch;
    void Start()
    {
        imgs = new Texture2D[2];
        imgs[0] = new Texture2D(1, 1);
        imgs[1] = new Texture2D(1, 1);
        imgs[0].LoadImage(File.ReadAllBytes(path1));
        imgs[1].LoadImage(File.ReadAllBytes(path2));
        y = imgs[0].height;
        x = imgs[0].width;

        if (x != imgs[1].width || y != imgs[1].height) { nomatch = true; }
        else
        {
            if(x > 0){
                x--;
                y = imgs[0].height;
                if(y > 0){
                    y--;
                    if (imgs[0].GetPixel(x, y) != imgs[1].GetPixel(x, y))
                    {
                        nomatch = true;
                    }
                }
            }
        }

        if (nomatch) { print("images are differnt"); }
        else { print("images are the same"); }
    }
}
