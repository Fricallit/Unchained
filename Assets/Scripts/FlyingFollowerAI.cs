using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingFollowerAI : AI
{
    void Update()
    {
        distanceToPlayer = Vector2.Distance(transform.position, target.position);
        if (!seenPlayer)
        {
            if (distanceToPlayer < 5)
                seenPlayer = true;
        }
        else
        {
            if (distanceToPlayer > 0.7)
            {
                Move();
            }
            else if (distanceToPlayer < enemyScript.firingDistance)
            {
                if (canAttack && !player.dead)
                    StartCoroutine(Attack());
            }
        }
    }

    public IEnumerator Attack()
    {
        canAttack = false;
        enemyScript.PerformAttack();
        target.gameObject.GetComponent<Player>().LoseHitPoints(enemyScript.damage);
        yield return new WaitForSeconds(enemyScript.attackDelay);
        canAttack = true;
    }
}
