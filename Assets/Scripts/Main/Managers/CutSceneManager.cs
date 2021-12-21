using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutSceneManager : MonoBehaviour
{
    [SerializeField]
    private Image cutScene = null;
    private Text text = null;
    public static bool isStart = false;

    private Sprite sprite = null;

    private void Start()
    {
        sprite = Resources.Load<Sprite>("Textures/CatBox");
        text = cutScene.GetComponentInChildren<Text>();
        
        StartCoroutine(LeftToRight(sprite));
        isStart = true;
    }

    //move cutScene left middle to right middle Coroutine
    private IEnumerator LeftToRight(Sprite _sprite)
    {

        yield return new WaitUntil(() => isStart);

        Canvas canvas = FindObjectOfType<Canvas>();
        float canvasScaleX = canvas.GetComponent<RectTransform>().localScale.x;
        float canvasScaleY = canvas.GetComponent<RectTransform>().localScale.y;
        isStart = false;

        text.text = _sprite.name;
        cutScene.gameObject.SetActive(true);

        float velocity = 0f;
        float posX = -Screen.width / canvasScaleX;
        cutScene.sprite = _sprite;
        float height = 0;
        //start Position
        cutScene.rectTransform.anchoredPosition = new Vector2(-Screen.width / canvasScaleX, height);
        while (cutScene.rectTransform.anchoredPosition.x < 0)
        {
            posX = Mathf.SmoothDamp(posX, 0.03f, ref velocity, 0.4f);
            cutScene.rectTransform.anchoredPosition = new Vector2(posX, height);
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        while (cutScene.rectTransform.anchoredPosition.x < Screen.width)
        {
            posX = Mathf.SmoothDamp(posX, (Screen.width + 0.03f) / canvasScaleX, ref velocity, 0.4f);
            cutScene.rectTransform.anchoredPosition = new Vector2(posX, height);
            yield return null;
        }
        cutScene.gameObject.SetActive(false);
        _sprite = sprite;
        StartCoroutine(LeftToRight(_sprite));
    }
}
