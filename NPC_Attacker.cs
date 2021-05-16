using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Attacker : MonoBehaviour
{
    private NavMeshAgent navAgent;
    [SerializeField]
    private NPCMode npcMode = NPCMode.wander;

    [SerializeField]
    private float chaseRadius = 33f;


    float dis;
    public bool offGizmo = false;

    public Transform target;


    public Renderer colorEnemy;

    float timer;
    bool timeReached = false;


    //Another go!

    public LayerMask ground, player;

    // Patrol
    public Vector3 walkPoint;
    bool walkSet = false;
    public float walkPointRange;


    void Start()
    {

        navAgent = this.GetComponent<NavMeshAgent>();
       
    }

    void Update()
    {
        dis = Vector3.Distance(target.position, transform.position);

        //Finite state machine
        switch (npcMode)
        {
            case NPCMode.wander:
                timer = 0;
                navAgent.speed = 3;
                colorEnemy.material.color = Color.green;
                if (dis < chaseRadius)
                {
                    npcMode = NPCMode.chase;
                }
                else
                    DoWander();
                break;

            case NPCMode.chase:
                colorEnemy.material.color = Color.red;
                FindTarget();
                if (dis > chaseRadius)
                    npcMode = NPCMode.idle;
                break;

            case NPCMode.idle:
                navAgent.speed = 0;
                colorEnemy.material.color = Color.yellow;
                if (!timeReached)
                {
                    timer += Time.deltaTime;
                }

                if (!timeReached && timer >= 3)
                {
                    npcMode = NPCMode.wander;
                }
                break;

            default:
                break;
        }
    }


    void FindTarget()
    {
        navAgent.SetDestination(target.position);
        navAgent.speed = 4;
    }

    private void DoWander()
    {
        if(!walkSet)
        {
            SearchForPoint();
        }
        if (walkSet)
            navAgent.SetDestination(walkPoint);

        float dist = Vector3.Distance(transform.position, walkPoint);

        if (dist < 4f)
            walkSet = false;
    }

    private void SearchForPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 5f, ground))
            walkSet = true;
    }

    public enum NPCMode
    {
        wander,
        chase,
        idle
    };

    private void OnDrawGizmos()
    {
        if (offGizmo)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);
        }
    }
}
