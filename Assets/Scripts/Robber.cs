using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Robber : MonoBehaviour
{

    NavMeshAgent agent;
    
    
    Movement cop;

    bool coolDown;

    float copSpeed;
    Vector3 copPos;

    public float beheviourCoolDown = 5.0f;
    public float wanderRadius = 10.0f,
        wanderDistance = 20.0f,
        wanderJitter = 1.0f;

    Vector3 wanderTarget = Vector3.zero;



    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        cop = FindObjectOfType<Movement>();
        coolDown = false;
    }

    void Seek(Vector3 location)
    {

        agent.SetDestination(location);

    }

    void Flee(Vector3 location)
    {
        //gets the movement vector between agents and sets the destination in opposite direction
        Vector3 fleeVector = location - this.transform.position;
        agent.SetDestination(this.transform.position - fleeVector);
    }

    void Persue()
    {
        Vector3 targetDIR = cop.transform.position - this.transform.position;

        //to calculate the relative angle between agents
        float relativeHeading = Vector3.Angle(this.transform.forward, this.transform.TransformVector(cop.transform.forward));
        //forward dir of the agent and the target
        float toTarget = Vector3.Angle(this.transform.forward, this.transform.TransformVector(targetDIR));


        //this will stop the lookAHead calculation if certain conditions are met
        if ( relativeHeading >90 && toTarget<20 ||copSpeed < 0.01f)
        {

            Seek(copPos);
            return;
        }

        float lookAHead = targetDIR.magnitude / (agent.speed + copSpeed);
        Seek(copPos + cop.transform.forward * lookAHead);

    }

    void Evade()
    {
        Vector3 targetDIR = cop.transform.position - this.transform.position;

        float lookAHead = targetDIR.magnitude / (agent.speed + copSpeed);
        Flee(copPos + cop.transform.forward * lookAHead);
    }




    void Wander()
    {
        wanderTarget += new Vector3(Random.Range(-1.0f, 1.0f) * wanderJitter,
            0, Random.Range(-1.0f, 1.0f) * wanderJitter);

        //to normalize the value we got
        wanderTarget.Normalize();

        wanderTarget *= wanderRadius;

        //give a offset to wandertarget and use it in the world
        Vector3 targetLocal = wanderTarget + new Vector3(0.0f, 0.0f, wanderDistance);
        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);

        Seek(targetWorld);
    }

    void Hide()
    {
        //to distance large enough
        float dist = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenHide = World.Instance.GetHidingSpots()[0];

        //to find the closest hiding spot
        for(int i= 0; i<World.Instance.GetHidingSpots().Length; i++)
        {
            //vector from cop to the hiding spot
            Vector3 hideDir = World.Instance.GetHidingSpots()[i].transform.position - copPos;
            Vector3 hidePos = World.Instance.GetHidingSpots()[i].transform.position + hideDir.normalized * 100;

            if(Vector3.Distance(this.transform.position,hidePos)<dist)
            {
                chosenSpot = hidePos;
                chosenDir = hideDir;
                chosenHide = World.Instance.GetHidingSpots()[i];
                dist = Vector3.Distance(this.transform.position, hidePos);
            }
        }
        //to find a suitable hiding spot in any object
        Collider hideCol = chosenHide.GetComponent<Collider>();
        Ray backRay = new Ray(chosenSpot, -chosenDir.normalized);
        RaycastHit hit;
        //must be longer than the push distance in hidepos
        float distance = 250.0f;
        hideCol.Raycast(backRay, out hit, distance);

        Seek(hit.point + chosenDir.normalized *2);
    }

    bool CanSeeCop()
    {
        RaycastHit copHit;
        Vector3 rayToCop = copPos - this.transform.position;
        if (Physics.Raycast(this.transform.position, rayToCop,out copHit))
        {
            if(copHit.transform.gameObject.tag == "cop" )
            {
                return true;
            }
        }
        return false;
    }

    bool CopCanSeeMe()
    {
        Vector3 toRobber = this.transform.position - copPos;
        float lookingAngle = Vector3.Angle(cop.transform.forward, toRobber);

        if(lookingAngle<60)
        {
            return true;
        }

        return false;
    }

    void BehvCoolDown()
    {
        coolDown = false;
    }

    bool CopIsNear()
    {
        if (Vector3.Distance(this.transform.position, copPos) < 10)
        { return true; }

        return false;
    }
    // Update is called once per frame
    void Update()
    {

        copSpeed = cop.GetComponent<Movement>().currentSpeed;
        copPos = cop.transform.position;
        if (!coolDown)
        {
            if (CopCanSeeMe()&& CanSeeCop() && !CopIsNear())
            {
                Hide();
                coolDown = true;
                Invoke("BehvCoolDown", beheviourCoolDown);
            }
            else if(CopIsNear())
            {
                Evade();
            }
            else { Wander(); }
        }
        
    }



}
