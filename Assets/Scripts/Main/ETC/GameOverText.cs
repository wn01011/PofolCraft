using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverText : MonoBehaviour
{
    private Text text = null;
    private string textString = null;

    private void Awake()
    {
        text = GetComponent<Text>();
        textString = text.text;
    }

    private void OnEnable()
    {
        StartCoroutine(TypingCo());
    }

    private IEnumerator TypingCo()
    {
        for(int i = 0; i <= textString.Length; ++i)
        {
            text.text = textString.Substring(0, i);

            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(2.0f);

        for(int i=0; i<4; ++i)
        {
            text.text = (3 - i).ToString();
            yield return new WaitForSeconds(1.0f);
        }
    }
}
