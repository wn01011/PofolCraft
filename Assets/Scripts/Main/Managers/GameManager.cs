using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    #region variables

    #region FilePath
    private readonly string mainDataFileName = "Data.json";
    private string filePath = "";
    #endregion

    #region Managers
    private SoundManager soundManager = null;
    private AimController aim = null;
    private OptionManager optionManager = null;
    private ScreenShot screenShot = null;
    #endregion

    #region player Resources
    public static int gold  = 0;
    public static int exp   = 0;
    public static int level = 0;
    public static float time  = 0;
    public static string id = null;
    #endregion

    #region SkillUp
    public static int AtkUp = 0;
    public static int HpUp = 0;
    public static int AtkSpeedUp = 0;
    public static int SpeedUp = 0;
    public static int JumpUp = 0;
    public static int SplashDmgUp = 0;
    #endregion

    #region Quest
    //Bit-Flag
    public static int questClear = 0;
    public static int questProgress = 0;
    #endregion

    private Player player = null;
    public MainData mainData = new MainData();

    //periodical Save WaitTime Cache
    private readonly WaitForSecondsRealtime wait30sec = new WaitForSecondsRealtime(30f);

    public static int expRequirement = 5;
    public static int stageNum = 0;
    private GameOverText gameOverText = null; 
    #endregion

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if(FindObjectsOfType<GameManager>().Length != 1)
        {
            Destroy(FindObjectsOfType<GameManager>()[1].gameObject);
        }

        mainData = null;
        mainData = new MainData();

        AssignFilePath();
        Load();

    }
    private void Start()
    {
        StartCoroutine(WaitForMainScene());
    }

    private IEnumerator WaitForMainScene()
    {
        Debug.Log("Wait For Main!");

        mainData = null;
        mainData = new MainData();

        AssignFilePath();
        Load();

        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Main");

        Save();

        Initialization();

        StartCoroutine(PeriodicalSaveCo());

        Debug.Log("Wait For Lobby");
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Lobby");

        StartCoroutine(WaitForMainScene());
    }

    public void Initialization()
    {
        soundManager = FindObjectOfType<SoundManager>();
        aim = FindObjectOfType<AimController>();
        optionManager = FindObjectOfType<OptionManager>();
        player = FindObjectOfType<Player>();


        screenShot = FindObjectOfType<ScreenShot>();
        gameOverText = FindObjectOfType<GameOverText>();
        if(gameOverText)
            gameOverText.gameObject.SetActive(false);
    }

    //auto save coroutine for each 30secs
    private IEnumerator PeriodicalSaveCo()
    {
        Load();

        expRequirement = level * 5 + 5;

        #region Init Func

        player.Init();
        optionManager.Init();

        #endregion

        yield return wait30sec;

        while (!player.isDie)
        {
            float timer = 0f;
            while(timer <= 30.0f)
            {
                timer += Time.deltaTime;
                if (player.isDie) break;
                yield return null;
            }
            screenShot.takeHiResShot = true;
            Save();
        }

        //player.isDie == true
        gameOverText.gameObject.SetActive(true);
        ReSetDataWhenPlayerDie();
        Save();
        //Wait gameOverText Typing End (magic number = 7.05f)
        yield return new WaitForSeconds(7.05f);

        SceneManager.LoadScene(1);
    }

    private void Update()
    {
        //calculate elapsed Time;
        time += Time.fixedUnscaledDeltaTime;

        LevelUpFunc();
    }

    #region Save & Load Func
    public void Save()
    {
        SetData();
        DataManager.Single().SaveFile(filePath, mainData);
        Debug.Log("Save");
    }
    public void Load()
    {
        if(!File.Exists(filePath))
        {
            Debug.Log("File Don't Exist");
            DataManager.Single().SaveFile(filePath, mainData);
        }
        DataManager.Single().LoadFile(filePath, ref mainData);

        GetData();
    }
    #endregion

    #region Get & Set Data From MainData
    private void SetData()
    {
        if (soundManager)
        {
            //Sound
            mainData.BGMVolume = soundManager.BGMVolume;
            mainData.SFXVolume = soundManager.SFXVolume;
        }

        //Resolution & FullScreen
        mainData.fullScreen     = Screen.fullScreenMode;
        mainData.width          = Screen.currentResolution.width;
        mainData.height         = Screen.currentResolution.height;
        
        if (aim)
        {
            //Mouse
            mainData.MouseX = aim.xSensitivity;
            mainData.MouseY = aim.ySensitivity;
            mainData.MouseTotal = aim.totalSensitivity;
        }
        //Player
        mainData.gold           = gold;
        mainData.exp            = exp;
        mainData.level          = level;
        mainData.time           = time;
        mainData.id             = id;
        //Quest
        mainData.questClear     = questClear;
        mainData.questProgress  = questProgress;
        //
        mainData.AtkUp          = AtkUp;
        mainData.HpUp           = HpUp;
        mainData.AtkSpeedUp     = AtkSpeedUp;
        mainData.SpeedUp        = SpeedUp;
        mainData.JumpUp         = JumpUp;
        mainData.SplashDmgUp    = SplashDmgUp;
    }
    private void GetData()
    {
        #region MainData
        if(soundManager)
        {
            //Sound
            soundManager.BGMVolume = mainData.BGMVolume;
            soundManager.SFXVolume = mainData.SFXVolume;
        }
        
        //Resolution & FullScreen
        Screen.fullScreenMode  = mainData.fullScreen;
        Screen.SetResolution(mainData.width, mainData.height, mainData.fullScreen);
        
        if(aim)
        {
            //Mouse
            aim.xSensitivity = mainData.MouseX;
            aim.ySensitivity = mainData.MouseY;
            aim.totalSensitivity = mainData.MouseTotal;
        }
        
        //Player
        gold                   = mainData.gold;
        exp                    = mainData.exp;
        level                  = mainData.level;
        time                   = mainData.time;
        id                     = AccountDataManager.UID;
        
        //Quest
        questClear             = mainData.questClear;
        questProgress          = mainData.questProgress;
        #endregion

        #region SkillUpgradeData
        AtkUp                  = mainData.AtkUp;
        HpUp                   = mainData.HpUp;
        AtkSpeedUp             = mainData.AtkSpeedUp;
        SpeedUp                = mainData.SpeedUp;
        JumpUp                 = mainData.JumpUp;
        SplashDmgUp            = mainData.SplashDmgUp;
        #endregion
    }
    #endregion

    private void AssignFilePath()
    {
        filePath = Application.persistentDataPath;
        filePath += AccountDataManager.UID + mainDataFileName;
    }

    private void LevelUpFunc()
    {
        if(exp >= expRequirement && player)
        {
            exp -= expRequirement;
            expRequirement += 5;

            ++level;
            //upgrade player's maxHp to +5 and give full hp
            player.LevelUp();
        }
    }

    private void ReSetDataWhenPlayerDie()
    {
        level = 0;
        exp = 0;
        questClear = 0;
        questProgress = 0;
    }
}