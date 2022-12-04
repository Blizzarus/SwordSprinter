using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GameManager gameManager;
    [SerializeField] TextMeshProUGUI playerStatus;
    EnemyActions enemy;
    Animator animator;
    public float delayState = 0;
    public List<float> atkStates = new List<float>() { 0, 0, 0, 0 }; // Attack States: 0 = Left, 1 = Right, 2 = Up, 3 = Down
    int i; // index for Max (current active attack)
    int queuedAtk = -1; // used to check for attacks initiated while in delay
    float attackLength;
    float impactDelay;
    float counterThreshold;
    int HP;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        attackLength = gameManager.attackLength;
        impactDelay = gameManager.impactDelay;
        counterThreshold = gameManager.counterThreshold;
        HP = 10;
        playerStatus.text = "HP Remaining: " + HP;

        setupAnimations();
    }

    void Update()
    {
        // set index of Max atkState
        i = atkStates.IndexOf(atkStates.Max());

        // check for user input
        if (gameManager.encounter) { inputCheck(); }

        // update Delay and Attack States
        updateStates();
    }

    private void LateUpdate()
    {
        //playerStatus.text = "Delay: " + delayState + "\nLeft: " + atkStates[0] + "\nRight: " + atkStates[1] + "\nUp: " + atkStates[2] + "\nDown: " + atkStates[3];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            enemy = other.GetComponent<EnemyActions>();
            gameManager.EncounterStart();
            animator.SetTrigger("stopRun");
        }
    }

    void inputCheck()
    {
        int j;
        // check for user input (TODO: add touch swipe control support)
        if (Input.GetKeyDown(KeyCode.LeftArrow) && atkStates[0] == 0)
        {
            j = 0;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && atkStates[1] == 0)
        {
            j = 1;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && atkStates[2] == 0)
        {
            j = 2;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && atkStates[3] == 0)
        {
            j = 3;
        }
        else
        {
            return;
        }

        if (delayState == 0)
        {
            // on attack change (feint)
            if (atkStates[i] != 0)
            {
                delayState = (attackLength - atkStates[i]) / 2;
                atkStates[i] = 0;
                queuedAtk = j;
                return;
            }

            // on counter applicable
            if (enemy.atkStates[j] > counterThreshold)
            {
                enemy.countered(j);
                return;
            }

            // otherwise, initiate attack
            attack(j);
        }
        else // if delay active, queue attack
        {
            queuedAtk = j;
        }
    }

    void updateStates()
    {
        //update delayState
        if (delayState > 0)
        {
            delayState -= Time.deltaTime;
            if (delayState <= 0)
            {
                delayState = 0;
                if (queuedAtk != -1)
                {
                    atkStates[queuedAtk] = attackLength;
                    queuedAtk = -1;
                }
            }
        }

        i = atkStates.IndexOf(atkStates.Max());

        //update attack states
        if (atkStates[i] > 0)
        {
            atkStates[i] -= Time.deltaTime;
            if (gameManager.encounter)
            {
                // on hit
                if (atkStates[i] <= 0)
                {
                    enemy.takeDamage();
                    atkStates[i] = 0;
                    delayState = impactDelay;
                    return;
                }

                // on clash
                if (enemy.atkStates[i] != 0 && atkStates[i] + enemy.atkStates[i] > 0.9f && atkStates[i] + enemy.atkStates[i] < attackLength)
                {
                    clash(i);
                }
            }
        }

        if (atkStates[i] < 0)
        {
            atkStates[i] = 0;
        }
    }

    void attack(int x)
    {
        if (gameManager.encounter && enemy.CounterPlayer(x))
        {
            countered(x);
            return;
        }
        atkStates[x] = attackLength;
        switch (x)
        {
            case 0:
                animator.SetTrigger("startLAtk");
                break;
            case 1:
                animator.SetTrigger("startRAtk");
                break;
            case 2:
                animator.SetTrigger("startUAtk");
                break;
            case 3:
                animator.SetTrigger("startDAtk");
                break;
        }
    }

    void clash(int x)
    {
        enemy.clash(x);
        delayState = impactDelay;
        switch(x)
        {
            case 0:
                animator.Play("Left_Clash", 0);
                break;
            case 1:
                animator.Play("Right_Clash", 0);
                break;
            case 2:
                animator.Play("Up_Clash", 0);
                break;
            case 3:
                animator.Play("Down_Clash", 0);
                break;
        }
        atkStates[x] = 0;
    }

    void countered(int x)
    {
        atkStates[x] = enemy.atkStates[x] = 0;
        switch (x)
        {
            case 0:
                animator.Play("Left_Clash", 0);
                break;
            case 1:
                animator.Play("Right_Clash", 0);
                break;
            case 2:
                animator.Play("Up_Clash", 0);
                break;
            case 3:
                animator.Play("Down_Clash", 0);
                break;
        }
        delayState = attackLength;
    }

    public void takeDamage()
    {
        HP--;
        if (HP <= 0)
        {
            playerStatus.text = ":(";
            animator.Play("Die", 1);
            enemy.Cease();
            StartCoroutine(delayLoseTrigger());
        }
        else
        {
            playerStatus.text = "HP Remaining: " + HP;
            animator.Play("Hit", 1);
            atkStates[i] = 0;
            delayState = impactDelay / 2;
        }
    }

    IEnumerator delayLoseTrigger()
    {
        yield return new WaitForSeconds(3);
        gameManager.Lose();
    }

    public void startRunning()
    {
        animator.Play("Run", 0);
    }

    public void endDance()
    {
        gameManager.encounter = true;
        animator.Play("Dance", 1);
        StartCoroutine(delayWinTrigger());
    }

    IEnumerator delayWinTrigger()
    {
        yield return new WaitForSeconds(8);
        gameManager.Win();
    }

    void setupAnimations()
    {
        animator = GetComponent<Animator>();
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            switch (clip.name)
            {
                case "leftAtkClip":
                    animator.SetFloat("LAtkMulti", clip.length / attackLength);
                    break;
                case "rightAtkClip":
                    animator.SetFloat("RAtkMulti", clip.length / attackLength);
                    break;
                case "upAtkClip":
                    animator.SetFloat("UAtkMulti", clip.length / attackLength);
                    break;
                case "downAtkClip":
                    animator.SetFloat("DAtkMulti", clip.length / attackLength);
                    break;
                case "leftClashClip":
                    animator.SetFloat("LClashMulti", clip.length / impactDelay);
                    break;
                case "rightClashClip":
                    animator.SetFloat("RClashMulti", clip.length / impactDelay);
                    break;
                case "upClashClip":
                    animator.SetFloat("UClashMulti", clip.length / impactDelay);
                    break;
                case "downClashClip":
                    animator.SetFloat("DClashMulti", clip.length / impactDelay);
                    break;
                case "hitClip":
                    animator.SetFloat("hitMulti", (clip.length / impactDelay) * 2);
                    break;
            }
        }
    }
}
