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
    public int HP;
    public float delayState;
    // Attack States: 0 = Left, 1 = Right, 2 = Up, 3 = Down
    // NOTE: Enemy attack states are named from the PLAYER perspective (0 = attack to PLAYER's left)
    public List<float> atkStates = new List<float>() { 0, 0, 0, 0 };
    float speed;
    // Start is called before the first frame update
    void Awake()
    {
        enemyStatus = GameObject.Find("EnemyStatus").GetComponent<TextMeshProUGUI>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        speed = gameManager.speed;
        HP = 3;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.encounter)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        else
        {
            combatActions();
        }
    }

    private void LateUpdate()
    {
        enemyStatus.text = "Stagger: " + delayState + "\nLeft: " + atkStates[0] + "\nRight: " + atkStates[1];
    }

    void combatActions()
    {
        if (delayState == 0 && atkStates.Max() == 0)
        {
            atkStates[0] = 1.0f;
        }

        // update delay state
        if (delayState > 0)
        {
            delayState -= Time.deltaTime / 10;
            if (delayState < 0) { delayState = 0; }
        }

        // update attack states
        if (atkStates.Max() > 0)
        {
            int i = atkStates.IndexOf(atkStates.Max());
            atkStates[i] -= Time.deltaTime / 10;
            if (atkStates[i] <= 0)
            {
                player.takeDamage();
                atkStates[i] = 0;
                delayState = 0.2f;
            }
        }
    }

    public void clash(int i)
    {
        delayState = atkStates[i];
        atkStates[i] = 0;
    }

    public void takeDamage()
    {
        HP--;
        if (HP <= 0)
        {
            gameManager.EncounterEnd();
            Destroy(gameObject);
        }
        else
        {
            delayState = 5.0f;
        }
    }
}
