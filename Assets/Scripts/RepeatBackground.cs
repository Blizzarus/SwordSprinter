using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RepeatBackground : MonoBehaviour
{
    Vector3 startPos;
    float gameSpeed;
    public float parralaxMulti;
    Vector2 repeatWidth;
    GameManager gameManager; 

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        startPos = transform.position;
        repeatWidth = GetComponentInChildren<SpriteRenderer>().bounds.size;
        gameSpeed = gameManager.moveSpeed;
    }

    void Update()
    {
        if (gameManager.encounter == false)
        {
            transform.Translate(Vector3.left * Time.deltaTime * gameSpeed * parralaxMulti);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (transform.position.x < startPos.x - repeatWidth.x)
        {
            transform.position = startPos;
        }
    }
}
