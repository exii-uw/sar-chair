using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [System.Serializable]
    public enum Interaction
    {
        None,

        // specific postures (FSRs)
        Standing,
        Sitting,
        SittingUpright,
        LeaningBackward,
        LeaningBackwardToggle,
        NotLeaningBackward,
        LeaningForward,
        NotLeaningForward,
        LeaningLeft,
        LeaningRight,

        // specific regions (FSRs)
        UsingUpperBack,
        UsingLowerBack,
        UsingRightArm,
        UsingLeftArm,

        // touch events (right)
        TouchRightOuterUp,
        TouchRightOuterDown,
        TouchRightInnerUp,
        TouchRightInnerDown,
        TouchRightBothUp,
        TouchRightBothDown,

        // touch events (left)
        TouchLeftOuterUp,
        TouchLeftOuterDown,
        TouchLeftInnerUp,
        TouchLeftInnerDown,
        TouchLeftBothUp,
        TouchLeftBothDown,


        // Movement
        BehindTrigger,
        AheadTrigger,

        // The chair is facing the general direction of the demo
        InView,
        NotInView,

        // The chair is facing another object
        OtherInView,
        OtherNotInView,

        // Use time
        TimePassed

    }

    GameObject chair;
    public GameObject chairPositionTrigger;
    public GameObject otherGameObj;
    public float period = 10;
    public float timeShown = 0;
    PostureDetection postureDetection;
    TouchInput leftArmTouch;
    TouchInput rightArmTouch;
    RaycastEnv raycastEnv;
    bool prevTilt = false;

    void Update()
    {
        if (chair == null)
        {
            chair = GameObject.Find("Chair-Mapped");
            chairPositionTrigger.name = GetComponent<DemoManager>().currentDemo.ToString() + "_Trigger";
            postureDetection = chair.GetComponent<PostureDetection>();
            leftArmTouch = GameObject.Find("LeftArm").transform.Find("TouchInput").GetComponent<TouchInput>();
            rightArmTouch = GameObject.Find("RightArm").transform.Find("TouchInput").GetComponent<TouchInput>();
            raycastEnv = chair.GetComponent<RaycastEnv>();
        }
    }



    // Check the chair and respond to the DemoManager
    public void None() { }

    public void Standing(string triggerType)
    {
        SendMessage(triggerType + "Response", postureDetection.IsStanding());
    }

    public void Sitting(string triggerType)
    {
        SendMessage(triggerType + "Response", postureDetection.IsSitting());
    }

    public void SittingUpright(string triggerType)
    {
        SendMessage(triggerType + "Response", postureDetection.IsSittingUpright());
    }

    public void LeaningBackward(string triggerType)
    {
        SendMessage(triggerType + "Response", postureDetection.IsLeaningBack());
    }

    public void LeaningBackwardToggle(string triggerType)
    {
        bool isLeaning = postureDetection.IsUsingTilt();
        
        // was leaning and is now in a neutral position
        if (isLeaning != prevTilt && !isLeaning)
        {
            SendMessage(triggerType + "Response", true);
        }
        
        else
        {
            SendMessage(triggerType + "Response", false);
        }
        prevTilt = isLeaning;
    }

    public void NotLeaningBackward(string triggerType)
    {
        SendMessage(triggerType + "Response", !postureDetection.IsLeaningBack());
    }

    public void LeaningForward(string triggerType)
    {
        SendMessage(triggerType + "Response", postureDetection.IsLeaningForward());
    }

    public void NotLeaningForward(string triggerType)
    {
        SendMessage(triggerType + "Response", !postureDetection.IsLeaningForward());
    }

    public void LeaningLeft(string triggerType)
    {
        
        SendMessage(triggerType + "Response", postureDetection.IsLeaningLeft());
    }

    public void LeaningRight(string triggerType)
    {
        SendMessage(triggerType + "Response", postureDetection.IsLeaningRight());
    }

    public void UsingUpperBack(string triggerType)
    {
        SendMessage(triggerType + "Response", postureDetection.IsUsingUpperBack());
    }

    public void UsingLowerBack(string triggerType)
    {
        SendMessage(triggerType + "Response", postureDetection.IsUsingLowerBack());
    }

    public void UsingRightArm(string triggerType)
    {
        SendMessage(triggerType + "Response", postureDetection.IsUsingRightArmrest());
    }

    public void UsingLeftArm(string triggerType)
    {
        SendMessage(triggerType + "Response", postureDetection.IsUsingLeftArmrest());
    }

    // Right touch input
    public void TouchRightOuterUp(string triggerType) {
        string lastInteraction = rightArmTouch.lastInteraction;
        if (lastInteraction != null)
        {
            bool matchingCommand = lastInteraction.Equals("Right_Outer_Up");
            if (matchingCommand)
            {
                SendMessage(triggerType + "Response", matchingCommand);
                rightArmTouch.lastInteraction = null;
            }
        }
    }

    public void TouchRightOuterDown(string triggerType) {
        string lastInteraction = rightArmTouch.lastInteraction;
        if (lastInteraction != null)
        {
            bool matchingCommand = lastInteraction.Equals("Right_Outer_Down");
            if (matchingCommand)
            {
                SendMessage(triggerType + "Response", matchingCommand);
                rightArmTouch.lastInteraction = null;
            }
        }
    }

    public void TouchRightInnerUp(string triggerType) {
        string lastInteraction = rightArmTouch.lastInteraction;
        if (lastInteraction != null)
        {
            bool matchingCommand = lastInteraction.Equals("Right_Inner_Up");
            if (matchingCommand)
            {
                SendMessage(triggerType + "Response", matchingCommand);
                rightArmTouch.lastInteraction = null;
            }
        }
    }

    public void TouchRightInnerDown(string triggerType) {
        string lastInteraction = rightArmTouch.lastInteraction;
        if (lastInteraction != null)
        {
            bool matchingCommand = lastInteraction.Equals("Right_Inner_Down");
            if (matchingCommand)
            {
                SendMessage(triggerType + "Response", matchingCommand);
                rightArmTouch.lastInteraction = null;
            }
        }
    }

    public void TouchRightBothUp(string triggerType)
    {
        string lastInteraction = rightArmTouch.lastInteraction;
        if (lastInteraction != null)
        {
            bool matchingCommand = lastInteraction.Equals("Right_Both_Up");
            if (matchingCommand)
            {
                SendMessage(triggerType + "Response", matchingCommand);
                rightArmTouch.lastInteraction = null;
            }
        }
    }

    public void TouchRightBothDown(string triggerType)
    {
        string lastInteraction = rightArmTouch.lastInteraction;
        if (lastInteraction != null)
        {
            bool matchingCommand = lastInteraction.Equals("Right_Both_Down");
            if (matchingCommand)
            {
                SendMessage(triggerType + "Response", matchingCommand);
                rightArmTouch.lastInteraction = null;
            }
        }
    }

    // Left touch input
    public void TouchLeftOuterUp(string triggerType) {
        string lastInteraction = leftArmTouch.lastInteraction;
        if (lastInteraction != null)
        {
            bool matchingCommand = lastInteraction.Equals("Left_Outer_Up");
            if (matchingCommand)
            {
                SendMessage(triggerType + "Response", matchingCommand);
                leftArmTouch.lastInteraction = null;
            }
        }
    }

    public void TouchLeftOuterDown(string triggerType)
    {
        string lastInteraction = leftArmTouch.lastInteraction;
        if (lastInteraction != null)
        {
            bool matchingCommand = lastInteraction.Equals("Left_Outer_Down");
            if (matchingCommand)
            {
                SendMessage(triggerType + "Response", matchingCommand);
                leftArmTouch.lastInteraction = null;
            }
        }
    }

    public void TouchLeftInnerUp(string triggerType)
    {
        string lastInteraction = leftArmTouch.lastInteraction;
        if (lastInteraction != null)
        {
            bool matchingCommand = lastInteraction.Equals("Left_Inner_Up");
            if (matchingCommand)
            {
                SendMessage(triggerType + "Response", matchingCommand);
                leftArmTouch.lastInteraction = null;
            }
        }
    }
 
    public void TouchLeftInnerDown(string triggerType)
    {
        string lastInteraction = leftArmTouch.lastInteraction;
        if (lastInteraction != null)
        {
            bool matchingCommand = lastInteraction.Equals("Left_Inner_Down");
            if (matchingCommand)
            {
                SendMessage(triggerType + "Response", matchingCommand);
                leftArmTouch.lastInteraction = null;
            }
        }
    }

    public void TouchLeftBothUp(string triggerType) {
        string lastInteraction = leftArmTouch.lastInteraction;
        if (lastInteraction != null)
        {
            bool matchingCommand = lastInteraction.Equals("Left_Both_Up");
            if (matchingCommand)
            {
                SendMessage(triggerType + "Response", matchingCommand);
                leftArmTouch.lastInteraction = null;
            }
        }
    }

    public void TouchLeftBothDown(string triggerType) {
        string lastInteraction = leftArmTouch.lastInteraction;
        if (lastInteraction != null)
        {
            bool matchingCommand = lastInteraction.Equals("Left_Both_Down");
            if (matchingCommand)
            {
                SendMessage(triggerType + "Response", matchingCommand);
                leftArmTouch.lastInteraction = null;
            }
        }
    }

    // Movement
    public void BehindTrigger(string triggerType)
    {
        SendMessage(triggerType + "Response", CheckTriggerDistance().Equals("Behind"));
    }

    public void AheadTrigger(string triggerType)
    {
        SendMessage(triggerType + "Response", CheckTriggerDistance().Equals("Ahead"));
    }

    // In View
    public void InView(string triggerType)
    {
        SendMessage(triggerType + "Response", CheckDemoInView(gameObject));
    }

    public void NotInView(string triggerType)
    {
        SendMessage(triggerType + "Response", !CheckDemoInView(gameObject));
    }

    public void OtherInView(string triggerType)
    {
        SendMessage(triggerType + "Response", CheckDemoInView(otherGameObj));
    }

    public void OtherNotInView(string triggerType)
    {
        SendMessage(triggerType + "Response", !CheckDemoInView(otherGameObj));
    }

    private string CheckTriggerDistance()
    {
        Vector3 forward = chair.transform.forward;
        Vector3 toOther = chairPositionTrigger.transform.position - chair.transform.position;
        float dot = Vector3.Dot(forward, toOther);

        if (Mathf.Abs(dot) < 0.1f)
        {
            return "Equal";
        }
        else if (dot < 0)
        {
            return "Ahead";
        }

        else
        {
            return "Behind";
        }
    }

    private bool CheckDemoInView(GameObject g)
    {
        // whether the demo is in the camera's view
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(g.transform.position);
        return screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

        // whether the angles are compatible
        //return !(raycastEnv.incompatibleTop || raycastEnv.incompatibleBottom);
    }

    // Time passed
    public void TimePassed(string triggerType)
    {
        if (Time.time > period)
        {
            //print("TRUE");
            SendMessage(triggerType + "Response", true);
        }
        else
            SendMessage(triggerType + "Response", false);
    }
}
