using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;
using System;

public class Enemy : CharacterStats
{
    [SerializeField]
    protected enum State
    {
        Idle,
        Chasing,
        Attacking
    }

    protected State enemyState;
    protected NavMeshAgent enemyNavMeshAgent;
    protected PlayerStats playerStats;

    protected float viewDistance;
    protected float hearingDistance;
    protected float attackDistance;
    protected float speed;
    protected string audioGroup;

    protected float distanceFromPlayer;
    protected float hitTimer;
    protected float stunnedHitDuration;

    protected Vector3 playerLocation;
    protected Vector3 enemyLocation;
    protected Ray enemySight;
    protected RaycastHit hitInfo;

    [SerializeField] private Image healthColour;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Transform cam;
    [SerializeField] private GameObject[] availableDrops;
    [SerializeField] private int itemDropChance;

    protected SoundManager.Sound attackSound;
    protected SoundManager.Sound chasingSound;
    protected SoundManager.Sound deathSound;
    protected SoundManager.Sound idleSound;

    public void Update()
    {
        UpdateHealth();
        transform.GetChild(0).transform.LookAt(transform.GetChild(0).transform.position + cam.forward);

        enemySight = new Ray(transform.position, transform.TransformDirection(Vector3.forward));
        enemyLocation = this.enemyNavMeshAgent.transform.position;
        playerLocation = playerStats.gameObject.transform.position;
        distanceFromPlayer = Vector3.Distance(playerLocation, enemyLocation);

        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * viewDistance, Color.white);

        switch (enemyState)
        {
            case State.Idle:
                PlayAudio(this);
                Idle();
                break;

            case State.Chasing:
                PlayAudio(this);
                Chasing();
                break;

            case State.Attacking:
                
                Attacking();
                break;
        }

        //Debug.Log(enemyState);
    }

    public virtual void Idle()
    {
        playerLocation = playerStats.gameObject.transform.position;

        if (Physics.Raycast(enemySight, out hitInfo, viewDistance))
        {
            if (hitInfo.collider.tag == "Player")
            {
                SwitchState(State.Chasing);
            }
        }

        //Debug.Log("enemy ai running");
    }

    public virtual void Chasing()
    {
        enemyNavMeshAgent.SetDestination(playerLocation);

        if (distanceFromPlayer <= attackDistance)
        {
            hitTimer = attackSpeed;
            SwitchState(State.Attacking);
        }

        if (distanceFromPlayer >= viewDistance)
        {
            SwitchState(State.Idle);
        }
    }

    void Attacking()
    {
        enemyNavMeshAgent.SetDestination(enemyLocation);

        hitTimer -= Time.deltaTime;

        if (hitTimer <= 0.0f)
        {
            if (playerStats.shield.activeSelf == true)
            {
                playerStats.TakeDamage((int)(damage / 4), playerStats.GetComponent<Transform>());
                hitTimer = stunnedHitDuration; // <----- will be replaced by a possible stunned state
                SoundManager.PlaySound(SoundManager.Sound.MetalClang, playerLocation);
            }
            else
            {
                playerStats.TakeDamage(damage, playerStats.GetComponent<Transform>());
                hitTimer = attackSpeed;
                PlayAudio(this);
            }
        }   

        if (distanceFromPlayer > attackDistance)
        {
            SwitchState(State.Chasing);
        }
    }

    protected void SwitchState(State newState)
    {
        enemyState = newState;
    }

    public void UpdateHealth()
    {
        healthBar.value = Health;
        healthBar.maxValue = maxHealth;

        if (Health < maxHealth * 0.8 && Health > maxHealth * 0.6)
            healthColour.color = new Color32(167, 227, 16, 255);

        if (Health < maxHealth * 0.6 && Health > maxHealth * 0.4)
            healthColour.color = new Color32(227, 176, 9, 255);

        if (Health < maxHealth * 0.4 && Health > maxHealth * 0.2)
            healthColour.color = new Color32(240, 86, 48, 255);

        if (Health < maxHealth * 0.2)
            healthColour.color = new Color32(204, 40, 0, 255);
    }

    public void InitEnemy()
    {
        SwitchState(State.Idle);

        healthColour = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();
        healthBar = transform.GetChild(0).GetChild(0).GetComponent<Slider>();
        healthColour.color = new Color32(74, 227, 14, 255);

        cam = GameManager.manager.playerAndCamera.transform.GetChild(1);
        playerStats = GameManager.manager.playerStats;
        enemyNavMeshAgent = GetComponent<NavMeshAgent>();

        maxHealth = Health;
        healthBar.maxValue = maxHealth;
        stunnedHitDuration = attackSpeed * 1.5f;
    }

    public override void TakeDamage(int damage, Transform character)
    {
        base.TakeDamage(damage, character);
        DamageFeedback(character, "-" + damage, new Color32(255, 69, 0, 255));
        if (Health <= 0)
        {
            Death();
        }
    }

    protected override void Death()
    {
        SoundManager.PlaySound(this.deathSound, enemyLocation); 
        base.Death();
        
        // ENTER CODE FOR DEATH ANIMATIONS, ETC
        this.gameObject.SetActive(false);

        DropItemOnDeath();
    }

    protected void DropItemOnDeath()
    {
        if (availableDrops.Length <=0) { return; }
        int dropDecision = ChooseNumbByChance(0, 1, itemDropChance);
        if (dropDecision == 1) { return; }
        int selectedItem = UnityEngine.Random.Range(0, availableDrops.Length);
        Instantiate(availableDrops[selectedItem], transform.position, Quaternion.identity);
    }

    public void PlayAudio(Enemy enemy)
    {
        if (enemyState == State.Idle)
            SoundManager.PlaySound(enemy.idleSound, enemy.transform.position);
        if (enemyState == State.Chasing)
            SoundManager.PlaySound(enemy.chasingSound, enemy.transform.position);
        if (enemyState == State.Attacking)
            SoundManager.PlaySound(enemy.attackSound, enemy.transform.position);
    }

    private int ChooseNumbByChance(int output1, int output2, int chanceNum)
    {
        int chance = UnityEngine.Random.Range(0, 101);
        if (chance < chanceNum) { return output1; }
        else if (chance > chanceNum) { return output2; }
        return 0;
    }
}