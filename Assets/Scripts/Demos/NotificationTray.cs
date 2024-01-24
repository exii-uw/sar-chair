using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationTray : MonoBehaviour, IDemo
{
    [Range(1, 5)]
    public int currIndex = 1;
    private int prevIndex = 1;
    public string[] notifications = { "New email from Bob.", 
        "Sam liked your tweet.",
        "1 upcoming meeting at 10am.",
        "5 pending todos.",
        "No weather updates." };
    
    Transform selectQuad;
    Text notificationText;
    public bool showNotificationText = false;

    // Start is called before the first frame update
    void Start()
    {
        selectQuad = gameObject.transform.Find("Canvas").Find("Select");
        notificationText = gameObject.transform.Find("Canvas").Find("Text").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currIndex > 5)
            currIndex = 5;
        else if (currIndex < 1)
            currIndex = 1;
        if (currIndex != prevIndex)
        {
            Transform selectedIcon = gameObject.transform.Find("Canvas").Find(currIndex.ToString());
            selectQuad.position = new Vector3(selectedIcon.position.x, selectedIcon.position.y, selectedIcon.position.z);
            notificationText.text = notifications[currIndex - 1];
            prevIndex = currIndex;
        }

        ToggleIcons(gameObject.transform.Find("Canvas"), !showNotificationText);
    }

    private void ToggleIcons(Transform canvas, bool show)
    {
        for(int i = 1; i < canvas.childCount; i++)
        {
            try
            {
                Transform icon = canvas.Find(i.ToString());
                icon.gameObject.SetActive(show);
            }
            catch (System.Exception e) { 
                // Select quad
            }
        }

        selectQuad.gameObject.SetActive(show);
        notificationText.gameObject.SetActive(!show);
    }

    void IDemo.TriggerNext()
    {
        currIndex++;
    }

    void IDemo.TriggerBack()
    {
        currIndex--;
    }

    void IDemo.TriggerPlay()
    {
        showNotificationText = true;
    }

    void IDemo.TriggerPause()
    {
        showNotificationText = false;
    }
}
