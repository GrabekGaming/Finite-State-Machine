using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_BOSS : MonoBehaviour
{
    private NavMeshAgent navAgent;
    [SerializeField]
    private NPCMode npcMode = NPCMode.wander;

    [SerializeField]
    private float chaseRadius = 33f;
    public bool inchaserange;

    [SerializeField]
    private float attackRadius = 5f;
    public bool inattackrange;

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

    public LayerMask ground;

    // Patrol
    public Vector3 walkPoint;
    public bool walkSet = false;
    public float walkPointRange;

    //random number
    public int rng;

    void Start()
    {
        InvokeRepeating("NGG", 2f, 4.0f);

        navAgent = this.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        dis = Vector3.Distance(target.position, transform.position);
        inattackrange = Physics.CheckSphere(transform.position, attackRadius);

        //Finite state machine
        switch (npcMode)
        {   
            ////////////////////////////////////////////////////////////////////////
            case NPCMode.wander:
                //------------------------------------------------------------------
                //      Specs of the state    //
                navAgent.speed = 6;
                colorEnemy.material.color = Color.green;
                //------------------------------------------------------------------
                WanderToSprint();
                if (dis < chaseRadius)
                {
                    npcMode = NPCMode.chase;
                }
                else
                    DoWander();
                break;

            ////////////////////////////////////////////////////////////////////////
            case NPCMode.sprint:
                //------------------------------------------------------------------
                //      Specs of the state    //
                navAgent.speed = 10;
                colorEnemy.material.color = Color.cyan;
                //------------------------------------------------------------------
                if (dis < chaseRadius)
                {
                    npcMode = NPCMode.chase;
                }
                else
                    DoWander();
                if (!timeReached)
                {
                    timer += Time.deltaTime;
                }

                if (!timeReached && timer > 6)
                {
                    timer = 0;
                    npcMode = NPCMode.wander;
                }
                Debug.Log("Sprint: " + timer);
                break;

            ////////////////////////////////////////////////////////////////////////
            case NPCMode.chase:
                //------------------------------------------------------------------
                //      Specs of the state    //
                navAgent.speed = 8;
                colorEnemy.material.color = Color.Lerp(Color.blue, Color.red, 0.5f);

                //------------------------------------------------------------------
                
                ChaseToFlee();
                FindTarget();
                if (dis < attackRadius)
                    npcMode = NPCMode.attack;
                if (dis > chaseRadius)
                    npcMode = NPCMode.idle;
                break;

            ////////////////////////////////////////////////////////////////////////       
            case NPCMode.flee:
                //------------------------------------------------------------------
                //      Specs of the state    //
                navAgent.speed = 7;
                colorEnemy.material.color = Color.blue;
                //------------------------------------------------------------------
                Flee();
                FleeToDie();
                if (!timeReached)
                {
                    timer += Time.deltaTime;
                }

                if (!timeReached && timer >= 6)
                {
                    timer = 0;
                    npcMode = NPCMode.idle;
                }
                break;

            ////////////////////////////////////////////////////////////////////////
            case NPCMode.attack:
                //------------------------------------------------------------------
                //      Specs of the state    //
                colorEnemy.material.color = Color.red;
                //------------------------------------------------------------------
                Debug.Log("attack: " + rng);
                AttackPlayer();
                AttackToEnrage();
                if (dis > chaseRadius)
                    npcMode = NPCMode.idle;
                if (dis > attackRadius)
                    npcMode = NPCMode.chase;
                break;

            ////////////////////////////////////////////////////////////////////////    
            case NPCMode.idle:
                //------------------------------------------------------------------
                //      Specs of the state    //
                navAgent.speed = 0;
                colorEnemy.material.color = Color.yellow;
                //------------------------------------------------------------------
                
                if (!timeReached)
                {
                    timer += Time.deltaTime;
                }

                if (!timeReached && timer >= 3)
                {
                    timer = 0;
                    npcMode = NPCMode.wander;
                }
                break;

            ////////////////////////////////////////////////////////////////////////
            case NPCMode.enrage://enragemode
                //------------------------------------------------------------------
                //      Specs of the state    //

                colorEnemy.material.color = Color.Lerp(Color.red, Color.yellow, 0.5f);//Orange
                //------------------------------------------------------------------
                Debug.Log("enrage: " + rng);
                EnrageAttack();
                if (!timeReached)
                {
                    timer += Time.deltaTime;
                }

                if (!timeReached && timer >= 4)
                {
                    timer = 0;
                    if(dis > attackRadius && dis < chaseRadius)
                        npcMode = NPCMode.chase;
                    if(dis > attackRadius && dis > chaseRadius)
                        npcMode = NPCMode.idle;
                    if(dis < attackRadius)
                        npcMode = NPCMode.attack;
                }
                break;

            ////////////////////////////////////////////////////////////////////////   
            case NPCMode.death:
                //------------------------------------------------------------------
                //      Specs of the state    //
                JustDie();
                npcMode = NPCMode.wander;
                //------------------------------------------------------------------
                break;

            default:
                break;
        }
    }

    //Random number generator
    int NGG()
    {
        rng = UnityEngine.Random.Range(1, 100);//choosing the random number between 1 - 100
        
        return rng;
    }
    //===================================
    //Probability functions
    //===================================

    void FleeToDie()
    {
        int deathProb = 2;

        if (rng <= deathProb)
            npcMode = NPCMode.death;
    }

    void WanderToSprint()
    {
        int sprintProb = 76;

        if (rng >= sprintProb)
            npcMode = NPCMode.sprint;

    }
    void ChaseToFlee()
    {
        int scareProb = 15;

        if (rng <= scareProb)
        {
            npcMode = NPCMode.flee;
        }
    }

    void AttackToEnrage()
    {
        int enrageProb = 30;

        if (rng <= enrageProb)
        {
            npcMode = NPCMode.enrage;
        }
    }
    //===================================


    //===================================
    //Flee function
    //===================================
    void Flee()
    {
        Vector3 disToTarget = transform.position - target.transform.position;

        Vector3 newPos = transform.position + disToTarget;

        navAgent.SetDestination(newPos);
    }
    //===================================


    //===================================
    //Destroy the npc function
    //===================================
    void JustDie()
    {
        gameObject.SetActive(false);
    }
    //===================================



    //===================================
    //Patroling functions
    //===================================
    private void DoWander()
    {
        if (!walkSet)
        {
            SearchForPoint();
        }
        if (walkSet)
            navAgent.SetDestination(walkPoint);

        float dist = Vector3.Distance(transform.position, walkPoint);

        if (dist < 5f)
            walkSet = false;
    }

    private void SearchForPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 7f, ground))
            walkSet = true;
    }
    //===================================


    //===================================
    // Normal range attack and range attack
    //===================================
    void EnrageAttack()
    {
        navAgent.SetDestination(transform.position);
        transform.Rotate(new Vector3(0f, 10f, 0f));

        if (!alreadyAttacked)
        {

            Rigidbody rb = Instantiate(bullets, attackpoint.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 20f, ForceMode.Impulse);
            rb.AddForce(transform.up * 2f, ForceMode.Impulse);

            GameObject raw = rb.gameObject;

            Destroy(raw, 2f);
        }
    }
    void AttackPlayer()
    {
        navAgent.SetDestination(transform.position);

        transform.LookAt(target);

        if (!alreadyAttacked)
        {
            Rigidbody rb = Instantiate(bullets, attackpoint.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 20f, ForceMode.Impulse);
            rb.AddForce(transform.up * 2f, ForceMode.Impulse);

            GameObject raw = rb.gameObject;

            Destroy(raw, 2f);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);

        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    //===================================


    //===================================
    //Chase the target function
    //===================================
    void FindTarget()
    {
        navAgent.SetDestination(target.position);
        navAgent.speed = 4;
        if (navAgent.remainingDistance < 2.0f)
        {
            //do something
        }
    }
    //===================================


    //===================================
    //All posibile states
    //===================================

    public enum NPCMode
    {
        wander,
        chase,
        idle,
        attack,
        enrage,
        sprint,
        death,
        flee
    };
    //===================================


    private void OnDrawGizmos()
    {
        if (offGizmo)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius); 
        }
    }
}
