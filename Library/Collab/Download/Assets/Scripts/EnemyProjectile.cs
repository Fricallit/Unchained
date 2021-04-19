using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed;
    public AudioClip hitSound;
    public GameObject particleEffect;

    private int damage;
    private Vector2 target;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!(collider.tag == "Wall" || collider.tag == "Player"))
            return;
        if (collider.tag == "Player")
        {
            Player enemy = collider.gameObject.GetComponent<Player>();
            enemy.LoseHitPoints(damage);
        }
        DestroyProjectile();
    }

    public void init(Vector2 trg, int dmg)
    {
        target = trg;
        damage = dmg;
    }

    private void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if ((Vector2)transform.position == target)
        {
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        SoundManager.instance.PlaySingle(hitSound);
        Instantiate(particleEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
