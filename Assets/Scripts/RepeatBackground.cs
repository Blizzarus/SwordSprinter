using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatBackground : MonoBehaviour
{
    Vector3 startPos;
    float speed;
    float repeatWidth;
    GameManager gameManager; 

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        startPos = transform.position;
        repeatWidth = GetComponent<BoxCollider>().size.x / 2;
        speed = gameManager.speed;
    }

    void Update()
    {
        if (gameManager.encounter == false)
        {
            transform.Translate(Vector3.left * Time.deltaTime * speed);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (transform.position.x < startPos.x - repeatWidth)
        {
            transform.position = startPos;
        }
    }
}
