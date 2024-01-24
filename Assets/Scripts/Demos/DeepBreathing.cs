using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class DeepBreathing : MonoBehaviour, IDemo
{
    // for modifying the scale
    float currScale = 0.0f;
    float scaleIncrement = 0.001f;
    const float MAX_SIZE = 0.15f;

    // for modifying the transparency
    float currAlpha = 0.01f;
    float alphaIncrement = 0.001f;
    const float MIN_ALPHA = 0.01f;

    // for pulsating the ellipse
    float period = 4.0f;
    float nextAction = 4.0f;
    bool isExpanding = true;


    private GameObject ellipse;
    private Text instructions;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (ellipse == null)
        {
            ellipse = gameObject.transform.Find("Quad").gameObject;
            ellipse.transform.localScale = new Vector3(currScale, currScale, 1);
            ellipse.GetComponent<MeshRenderer>().material.color = new Color(0, 1.571885f, 1.789425f, currAlpha);

            instructions = gameObject.transform.Find("Canvas")
                .transform.Find("Instructions").GetComponent<Text>();
            
        }

        // verify whether the ellipse should expand or contract
        if (Time.time > nextAction)
        {
            nextAction += period;
            isExpanding = !isExpanding;
        }

        // adjust the size and transparency based on whether it's expanding or contracting
        if (isExpanding)
        {
            currScale += scaleIncrement;
            currAlpha += alphaIncrement;
            instructions.text = "Inhale";
        }
        else
        {
            currScale -= scaleIncrement;
            currAlpha -= alphaIncrement;
            instructions.text = "Exhale";
        }

        // don't let the ellipse exceed the max/min
        if (currScale > MAX_SIZE)
            currScale = MAX_SIZE;
        if (currScale < 0)
            currScale = 0;

        if (currAlpha < MIN_ALPHA)
            currAlpha = MIN_ALPHA;
        if (currAlpha > 1)
            currAlpha = 1;

        GrowNotification(currScale);
        ChangeNotficationOpacity(currAlpha);
    }

    // Slowly grow the ellipse
    private void GrowNotification(float size)
    {
        ellipse.transform.localScale = new Vector3(size, size, 1);
    }

    // Slowly change the ellipse's opacity
    private void ChangeNotficationOpacity(float alpha)
    {
        ellipse.GetComponent<MeshRenderer>().material.color = new Color(0, 1.571885f, 1.789425f, alpha);
        instructions.color = new Color(1, 1, 1, alpha*2);
    }

    void IDemo.TriggerNext()
    {
        throw new System.NotImplementedException();
    }

    void IDemo.TriggerBack()
    {
        throw new System.NotImplementedException();
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
