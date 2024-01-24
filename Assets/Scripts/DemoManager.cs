using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DemoManager : MonoBehaviour
{
    [System.Serializable]
    public struct Trigger
    {
        public string type;
        public string response;
        public InteractionManager.Interaction interaction;
    }

    public Trigger[] triggers;
    private bool[] triggerResponses;
    private int currTrigger = -1;

    [System.Serializable]
    public enum Demo
    {
        DayInAGlance,
        BeBackSoon,
        NotificationTray,
        ExpandedNotification,
        AmbientNotifications,
        MeetingTable,
        Whiteboard,
        VideoPlayer,
        Slideshow,
        Workbench,
        Equipment,
        Relaxation,
        Streaming,
        WebBrowsing,
        Twenty,
        Games,
        None
    }
    public GameObject greyWall;
    public Demo currentDemo = Demo.None;

    [System.Serializable]
    public enum Placement
    {
        Floor,
        Ceiling,
        World
    }

    public Placement placement = Placement.World; // for state-based demos
    public bool isStateBased = false;
    public bool useAnd = true;
    public bool onlyScreen = true;
    
    // anchors for floor and ceiling placements
    private GameObject floor;
    private GameObject ceiling;

    
    public bool triggered = false; // whether the loaded demo should be visible
    private bool prevTriggered;

    private GameObject demoObj;

    bool initialized = false;

    GameObject controller;

    private void Start()
    {
        prevTriggered = triggered;
        triggerResponses = new bool[triggers.Length];
        //isStateBased = false;
        //foreach (Trigger t in triggers)
        //{
        //    if (t.type.Equals("Show"))
        //    {
        //        if (!t.interaction.Equals(InteractionManager.Interaction.None))
        //        {
        //            isStateBased = true;
        //            break;
        //        }
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {
        LoadDemo();
        if (prevTriggered != triggered)
        {
            // demo should not show
            if (!triggered)
                demoObj.SetActive(false);
            else
                demoObj.SetActive(true);

            prevTriggered = triggered;
        }

        if (floor == null)
            floor = GameObject.Find("FloorDemo");
        if (ceiling == null)
            ceiling = GameObject.Find("CeilingDemo");
        if (controller == null)
            controller = GameObject.Find("Controller");

        PlaceDemo();
        CheckTriggers();

    }

    private void CheckTriggers()
    {
        for (int i = 0; i < triggers.Length; i++)
        {
            currTrigger = i;
            triggerResponses[i] = false; // reset
            Trigger trigger = triggers[i];
            SendMessage(trigger.interaction.ToString(), trigger.response);
        }

        // check the responses
        List<string> seenTypes = new List<string>();
        for (int i = 0; i < triggerResponses.Length; i++)
        {
            Trigger baseTrigger = triggers[i];
            bool response = triggerResponses[i];

            if (seenTypes.Contains(baseTrigger.type))
                continue;

            seenTypes.Add(baseTrigger.type);

            for (int j = i; j < triggerResponses.Length; j++)
            {
                Trigger currTrigger = triggers[j];
                
                // same type, check that all responses are true
                if (currTrigger.type.Equals(baseTrigger.type))
                {
                    // use logical AND
                    if (useAnd)
                        response = (response && triggerResponses[j]);
                    
                    // use logical OR
                    else
                        response = (response || triggerResponses[j]);
                }
            }

            // if all trigger conditions have been met, can actually invoke the corresponding command'
            if (response)
                Invoke(baseTrigger.type, 0);
        }
        
    }

    /* CAN ADD TO THIS LIST OF INTERACTIONS - 6 BY DEFAULT */
    
    public void ShowResponse(bool response)
    {
        if (!response || triggered || !isStateBased)
        //if (!response)
            triggerResponses[currTrigger] = false;
        
        else
            triggerResponses[currTrigger] = true;
    }

    // Trigger the demo to show
    private void Show()
    {
        triggered = true;
        //if (currentDemo.Equals(Demo.Twenty))
        //{
        //    InteractionManager im = gameObject.GetComponent<InteractionManager>();
        //    im.timeShown = Time.time;
        //}
    }

    // Un-trigger the demo
    public void HideResponse(bool response)
    {
        if (!response || !triggered || !isStateBased)
        //if (!response)
            triggerResponses[currTrigger] = false;

        else
            triggerResponses[currTrigger] = true;
    }

    private void Hide()
    {
        if (!currentDemo.Equals(Demo.Twenty))
            triggered = false;
        else {
            gameObject.transform.position = greyWall.transform.position;
            gameObject.transform.rotation = greyWall.transform.rotation;
            gameObject.transform.localScale = greyWall.transform.localScale;
            WebBrowsing wb = gameObject.transform.Find("Twenty").GetComponent<WebBrowsing>();
            wb.zoomAmount = 440;
        }
    }

    // the default response for demo triggers   
    public void DefaultResponse(bool response)
    {
        //if (!response || !triggered)
        if (!response)
            triggerResponses[currTrigger] = false;

        else
            triggerResponses[currTrigger] = true;
       
    }

    // "Start" the demo (useful for things like playing videos)
    private void Play()
    {
        try
        {
            IDemo demo = demoObj.GetComponent<IDemo>();
            demo.TriggerPlay();
        }
        catch (System.NullReferenceException e)
        {
            // no interactions associated with that demo
        }
    }

   
    // "Stop" the demo (useful for things like pausing videos)
    private void Pause()
    {
        try
        {
            IDemo demo = demoObj.GetComponent<IDemo>();
            demo.TriggerPause();
        }
        catch (System.NullReferenceException e)
        {
            // no interactions associated with that demo
        }
    }

    // Move to the next screen (useful for moving slides forward)
    private void Next()
    {
        try
        {
            IDemo demo = demoObj.GetComponent<IDemo>();
            demo.TriggerNext();
        }
        catch (System.NullReferenceException e)
        {
            // no interactions associated with that demo
        }
    }

    // Move to the previous screen (useful for moving slides back)
    private void Back()
    {
        try
        {
            IDemo demo = demoObj.GetComponent<IDemo>();
            demo.TriggerBack();
        }
        catch (System.NullReferenceException e)
        {
            // no interactions associated with that demo
        }
    }


    // Load demos and place them in the scene
    private void LoadDemo()
    {
        if (initialized)
            return;

        // Load the right demo
        if (currentDemo != Demo.None)
        {
            string demoName = currentDemo.ToString();
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject currChild = gameObject.transform.GetChild(i).gameObject;
                if (currChild.name.Equals(demoName))
                {
                    if (triggered)
                        currChild.SetActive(true);
                    demoObj = currChild;
                    gameObject.name = demoName;
                }

                // disable the ROI view
                else if (currChild.name.Equals("DefaultROI"))
                {
                    currChild.SetActive(false);
                }
            }
        }

        // No demo - disable the demo manager
        else
        {
            gameObject.SetActive(false);
        }

        initialized = true;
    }


    private void PlaceDemo()
    {
        if (placement == Placement.Floor)
        {

            Vector3 cPos = controller.transform.position;
            Vector3 offset = new Vector3(0.072f, -0.1695f, 1.383f);
            gameObject.transform.position = cPos + offset;
            
            
            //gameObject.transform.LookAt(Camera.main.transform);
            //gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.eulerAngles.x + 90, gameObject.transform.eulerAngles.y, gameObject.transform.eulerAngles.z);



            Vector3 cRot = controller.transform.eulerAngles;
            Vector3 rOffset = new Vector3(30.45f, 190.198f, -1.697f);
            gameObject.transform.eulerAngles = cRot + rOffset;

            //gameObject.transform.position = floor.transform.position;

            //Vector3 middle = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0.5f));
            //gameObject.transform.position = middle;

            //gameObject.transform.LookAt(Camera.main.transform);
            //gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.eulerAngles.x + 90, gameObject.transform.eulerAngles.y, gameObject.transform.eulerAngles.z);
            //gameObject.transform.rotation = Quaternion.Euler(floor.transform.eulerAngles.x, floor.transform.eulerAngles.y + 180f, floor.transform.eulerAngles.z);


            //float scale = 0.3f;
            //gameObject.transform.localScale = new Vector3(scale, scale, scale);


        }

        else if (placement == Placement.Ceiling)
        {
            Vector3 cPos = controller.transform.position;
            Vector3 offset = new Vector3(0.045093f, 1.794488f, 1.391527f);
            gameObject.transform.position = cPos + offset;


            gameObject.transform.LookAt(Camera.main.transform);
            gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.eulerAngles.x + 90, gameObject.transform.eulerAngles.y, gameObject.transform.eulerAngles.z);



            //Vector3 cRot = controller.transform.eulerAngles;
            //Vector3 rOffset = new Vector3(13.184f, 25.215f, -175.246f);
            //gameObject.transform.eulerAngles = cRot + rOffset;




            //gameObject.transform.position = ceiling.transform.position;
            //gameObject.transform.LookAt(Camera.main.transform);
            //gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.eulerAngles.x + 90, gameObject.transform.eulerAngles.y, gameObject.transform.eulerAngles.z);
            ////gameObject.transform.rotation = Quaternion.Euler(ceiling.transform.eulerAngles.x + 180f, ceiling.transform.eulerAngles.y + 180f, ceiling.transform.eulerAngles.z);


            //float scale = 0.3f;
            //gameObject.transform.localScale = new Vector3(scale, scale, scale);
        }

    }



}
