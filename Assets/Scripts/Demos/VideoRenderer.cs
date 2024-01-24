using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class VideoRenderer : MonoBehaviour, IDemo
{
    public VideoPlayer videoPlayer;
    
    // TODO: change to touch input/seat interactions
    public bool isPlaying = true;
    int prevVideo = 0;
    public int currVideo = 0;
    public string folderName;
    string[] videoNames;

    // Start is called before the first frame update
    void Start()
    {
        videoNames = Directory.GetFiles("Assets/Resources/" + folderName, "*.mp4");
        ChangeVideo(currVideo);
    }

    // Update is called once per frame
    void Update()
    {
        // make sure the currVideo does not exceed the max/min values
        if (currVideo < 0)
            currVideo = 0;
        else if (currVideo >= videoNames.Length)
            currVideo = videoNames.Length - 1;

        // TODO: Change to seat values
        if (isPlaying)
            videoPlayer.Play();
        else
            videoPlayer.Pause();

        if (prevVideo != currVideo)
        {
            prevVideo = currVideo;
            ChangeVideo(currVideo);
        }

    }

    private void ChangeVideo(int index)
    {
        videoPlayer.Stop();
        videoPlayer.url = videoNames[index];
        videoPlayer.Play();
        DemoManager dm = transform.parent.GetComponent<DemoManager>();
        //// pause it right away
        //if (dm.currentDemo.Equals(DemoManager.Demo.Workbench)){
        //    isPlaying = false;
        //}
    }

    void IDemo.TriggerNext()
    {
        currVideo++;
    }

    void IDemo.TriggerBack()
    {
        currVideo--;
    }

    void IDemo.TriggerPlay()
    {
        isPlaying = true;
    }

    void IDemo.TriggerPause()
    {
        isPlaying = false;
    }
}
