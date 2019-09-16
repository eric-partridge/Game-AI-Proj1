﻿using System.Collections;
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
    public float maxPrediction = 3f;
    public float maxAcceleration = 10f;

    // For arrive function
    public float maxSpeed;
    public float targetRadiusL;
    public float slowRadiusL;
    public float timeToTarget;
    public float targetSpeedL;

    // For Face function
    public float maxRotation = 1f;
    public float maxAngularAcceleration = 3f; 
    public float targetRadiusA = 0.02f;
    public float slowRadiusA = 0.14f;

    // For wander function
    public float wanderOffset;
    public float wanderRadius;
    public float wanderRate;
    private float wanderOrientation;

    // Holds the path to follow
    public GameObject[] Path;
    public int current = 0;

    public struct wanderSteering
    {
        public float angular;
        public Vector3 linear;
    }

    protected void Start() {
        agent = GetComponent<NPCController>();
        //wanderOrientation = agent.orientation;
    }

    public Vector3 DynamicArrive()
    {
        //get the distance between target and agent
        Vector3 direction = GetDirectionVec();
        float distance = GetDistance();
        print("Checking");
        //distance check
        if(distance < slowRadiusL && distance > targetRadiusL)
        {
            //here is the condition we need to think about reduce speed
            float targetSpeed = maxSpeed * (distance / slowRadiusL);
            Vector3 targetVelocity = direction.normalized * targetSpeed;
            Vector3 linear = targetVelocity - agent.velocity;
            linear /= timeToTarget;
            return linear;
        }
        return new Vector3(0,0,0);
    }


    public Vector3 DynamicPursue()
    {
        //firstly, get the values we need
        float distance = GetDistance();
        float agentSpeed = agent.velocity.magnitude;
        float prediction = maxPrediction;

        //check agent should persue or approach
        if(distance >= slowRadiusL)
        {
            //check could agent get target in maxPrediction time
            if(agentSpeed > distance / maxPrediction)
            {
                prediction = distance / agentSpeed;
            }

            //get the future location
            Vector3 futureLocation = target.position + (target.velocity * prediction);

            //seek for future location
            Vector3 futureAccleration = futureLocation - agent.position;
        
            //clip to max acceleration
            if (futureAccleration.magnitude > maxAcceleration)
            {
                futureAccleration = futureAccleration.normalized * maxAcceleration;
            }

            return futureAccleration;
        }
        else if(distance < slowRadiusL)
        {
            Vector3 apporachingAcceleration = DynamicArrive();
            return apporachingAcceleration;
        }

        return new Vector3(0, 0, 0);
    }

    public Vector3 DynamicEvade()
    {
        float distance = GetDistance();
        float agentSpeed = agent.velocity.magnitude;
        float prediction = maxPrediction;

        //check could agent get target in maxPrediction time
        if (agentSpeed > distance / maxPrediction)
        {
            prediction = distance / agentSpeed;
        }

        //get the future location
        Vector3 futureLocation = target.position + (target.velocity * prediction);

        //seek for future location
        Vector3 futureAccleration = agent.position - futureLocation;

        //clip to max acceleration
        if (futureAccleration.magnitude > maxAcceleration)
        {
            futureAccleration = futureAccleration.normalized * maxAcceleration;
        }

        return futureAccleration;
    }

    public float DynamicFace()
    {
        Vector3 direciton = target.position - agent.position;
        if(direciton.magnitude <= targetRadiusA)
        {
            return 0.0f;
        }
        else
        {
            //get the targetOrientation
            float targetOrientation = Mathf.Atan2(-direciton.x, direciton.z);

            //do the calculation
            //map the orientation to interval (-pi, pi] first
            targetOrientation = MapOrientation(targetOrientation);
            float angularAcc = targetOrientation - agent.orientation;

            //check should agent approach or reduce speed
            if(Mathf.Abs(angularAcc) >= slowRadiusA)
            {
                //if this is approaching
                if(angularAcc > 0)
                {
                    if( Mathf.Abs(angularAcc) < maxRotation)
                    {
                        return angularAcc;
                    }
                    return maxRotation;
                }
                else if(angularAcc < 0){
                    if( Mathf.Abs(angularAcc) < maxRotation)
                    {
                        return angularAcc;
                    }
                    return -maxRotation;
                }
                else
                {
                    return 0f;
                }
            }
            else if (Mathf.Abs(angularAcc) < slowRadiusA && Mathf.Abs(angularAcc) > targetRadiusA)
            {
                float targetRotation = maxRotation * (angularAcc / slowRadiusA);
                angularAcc = targetRotation - angularAcc;
                //over here we need to make sure in next delta time, the rotation is
                //reduced to the wanting rotation, so we devide the angularAcc with
                //delta time
                angularAcc /= Time.deltaTime;
                return angularAcc;
                
            }

            return 0.0f;
        }
    }

    //helper functions

    //this function returns the distance between agent and target
    private float GetDistance()
    {
        float result = (target.position - agent.position).magnitude;
        return result;
    }

    //this function returns the vector from agent to target
    private Vector3 GetDirectionVec()
    {
        Vector3 result = target.position - agent.position;
        return result;
    }

    private float MapOrientation( float i )
    {
        //we will map the value to (-pi, pi] interval
        if( i <= Mathf.PI && i > -Mathf.PI)
        {
            return i;
        }else if(i > Mathf.PI)
        {
            while(i > Mathf.PI)
            {
                i -= 2 * Mathf.PI;
            }
            return i;
        }
        else
        {
            while(i <= -Mathf.PI)
            {
                i += 2 * Mathf.PI;
            }
            return i;
        }
    }

    public Vector3 Seek()
    {
        Vector3 steering = new Vector3();
        steering = target.position - agent.position;
        float distance = (target.position - agent.position).magnitude;
        if (distance < slowRadiusL)
        {
            Vector3 apporachingAcceleration = DynamicArrive();
            return apporachingAcceleration;
        }
        else {
            steering.Normalize();
            steering *= maxAcceleration;
        }

        return steering;
    }

    public Vector3 Flee()
    {
        Vector3 steering = new Vector3();
        steering = agent.position - target.position;
        steering.Normalize();
        steering *= maxAcceleration;
        return steering;
    }

    public float Align()
    {
        float rotation = target.orientation - agent.orientation;
        float targetRotation, steering, rotationDirection;
        rotation = MapOrientation(rotation);
        rotationDirection = Mathf.Atan2(target.position.x - agent.position.x, target.position.z - agent.position.z) - agent.orientation;
        float rotationSize = Mathf.Abs(rotationDirection);
        print("Rotation " + rotation + " and size: " + rotationSize);
        if(rotationSize < targetRadiusL) { print("Returning jack shit");  return 0f; }
        if(rotationSize > slowRadiusL) { targetRotation = maxRotation; }
        else { targetRotation = maxRotation * (rotationSize / slowRadiusL); }
        targetRotation *= rotation / slowRadiusL;
        steering = targetRotation - agent.rotation;

        float angularAccel = Mathf.Abs(steering);
        if(angularAccel > maxAngularAcceleration)
        {
            steering /= angularAccel;
            steering *= maxAngularAcceleration;
        }
        print("Steering is: " + steering);
        return steering;
    }

    public wanderSteering Wander()
    {
        wanderOrientation += (Random.value - Random.value) * wanderRate;
        float targetOr = agent.orientation + wanderOrientation;
        Vector3 position = agent.position + wanderOffset * new Vector3(Mathf.Sin(agent.orientation), 0, Mathf.Cos(agent.orientation));
        position += wanderRadius * new Vector3(Mathf.Sin(targetOr), 0, Mathf.Cos(targetOr));
        wanderSteering ret;
        ret.angular = DynamicFace();
        ret.linear = maxAcceleration * new Vector3(Mathf.Sin(agent.orientation), 0, Mathf.Cos(agent.orientation));
        return ret;
    }


}
