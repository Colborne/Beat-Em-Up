using UnityEngine;
using UnityEngine.AI;

public class EnemyControl : MonoBehaviour
{
    public GameObject[] players;
    public GameObject target;
    public GameObject hitbox;
    Animator animator;

    //Attacking
    public float timeBetweenAttacks;
    public float attackRange;
    public float setSpeed;
    public int health;
    bool alreadyAttacked;
    float speed;

    private void Awake()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        animator = GetComponent<Animator>();
        speed = setSpeed;
    }

    private void Update()
    {
        float minSqrDistance = Mathf.Infinity;
        for (int i = 0; i < players.Length; i++)
        {
            float sqrDistance = (transform.position - players[i].transform.position).sqrMagnitude;
            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                target = players[i];

            }
        }

        if(!animator.GetBool("takingDamage"))
        {
            if (!CheckDistance())
            {
                ChasePlayer();
                Rotate();
            }
            else
                AttackPlayer(); 
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.GetComponent<Damage>())
        {
            Debug.Log("fuck");
            health -= other.GetComponent<Damage>().damage;
            TakeDamage();
            
        }
    }

    private void Rotate()
    {
        if(target.transform.position.x > transform.position.x)
            transform.rotation = Quaternion.Euler(0,90,0);
        else
            transform.rotation = Quaternion.Euler(0,-90,0);
    }
    private bool CheckDistance()
    {
        Vector3 diff = transform.position - target.transform.position;
        if(Mathf.Abs(diff.x) <= attackRange && Mathf.Abs(diff.z) <= .25f)
            return true;
        else
            return false;
    }

    private void ChasePlayer()
    {
        float step =  speed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);
        animator.SetFloat("V", 1f);
    }

    private void AttackPlayer()
    {
        animator.SetFloat("V", 0f);
        speed = 0;

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;
            Invoke("ResetAttack", timeBetweenAttacks);
            animator.CrossFade("Attack", 0f);
        }       
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
        speed = setSpeed;
    }

    public void DamageColliderOn()
    {
        hitbox.SetActive(true);
    }

    public void DamageColliderOff()
    {
        hitbox.SetActive(false);
    }

    public void TakeDamage()
    {
        if(health <= 0)
            Destroy(gameObject);
        animator.SetBool("takingDamage", true);
        animator.CrossFade("Damage", 0.2f);
        hitbox.SetActive(false);  
    }
}
