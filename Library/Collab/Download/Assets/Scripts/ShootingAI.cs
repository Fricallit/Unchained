using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingAI : AI
{
    public float stoppingDistance;
    public float retreatingDistance;
    public float projectileSpeed;
    public GameObject projectile;

    void Update()
    {
        distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (!seenPlayer)
        {
            if (distanceToPlayer < 4)
                seenPlayer = true;
        }
        else
        {
            if (distanceToPlayer > stoppingDistance)
            {
                Move();
            }
            else if (distanceToPlayer < stoppingDistance && distanceToPlayer > retreatingDistance)
            {
                transform.position = this.transform.position;
            }
            else if (distanceToPlayer < retreatingDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, target.position, -enemyScript.speed * Time.deltaTime);
            }

            if (canAttack && !player.dead)
                StartCoroutine(Attack());
        }
    }

    public IEnumerator Attack()
    {
        canAttack = false;
        float animationTime = enemyScript.PerformAttack();
        yield return new WaitForSeconds(animationTime);

        Vector2 enemyPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 projDirection = new Vector2(target.position.x, target.position.y) - enemyPosition;
        Vector2 projTarget = enemyPosition + (projDirection.normalized * enemyScript.firingDistance);
        GameObject proj = Instantiate(projectile, transform.position, transform.rotation);
        EnemyProjectile projScript = proj.GetComponent<EnemyProjectile>();
        projScript.init(projTarget, enemyScript.damage);

        yield return new WaitForSeconds(enemyScript.attackDelay);
        canAttack = true;
    }
}
