using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the place to put all of the various steering behavior methods we're going
/// to be using. Probably best to put them all here, not in NPCController.
/// </summary>

public class SteeringBehavior : MonoBehaviour {

    // The agent at hand here, and whatever target it is dealing with
    public NPCController agent;
    public NPCController target;

    // Below are a bunch of variable declarations that will be used for the next few
    // assignments. Only a few of them are needed for the first assignment.

    // For pursue and evade functions
    public float maxPrediction;
    public float maxAcceleration;

    // For arrive function
    public float maxSpeed;
    public float targetRadiusL;
    public float slowRadiusL;
    public float timeToTarget;
    public float targetSpeedL;

    // For Face function
    public float maxRotation;
    public float maxAngularAcceleration;
    public float targetRadiusA;
    public float slowRadiusA;

    // For wander function
    public float wanderOffset;
    public float wanderRadius;
    public float wanderRate;
    private float wanderOrientation;

    // Holds the path to follow
    public GameObject[] Path;
    public int current = 0;

    protected void Start() {
        agent = GetComponent<NPCController>();
        //wanderOrientation = agent.orientation;
    }

    private Vector3 Arrive(Vector3 currVec)
    {
        float dis = currVec.magnitude;
        if (dis < targetRadiusL) {
            print("Inside targetRadius");
            return Vector3.zero; 
        }
        else if (dis > slowRadiusL) { targetSpeedL = maxAcceleration; }
        else {
            print("Reducing target speed")
;            targetSpeedL = (maxSpeed * dis) / slowRadiusL; 
        }

        Vector3 returnVel = new Vector3();
        returnVel = currVec;
        returnVel.Normalize();
        returnVel *= targetSpeedL;

        return returnVel;
    }

    public Vector3 Seek()
    {
        Vector3 steering = new Vector3();
        steering = target.position - agent.position;
        //print("player pos is: " + target.position);
        Vector3 targetVel = Arrive(steering);
        steering = targetVel - target.velocity;
        steering /= timeToTarget;

        if(steering.magnitude > maxAcceleration)
        {
            print("Slowing down");
            steering.Normalize();
            steering *= maxAcceleration;
        }
        return steering;
    }

    public Vector3 Flee()
    {
        Vector3 steering = new Vector3();
        steering = agent.position - target.position;
        print("player pos is: " + target.position);
        steering.Normalize();
        steering *= maxAcceleration;
        return steering;
    }

}
