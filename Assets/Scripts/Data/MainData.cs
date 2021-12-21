using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainData
{
    #region Sound
    public float BGMVolume              = 1f;
    public float SFXVolume              = 1f;
    #endregion

    #region MouseSensitivity
    public float MouseX                 = 3f;
    public float MouseY                 = 3f;
    public float MouseTotal             = 2f;
    #endregion

    #region Resolution
    public int width                    = 640;
    public int height                   = 360;
    public FullScreenMode fullScreen    = FullScreenMode.ExclusiveFullScreen;
    #endregion

    #region Player
    public string id                    = null;
    public int gold                     = 0;
    public int level                    = 0;
    public int exp                      = 0;
    public float time                   = 0f;
    #endregion

    #region SkillUp
    public int AtkUp                    = 1;
    public int HpUp                     = 1;
    public int AtkSpeedUp               = 1;
    public int SpeedUp                  = 1;
    public int JumpUp                   = 1;
    public int SplashDmgUp              = 1;
    #endregion

    #region Quest
    public int questClear               = 0;
    public int questProgress            = 0;
    #endregion
}
