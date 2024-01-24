using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpandedNotification : MonoBehaviour
{
    public GameObject notificationDemo;
    NotificationTray notificationTray;
    Text subject;
    Text body;
    string[] content = { "Hey Alice,\n" +
            "Are we still good for 10am? " +
            "I wanted to ask you about the upcoming presentation but we might need a little more time.\n" +
            "Hope this works for you.\n" +
            "- Bob",
        "Had a great time at pilates with my friends! :)",
        "Meeting with Bob at 10am (Alice's office)",
        "- review slides for VP presentation\n" +
            "- practice presentation with Bob\n" +
            "- review intern applicants\n" +
            "- schedule interviews\n" +
            "- performance reviews",
        "Sunny with a high of 16°C." };

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (notificationTray == null)
        {
            notificationTray = notificationDemo.GetComponent<NotificationTray>();
            subject = gameObject.transform.Find("Canvas").Find("Message").GetComponent<Text>();
            body = gameObject.transform.Find("Canvas").Find("NotificationContent").GetComponent<Text>();

        }

        try
        {
            int currIndex = notificationTray.currIndex - 1;
            string title = notificationTray.notifications[currIndex];
            string message = content[currIndex];

            subject.text = title;
            body.text = message;
        }
        catch { }


    }
}
