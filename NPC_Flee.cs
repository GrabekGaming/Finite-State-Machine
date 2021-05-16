using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Flee : MonoBehaviour
{
    private NavMeshAgent navAgent;
    [SerializeField]
    private NPCMode npcMode = NPCMode.idle;

    [SerializeField]
    public float fleeRadius = 25;

    public List<Transform> moveSpots;

    public Transform target;


    public bool offGizmo = false;

    public Renderer fleeNPC;

    void Start()
    {
        navAgent = this.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, target.transform.position);// distance to the target(Flee)
        switch (npcMode)
        {

            case NPCMode.flee:
                fleeNPC.material.color = Color.cyan;
                Flee();
                navAgent.speed = 5;
                if (dist > fleeRadius)
                {
                    npcMode = NPCMode.idle;
                }
                break;

            case NPCMode.idle:
                fleeNPC.material.color = Color.magenta;
                navAgent.speed = 0;
                if (dist < fleeRadius)
                {
                    npcMode = NPCMode.flee;
                }
                break;

            default:
                break;
        }
    }


    void Flee()
    {
        Vector3 disToTarget = transform.position - target.transform.position;

        Vector3 newPos = transform.position + disToTarget;

        navAgent.SetDestination(newPos);
    }

    public enum NPCMode
    {
        flee,
        idle
    };

    private void OnDrawGizmos()
    {
        if (offGizmo)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, fleeRadius);
        }
    }
}
