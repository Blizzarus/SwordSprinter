using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnemyActions : MonoBehaviour
{
    GameManager gameManager;
    TextMeshProUGUI enemyStatus;
    Animator animator;
    PlayerController player;

    List<List<int>> attackPatterns; // list of lists of ints used to track enemy attack patterns
    int currentPattern; // list index to track current active attack pattern
    int nextAttack; // list inex to track next attack in pattern

    List<int> playerAttackHistory; // list of all attacks the player has executed
    List<List<int>> predictionPatterns; // list of patterns recorded; length varies by enemy intelligence
    int predictAttack; // value corresponding to the expected next attack

    public float delayState;
    public List<float> atkStates = new List<float>() { 0, 0, 0, 0 }; // Attack States: 0 = Left, 1 = Right, 2 = Up, 3 = Down
    // NOTE: Enemy attack states are named from the PLAYER perspective (0 = attack to PLAYER's left)

    int i; // index for Max (current active attack)
    int intelligence; // determines pattern complexity and recognition ability
    float AIDelay; // determines how long enemy takes to react and initiate new attacks
    float moveSpeed;
    float attackLength;
    float impactDelay;
    float counterThreshold;
    int HP;

    void Awake()
    {
        enemyStatus = GameObject.Find("EnemyStatus").GetComponent<TextMeshProUGUI>();
        animator = GetComponent<Animator>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        intelligence = gameManager.enemyIntelligence;
        AIDelay = gameManager.enemyDelay;
        moveSpeed = gameManager.moveSpeed;
        attackLength = gameManager.attackLength;
        impactDelay = gameManager.impactDelay;
        counterThreshold = gameManager.counterThreshold;


        attackPatterns = setAttackPatterns();
        string print;
        foreach(List<int> list in attackPatterns)
        {
            print = string.Join(',', list);
            Debug.Log(print);
        }

        playerAttackHistory = new List<int>();
        predictionPatterns = new List<List<int>>();
        predictAttack = -1;

        HP = 3;
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
            combatActions();

            // update Atk and Delay states
            updateStates();
        }
    }

    private void LateUpdate()
    {
        enemyStatus.text = "Delay: " + delayState + "\nLeft: " + atkStates[0] + "\nRight: " + atkStates[1] + "\nUp: " + atkStates[2] + "\nDown: " + atkStates[3];
    }

    void combatActions()
    {
        if (delayState == 0 && atkStates[i] == 0)
        {
            int j = player.atkStates.IndexOf(player.atkStates.Max());
            if (player.atkStates[j] > 0)
            {
                attack(j);
                return;
            }

            if (nextAttack == attackPatterns[currentPattern].Count())
            {
                newPattern();
                return;
            }

            j = attackPatterns[currentPattern][nextAttack];
            nextAttack++;

            // on counter applicable
            if (player.atkStates[j] > counterThreshold)
            {
                countered(j);
                return;
            }

            // otherwise, initate attack
            attack(j);
        }
    }

    void newPattern()
    {
        currentPattern = Random.Range(0, attackPatterns.Count());
        nextAttack = 0;
        delayState = AIDelay;
    }

    void updateStates()
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
            if (atkStates[i] <= 0)
            {
                player.takeDamage();
                atkStates[i] = 0;
                delayState = impactDelay;
            }
        }
    }

    void attack(int x)
    {
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

    public bool CounterPlayer(int x)
    {
        addPlayerPatterns(x);
        foreach (List<int> pattern in predictionPatterns)
        {
            predictAttack = Search(playerAttackHistory, pattern);
            if (predictAttack != -1) { break; }
        }
        Debug.Log("Predicted " + predictAttack);
        if (x == predictAttack && (atkStates[i] > counterThreshold || atkStates[i] == 0))
        {
            predictAttack = -1;
            return true;
        }
        return false;
    }

    void addPlayerPatterns(int x)
    {
        playerAttackHistory.Add(x);
        int i = 4 - (intelligence);
        int j = playerAttackHistory.Count();
        int d = j % i;
        if (d == 0)
        {
            List<int> pattern = new List<int>();
            for (int k = i; k >= 1; k--)
            {
                pattern.Add(playerAttackHistory[j - k]);
            }
            predictionPatterns.Add(pattern);
        }
    }

    int Search(List<int> src, List<int> pattern)
    {
        int maxFirstCharSlot = src.Count() - pattern.Count() + 1;
        for (int i = 0; i < maxFirstCharSlot; i++)
        {
            if (src[i] != pattern[0]) // compare first value
                continue;

            // found a match on first value, now try to match rest of the pattern
            for (int j = pattern.Count() - 1; j >= 1; j--)
            {
                if (src[i + j] != pattern[j]) break;
                if (j == 1) return i;
            }
        }
        return -1;
    }

    public void clash(int x)
    {
        delayState = impactDelay;
        atkStates[x] = 0;
        animator.SetTrigger("startClash");
    }

    public void countered(int x)
    {
        newPattern();
        delayState += attackLength;
        animator.SetTrigger("startClash");
        player.atkStates[x] = atkStates[x] = 0;
    }

    public void takeDamage()
    {
        HP--;
        Debug.LogWarning("Enemy Damage!  HP = " + HP);
        if (HP <= 0)
        {
            gameManager.EncounterEnd();
            Destroy(gameObject);
        }
        else
        {
            animator.SetTrigger("startClash");
            atkStates[i] = 0;
            newPattern();
        }
    }

    List<List<int>> setAttackPatterns()
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
        return attackPatterns;
    }
}
