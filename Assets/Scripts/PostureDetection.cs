using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostureDetection : MonoBehaviour
{
    Arduino arduinoLoop;
    Chair chair;

    // Update is called once per frame
    void Update()
    {
        if (arduinoLoop == null)
            arduinoLoop = GameObject.Find("Controller").GetComponent<Arduino>();

        if (chair == null)
            chair = gameObject.GetComponent<Chair>();


        if (Input.GetKeyDown(KeyCode.P))
        {
            DebugPrint();
        }
        
    }

    private void DebugPrint()
    {
        // Armrests
        print("Is using left armrest " + IsUsingLeftArmrest());
        print("Is using right armrest " + IsUsingRightArmrest());

        // back
        print("Is using lower back " + IsUsingLowerBack());
        print("Is using upper back " + IsUsingUpperBack());

        // standing/sitting
        print("Is standing " + IsStanding());
        print("Is sitting " + IsSitting());

        // postures
        print("Is sitting upright " + IsSittingUpright());
        print("Is leaning back " + IsLeaningBack());
        print("Is leaning forward " + IsLeaningForward());
        print("Is leaning left " + IsLeaningLeft());
        print("Is leaning right " + IsLeaningRight());
    }

    /*************
     * Check if specific chair parts are in use
     *************/
    // Left armrest is in use
    public bool IsUsingLeftArmrest()
    {
        GameObject fsr = chair.lArmRest.transform.Find("FSR").Find("1").gameObject;
        return fsr.GetComponent<Sensor>().isActive;
    }

    // right armrest is in use
    public bool IsUsingRightArmrest()
    {
        GameObject fsr = chair.rArmRest.transform.Find("FSR").Find("1").gameObject;
        return fsr.GetComponent<Sensor>().isActive;
    }

    // is using the lower back
    public bool IsUsingLowerBack()
    {
        Transform lBackFSRs = chair.back.transform.Find("LowerBackFSRs");

        // if any seat FSRs are active, the user is sitting (not standing)
        if (AnyActive(lBackFSRs))
        {
            return true;
        }

        else
        {
            return false;
        }
    }

    // leaning back in the chair (using the back tilt)
    public bool IsUsingTilt()
    {
        if (chair.tilt <= (-90 - 8))
        {
            return true;
        }

        return false;
    }

    // is using the upper back
    public bool IsUsingUpperBack()
    {
        Transform uBackFSRs = chair.back.transform.Find("UpperBackFSRs");

        // if any seat FSRs are active, the user is sitting (not standing)
        if (AnyActive(uBackFSRs))
        {
            return true;
        }

        else
        {
            return false;
        }
    }

    // not sitting (standing)
    public bool IsStanding()
    {
        Transform seatFSRs = chair.seat.transform.Find("SeatFSRs");
       
        // if any seat FSRs are active, the user is sitting (not standing)
        if (AnyActive(seatFSRs))
        {
            return false;
        }

        else
        {
            return true;
        }
    }

    public bool IsSitting()
    {
        return !IsStanding();
    }




    
    /*************
     * "Classify" more "complex" postures
    *************/
    
    // someone is a freak and has insanely good posture (no back tilt)
    public bool IsSittingUpright()
    {
        if (IsSitting() && chair.tilt >= (-95))
        {
            return true;
        }

        return false;
    }

    // leaning back in the chair (using the back tilt)
    public bool IsLeaningBack()
    {
        if (IsSitting() && IsUsingLowerBack() && IsUsingUpperBack() && chair.tilt < (-90 - 5))
        {
            return true;
        }

        return false;
    }

    // leaning forward in the seat (no back pressure)
    public bool IsLeaningForward()
    {
        //if(IsSitting() && !IsUsingUpperBack() && !IsUsingLowerBack())
        if (IsSitting() && !IsUsingLowerBack())
        {
            return true;
        }

        return false;
    }

    // leaning to the right
    public bool IsLeaningRight()
    {
        return IsLeaning(50, 1, 2, 4);
    }

    // leaning to the left
    public bool IsLeaningLeft()
    {
        return IsLeaning(50, 3, 5, 4);
    }




    /*************
     * Helpers
     *************/

    // Check if any children are active
    private bool AnyActive(Transform baseObject)
    {
        for(int i = 1; i <= baseObject.childCount; i++)
        {
            Transform fsr = baseObject.Find(i.ToString());
            Sensor s = fsr.GetComponent<Sensor>();
            if (s.isActive)
            {
                return true;
            }
        }

        return false;
    }

    // check where the weight is
    private bool IsLeaning(float threshold, int sideFSR1, int sideFSR2, int middle)
    {
        // cannot lean if they aren't sitting
        if (!IsSitting())
            return false;

        Transform seatFSRs = chair.seat.transform.Find("SeatFSRs");

        // get the sum of raw values for each side
        // simplistic approach: if the sum of one side is greater than
        // the other, they are leaning on that side.
        float sideSum = 0;
        float otherSideSum = 0;

        for (int i = 1; i < seatFSRs.childCount + 1; i++)
        {
            // don't count the middle fsr
            if (i == middle)
                continue;

            Sensor sensor = seatFSRs.Find(i.ToString()).GetComponent<Sensor>();
            if (i == sideFSR1 || i == sideFSR2)
                sideSum += sensor.rawValue;
            else
                otherSideSum += sensor.rawValue;
        }

        // check where most of the weight is placed
        if ((otherSideSum + threshold) < sideSum)
        {
            return true;
        }

        return false;

    }
}
