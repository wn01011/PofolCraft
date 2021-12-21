using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AimController : MonoBehaviour
{
    private void Awake()
    {
        #region Initialization

        player = FindObjectOfType<Player>().transform;
        canvas = FindObjectOfType<Canvas>();
        canvasScaleX = canvas.GetComponent<RectTransform>().localScale.x;
        playerGo = player.transform.Find("Body");
        playerCam = player.transform.Find("Main Camera");
        cameraAcceleration = cameraAccelerationLowLimit;

        defaultScreenSize[0] = 800f;
        defaultScreenSize[1] = 600f;
        

        #endregion
    }

    private void Start()
    {
        SetScreenRatio();
        StartCoroutine(UpdateCoroutine());
        //CursorLock();
        CursorFree();
    }
    private IEnumerator UpdateCoroutine()
    {
        while (player)
        {
            GetEnemy();
            Rotate(player, playerCam);
            GetEnemyPos();
            
            yield return null;
        }
    }

    public void SetScreenRatio()
    {
        screenSize[0] = Screen.width;
        screenSize[1] = Screen.height;
        screenRatio[0] = screenSize[0] / defaultScreenSize[0];
        screenRatio[1] = screenSize[1] / defaultScreenSize[1];
        canvasScaleX = canvas.GetComponent<RectTransform>().localScale.x;
        canvasScaleY = canvas.GetComponent<RectTransform>().localScale.y;
        canvasScale = new Vector2(canvasScaleX, canvasScaleY);
    }

    private void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void CursorFree()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Rotate(Transform _target, Transform _targetCam)
    {
        //if frame rate too low, skip Rotate
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        if (fps <= 10f) return;

        /*
        // ���콺�Է¿� ���� ī�޶� ȸ���� �ݿ�
        ���� ���� (*��ó GDC (Resistance3 AimAssist��ǥ))
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

        //ī�޶� ȸ�� ���ӵ� : ���콺 �����϶� ���, ���������� �ϰ�, ���Ѱ� 1f, ���Ѱ� cameraAceelerationLowLimit
        //ȸ�� ���ӵ��� ���� ȸ���� ����
        if (mouseDir.sqrMagnitude != 0)
        {
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
        
        //ȸ������ ���� �ִ� ���� = ���콺 �Է°�, ���콺 � ����(���, ������ �), ����, ��ũ�� ũ�⺸����
        xAngle = normHori * cameraSpeed *  xSensitivity * totalSensitivity * rotateConstantX * Time.deltaTime * screenRatio[0];
        yAngle = normVerti * cameraSpeed * ySensitivity * totalSensitivity * rotateConstantY * Time.deltaTime * screenRatio[1];

        xAngle += AimAssistance(enemyInScreenList, _targetCam).x * screenRatio[0] * 0.4f * (Mathf.Clamp01(mouseDelta * 5f));
        yAngle += AimAssistance(enemyInScreenList, _targetCam).y * screenRatio[1] * 0.4f * (Mathf.Clamp01(mouseDelta * 5f));

        //�������� Ƣ�� deltaTime�� Ŀ���� �����ϰ� ȸ������ Ŀ���°��� �����ϱ� ����
        //���������� ��ó�� fps�� ����ġ �����϶� Rotate�Լ��� return�� ��Ű�� �ִ�.
        //�ι�°�� ȸ������ Clamp�� ����ġ�� ���س��� �����̴�.
        xAngle = Mathf.Clamp(xAngle, -2.0f * screenRatio[0] * xSensitivity * totalSensitivity, 2.0f * screenRatio[0] * xSensitivity * totalSensitivity);
        yAngle = Mathf.Clamp(yAngle, -2.0f * screenRatio[1] * ySensitivity * totalSensitivity, 2.0f * screenRatio[1] * ySensitivity * totalSensitivity);

        xRotation += yAngle * screenRatio[1];
        //�� �Ʒ� ���� ���� -90 ~ 90
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        _target.localRotation *= Quaternion.AngleAxis(xAngle, Vector3.up) ;
        _targetCam.localRotation = Quaternion.AngleAxis(xRotation , Vector3.left);
    }

    private Vector2 AimAssistance(List<GameObject> _enemies, Transform _targetCam)
    {
        //���� ȭ�鿡 ���Ͱ� 10�����̻� ���̰ų� ������ ���� ��ý�Ʈ�� ���� �ʴ´�.
        if (_enemies.Count >= 10 || _enemies.Count <= 0) return Vector2.zero;

        assistVec = Vector3.zero;
        for(int i=0; i< _enemies.Count; ++i)
        {
            if (!_enemies[i])
            {
                _enemies.RemoveAt(i);
                continue;
            }
            if (!_enemies[i].GetComponent<BoxCollider>()) return Vector2.zero;

            Vector2 enimScreenPos = Camera.main.WorldToScreenPoint(_enemies[i].GetComponent<BoxCollider>().bounds.center);

            Ray monsterRay = Camera.main.ScreenPointToRay(enimScreenPos);
            RaycastHit raycastHit = new RaycastHit();
            Vector2[,] imagineHitBoxes = new Vector2[2,2];

            if(Physics.Raycast(monsterRay, out raycastHit) && raycastHit.collider.CompareTag("Enemy"))
            {
                //HitBox vector2[2]�� �ְ� �װ� ���ΰ��ִ� �� ū vector2[2] assistZone? �� �ִ�.
                //imagineHitBox[0,?] �� ��Ʈ�ڽ�, imagineHitBox[1,?] �� assistZone�̴�.
                //----1
                //|   |
                //0----
                Collider curCollider = raycastHit.collider;
                //curCollider�� ���ǰ� 80�̻��϶� aimassist�� ���� �ʴ´�
                if( curCollider.bounds.size.x * curCollider.bounds.size.y * curCollider.bounds.size.z >= 80f)
                {
                    return Vector3.zero;
                }

                #region ImagineHitBox Positioning

                Vector3 lookDir = (curCollider.transform.position - _targetCam.position).normalized;
                float dist = Vector3.Distance(curCollider.transform.position, _targetCam.position);
                float curColliderSizeX = curCollider.bounds.size.x;
                float curColliderSizeY = curCollider.bounds.size.y;
                Quaternion lookRotation = Quaternion.LookRotation(lookDir);
                Vector3 rectSize = new Vector3(curColliderSizeX, curColliderSizeY, 0f) * dist * 0.04f;
                rectSize = lookRotation * rectSize;
                
                imagineHitBoxes[0, 0] = Camera.main.WorldToScreenPoint(curCollider.bounds.center - rectSize) / canvasScale;
                imagineHitBoxes[0, 1] = Camera.main.WorldToScreenPoint(curCollider.bounds.center + rectSize) / canvasScale;
                imagineHitBoxes[1, 0] = Camera.main.WorldToScreenPoint(curCollider.bounds.center - rectSize * assistZoneConst) / canvasScale;
                imagineHitBoxes[1, 1] = Camera.main.WorldToScreenPoint(curCollider.bounds.center + rectSize * assistZoneConst) / canvasScale;
                #endregion

                #region DebugImagineHitBoxes

                rectImg[0].rectTransform.anchoredPosition = imagineHitBoxes[0, 0] - (new Vector2(screenSize[0], screenSize[1]) * 0.5f) / canvasScale;
                rectImg[1].rectTransform.anchoredPosition = imagineHitBoxes[0, 1] - (new Vector2(screenSize[0], screenSize[1]) * 0.5f) / canvasScale;
                rectImg[2].rectTransform.anchoredPosition = imagineHitBoxes[1, 0] - (new Vector2(screenSize[0], screenSize[1]) * 0.5f) / canvasScale;
                rectImg[3].rectTransform.anchoredPosition = imagineHitBoxes[1, 1] - (new Vector2(screenSize[0], screenSize[1]) * 0.5f) / canvasScale;

                #endregion
            }
            

            #region Assist Condition
            //Input.mousePosition �� ���� �ȵȴ�.(Lock�ɾ���� ������ Vector2.zero��)
            if (screenSize[0] * 0.5f < imagineHitBoxes[0,0].x * canvasScaleX && screenSize[0] * 0.5f >= imagineHitBoxes[1,0].x * canvasScaleX)
            {
                assistVec += Vector2.right;
                if (mouseDir.x <= 0)
                    assistVec += Vector2.right * 10f;
            }
            
            else if(screenSize[0] * 0.5f > imagineHitBoxes[0,1].x * canvasScaleX && screenSize[0] * 0.5f <= imagineHitBoxes[1,1].x * canvasScaleX)
            {
                assistVec += Vector2.left;
                if (mouseDir.x >= 0)
                    assistVec += Vector2.left * 10f;
            }
            
            
            if(screenSize[1] * 0.5f < imagineHitBoxes[0,0].y * canvasScaleY && screenSize[1] * 0.5f >= imagineHitBoxes[1,0].y * canvasScaleY)
            {
                assistVec += Vector2.up;
                if (mouseDir.y <= 0)
                    assistVec += Vector2.up * 10f;
            }
            
            else if(screenSize[1] * 0.5f > imagineHitBoxes[0,1].y * canvasScaleY && screenSize[1] * 0.5f <= imagineHitBoxes[1,1].y * canvasScaleY)
            {
                assistVec += Vector2.down;
                if (mouseDir.y >= 0)
                    assistVec += Vector2.down * 10f;
            }
            #endregion

            assistVec.Normalize();
            assistVec = new Vector2(assistVec.x * assistHoriConst, assistVec.y * assistVertiConst);
        }

        if (assistVec != Vector2.zero)
        {
            return Vector3.Lerp(Vector3.zero, assistVec, Mathf.Clamp(Time.deltaTime * 100f, 0f, 0.5f));
        }
        else
            return Vector3.zero;
    }

    private void GetEnemy()
    {
        Monster[] enemyAry = spawnManager.monsterList.ToArray();
        if (enemyAry.Length <= 0) return;

        float distance = 100f;
        for (int i = 0; i < enemyAry.Length; ++i)
        {
            if (!enemyAry[i]) return;
            if (Vector3.Distance(enemyAry[i].transform.position, playerGo.transform.position) <= distance)
            {
                if (!enemies.Contains(enemyAry[i].gameObject))
                    enemies.Add(enemyAry[i].gameObject);
            }
            else
            {
                if (enemies.Contains(enemyAry[i].gameObject))
                    enemies.Remove(enemyAry[i].gameObject);
            }
        }
    }

    private void GetEnemyPos()
    {
        //camera�� field of view�� �ҷ��ͼ� enemy��� player.transform.forward�� ���� ����ѵ�
        //�þ߰� �ȿ� �ִ� �༮�鸸 enemyInScreenList�� �ҷ����δ�.
        fieldOfView = Camera.main.fieldOfView; //degrees

        for (int i=0; i<enemies.Count; ++i)
        {
            if (!enemies[i]) continue;
            float distance = (enemies[i].transform.position - player.transform.position).sqrMagnitude;

            //Vector3 enemyVec = Vector3.ProjectOnPlane(enemies[i].transform.position, Vector3.up)
            //    - Vector3.ProjectOnPlane(playerGo.forward, Vector3.up);
            Vector3 enemyVec = enemies[i].transform.position - playerGo.position;

            float angle = Mathf.Abs(Vector3.Angle(enemyVec, playerGo.forward) * 2f);
            if(fieldOfView > angle && distance <= 1000f)
            {
                if(!enemyInScreenList.Contains(enemies[i]))
                    enemyInScreenList.Add(enemies[i]);
            }
            else
            {
                if(enemyInScreenList.Contains(enemies[i]))
                {
                    enemyInScreenList.Remove(enemies[i]);
                }
            }
        }
        if(enemyInScreenList.Count == 0)
        {
            for (int i = 0; i < 4; ++i)
            {
                rectImg[i].rectTransform.anchoredPosition = Vector2.zero;
            }
        }
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

    private readonly float rotateConstantX = 108f;
    private readonly float rotateConstantY = 192f;

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
    
    [SerializeField] private float cameraAcceleration = 0f;
    private float cameraAccelerationLowLimit = 0.4f;

    #endregion

    #region general

    private Transform player = null;
    private Transform playerGo = null;
    private Transform playerCam = null;
    private Canvas canvas = null;
    private float canvasScaleX = 0f;
    private float canvasScaleY = 0f;
    private Vector2 canvasScale = Vector2.zero;
    private readonly float[] screenSize = new float[2];
    private readonly float[] defaultScreenSize = new float[2];
    private readonly float[] screenRatio = new float[2];
    [SerializeField]
    private List<GameObject> enemies = new List<GameObject>();
    [SerializeField]
    private SpawnManager spawnManager = null;
    private float fieldOfView = 0f;

    public Image[] rectImg = new Image[4];

    private float deltaTime = 0f;

    #endregion

    #region AimAssistBox
    [SerializeField]
    Vector2 assistVec = Vector2.zero;
    private readonly float assistHoriConst = 0.9f;
    private readonly float assistVertiConst = 0.6f;
    private readonly float assistZoneConst = 5f;
    [SerializeField]
    private List<GameObject> enemyInScreenList = new List<GameObject>();

    #endregion

    #endregion
}
