using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    protected Transform target;
    protected Player player;
    protected float distanceToPlayer;
    protected bool canAttack = true;
    protected bool seenPlayer = false;
    protected Enemy enemyScript;

    void Start()
    {
        enemyScript = gameObject.GetComponent<Enemy>();
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    protected void Move()
    {
        if (!enemyScript.knockbackImmunity)
            transform.position = Vector2.MoveTowards(transform.position, target.position, enemyScript.speed * Time.deltaTime);
    }
}
