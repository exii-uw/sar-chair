using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class ImageRenderer : MonoBehaviour, IDemo
{
   
    private Texture2D[] images;
    public int currImage = 0; 
    private RawImage background;
    public string folderName;

    // Start is called before the first frame update
    void Start()
    {
        string[] imageNames = Directory.GetFiles("Assets/Resources/" + folderName, "*.png");
        images = new Texture2D[imageNames.Length];

        // creating textures for every image
        for (int i = 0; i < imageNames.Length; i++)
        {
            byte[] rawImageData = File.ReadAllBytes(imageNames[i]);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(rawImageData);
            // flip the texture so it is the right orientation
            images[i] = texture;
                //FlipTexture(FlipTexture(texture, true), false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (background == null)
        {
            background = gameObject.transform.Find("Canvas").Find("RawImage").GetComponent<RawImage>();
        }

        // make sure the currImage does not exceed the max/min values
        if (currImage < 0)
            currImage = 0;
        else if (currImage >= images.Length)
            currImage = images.Length - 1;
        
        background.texture = images[currImage];
        //background.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", images[currImage]);
        //background.transform.localEulerAngles = Quaternion.Euler()
    }

    private Texture2D FlipTexture(Texture2D original, bool upSideDown = true)
    {

        Texture2D flipped = new Texture2D(original.width, original.height);

        int xN = original.width;
        int yN = original.height;


        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                if (upSideDown)
                {
                    flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i));
                }
                else
                {
                    flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                }
            }
        }
        flipped.Apply();

        return flipped;
    }

    void IDemo.TriggerNext()
    {
        currImage++;
    }

    void IDemo.TriggerBack()
    {
        currImage--;
    }

    void IDemo.TriggerPlay()
    {
        throw new System.NotImplementedException();
    }

    void IDemo.TriggerPause()
    {
        throw new System.NotImplementedException();
    }
}
