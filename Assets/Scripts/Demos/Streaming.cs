using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Streaming : MonoBehaviour, IDemo
{
    [System.Serializable]
    public enum StreamState
    {
        GainedHealth,
        LostHealth,
        Neutral
    }
    public StreamState streamState = StreamState.Neutral;
    float greenTime = 4;
    float greenDur = 12;
    float redTime = 92;
    float redDur = 10;

    GameObject backgroundQuad;
    Material backgroundMat;

    // Start is called before the first frame update
    void Start()
    {
        backgroundQuad = gameObject.transform.Find("Video").gameObject;
        backgroundMat = backgroundQuad.GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        // reset to neutral after the lost health
        if (Time.time > redTime + redDur)
        {
            streamState = StreamState.Neutral;
        }

        else if (Time.time > redTime)
        {
            streamState = StreamState.LostHealth;
        }

        else if (Time.time > greenTime + greenDur)
        {
            streamState = StreamState.Neutral;
        }

        else if (Time.time > greenTime)
        {
            streamState = StreamState.GainedHealth;
        }

        else
        {
            streamState = StreamState.Neutral;
        }


        CheckState();
    }

    private void CheckState()
    {
        Color currColor = backgroundMat.color;
        Color newColor = Color.white;

        if (streamState == StreamState.LostHealth)
        {
            newColor = Color.red;
        }

        else if (streamState == StreamState.GainedHealth)
        {
            newColor = Color.green;
        }


        backgroundMat.color = Color.Lerp(currColor, newColor, 0.01f);
    }

    void IDemo.TriggerNext()
    {
        streamState++;
    }

    void IDemo.TriggerBack()
    {
        streamState--;
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
