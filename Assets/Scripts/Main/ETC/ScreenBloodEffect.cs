using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ScreenBloodEffect : MonoBehaviour
{
    private Texture defaultTex = null;
    private Texture bloodTex = null;
    private Player player = null;

    private float alpha = 0f;
    private float timer = 0f;

    private void Start()
    {
        player = GetComponent<Player>();
        bloodTex = Resources.Load<Texture>("Textures/blood Splatter");
        defaultTex = Resources.Load<Texture>("Textures/frame");
    }

    private void OnGUI()
    {
        //alpha to player hp remains
        float calculatedAlpha = 0.8f * (player.GetMaxHp() - player.GetHp()) / player.GetMaxHp();

        alpha = Mathf.Lerp(alpha, calculatedAlpha, Time.deltaTime);
        timer += Time.deltaTime;

        //border Blood Color
        Color color = new Color(1, 1, 1, alpha);
        //center default Color
        Color red = new Color(0.5f, 0, 0, Mathf.Clamp01(alpha - 0.3f * Mathf.Abs(Mathf.Sin(timer)) - 0.4f));

        //DrawTexture
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), defaultTex, ScaleMode.StretchToFill, true, 0, red, 0, 10);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bloodTex, ScaleMode.StretchToFill, true, 0, color, 0, 10);
    }

}
