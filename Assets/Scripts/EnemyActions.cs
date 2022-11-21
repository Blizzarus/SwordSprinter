using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EnemyActions : MonoBehaviour
{
    GameManager gameManager;
    TextMeshProUGUI enemyStatus;
    PlayerController player;
    public float delayState;
    public List<float> atkStates = new List<float>() { 0, 0, 0, 0 }; // Attack States: 0 = Left, 1 = Right, 2 = Up, 3 = Down
    // NOTE: Enemy attack states are named from the PLAYER perspective (0 = attack to PLAYER's left)

    int i; // index for Max (current active attack)
    [SerializeField] int intelligence; // determines pattern complexity and recognition ability
    [SerializeField] float AIDelay; // determines how long enemy takes to react and initiates new attacks
    float moveSpeed;
    float attackLength;
    float impactDelay;
    int HP;

    void Awake()
    {
        enemyStatus = GameObject.Find("EnemyStatus").GetComponent<TextMeshProUGUI>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        moveSpeed = gameManager.moveSpeed;
        attackLength = gameManager.attackLength;
        impactDelay = gameManager.impactDelay;
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
            atkStates[0] = attackLength;
        }
    }

    void updateStates()
    {
        i = atkStates.IndexOf(atkStates.Max());

        // update delay state
        if (delayState > 0)
        {
            delayState -= Time.deltaTime / 10;
            if (delayState < 0) { delayState = 0; }
        }

        // update attack states
        if (atkStates[i] > 0)
        {
            atkStates[i] -= Time.deltaTime / 10;
            if (atkStates[i] <= 0)
            {
                player.takeDamage();
                atkStates[i] = 0;
                delayState = impactDelay + AIDelay / 2;
            }
        }
    }

    public void clash(int x)
    {
        delayState = impactDelay + AIDelay;
        atkStates[x] = 0;
    }

    public void counter(int x)
    {
        player.countered(x);
        atkStates[x] = 0;
    }

    public void countered(int x)
    {
        delayState = attackLength + AIDelay;
        atkStates[x] = 0;
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
            delayState = impactDelay;
        }
    }
}
