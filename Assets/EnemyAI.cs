using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsPlayer;
    Animator animator;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    GameObject hitbox;
    public GameObject projectile;

    //States
    public float alertRange, attackRange;
    public Vector3 sightRange;
    public bool playerInAlertRange, playerInAttackRange, playerInSight;
    public bool patrolState, idleState = true, waitingToPatrol;
    public bool canRange, ranging;

    float speed;
    Vector3 lastPosition;
    [Range(0,5)] public int meleeCount;
    [Range(0,5)] public int rangedCount;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();       
        animator = GetComponent<Animator>();
        hitbox = transform.GetChild(2).gameObject;
    }

    void Start()
    {
        NavMeshHit closestHit;
 
        if (NavMesh.SamplePosition(transform.position, out closestHit, 500f, NavMesh.AllAreas))
            transform.position = closestHit.position;
    }

    private void Update()
    {
        if (agent.isOnNavMesh)
        {
            //Check for alert and attack range
            playerInAlertRange = Physics.CheckSphere(transform.position, alertRange, whatIsPlayer);
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
            playerInSight = Physics.CheckBox(transform.position + new Vector3(0, sightRange.y/2, sightRange.z/2), sightRange, Quaternion.LookRotation(transform.forward), whatIsPlayer);

            if(!animator.GetBool("takingDamage"))
            {
                if (!playerInAlertRange && !playerInAttackRange && !playerInSight) 
                {
                    if(patrolState)
                        Patrol();
                    else
                        Idle();  
                }

                if (playerInAlertRange && !playerInAttackRange && !animator.GetBool("isAttacking")) 
                {
                    if(canRange && Random.Range(0,100) == 0)
                    {
                        ranging = true;
                        AttackPlayer();
                    }
                    else
                        ChasePlayer(); 
                }
                
                if (playerInAttackRange)
                    AttackPlayer(); 
                
            
                speed = Mathf.Lerp(speed, (transform.position - lastPosition).magnitude, 0.7f);
                lastPosition = transform.position;
            }
        }
    }

    private void Idle()
    {
        animator.SetFloat("V", 0.0f);
        
        if(!waitingToPatrol)
        {
            Invoke("StartPatrol", Random.Range(2,6));
            waitingToPatrol = true;
        }

        if(!walkPointSet)
            SearchWalkPoint();
    }

    private void StartPatrol()
    {
        waitingToPatrol = false;
        patrolState = true;
        idleState = false;
        agent.SetDestination(walkPoint);
    }

    private void Patrol()
    {
        if (walkPointSet)
        {
            if(speed > 0)
                animator.SetFloat("V", Mathf.Clamp(speed * 10f, 0f, .5f));
            else
                animator.SetFloat("V", 0f);     
        }     
        
        Vector3 distanceToWalkPoint = agent.transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 3f)
        {
            animator.SetFloat("V", 0f);
            walkPointSet = false;
            idleState = true;
            patrolState = false;
        }
    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        Vector3 start = new Vector3(transform.position.x + randomX, transform.position.y + 500f, transform.position.z + randomZ);

        RaycastHit hit;
        if(Physics.Raycast(start, Vector3.down, out hit) && hit.collider.CompareTag("Ground") && hit.point.y > -1)
        {
            walkPoint = new Vector3(transform.position.x + randomX, hit.point.y, transform.position.z + randomZ);
            walkPointSet = true; 
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        walkPoint = player.position;
        animator.SetFloat("V", Mathf.Clamp(speed * 10f, 0f, 1f));
        patrolState = false;
    }

    private void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.ResetPath();
        animator.SetFloat("V", 0f);
        //transform.LookAt(player.position);

        if (!alreadyAttacked)
        {
            animator.SetBool("isAttacking", true);
            alreadyAttacked = true;
            Invoke("ResetAttack", timeBetweenAttacks);
            if(ranging || meleeCount == 0)
                animator.CrossFade("RangedAttack" + Random.Range(1, rangedCount + 1).ToString(), 0.2f);
            else
                animator.CrossFade("Attack" + Random.Range(1, meleeCount + 1).ToString(), 0.2f);
            ranging = false;
        }       
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void DamageColliderOn()
    {
        hitbox.SetActive(true);
    }

    public void DamageColliderOff()
    {
        hitbox.SetActive(false);
    }

    public void RangedAttack()
    {
        if(projectile != null)
        {
            Rigidbody rb;
            GameObject proj = Instantiate(projectile, transform.position + transform.forward * sightRange.y/2 + transform.up * sightRange.y/2, Quaternion.identity);
            if(proj.GetComponent<Rigidbody>() == null)
                rb = proj.GetComponentInChildren<Rigidbody>();
            else
                rb = proj.GetComponent<Rigidbody>();
            proj.transform.LookAt(player);
            
            rb.AddForce(transform.forward * 2500f);
        }
    }

    public void TakeDamage()
    {
        if(!animator.GetBool("isAttacking"))
        {
            animator.SetBool("takingDamage", true);
            animator.CrossFade("Damage", 0.2f);
            hitbox.SetActive(false);
        }
    }
}
