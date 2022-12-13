using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnemyActions : MonoBehaviour
{
    GameManager gameManager;
    TextFade counterFade;
    TextMeshProUGUI enemyStatus;
    Animator animator;
    [SerializeField] ParticleSystem[] bloodFX;
    [SerializeField] GameObject[] predictionLines;
    PlayerController player;

    List<List<int>> attackPatterns; // list of lists of ints used to track enemy attack patterns
    int currentPattern; // list index to track current active attack pattern
    int nextAttack; // list inex to track next attack in pattern

    List<int> playerAttackHistory; // list of all attacks the player has executed
    List<List<int>> predictionPatterns; // list of patterns recorded; length varies by enemy intelligence
    int predictionLength; // the length of prediction patterns
    int counterMetric; // the number of times a pattern must be matched in order for the enemy to counter it

    public float delayState;
    public List<float> atkStates = new List<float>() { 0, 0, 0, 0 }; // Attack States: 0 = Left, 1 = Right, 2 = Up, 3 = Down
    // NOTE: Enemy attack states are named from the PLAYER perspective (0 = attack to PLAYER's left)

    int i; // index for Max (current active attack)
    int intelligence; // determines pattern complexity and recognition ability
    float AIDelay; // determines how long enemy takes to react and initiate new attacks
    float moveSpeed;
    public float attackLength;
    float impactDelay;
    float counterThreshold;
    int HP;

    void Awake()
    {
        counterFade = GameObject.Find("CounterPrompt").GetComponent<TextFade>();
        enemyStatus = GameObject.Find("EnemyStatus").GetComponent<TextMeshProUGUI>();
        animator = GetComponent<Animator>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        foreach (ParticleSystem particle in bloodFX)
        {
            particle.GetComponent<Renderer>().sortingOrder = 11;
        }

        intelligence = gameManager.enemyIntelligence;
        AIDelay = gameManager.enemyDelay;
        moveSpeed = gameManager.moveSpeed;
        attackLength = gameManager.attackLength;
        impactDelay = gameManager.impactDelay;
        counterThreshold = gameManager.counterThreshold;


        attackPatterns = SetAttackPatterns();
        SetupAnimations();

        playerAttackHistory = new List<int>();
        predictionPatterns = new List<List<int>>();

        HP = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.encounter)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
        }
        else
        {
            // set index of Max atkState
            i = atkStates.IndexOf(atkStates.Max());

            // choose action to perform
            CombatActions();

            // update Atk and Delay states
            UpdateStates();
        }
    }

    private void LateUpdate()
    {
        enemyStatus.text = "Delay: " + delayState + "\nLeft: " + atkStates[0] + "\nRight: " + atkStates[1] + "\nUp: " + atkStates[2] + "\nDown: " + atkStates[3];
    }

    void CombatActions()
    {
        if (delayState == 0 && atkStates[i] == 0)
        {
            int j = player.atkStates.IndexOf(player.atkStates.Max());
            if (player.atkStates[j] > AIDelay / 2)
            {
                Attack(j);
                return;
            }

            if (nextAttack == attackPatterns[currentPattern].Count())
            {
                NewPattern();
                return;
            }

            j = attackPatterns[currentPattern][nextAttack];
            nextAttack++;

            // on counter applicable
            if (player.atkStates[j] > counterThreshold)
            {
                Countered(j);
                return;
            }

            // otherwise, initate attack
            predictionLines[j].SetActive(true);
            Attack(j);
        }
    }

    void NewPattern()
    {
        currentPattern = Random.Range(0, attackPatterns.Count());
        nextAttack = 0;
        delayState += AIDelay;
    }

    void UpdateStates()
    {
        i = atkStates.IndexOf(atkStates.Max());

        // update delay state
        if (delayState > 0)
        {
            delayState -= Time.deltaTime;
            if (delayState < 0) { delayState = 0; }
        }

        // update attack states
        if (atkStates[i] > 0)
        {
            atkStates[i] -= Time.deltaTime;
            if (atkStates[i] <= 0 && HP > 0)
            {
                predictionLines[i].SetActive(false);
                player.TakeDamage(i);
                atkStates[i] = 0;
                NewPattern();
            }
        }
    }

    void Attack(int x)
    {
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

    public bool CounterPlayer(int x)
    {
        playerAttackHistory.Add(x);
        if(playerAttackHistory.Count < predictionLength) { return false; }
        bool counter = false;
        List<int> recentAttacks = new List<int>();
        for (int i = playerAttackHistory.Count - predictionLength; i < playerAttackHistory.Count; i++)
        {
            recentAttacks.Add(playerAttackHistory[i]);
        }

        if (delayState <= 0 && atkStates[i] == 0 && predictionPatterns.Count > 0)
        {
            //Debug.Log("Attmepting to match recent attacks: " + string.Join(',', recentAttacks));
            int occurences = 0;
            foreach (List<int> pattern in predictionPatterns)
            {
                //Debug.Log("Checking against pattern: " + string.Join(',', pattern));
                if(pattern.SequenceEqual(recentAttacks))
                {
                    occurences++;
                    if(occurences == counterMetric)
                    {
                        counter = true;
                        if (atkStates[i] == 0)
                        {
                            delayState = 0;
                            Attack(x);
                        }
                    }
                }
            }
            //Debug.LogWarning("Done checking.  Found " + occurences + " matches");
        }

        if ((playerAttackHistory.Count % predictionLength) == 0)
        {
            List<int> pattern = new List<int>(recentAttacks);
            predictionPatterns.Add(pattern);
            if (predictionPatterns.Count > 10)
            {
                predictionPatterns.RemoveAt(0);
            }
            //Debug.Log("Player Pattern Recorded: " + string.Join(',', pattern + " | there are now " + predictionPatterns.Count));
        }

        return counter;
    }

    public void Clash(int x)
    {
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
        predictionLines[x].SetActive(false);
    }

    public void Countered(int x)
    {
        NewPattern();
        atkStates[x] = 0;
        predictionLines[x].SetActive(false);
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
        delayState = attackLength + AIDelay;
        gameManager.clashSFX();
        counterFade.StartFade();
    }

    public void TakeDamage(int x)
    {
        HP--;
        predictionLines[x].SetActive(false);
        bloodFX[x].Play();
        if (HP <= 0)
        {
            atkStates[i] = -1;
            gameManager.EncounterEnd();
            player.StartRunning();
            animator.Play("Die", 1);
            gameManager.dieSFX();
            StartCoroutine(DeleteDeadBody());
            return;
        }
        animator.Play("Hit", 1);
        gameManager.hurtSFX();
        atkStates[i] = 0;
        NewPattern();
    }

    IEnumerator DeleteDeadBody()
    {
        yield return new WaitForSeconds(2.0f);
        Destroy(gameObject);
    }

    public void Cease()
    {
        gameObject.GetComponent<EnemyActions>().enabled = false;
    }

    List<List<int>> SetAttackPatterns()
    {
        List<List<int>> attackPatterns = new List<List<int>>();
        for(int x = 0; x < Random.Range(1, 1 + intelligence / 3) + intelligence / 3; x++)
        {
            List<int> pattern = new List<int>();
            for (int y = 0; y < Random.Range(1, intelligence + 3); y++)
            {
                pattern.Add(Random.Range(0,4));
            }
            attackPatterns.Add(pattern);
        }
        nextAttack = 0;
        currentPattern = Random.Range(0, attackPatterns.Count());
        predictionLength = 7 - intelligence;
        if (predictionLength < 2) { predictionLength = 2; }
        counterMetric = predictionLength switch
        {
            <= 2 => 3,
            >= 3 => 2
        };
        return attackPatterns;
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
                    animator.SetFloat("hitMulti", clip.length / impactDelay);
                    break;
            }
        }
    }
}
