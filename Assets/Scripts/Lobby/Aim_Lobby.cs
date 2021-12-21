using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Aim_Lobby : MonoBehaviour
{
    

    private void Awake()
    {
        #region Initialization

        player = FindObjectOfType<Player_Lobby>().transform;
        playerCam = player.transform.Find("MainCamera");
        cameraAcceleration = cameraAccelerationLowLimit;

        defaultScreenSize[0] = 2560f;
        defaultScreenSize[1] = 1440f;
        screenSize[0] = Screen.width;
        screenSize[1] = Screen.height;
        screenRatio[0] = screenSize[0] / defaultScreenSize[0];
        screenRatio[1] = screenSize[1] / defaultScreenSize[1];

        #endregion
    }

    private void Start()
    {
        CursorLock();
        StartCoroutine(UpdateCoroutine());
        Debug.Log("StartCo");
    }
    private IEnumerator UpdateCoroutine()
    {
        while (player)
        {
            if(isCanMouseRotate)
            {
                Rotate(player, playerCam);
            }
            //CursorStateChange();
            yield return null;
        }
    }

    public void CursorStateChange()
    {
        if(Cursor.lockState == CursorLockMode.Locked)
        {
            CursorFree();
            isCanMouseRotate = false;
        }
        else
        {
            CursorLock();
            isCanMouseRotate = true;
        }
    }

    private void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void CursorFree()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void Rotate(Transform _target, Transform _targetCam)
    {
        /*
        // 마우스입력에 따른 카메라 회전값 반영
        참고 공식 (*출처 GDC (Resistance3 AimAssist발표))
        https://www.gdcvault.com/play/1017942/Techniques-for-Building-Aim-Assist
        abs_x = pow(abs(x_norm), 4.0)
        aby_y = abs(y_norm)
        
        camera_speed = (horizontal_turn_speed * abs_x + 
                                    vertical_turn_speed * abs_y) / (abs_x + abs_y)
        
        horizontal_turn_speed = camera_speed * x_norm * abs(x_norm)
        vertical_turn_speed = camera_speed * y_norm
        */

        hori = Input.GetAxis("Mouse X");
        verti = Input.GetAxis("Mouse Y");
        mouseDir = new Vector2(hori, verti).normalized;
        mouseDelta = new Vector2(hori, verti).sqrMagnitude * mouseDeltaConst;

        normHori = mouseDir.x;
        normVerti = mouseDir.y;

        absHori = Mathf.Pow(Mathf.Abs(normHori), 4);
        absVerti = Mathf.Abs(normVerti);

        if (mouseDir.sqrMagnitude != 0)
        {
            //카메라 회전 가속도
            if (cameraAcceleration <= 1f)
            {
                cameraSpeed = (absHori + absVerti);
                cameraAcceleration += Time.deltaTime * mouseDir.sqrMagnitude * cameraAccelerationCoefficient;
                if (cameraAcceleration > 1f)
                {
                    cameraAcceleration = 1f;
                }
            }
        }
        else
        {
            if (cameraAcceleration >= cameraAccelerationLowLimit)
            {
                cameraSpeed = 0f;
                if (cameraAcceleration < cameraAccelerationLowLimit)
                {
                    cameraAcceleration = cameraAccelerationLowLimit;
                }
                else
                {
                    cameraAcceleration -= Time.deltaTime * cameraAccelerationCoefficient * 0.5f;
                }
            }
        }
        cameraSpeed *= cameraAcceleration * Mathf.Clamp(mouseDelta, 0 , screenRatio[0]);
        cameraSpeed = Mathf.Abs(cameraSpeed);
        xAngle = normHori * cameraSpeed *  xSensitivity * totalSensitivity * rotateConstantX * Time.deltaTime * screenRatio[0];
        yAngle = normVerti * cameraSpeed * ySensitivity * totalSensitivity * rotateConstantY * Time.deltaTime * screenRatio[1];

        
        xAngle = Mathf.Clamp(xAngle, -5.0f * screenRatio[0] * xSensitivity, 5.0f * screenRatio[0] * xSensitivity);
        yAngle = Mathf.Clamp(yAngle, -5.0f * screenRatio[1] * ySensitivity, 5.0f * screenRatio[1] * ySensitivity);

        xRotation += yAngle * screenRatio[1];
        //위 아래 각도 제한 -90 ~ 90
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        _target.localRotation *= Quaternion.AngleAxis(xAngle, Vector3.up) ;
        _targetCam.localRotation = Quaternion.AngleAxis(xRotation , Vector3.left);//target근처에서만 보정을 적용 시켜야 한다.
    }

   
    

   

    #region variables

    #region sensitivity slider
    
    [Header("Senesitivity")] 
    [Range(0.1f, 10f)]
    public float xSensitivity = 3f;
    [Range(0.1f, 10f)]
    public float ySensitivity = 3f;
    [Range(0.1f, 10f)]
    public float totalSensitivity = 2f;
    [Range(1f, 30f)]
    public float cameraAccelerationCoefficient = 10f;

    #endregion

    #region RotateMouse

    private readonly float rotateConstantX = 2000f;
    private readonly float rotateConstantY = 6000f;


    private float hori = 0f;
    private float verti = 0f;
    private float normHori = 0f;
    private float normVerti = 0f;
    private float xRotation = 0f;
    private float absHori = 0f;
    private float absVerti = 0f;
    private float cameraSpeed = 0f;
    private float xAngle = 0f;
    private float yAngle = 0f;
    private Vector2 mouseDir = Vector2.zero;
    private float mouseDelta = 0f;
    private readonly float mouseDeltaConst = 0.2f;
    private bool isCanMouseRotate = true;

    [SerializeField] private float cameraAcceleration = 0f;
    private float cameraAccelerationLowLimit = 0.4f;

    #endregion

    #region general

    private Transform player = null;
    private Transform playerCam = null;
    private readonly float[] screenSize = new float[2];
    private readonly float[] defaultScreenSize = new float[2];
    private readonly float[] screenRatio = new float[2];
    

    public Image[] rectImg = new Image[4];


    #endregion

    #endregion
}
