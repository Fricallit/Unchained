using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Enemy : MonoBehaviour
{
    public float speed;
    public int damage;
    public float knockback;
    public float firingDistance;
    public float attackDelay;
    public int health;
    public int currentHelath;
    public AudioClip deathSound;
    public int scoreReward;
    public string enemyName;
    public float knockbackImmunityTime;
    public float dazeTime;
    public float flashSpeed;
    public bool knockbackImmunity = false;

    public AudioClip attackSound;
    private Animator animator;
    private Rigidbody2D rb;
    private bool flashing = false;
    
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        currentHelath = health;
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    public float PerformAttack()
    {
        animator.SetTrigger("Attacking");
        SoundManager.instance.PlaySingle(attackSound);
        AnimatorStateInfo animation = animator.GetCurrentAnimatorStateInfo(0);
        float seconds = animation.length * animation.speed;
        return seconds;
    }

    public void LoseHitPoints(int loss)
    {
        currentHelath -= loss;
        StartCoroutine(Flash(gameObject.GetComponent<SpriteRenderer>(), flashSpeed));
        Debug.Log(currentHelath);
        if (currentHelath <= 0)
        {
            SoundManager.instance.PlaySingle(deathSound);
            GameManager.instance.AddScore(scoreReward);
            GameManager.instance.UpdateStatus("Gained " + scoreReward + " from killing " + enemyName);
            gameObject.SetActive(false);
        }
    }

    IEnumerator Flash(SpriteRenderer toFlash, float flashSpeed)
    {
        if (!flashing)
        {
            flashing = true;
            toFlash.color = Color.red;
            yield return new WaitForSeconds(flashSpeed);
            toFlash.color = Color.white;
            flashing = false;
        }
    }

    private void DisableKnockbackImmunity()
    {
        knockbackImmunity = false;
    }

    private void ZeroVelocity()
    {
        rb.velocity = Vector2.zero;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (!knockbackImmunity)
        {
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
            knockbackImmunity = true;
            Invoke("ZeroVelocity", dazeTime);
            Invoke("DisableKnockbackImmunity", knockbackImmunityTime);
        }
    }
}
