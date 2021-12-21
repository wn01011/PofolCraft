using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [HideInInspector] public static SoundManager Instance = null;

    #region variables
    [HideInInspector] public AudioSource BGMSource = null;
    [HideInInspector] public AudioSource SFXSource = null;

    public float BGMVolume = 1f;
    public float SFXVolume = 1f;

    [HideInInspector] public AudioClip BGM0 = null;
    [HideInInspector] public AudioClip BGM1 = null;
    [HideInInspector] public AudioClip BGM2 = null;
    [HideInInspector] public AudioClip Click = null;
    [HideInInspector] public AudioClip Explosion = null;
    [HideInInspector] public AudioClip FireImpact = null;
    [HideInInspector] public AudioClip GoldDrop = null;
    [HideInInspector] public AudioClip Hit = null;
    [HideInInspector] public AudioClip Hit2 = null;
    [HideInInspector] public AudioClip InWater = null;
    [HideInInspector] public AudioClip GunSilencer = null;
    [HideInInspector] public AudioClip Silencer = null;
    [HideInInspector] public AudioClip OK = null;
    [HideInInspector] public AudioClip Portal = null;
    [HideInInspector] public AudioClip Reload = null;
    [HideInInspector] public AudioClip Scroll = null;
    [HideInInspector] public AudioClip StoneBreakDown = null;
    [HideInInspector] public AudioClip Tack = null;
    [HideInInspector] public AudioClip WaterWalk = null;
    [HideInInspector] public AudioClip WaterWalk2 = null;
    [HideInInspector] public AudioClip WaterWalk3 = null;
    [HideInInspector] public AudioClip WoodBreak = null;
    [HideInInspector] public AudioClip Cat = null;
    [HideInInspector] public AudioClip BonFire = null;
    [HideInInspector] public AudioClip CampFire = null;
    [HideInInspector] public AudioClip FlameThrower = null;
    [HideInInspector] public AudioClip GrassFire = null;
    [HideInInspector] public AudioClip TreeFire = null;
    [HideInInspector] public AudioClip Blop = null;
    [HideInInspector] public AudioClip Jab = null;
    [HideInInspector] public AudioClip Teemo = null;
    [HideInInspector] public AudioClip Jump = null;
    #endregion

    private void Awake()
    {
        #region Singleton
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
                Destroy(this.gameObject);
        }
        #endregion
    }
    private void Start()
    {
        #region initialization
        BGMSource = GetComponents<AudioSource>()[0];
        SFXSource = GetComponents<AudioSource>()[1];

        BGMSource.volume = BGMVolume;
        SFXSource.volume = SFXVolume;

        BGM0 = Resources.Load<AudioClip>("Audios/BGM/Bonjour Jean Marie");
        BGM1 = Resources.Load<AudioClip>("Audios/BGM/식물 소개");
        BGM2 = Resources.Load<AudioClip>("Audios/BGM/저 놈 또 시작");
        Blop = Resources.Load<AudioClip>("Audios/SFX/MP_Blop");
        Jab = Resources.Load<AudioClip>("Audios/SFX/Jab");
        Teemo = Resources.Load<AudioClip>("Audios/SFX/Teemo");
        Jump = Resources.Load<AudioClip>("Audios/SFX/Jump");
        Click = Resources.Load<AudioClip>("Audios/SFX/Click");
        Explosion = Resources.Load<AudioClip>("Audios/SFX/Explosion");
        FireImpact = Resources.Load<AudioClip>("Audios/SFX/FireImpact");
        GoldDrop = Resources.Load<AudioClip>("Audios/SFX/GoldDrop");
        Hit = Resources.Load<AudioClip>("Audios/SFX/Hit");
        Hit2 = Resources.Load<AudioClip>("Audios/SFX/Hit2");
        InWater = Resources.Load<AudioClip>("Audios/SFX/InWater");
        GunSilencer = Resources.Load<AudioClip>("Audios/SFX/MP_Gun Silencer");
        Silencer = Resources.Load<AudioClip>("Audios/SFX/MP_Silencer");
        OK = Resources.Load<AudioClip>("Audios/SFX/OK");
        Portal = Resources.Load<AudioClip>("Audios/SFX/Portal");
        Reload = Resources.Load<AudioClip>("Audios/SFX/Reload");
        Scroll = Resources.Load<AudioClip>("Audios/SFX/Scroll");
        StoneBreakDown = Resources.Load<AudioClip>("Audios/SFX/StoneBreakDown");
        Tack = Resources.Load<AudioClip>("Audios/SFX/Tack");
        WaterWalk = Resources.Load<AudioClip>("Audios/SFX/WaterWalk");
        WaterWalk2 = Resources.Load<AudioClip>("Audios/SFX/WaterWalk2");
        WaterWalk3 = Resources.Load<AudioClip>("Audios/SFX/WaterWalk3");
        WoodBreak = Resources.Load<AudioClip>("Audios/SFX/WoodBreak");
        Cat = Resources.Load<AudioClip>("Audios/SFX/Cat");
        BonFire = Resources.Load<AudioClip>("Audios/SFX/Fire/Bonfire");
        CampFire = Resources.Load<AudioClip>("Audios/SFX/Fire/Campfire");
        FlameThrower = Resources.Load<AudioClip>("Audios/SFX/Fire/FlameThrower");
        GrassFire = Resources.Load<AudioClip>("Audios/SFX/Fire/GrassFire");
        TreeFire = Resources.Load<AudioClip>("Audios/SFX/Fire/TreeFire");
        #endregion
    }
    private void Update()
    {
        BGMSource.volume = BGMVolume;
        SFXSource.volume = SFXVolume;
    }
}
