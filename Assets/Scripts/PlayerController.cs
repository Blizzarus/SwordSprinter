using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    GameManager gameManager;
    [SerializeField] GameObject playerHP;
    [SerializeField] Sprite damageIcon;
    [SerializeField] TextMeshProUGUI counteredText;
    TextFade counterFade;
    [SerializeField] GameObject clash;
    ParticleSystem clashFX;
    [SerializeField] ParticleSystem[] bloodFX;
    EnemyActions enemy;
    Animator animator;
    public float delayState = 0;
    public List<float> atkStates = new List<float>() { 0, 0, 0, 0 }; // Attack States: 0 = Left, 1 = Right, 2 = Up, 3 = Down
    int i; // index for Max (current active attack)
    int queuedAtk = -1; // used to check for attacks initiated while in delay
    float attackLength;
    float impactDelay;
    float counterThreshold;
    public int HP;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        counterFade = counteredText.GetComponent<TextFade>();
        clashFX = clash.GetComponent<ParticleSystem>();
        clashFX.GetComponent<Renderer>().sortingOrder = 10;
        foreach(ParticleSystem particle in bloodFX)
        {
            particle.GetComponent<Renderer>().sortingOrder = 11;
        }
        

        attackLength = gameManager.attackLength;
        impactDelay = gameManager.impactDelay;
        counterThreshold = gameManager.counterThreshold;
        HP = GameObject.Find("PlayerHP").transform.childCount;

        SetupAnimations();
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
        if(HP <= 0 && !gameManager.cheatMode) { return; }
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
        else if (gameManager.cheatMode && Input.GetKeyDown(KeyCode.C))
        {
            enemy.Countered(enemy.atkStates.IndexOf(enemy.atkStates.Max()));
            return;
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
                enemy.Countered(j);
            }

            // otherwise, initiate attack
            Attack(j);
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
                    Attack(queuedAtk);
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
                    enemy.TakeDamage(i);
                    atkStates[i] = 0;
                    delayState = impactDelay;
                    return;
                }

                // on clash
                if (enemy.atkStates[i] != 0 && atkStates[i] + enemy.atkStates[i] > 0.9f && atkStates[i] + enemy.atkStates[i] < attackLength)
                {
                    Clash(i);
                }
            }
        }

        if (atkStates[i] < 0)
        {
            atkStates[i] = 0;
        }
    }

    void Attack(int x)
    {
        queuedAtk = -1;
        if (gameManager.encounter && enemy.CounterPlayer(x))
        {
            Countered(x);
            return;
        }
        atkStates[x] = attackLength;
        switch (x)
        {
            case 0:
                animator.Play("Left_Attack", 0);
                break;
            case 1:
                animator.Play("Right_Attack", 0);
                break;
            case 2:
                animator.Play("Up_Attack", 0);
                break;
            case 3:
                animator.Play("Down_Attack", 0);
                break;
        }
    }

    void Clash(int x)
    {
        clashFX.Emit(1);
        gameManager.clashSFX();
        enemy.Clash(x);
        delayState = impactDelay;
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
        atkStates[x] = 0;
    }

    void Countered(int x)
    {
        atkStates[x] = 0;
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
        gameManager.clashSFX();
        counterFade.StartFade();
    }

    public void TakeDamage(int x)
    {
        HP--;
        queuedAtk = -1;
        bloodFX[x].Play();
        if(!gameManager.cheatMode)
        {
            GameObject.Find("HP_" + HP).GetComponent<Image>().sprite = damageIcon;
            if (HP <= 0)
            {
                atkStates[i] = -1;
                animator.Play("Die", 1);
                gameManager.dieSFX();
                enemy.Cease();
                StartCoroutine(delayLoseTrigger());
                return;
            }
        }
        animator.Play("Hit", 1);
        gameManager.hurtSFX();
        atkStates[i] = 0;
        delayState = impactDelay / 2;
    }

    IEnumerator delayLoseTrigger()
    {
        yield return new WaitForSeconds(3);
        gameManager.Lose();
    }

    public void StartRunning()
    {
        animator.Play("Run", 0);
    }

    public void EndDance()
    {
        gameManager.encounter = true;
        GameObject.FindGameObjectWithTag("PWeapon").SetActive(false);
        animator.Play("Dance", 1);
        StartCoroutine(DelayWinTrigger());
    }

    IEnumerator DelayWinTrigger()
    {
        yield return new WaitForSeconds(6.5f);
        gameManager.Win();
    }

    void SetupAnimations()
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
