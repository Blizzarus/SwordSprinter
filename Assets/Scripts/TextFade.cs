using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextFade : MonoBehaviour
{
    TextMeshProUGUI text;
    [SerializeField] float fadeTime;
    [SerializeField] bool fadingIn;
    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<TextMeshProUGUI>();
        text.CrossFadeAlpha(0, 0, false);
        fadeTime = 0;
        fadingIn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(fadingIn) { FadeIn(); }
        else if(text.color.a != 0)
        {
            text.CrossFadeAlpha(0, 0.1f, false);
        }
    }

    void FadeIn()
    {
        text.CrossFadeAlpha(1, 0.1f, false);
        fadeTime += Time.deltaTime;
        if (text.color.a == 1 && fadeTime > 0.5f)
        {
            fadingIn = false;
            fadeTime = 0;
        }
    }

    public void StartFade()
    {
        fadingIn = true;
    }
}
