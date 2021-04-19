using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed;
    public GameObject particleEffect;
    public AudioClip hitSound;

    private int damage;
    private Vector2 target;
    private Vector2 direction;
    private float force;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!(collider.tag == "Wall" || collider.tag == "Enemy"))
            return;
        if (collider.tag == "Enemy")
        {
            Enemy enemy = collider.gameObject.GetComponent<Enemy>();
            enemy.LoseHitPoints(damage);
            enemy.ApplyKnockback(direction, force);
        }
        DestroyProjectile();
    }

    public void init(Vector2 trg, Vector2 dir, int dmg, float frc)
    {
        target = trg;
        direction = dir;
        damage = dmg;
        force = frc;
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
