using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SocialPlatforms.Impl;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public float stunDuration = 1f;
    public int pointsPerPotion = 10;
    public int pointsLostOnTrap = 35;
    public int maxHitPoints = 150;
    public bool dead = false;
    public AudioClip stepSound1;
    public AudioClip stepSound2;
    public AudioClip stepSound3;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip deathSound;
    public AudioClip chestOpeningSound;
    public HealthBar healthBar;
    public float pixelsPerUnit = 64f;
    public Rigidbody2D firePoint;
    public int playerDamage;
    public float playerAttackDelay;
    public GameObject projectile;
    public float knockback;
    public float firingDistance;
    public int scorePerChest;
    public float attackDelay;

    private bool stunned = false;
    private Animator animator;
    private int currentHitPoints;
    private Vector2 mousePos;
    private Camera mainCam;
    private float timeFromLastAttack;
    
    private Vector2 movement;
    float timer = 0;
    float waitTime = 0.4f;

    void Start()
    {
        animator = GetComponent<Animator>();

        timeFromLastAttack = attackDelay;
        currentHitPoints = GameManager.instance.playerHitPoints;
        healthBar = GameObject.Find("HealthBar").GetComponent<HealthBar>();
        healthBar.setMaxHealth(maxHitPoints);
        healthBar.changeHealth(currentHitPoints);
    }

    private void OnDisable()
    {
        GameManager.instance.playerHitPoints = currentHitPoints;
    }

    // Update is called once per frame
    void Update()
     {
         if (!stunned)
         {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }

        mousePos = GameManager.instance.getCamera().ScreenToWorldPoint(Input.mousePosition);

        if (timeFromLastAttack >= attackDelay)
        {
            if (Input.GetButton("Fire1"))
            {
                timeFromLastAttack = 0;
                Shoot();
            }
        }
        else
        {
            timeFromLastAttack += Time.deltaTime;
        }

        animator.SetFloat("Horizontal", movement.x);
         animator.SetFloat("Vertical", movement.y);
         animator.SetFloat("Speed", movement.sqrMagnitude);

        if (movement.sqrMagnitude > 0.01)
        {
            timer += Time.deltaTime;
            if (timer > waitTime)
            {
                timer -= waitTime;
                SoundManager.instance.RandomizeSfx(stepSound1, stepSound2, stepSound3);
            }
        }

        movement *= moveSpeed * Time.deltaTime;
        movement = pixelPerfectClamp(movement, pixelsPerUnit);
        Vector2 oldPosition = pixelPerfectClamp(rb.position, pixelsPerUnit);

        rb.MovePosition(oldPosition + movement);
    }




    private Vector2 pixelPerfectClamp(Vector2 moveVector, float pixelsPerUnit)
    {
        Vector2 vectorInPixels = new Vector2(
            Mathf.RoundToInt(moveVector.x * pixelsPerUnit),
            Mathf.RoundToInt(moveVector.y * pixelsPerUnit)
            );

        return vectorInPixels / pixelsPerUnit;
    }

    private void Shoot()
    {
        Vector2 playerPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 projDirection = mousePos - playerPosition;
        Vector2 projTarget = playerPosition + (projDirection.normalized * firingDistance);
        GameObject proj = Instantiate(projectile, transform.position, firePoint.transform.rotation);
        PlayerProjectile projScript = proj.GetComponent<PlayerProjectile>();
        projScript.init(projTarget, projDirection, playerDamage, knockback);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Exit")
        {
            GoToNextLevel();
            GameManager.instance.PauseGame();
        }
        else if (other.tag == "Trap")
        {
            other.gameObject.GetComponent<Animator>().SetTrigger("Sprung");
            GameManager.instance.UpdateStatus("You have activated a trap");
            Stun(stunDuration);
            Trap trap = other.gameObject.GetComponent<Trap>();
            SoundManager.instance.RandomizeSfx(trap.activated);
            LoseHitPoints(trap.playerDamage);
        }
        else if (other.tag == "Potion")
        {
            currentHitPoints += pointsPerPotion;
            if (currentHitPoints > maxHitPoints)
                currentHitPoints = maxHitPoints;
            healthBar.changeHealth(currentHitPoints);

            GameManager.instance.UpdateStatus("Gained " + pointsPerPotion + "hp from potion.");
            Stun(stunDuration);
            animator.SetTrigger("PickUp");
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Chest")
        {
            GameManager.instance.AddScore(scorePerChest);

            GameManager.instance.UpdateStatus("Recieved " + scorePerChest + " score from chest!");
            Stun(stunDuration);
            animator.SetTrigger("PickUp");
            SoundManager.instance.PlaySingle(chestOpeningSound);
            other.gameObject.SetActive(false);
        }
    }

    public void Stun(float stunDuration)
    {
        stunned = true;
        movement = Vector2.zero;
        Invoke("UnStun", stunDuration);
    }

    private void UnStun()
    {
        stunned = false;
    }

    private void GoToNextLevel()
    {
        Debug.Log("Restarted level");
        GameManager.instance.NextLevel();
    }

    public void LoseHitPoints(int loss)
    {
        if (!dead)
        {
            currentHitPoints -= loss;
            healthBar.changeHealth(currentHitPoints);
            GameManager.instance.UpdateStatus("Lost " + loss + "hp.");
            checkIfGameOver();
        }
    }

    private IEnumerator PlayDead()
    {
        animator.SetTrigger("Dead");
        stunned = true;
        AnimatorStateInfo animation = animator.GetCurrentAnimatorStateInfo(0);
        float seconds = animation.length * animation.speed;
        yield return new WaitForSeconds(seconds);
        GameManager.instance.PauseGame();
        GameManager.instance.GameOver();
    }

    private void checkIfGameOver()
    {
        if (currentHitPoints <= 0 && !dead)
        {
            dead = true;
            SoundManager.instance.PlaySingle(deathSound);
            StartCoroutine(PlayDead());
        }
    }
}
