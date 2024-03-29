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
        //visual effects for target
        target.DrawConcentricCircle(targetRadiusL);
        //distance check
        if(distance < slowRadiusL && distance > targetRadiusL)
        {
            //here is the condition we need to think about reduce speed
            float temp = distance - targetRadiusL;
            float targetSpeed = maxSpeed * (temp / (slowRadiusL - targetRadiusL));
            Vector3 targetVelocity = direction.normalized * targetSpeed;
            Vector3 linear = targetVelocity - agent.velocity;
            linear /= timeToTarget;
            /*
             * Note: since linear /= timeToTarget will get the acceleration
             * we need to perform per time, we need to devide the linear with
             * another deltaTime in order to make sure the enough acceleration
             * is applied to the gameobject
             */
            linear /= Time.deltaTime;
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
            agent.DrawCircle(futureAccleration + agent.position , 1.0f);
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
        agent.DrawCircle(futureAccleration + agent.position, 1.0f);

        return futureAccleration;
    }

    public float DynamicFace()
    {
        Vector3 direciton = target.position - agent.position;
        float faceRot, faceRotDir, faceRotMag;
        
        //get the targetOrientation
        float targetOrientation = Mathf.Atan2(direciton.x, direciton.z);
        targetOrientation = MapOrientation(targetOrientation);

        //make sure we will only rotate less then pi, also calculate the
        //direction of the rotation
        faceRot = targetOrientation - MapOrientation(agent.orientation);
        if(faceRot > Mathf.PI)
        {
            //if the faceRot is bigger than pi, reverse that rotation
            faceRot = (2 * Mathf.PI) - faceRot;
        }
        faceRotMag = Mathf.Abs(faceRot);
        faceRotDir = faceRot / faceRotMag;
        

        //then check should we accelerate or arrive
        if (faceRotMag > slowRadiusA)
        {
            //here is the condition for accelerate
            if (agent.rotation < maxRotation)
            {
                if (faceRotMag > maxAngularAcceleration)
                {
                    return (maxAngularAcceleration * faceRotDir);
                }
                else
                {
                    return faceRot;
                }
            }
            else
            {
                return 0.0f;
            }
        }
        else if(faceRotMag <= slowRadiusA)
        {
            float targetRot = faceRotMag / slowRadiusA;
            targetRot *= faceRotDir;
            float targetAngAcc = targetRot - faceRot;
            float result = targetAngAcc / timeToTarget;
            result /= Time.deltaTime;
            return result;
        }
             
        return 0.0f;
     
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

        //get direction to player
        steering = target.position - agent.position;
        agent.DrawCircle(steering, 1.0f);

        //calculate distance
        float distance = (target.position - agent.position).magnitude;

        //if inside slowRadius, calculate speed using arrive
        if (distance < slowRadiusL)
        {
            Vector3 apporachingAcceleration = DynamicArrive();
            return apporachingAcceleration;
        }
        //else keep max acceleration
        else {
            steering.Normalize();
            steering *= maxAcceleration;
        }
        agent.DrawCircle(steering + agent.position, 1.0f);

        return steering;
    }

    public Vector3 Flee()
    {

        Vector3 steering = new Vector3();

        //calculate direction away from player and send player away at max acceleration
        steering = agent.position - target.position;
        steering.Normalize();
        steering *= maxAcceleration;
        agent.DrawCircle(steering + agent.position, 1.0f);
        return steering;
    }

    public float Align(Vector3 targetVec)
    {
        //get direction to target
        Vector3 direction = targetVec - agent.position;
        float rotation = Mathf.Atan2(direction.x, direction.z) - agent.orientation;
        float targetRotation, steering, rotationDirection;

        //map result to (-pi, pi) interval
        rotation = MapOrientation(rotation);
        float rotationSize = Mathf.Abs(rotation);

        //check if we are there
        if(rotationSize < targetRadiusA) { return 0f; }

        //if oiutside slow radius use max rotation
        if(rotationSize > slowRadiusA) { targetRotation = maxRotation; }

        //otherwise use scaled rotation
        else { targetRotation = maxRotation * (rotationSize / slowRadiusL); }

        //final target rotation use speed and direction
        targetRotation *= rotation / rotationSize;

        //acceleration tries to get to the target rotation
        steering = targetRotation - agent.rotation;

        //check if acceleration is too high
        float angularAccel = Mathf.Abs(steering);
        if(angularAccel > maxAngularAcceleration)
        {
            steering /= angularAccel;
            steering *= maxAngularAcceleration;
        }
        //print("Steering is: " + steering);
        agent.DrawFaceCircle(1.0f, 5.0f);
        return steering;
    }

    public wanderSteering Wander()
    {
        //update orientation
        wanderOrientation += (Random.value - Random.value) * wanderRate;

        //get target orientation
        float targetOr = agent.orientation + wanderOrientation;

        //get center of wander cirlce
        Vector3 position = agent.position + wanderOffset * new Vector3(Mathf.Sin(agent.orientation), 0, Mathf.Cos(agent.orientation));

        //get target locations
        position += wanderRadius * new Vector3(Mathf.Sin(targetOr), 0, Mathf.Cos(targetOr));

        //create struct to hold output
        wanderSteering ret;

        //delegate to face
        ret.angular = Align(target.position);

        if ((target.position - agent.position).magnitude < slowRadiusL)
        {
            ret.linear = DynamicArrive();
            return ret;
        }
        //else keep max acceleration
        else
        {
            ret.linear = maxAcceleration * new Vector3(Mathf.Sin(agent.orientation), 0, Mathf.Cos(agent.orientation));
            return ret;
        }
    }


}
