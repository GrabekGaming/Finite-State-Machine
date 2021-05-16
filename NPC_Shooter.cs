using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Shooter : MonoBehaviour
{
    private NavMeshAgent navAgent;
    [SerializeField]
    private NPCMode npcMode = NPCMode.wander;

    [SerializeField]
    private float chaseRadius = 30f;
    public bool inchaserange;

    [SerializeField]
    private float attackRadius = 15f;
    public bool inattackrange;

    public List<Transform> moveSpots;
    int currPatrolIndex;
    bool patrolForward = true;

    float dis;
    public bool offGizmo = false;

    public Transform target;

    public Renderer colorEnemy;

    float timer;
    bool timeReached = false;

    // Shooting npcs

    bool alreadyAttacked;
    public float timeBetweenAttacks;
    public GameObject bullets;
    public Transform attackpoint;

    //probability 
    public float porbabilitySwitcher = 0.1f;


    void Start()
    {

        navAgent = this.GetComponent<NavMeshAgent>();
        
            if (moveSpots != null && moveSpots.Count >= 2)
            {
                currPatrolIndex = 0;
                SetDestination();
            }

    }

    void Update()
    {
        dis = Vector3.Distance(target.position, transform.position);
        inattackrange = Physics.CheckSphere(transform.position, attackRadius);

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
                if (dis < attackRadius)
                    npcMode = NPCMode.attack;
                if (dis > chaseRadius)
                    npcMode = NPCMode.idle;
                break;

            case NPCMode.attack:

                AttackPlayer();
                if(dis > chaseRadius)
                    npcMode = NPCMode.idle;
                if (dis > attackRadius)
                    npcMode = NPCMode.chase;
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
    void AttackPlayer()
    {
        navAgent.SetDestination(transform.position);

        transform.LookAt(target);

        if(!alreadyAttacked)
        {
            Rigidbody rb = Instantiate(bullets, attackpoint.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 20f, ForceMode.Impulse);
            rb.AddForce(transform.up * 2f, ForceMode.Impulse);

           GameObject raw = rb.gameObject;

           Destroy(raw, 1f);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);

        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    void FindTarget()
    {
        navAgent.SetDestination(target.position);
        navAgent.speed = 5;
    }

    private void DoWander()
    {
        if (navAgent.remainingDistance < 2.0f)
        { 
            ChangePatrolPoint();
            SetDestination();
        }
    }

    private void ChangePatrolPoint()
    {

        if (patrolForward)
        {
            currPatrolIndex = (currPatrolIndex + 1) % moveSpots.Count;
        }
        else
        {
            if (--currPatrolIndex < 0)
            {
                currPatrolIndex = moveSpots.Count - 1;
            }
        }
    }

    private void SetDestination()
    {
        if (moveSpots != null)
        {
            Vector3 targetVec = moveSpots[currPatrolIndex].transform.position;
            navAgent.SetDestination(targetVec);
        }
    }

    public enum NPCMode
    {
        wander,
        chase,
        idle,
        attack
    };

    private void OnDrawGizmos()
    {
        if (offGizmo)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }
    }
}
