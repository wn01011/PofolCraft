using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    #region variables
    private Player player = null;
    private World world = null;
    private Animator animator = null;
    private AudioSource audioSource = null;
    private ScreenShot screenShot = null;

    [SerializeField]
    private GameObject[] cubes = new GameObject[4];
    private Slider hpBar = null;

    private float cubeRotateSpeed = 50f;
    private float cubesRotateSpeed = 10f;
    private float cubeRadius = 0.3f;
    private float cubeTimer = 0f;

    private float monsterWidth = 0f;
    private float detectRange = 30f;
    private float fightRange = 20f;
    private float walkSpeed = 0.8f;

    private float vertical = 0f;
    private float horizontal = 0f;
    private float rotateSpeed = 5f;
    private float verticalMomentum = 0f;
    private bool jumpRequest = false;
    private float jumpForce = 5f;

    private float _gravity = -9.8f;
    private Vector3 velocity = Vector3.zero;

    private float hp = 0f;
    private float maxHp = 100f;
    private float distance = 0f;

    private bool isDie = false;
    private bool onDie = false;
    private bool knockDown = false;
    private bool isActivated = false;

    private AudioClip treeBurn = null;

    [HideInInspector]
    public int phase = 1;

    #endregion

    public enum EState
    {
        IDLE = 1,
        CHASE,
        HIDE,
        KNOCKDOWN,
        CAST,
    }
    protected enum EAnimState
    {
        STOP = 1,
        IDLE,
        OPEN,
        CLOSE,
    }
    
    public EState _state = EState.IDLE;
    [SerializeField]
    private EAnimState _animState = EAnimState.STOP;


    private void Start()
    {
        #region Initialization
        hp = maxHp;
        isDie = false;

        hpBar = GameObject.FindGameObjectWithTag("BossHpBar").GetComponent<Slider>();
        hpBar.maxValue = maxHp;
        hpBar.value = hp;
        hpBar.gameObject.SetActive(false);

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        screenShot = FindObjectOfType<ScreenShot>();
        player = FindObjectOfType<Player>();
        world = FindObjectOfType<World>();
        monsterWidth = GetComponent<BoxCollider>().size.x * 0.5f;
        gameObject.AddComponent<Rigidbody>();
        gameObject.GetComponent<Rigidbody>().useGravity = false;
        treeBurn = SoundManager.Instance.TreeFire;
        #endregion

    }
    private void FixedUpdate()
    {
        RotateToPlayer();
        RotateCubes();
        CalculateVelocity();

        Move();
        if (jumpRequest)
            Jump();
    }

    private void Update()
    {
        HpBarUpdate();
        ChangeState();
        AnimationStateUpdate();
        ChangeAnimState();
        Action();
    }

    private void HpBarUpdate()
    {
        distance = Vector3.Distance(transform.position, player.transform.position);
        if(isDie)
        {
            SoundManager.Instance.BGMSource.clip = SoundManager.Instance.BGM1;
            if (!SoundManager.Instance.BGMSource.isPlaying)
                SoundManager.Instance.BGMSource.Play();
            BossSound(false);
            hpBar.gameObject.SetActive(false);
        }
        else if(distance <= detectRange)
        {
            SoundManager.Instance.BGMSource.clip = SoundManager.Instance.BGM2;
            if(!SoundManager.Instance.BGMSource.isPlaying)
                SoundManager.Instance.BGMSource.Play();
            BossSound(true);

            hpBar.gameObject.SetActive(true);
            hpBar.value = Mathf.Lerp(hpBar.value, hp, Time.deltaTime * 3f);
        }
        else
        {
            SoundManager.Instance.BGMSource.clip = SoundManager.Instance.BGM0;
            if (!SoundManager.Instance.BGMSource.isPlaying)
                SoundManager.Instance.BGMSource.Play();
            hpBar.gameObject.SetActive(false);
        }
    }

    #region Finite State Machine Real & Anim State
    private void ChangeState()
    {
        if (!player) return;

        if(isDie)
        {
            _state = EState.HIDE;
            return;
        }
        else if(knockDown)
        {
            _state = EState.KNOCKDOWN;
            return;
        }


        if(distance <= fightRange)
        {
            _state = EState.CAST;
        }
        else if(distance < detectRange && distance > fightRange)
        {
            _state = EState.CHASE;
        }
        else
        {
            _state = EState.IDLE;
        }
    }

    private void ChangeAnimState()
    {
        if (!player) return;

        if (isDie && !onDie)
        {
            StartCoroutine(DieCo());
        }
        else if(isDie)
        {
            return;
        }
        else if(!isDie && _state == EState.KNOCKDOWN)
        {
            _animState = EAnimState.IDLE;
        }
        else if(!isDie && _state == EState.CAST)
        {
            _animState = EAnimState.OPEN;
        }
        else
        {
            _animState = EAnimState.IDLE;
        }
    }
    private IEnumerator DieCo()
    {
        onDie = true;
        _animState = EAnimState.CLOSE;
        yield return new WaitForSeconds(1.0f);
        _animState = EAnimState.STOP;
        yield return new WaitForSeconds(1.0f);
        float timer = 3.0f;
        Material catboxMat = GetComponentInChildren<SkinnedMeshRenderer>().material;

        screenShot.takeHiResShot = true;

        while(timer >= 0f)
        {
            timer -= Time.deltaTime;
            Color color = Color.Lerp(Color.white, Color.black, (3 - timer) / 3f);
            catboxMat.SetColor("_Color", color);
            yield return null;
        }
        SpawnPortal();

        for(int i = 0; i < GetComponentsInChildren<BoxCollider>().Length; ++i)
        {
            Destroy(GetComponentsInChildren<BoxCollider>()[i]);
        }
        for(int i = 0; i < cubes.Length; ++i)
        {
            Destroy(cubes[i]);
        }
        Destroy(this);
    }
    private void AnimationStateUpdate()
    {
        switch (_animState)
        {
            case EAnimState.STOP:
                animator.SetInteger("animation", (int)EAnimState.STOP);
                break;
            case EAnimState.IDLE:
                animator.SetInteger("animation", (int)EAnimState.IDLE);
                break;
            case EAnimState.OPEN:
                animator.SetInteger("animation", (int)EAnimState.OPEN);
                break;
            case EAnimState.CLOSE:
                animator.SetInteger("animation", (int)EAnimState.CLOSE);
                break;
        }

    }
    private void Action()
    {
        if (!player) return;

        switch (_state)
        {
            case EState.IDLE:
                walkSpeed = 0f;
                break;
            case EState.CHASE:
                vertical = 1f;
                walkSpeed = 5f;
                break;
            case EState.HIDE:
                walkSpeed = 0f;
                break;
            case EState.KNOCKDOWN:
                walkSpeed = 0f;
                break;
            case EState.CAST:
                walkSpeed = 0f;
                break;
        }
    }
    #endregion

    private void SpawnPortal()
    {
        GameObject portal = Resources.Load<GameObject>("Prefabs/Portal");
        portal = Instantiate(portal, transform.position + transform.forward * 5f + Vector3.up * 3, transform.rotation);
        SoundManager.Instance.SFXSource.PlayOneShot(SoundManager.Instance.Portal);
    }

    #region TakeDmg
    public void Damage(float _dmg)
    {
        //if hide, can't attacked
        if(_state != EState.HIDE)
        {
            hp -= _dmg;
            DieCheck();
        }
    }

    //Die Check & Boss Phase Check
    private void DieCheck()
    {
        if(hp <= 0f)
        {
            isDie = true;
            hp = maxHp;
        }
        else if(hp <= 0.2f * maxHp)
        {
            phase = 3;
        }
        else if(hp <= 0.5f * maxHp)
        {
            phase = 2;
        }
        else
        {
            phase = 1;
        }
    }

    
    #endregion

    #region Rotate
    private void RotateToPlayer()
    {
        if (isDie) return;

        Vector3 lookDir = Vector3.zero;

        //LookPlayer in DetectArea
        if (distance <= detectRange)
        {
            lookDir = player.transform.position - transform.position;
            lookDir = new Vector3(lookDir.x, 0f, lookDir.z).normalized;

            horizontal = 0f;
            vertical = 1f;

            transform.forward = Vector3.Lerp(transform.forward, lookDir, Time.deltaTime * rotateSpeed);
        }
        else
        {
            horizontal = 0f;
            vertical = 0f;

            lookDir = new Vector3(horizontal, 0f, vertical).normalized;
            transform.forward = Vector3.Lerp(transform.forward, lookDir, Time.deltaTime * rotateSpeed);
        }
    }

    private void RotateCubes()
    {
        cubeTimer += Time.deltaTime;
        for(int i = 0; i < 4; ++i)
        {
            cubes[i].transform.rotation *= Quaternion.AngleAxis(cubeRotateSpeed * Time.deltaTime, Vector3.up);
            Vector3 newPos = Quaternion.AngleAxis(cubeTimer * cubesRotateSpeed + (360 * i) / cubes.Length, Vector3.up) * Vector3.right * cubeRadius + Vector3.up * 0.3f;
            cubes[i].transform.localPosition = newPos;
        }
    }
    #endregion

    #region MOVE
    protected virtual void Move()
    {
        if (isDie) return;
        transform.position += velocity;
    }
    protected void CalculateVelocity()
    {
        if (verticalMomentum > _gravity) //중력 모멘텀 관성 수직 운동량이 중력보다 크면 운동량에서 중력만큼 감소시킴 -> 내려감 
        {
            verticalMomentum += Time.fixedDeltaTime * _gravity;
        }

        velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

        // 항상 수직운동량*픽스시간 만큼 속도의 상승값에 더해줌 -> 항싱 위로 올리고 있으나 중력보다 모멘텀이 낮아서 안뜨는 상태
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
        {
            velocity.z = 0;
            jumpRequest = true;
        }
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
        {
            velocity.x = 0;
            jumpRequest = true;
        }
        // 내려갈 때 추락 검사 실행
        if (velocity.y < 0)
        {
            velocity.y = checkDownSpeed(velocity.y);
        }
        else if (velocity.y > 0) //올라갈 때 검사 실행 
        {
            velocity.y = checkUpSpeed(velocity.y);
        }
    }
    private float checkDownSpeed(float downSpeed) // 플레이어 추락 시 접점 확인 중심점 기준으로 4분면 
    {
        if (world.CheckForVoxel(new Vector3(transform.position.x - monsterWidth, transform.position.y + downSpeed, transform.position.z - monsterWidth)) ||
           world.CheckForVoxel(new Vector3(transform.position.x + monsterWidth, transform.position.y + downSpeed, transform.position.z - monsterWidth)) ||
           world.CheckForVoxel(new Vector3(transform.position.x + monsterWidth, transform.position.y + downSpeed, transform.position.z + monsterWidth)) ||
           world.CheckForVoxel(new Vector3(transform.position.x - monsterWidth, transform.position.y + downSpeed, transform.position.z + monsterWidth)))
        {
            return 0;
        }
        else
        {
            return downSpeed;
        }
    }
    private float checkUpSpeed(float upSpeed) // 플레이어 상승 시 접점 확인 중심점 기준으로 4분면 
    {
        if (world.CheckForVoxel(new Vector3(transform.position.x - monsterWidth, transform.position.y + 1f + upSpeed, transform.position.z - monsterWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + monsterWidth, transform.position.y + 1f + upSpeed, transform.position.z - monsterWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + monsterWidth, transform.position.y + 1f + upSpeed, transform.position.z + monsterWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - monsterWidth, transform.position.y + 1f + upSpeed, transform.position.z + monsterWidth)))
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }
    private void Jump()
    {
        verticalMomentum = jumpForce;
        jumpRequest = false;
    }
    public bool front
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + monsterWidth)) ||
              world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + monsterWidth)))
            {
                return true;
            }
            else
                return false;
        }
    }
    public bool back
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - monsterWidth)) ||
              world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - monsterWidth)))
            {
                return true;
            }
            else
                return false;
        }
    }
    public bool left
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x - monsterWidth, transform.position.y, transform.position.z)) ||
              world.CheckForVoxel(new Vector3(transform.position.x - monsterWidth, transform.position.y + 1f, transform.position.z)))
            {
                return true;
            }
            else
                return false;
        }
    }
    public bool right
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x + monsterWidth, transform.position.y, transform.position.z)) ||
              world.CheckForVoxel(new Vector3(transform.position.x + monsterWidth, transform.position.y + 1f, transform.position.z)))
            {
                return true;
            }
            else
                return false;
        }
    }
    #endregion

    #region BuringTree
    private void OnTriggerEnter(Collider other)
    {
        DestroyTree(other);
    }
    public void DestroyTree(Collider other)
    {
        if (other.CompareTag("Tree"))
        {
            //머테리얼의 버텍스 컬러를 black에서 red로 바꾼다.
            //vertex _Color : COLOR 값으로 가져와 텍스쳐를 바꾸기 위함
            BlacktoRed(other);
            Vector3 dir = (other.transform.parent.position - transform.position).normalized;
            for (int i = 0; i < other.transform.parent.GetComponentsInChildren<MeshRenderer>().Length; ++i)
            {
                MeshRenderer curMeshRenderer = other.transform.parent.GetComponentsInChildren<MeshRenderer>()[i];
                StartCoroutine(Burning(curMeshRenderer));
            }

            string blockName = "Air";
            ChunkCoord thisChunk = new ChunkCoord(other.transform.parent.position);
            int num = 0;
            while (blockName == "Tree" || blockName == "Air")
            {
                ++num;
                blockName = DestructVoxel(thisChunk, other.transform.parent.position, -Vector3.up * (-4.1f + num));
            }
        }
    }
    private IEnumerator Burning(MeshRenderer _meshRenderer)
    {
        Material _material = _meshRenderer.material;
        GameObject fireEmbers = _meshRenderer.transform.parent.Find("FireEmbers").gameObject;
        GameObject smoke = _meshRenderer.transform.parent.Find("Smoke").gameObject;
        fireEmbers.SetActive(true);
        smoke.SetActive(true);
        fireEmbers.GetComponent<ParticleSystem>().Play();

        //Burn Sound
        AudioSource treeAudio = _meshRenderer.gameObject.AddComponent<AudioSource>();
        treeAudio.spatialBlend = 1f;
        treeAudio.maxDistance = detectRange;
        treeAudio.volume *= SoundManager.Instance.SFXVolume;
        treeAudio.PlayOneShot(treeBurn);


        //CutOff (BurnEffect) : 서서히 사라지는 효과 material의 쉐이더 값에 접근해서 값 변경
        float cutOffValue = _material.GetFloat("_Cutoff");
        float timer = Mathf.Pow((cutOffValue - 0.1f), 0.5f);

        while (_material.GetFloat("_Cutoff") <= 0.9f)
        {
            timer += Time.deltaTime * 0.05f;
            cutOffValue = 0.1f + Mathf.Pow(timer, 2);
            _material.SetFloat("_Cutoff", cutOffValue);

            yield return null;
        }
        if (_meshRenderer)
        {
            Destroy(_meshRenderer.gameObject);
            Destroy(fireEmbers);
            Destroy(smoke);
        }
    }

    private void BlacktoRed(Collider _collider)
    {
        for (int i = 0; i < _collider.transform.parent.GetComponentsInChildren<MeshFilter>().Length; ++i)
        {
            MeshFilter curLeaf = _collider.transform.parent.GetComponentsInChildren<MeshFilter>()[i];
            Color[] colors = new Color[curLeaf.mesh.colors.Length];
            for (int j = 0; j < colors.Length; ++j)
            {
                colors[j] = Color.red;
            }
            curLeaf.mesh.colors = colors;
        }
    }
    #endregion

    #region Voxel Destruct
    public string DestructVoxel(ChunkCoord _thisChunk, Vector3 _pos, Vector3 _dir)
    {
        if (world.chunks[_thisChunk.x, _thisChunk.z] == null) return "";

        BlockType curBlockType = world.blockTypes[world.chunks[_thisChunk.x, _thisChunk.z].GetVoxelFromGlobalVector3(_pos + _dir)];
        string blockName = curBlockType.blockName;
        //Block Hit and hp-- => Destruct block
        if (blockName == "Air") return "Air";

        if(blockName == "Tree")
        {
            --curBlockType.hp;
        }

        if (curBlockType.hp <= 0)
        {
            curBlockType.hp = 1;
            Vector3 hitDirPos = _pos + _dir;

            CreateBrokenMesh(new Vector3((int)hitDirPos.x, (int)hitDirPos.y, (int)hitDirPos.z), _thisChunk);
            world.chunks[_thisChunk.x, _thisChunk.z].EditVoxel(_pos + _dir, 0);

            return blockName;
        }
        return blockName;
    }
    private void CreateBrokenMesh(Vector3 _pos, ChunkCoord _thisChunk)
    {
        Chunk curChunk = world.chunks[_thisChunk.x, _thisChunk.z];
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int vertexIndex = 0;

        int xCheck = Mathf.FloorToInt(_pos.x);
        int yCheck = Mathf.FloorToInt(_pos.y);
        int zCheck = Mathf.FloorToInt(_pos.z);

        xCheck -= Mathf.FloorToInt(curChunk.chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(curChunk.chunkObject.transform.position.z);

        Vector3 oriPos = _pos;

        _pos = new Vector3(xCheck, yCheck, zCheck);

        for (int i = 0; i < 6; ++i)
        {
            byte blockID = curChunk.voxelMap[xCheck, yCheck, zCheck];
            vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[i, 0]]);
            vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[i, 1]]);
            vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[i, 2]]);
            vertices.Add(VoxelData.voxelVerts[VoxelData.voxelTris[i, 3]]);

            curChunk.AddTexture(world.blockTypes[blockID].GetTextureID(i), uvs);

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 3);
            vertexIndex += 4;
        }

        GameObject brokenObject = new GameObject("BrokenObject" + world.blockTypes[curChunk.voxelMap[xCheck, yCheck, zCheck]].blockName);
        brokenObject.transform.position = oriPos;

        MeshFilter meshFilter = brokenObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = brokenObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        meshRenderer.material.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        //DestroyEffect
        brokenObject.AddComponent<DestructableObject>();
    }
    #endregion

    #region Sound // without main BGM
    private void BossSound(bool _bool)
    {
        audioSource.volume = SoundManager.Instance.BGMVolume;
        if(_bool && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if(!_bool)
        {
            audioSource.Stop();
        }
    }
    #endregion

    //Awake => OnEnable => Start
    private void OnEnable()
    {
        StartCoroutine(BossOnEnable());
    }
    private IEnumerator BossOnEnable()
    {
        yield return new WaitUntil(() => hpBar);
        yield return new WaitUntil(() => hpBar.gameObject.activeSelf == true);

        //Show BossCutScene 
        if (!isActivated)
        {
            isActivated = true;
            CutSceneManager.isStart = true;
        }

        yield return new WaitUntil(() => !hpBar.gameObject.activeSelf);
        StartCoroutine(BossOnEnable());
    }
}