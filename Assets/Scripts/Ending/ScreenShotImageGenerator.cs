using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenShotImageGenerator : MonoBehaviour
{
    private string filePath = null;
    private int count = 0;
    private float scrollSpeed = 4f;
    private float ranDelay = 0f;
    private float totalDelay = 0f;

    [SerializeField]
    private GameObject screenshot = null;
    [SerializeField]
    private Text endText = null;
    private Canvas canvas = null;

    private void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        if (Application.isEditor)
            filePath = Application.dataPath + "/Resources/screenshots";
        else
            filePath = Application.persistentDataPath + "/screenshots";
        System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(filePath);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
        directoryInfo.Refresh();

        count = directoryInfo.GetFiles().Length;
        
        int divideNum = 0;
        int addNum    = 0;

        if (Application.isEditor) addNum = 2;
        else                      addNum = 1;

        for(int i = 0; i < count; i += addNum)
        {
            GameObject curScreenshot = Instantiate(screenshot, canvas.transform);
            
            string curScreeshotTextureString = directoryInfo.GetFiles()[i].Name;
            curScreeshotTextureString = curScreeshotTextureString.Split('.')[0];

            Texture2D curScreenshotTexture = new Texture2D(800, 400);
            if (Application.isEditor)
                curScreenshotTexture = Resources.Load<Texture2D>("screenshots/" + curScreeshotTextureString);
            else
            {
                string curPath = directoryInfo.GetFiles()[i].Name;
                if(System.IO.File.Exists(filePath +"/"+ curPath))
                {
                    byte[] curScreenshotTextureBytes = System.IO.File.ReadAllBytes(filePath +"/"+ curPath);
                    curScreenshotTexture.LoadImage(curScreenshotTextureBytes, false);
                }
            }

            Rect rect = new Rect(0, 0, curScreenshotTexture.width, curScreenshotTexture.height);
            Sprite curSprite = Sprite.Create(curScreenshotTexture, rect, new Vector2(0.5f, 0.5f), 100);
            curSprite.name = curScreeshotTextureString;
            curScreenshot.GetComponent<Image>().sprite = curSprite;

            if (Application.isEditor) divideNum = 2;
            else
                divideNum = 1;
            StartCoroutine(ScrollUpCo(curScreenshot, i / divideNum));
        }
        StartCoroutine(ScrollUpCo(endText.gameObject, (int)totalDelay));
    }
    private IEnumerator ScrollUpCo(GameObject _screenshot, int _count)
    {
        float timer = 0f;
        Vector2 focus = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        float posX = Random.Range(Screen.width * 0.2f, Screen.width * 0.8f) - focus.x;
        float posY = 0f - focus.y - Screen.height * 0.2f;
        if (_screenshot.GetComponent<Text>())
        {
            posX = 0f;
        }

        _screenshot.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, posY);
        ranDelay = Random.Range(1.5f, 2.0f);
        totalDelay += ranDelay;

        if(_screenshot == endText.gameObject)
        {
            yield return new WaitForSeconds(totalDelay);
        }
        else
        {
            yield return new WaitForSeconds(_count * ranDelay);
        }

        while (timer <= 20f)
        {
            if(_screenshot.GetComponent<Text>() && posY >= 0f)
            {
                break;
            }
            posY += (Screen.height / 20) * scrollSpeed * Time.deltaTime;
            _screenshot.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, posY);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
