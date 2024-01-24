using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TouchInput : MonoBehaviour
{
    Dictionary<int, Sensor> innerTouchSensors;
    Dictionary<int, Sensor> outerTouchSensors;
    private bool initialized = false;
    private bool prevOuterActive = false;    
    private bool prevInnerActive = false;
    public string lastInteraction = null;

    [System.Serializable]
    public enum Armrest
    {
        Right,
        Left
    }

    // the mapping varies for each armrest
    public Armrest currArmrest;

    public enum Direction
    {
        Up,
        Down,
        None
    }

    public enum Location
    {
        Outer,
        Inner
    }

    // Start is called before the first frame update
    void Start()
    {
        innerTouchSensors = new Dictionary<int, Sensor>();
        outerTouchSensors = new Dictionary<int, Sensor>();
    }

    // Update is called once per frame
    void Update()
    {
        Initialize();
        var innerOrdered = innerTouchSensors.OrderBy(x => x.Value.timestamp);
        var outOrdered = outerTouchSensors.OrderBy(x => x.Value.timestamp);
        Direction innerDirection = UpdateDirection(innerOrdered, innerTouchSensors, Location.Inner);
        Direction outerDirection = UpdateDirection(outOrdered, outerTouchSensors, Location.Outer);

        // Assign an interaction
        if (innerDirection != Direction.None || outerDirection != Direction.None)
        {
            print(innerDirection + ", " + outerDirection);
            
            // both were selected
            if (innerDirection == Direction.Down && outerDirection == Direction.Down)
                lastInteraction = currArmrest + "_Both_Down";
            else if (innerDirection == Direction.Up && outerDirection == Direction.Up)
                lastInteraction = currArmrest + "_Both_Up";

            // inner armrest
            else if (innerDirection == Direction.Down)
                lastInteraction = currArmrest + "_Inner_Down";
            else if (innerDirection == Direction.Up)
                lastInteraction = currArmrest + "_Inner_Up";

            // outer armrest
            else if (outerDirection == Direction.Down)
                lastInteraction = currArmrest + "_Outer_Down";
            else if (outerDirection == Direction.Up)
                lastInteraction = currArmrest + "_Outer_Up";
        }

    }

    private Direction UpdateDirection(IOrderedEnumerable<KeyValuePair<int, Sensor>> orderedList, Dictionary<int, Sensor> sensors, Location location)
    {
        Direction result = Direction.None;

        bool isActive = AnyActive(sensors);
        bool prevActive;
        if (location == Location.Outer)
        {
            prevActive = prevOuterActive;
        }

        else
        {
            prevActive = prevInnerActive;
        }

        // Only print a new direction
        if (isActive != prevActive)
        {
            // interaction just happened
            if (!isActive && prevActive)
            {
                result = GetDirection(orderedList, location);
                ResetChildren(sensors);
            }

            if (location == Location.Outer)
            {
                prevOuterActive = isActive;
            }

            else
            {
                prevInnerActive = isActive;
            }

        }

        return result;
    }

    private void Initialize()
    {
        if (!initialized)
        {
            
                for (int i = 1; i <= 12; i++)
                {
                    Sensor sensor = gameObject.transform.Find(i.ToString()).GetComponent<Sensor>();

                    // Setup for the right armrest
                    if (currArmrest == Armrest.Right)
                    {
                        // outer
                        if (i <= 6)
                        {
                            innerTouchSensors.Add(i, sensor);
                        }

                        // inner
                        else
                        {
                            outerTouchSensors.Add(i, sensor);
                        }
                    }

                    else
                    {
                        // inner
                        if (i <= 6)
                        {
                            outerTouchSensors.Add(i, sensor);
                        }

                        // outer
                        else
                        {
                            innerTouchSensors.Add(i, sensor);
                        }   
                }
                }

            initialized = true;
        }
    }

    private Direction GetDirection(IOrderedEnumerable<KeyValuePair<int, Sensor>> orderedList, Location location)
    {
        Direction direction = Direction.None;
        
        // gameobject num increases as it moves up (towards the top)
        int prevSensor = 0;
        foreach (KeyValuePair<int, Sensor> pair in orderedList)
        {
            int sensorNum = pair.Key;
            Sensor sensor = pair.Value;

            // exclude the first contact point
            if (prevSensor > 0)
            {
                // up/down have different definitions, based on the armrest
                // and whether it's the outer or inner sensors
                if (prevSensor > sensorNum)
                {
                    if (currArmrest == Armrest.Right)
                    {
                        if (location == Location.Inner)
                            direction = Direction.Down;
                        else
                            direction = Direction.Up;
                    }

                    else
                    {
                        if (location == Location.Inner)
                            direction = Direction.Up;
                        else
                            direction = Direction.Down;
                    }
                    
                }


                else if (prevSensor < sensorNum)
                {
                    if (currArmrest == Armrest.Right)
                    {
                        if (location == Location.Inner)
                            direction = Direction.Up;
                        else
                            direction = Direction.Down;
                    }

                    else
                    {
                        if (location == Location.Inner)
                            direction = Direction.Down;
                        else
                            direction = Direction.Up;
                    }
                    
                }

            }

            //else
            //{
            //    print("Tap");
            //}

            prevSensor = sensorNum;
        }

        return direction;
    }

    // Check if any children are active
    private bool AnyActive(Dictionary<int, Sensor> sensors)
    {
        foreach (Sensor s in sensors.Values)
        {
            if (s.isActive)
            {
                return true;
            }
        }

        return false;
    }

    // Reset all children
    private void ResetChildren(Dictionary<int, Sensor> sensors)
    {
        foreach (Sensor s in sensors.Values)
        {
            s.Reset();
        }
    }



}
