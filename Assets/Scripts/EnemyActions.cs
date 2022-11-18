using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActions : MonoBehaviour
{
    GameManager gameManager;
    PlayerController player;
    IEnumerator coroutine;
    public int HP;
    public GameManager.combatState enemyState;
    float speed;
    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        speed = gameManager.speed;
    }

    // Update is called once per frame
    void Update()
    {
        if(HP < 1)
        {
            gameManager.EncounterEnd();
            Destroy(gameObject);
        }

        if (gameManager.encounter == false)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        else
        {
            combatActions();
        }
    }

    void combatActions()
    {
        if(player.playerState is 0 or (GameManager.combatState)5)
        {
            
        }
        //else if ()
    }
}
