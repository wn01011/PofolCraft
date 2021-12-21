using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    public GameObject hpBar = null;
    private RectTransform hpBarTr = null;
    private Canvas canvas = null;
    private SpawnManager spawnManager = null;
    private Vector2 screenPos = Vector2.zero;
    private float height = 0f;
    private Player player = null;
    private float hpLerpSpeed = 0f;
    private float detectRange = 0f;
    private float canvasScaleX = 0f;
    private float canvasScaleY = 0f;

    private void Start()
    {
        #region Initialization

        player = FindObjectOfType<Player>();
        hpLerpSpeed = 5f;

        height = gameObject.GetComponent<BoxCollider>().bounds.size.y;

        hpBar = Resources.Load<GameObject>("Prefabs/HpBar/HpBar");
        hpBarTr = hpBar.GetComponent<RectTransform>();
        hpBar.GetComponent<Slider>().maxValue = gameObject.GetComponent<Monster>().GetMaxHp();
        canvas = FindObjectOfType<Canvas>();
        canvasScaleX = canvas.GetComponent<RectTransform>().localScale.x;
        canvasScaleY = canvas.GetComponent<RectTransform>().localScale.y;
        spawnManager = FindObjectOfType<SpawnManager>();

        hpBar = Instantiate(hpBar, canvas.transform.Find("HpBars").transform);
        spawnManager.hpBarList.Add(hpBar);

        detectRange = VoxelData.ChunkWidth * VoxelData.ViewDistanceInChunks * 0.5f;
        #endregion
    }

    private void FixedUpdate()
    {
        Positioning();
        if(hpBar.activeSelf)
            HpUpdate();
    }

    private void Positioning()
    {
        screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * height);

        hpBar.GetComponent<RectTransform>().anchoredPosition = (screenPos - new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)) / new Vector2(canvasScaleX, canvasScaleY);

    }
    private void HpUpdate()
    {
        float dist = Vector3.Distance(transform.position, player.transform.position);
        float hpBarvalue = hpBar.GetComponent<Slider>().value;
        hpBarvalue = Mathf.Lerp(hpBarvalue, gameObject.GetComponent<Monster>().GetHp(), Time.deltaTime * hpLerpSpeed);
        hpBar.GetComponent<RectTransform>().localScale = new Vector3(1 - 0.7f * (dist / detectRange), 1, 1);

        hpBar.GetComponent<Slider>().value = hpBarvalue;

    }
}
