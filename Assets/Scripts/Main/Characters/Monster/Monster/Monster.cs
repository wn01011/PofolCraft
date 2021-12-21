using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Monster : MonoBehaviour
{
    #region variables

    protected Transform target = null;
    protected bool isDie = false;
    protected bool isGrounded = false;
    protected bool isIdle = false;
    private bool overlap = false;
    private bool onDamage = false;

    protected float monsterWidth = 0f;
    protected float hp = 10f;
    protected float maxHp = 10f;
    protected float atkDmg = 0f;
    protected float atkSpd = 0f;
    protected float dieAnimLength = 0f;
    protected float attackAnimLength = 0f;

    protected World world = null;
    private bool jumpRequest = false;
    private float verticalMomentum = 0f;
    private float knockbackMomentum = 10f;
    private float horizontal = 0f;
    private float vertical = 0f;
    private Vector3 velocity = Vector3.zero;
    private Vector3 enemyDir = Vector3.zero;

    private float waitTime = 0f;
    private float randomMoveTimer = 0f;
    private float rotateSpeed = 0f;

    private float detectRange = 0f;
    private float fightRange = 0f;

    private float walkSpeed = 3f;
    private float jumpForce = 5f;
    private float _gravity = -9.8f;
    private SpawnManager spawnManager = null;

    private MiniMap miniMap = null;

    private GameObject overlappedMonster = null;
    private bool onOverlapPushing = false;
    protected bool attack = false;
    protected bool attackAnim = false;
    private bool onDrop = false;

    private Animator animator = null;
    private GameObject coin = null;
    private AudioSource audioSource = null;
    private AudioClip hitClip = null;

    public int monsterType = 0;
    private QuestManager questManager = null;

    #endregion

    protected enum EState
    { 
        IDLE = 1,
        CHASE,
        FIGHT,
        RUN,
    }
    protected enum EAnimState
    {
        IDLE = 1,
        RUN,
        ATK,
        DAMAGE,
        DIE,
    }


    [SerializeField]
    protected EState _state = EState.IDLE;
    [SerializeField]
    protected EAnimState _animState = EAnimState.IDLE;

    protected virtual void Start()
    {
        #region Initialization

        world = FindObjectOfType<World>();
        miniMap = FindObjectOfType<MiniMap>();
        spawnManager = FindObjectOfType<SpawnManager>();
        questManager = FindObjectOfType<QuestManager>();
        animator = GetComponentInParent<Animator>();
        monsterWidth = transform.GetComponent<BoxCollider>().size.x * 0.5f;
        audioSource = gameObject.AddComponent<AudioSource>();
        hitClip = SoundManager.Instance.Jab;

        coin = Resources.Load<GameObject>("Prefabs/Coin/Coin Gold");

        isDie = false;
        isIdle = true;
        onOverlapPushing = false;
        attack = false;

        hp = maxHp;
        target = FindObjectOfType<Player>().transform;
        detectRange = VoxelData.ViewDistanceInChunks * VoxelData.ChunkWidth * 0.1f;
        audioSource.maxDistance = detectRange;
        audioSource.spread = 360f;
        audioSource.spatialBlend = 1f;
        fightRange = detectRange * 0.2f;
        rotateSpeed = 5f;

        #endregion
    }
    protected void FixedUpdate()
    {
        #region make direction

        CalculateVelocity();
        RotateToPlayer();
        RandomMove();

        #endregion

        #region actual move and jump

        if(!isDie)
        {
            Move();
            if (jumpRequest)
                Jump();
        }

        #endregion
    }
    protected void Update()
    {
        if (OverlapCheck())
        {
            overlappedMonster = OverlapCheck();
        }
        else
            overlap = false;
        ChangeMode();
        AnimationStateUpdate();
        AnimChangeMode();
        Action();
    }

    #region FINITE STATE MACHINE Physics & Animation

    //Change State
    private void ChangeMode()
    {
        if (!target) return;
        
        float distance = Vector3.Distance(target.position, transform.position);

        if (_state == EState.RUN)
        {
            walkSpeed = 3f;
            _state = EState.IDLE;
        }
        if (distance <= fightRange)
        {
            walkSpeed = 0f;
            _state = EState.FIGHT;
        }
        else if (distance < detectRange && distance > fightRange)
        {
            walkSpeed = 3f;
            _state = EState.CHASE;
        }
        else
        {
            walkSpeed = 3f;
            _state = EState.IDLE;
        }
    }

    //Change Animation State
    private void AnimChangeMode()
    {
        if (!target) return;

        if(isDie)
        {
            _animState = EAnimState.DIE;
        }
        else if(!isDie && onDamage)
        {
            horizontal = 0f; vertical = 0f;
            _animState = EAnimState.DAMAGE;
        }
        else if(!isDie && _state == EState.FIGHT && attackAnim)
        {
            _animState = EAnimState.ATK;
        }
        else if (velocity != Vector3.zero && !attackAnim)
        {
            _animState = EAnimState.RUN;
        }
        else if(velocity == Vector3.zero && !attackAnim)
        {
            _animState = EAnimState.IDLE;
        }
    }
    //Define monster action, what to do in each animState
    private void AnimationStateUpdate()
    {
        switch (_animState)
        {
            case EAnimState.IDLE:
                animator.SetInteger("animation", (int)EAnimState.IDLE);
                break;
            case EAnimState.RUN:
                animator.SetInteger("animation", (int)EAnimState.RUN);
                break;
            case EAnimState.ATK:
                animator.SetInteger("animation", (int)EAnimState.ATK);
                break;
            case EAnimState.DAMAGE:
                animator.SetInteger("animation", (int)EAnimState.DAMAGE);
                break;
            case EAnimState.DIE:
                animator.SetInteger("animation", (int)EAnimState.DIE);
                break;
        }

    }
    //each State monster have, define monster action
    private void Action()
    {
        if (!target) return;

        switch (_state)
        {
            case EState.IDLE:
                isIdle = true;
                break;
            case EState.CHASE:
                horizontal = 0f; vertical = 1f;
                velocity = new Vector3(horizontal, 0f, vertical);
                break;
            case EState.RUN:
                Vector3 runDir = -transform.parent.forward;
                horizontal = runDir.x;
                vertical = runDir.z;
                break;
            case EState.FIGHT:
                horizontal = 0; vertical = 0;
                if(!attack)
                    Attack(target.GetComponent<Player>());
                break;
        }
    }

    #endregion

    #region Get&Set Hp

    protected void SetMaxHp(float _maxHp)
    {
        maxHp = _maxHp;
    }

    public float GetMaxHp()
    {
        return maxHp;
    }

    public float GetHp()
    {
        return hp;
    }

    public void GetDmg(float _dmg)
    {
        if (isDie) return;

        hp -= _dmg;
        audioSource.volume *= SoundManager.Instance.SFXVolume;
        audioSource.PlayOneShot(hitClip, audioSource.volume);

        StartCoroutine(OnDamage());
        StartCoroutine(DieCheck());
    }

    protected IEnumerator OnDamage()
    {
        onDamage = true;
        _animState = EAnimState.DAMAGE;
        yield return new WaitForSeconds(0.375f);
        onDamage = false;
    }

    protected IEnumerator DieCheck()
    {
        if(hp <= 0f)
        {
            hp = 0f;

            //Do Die Here
            ++GameManager.exp;

            int idx = miniMap.enemies.FindIndex((x) => gameObject);
            GameObject myMini = miniMap.miniEnemies[idx];
            miniMap.miniEnemies.RemoveAt(idx);
            miniMap.enemies.Remove(gameObject);
            
            isDie = true;
            Destroy(GetComponent<BoxCollider>());
            //wait For Die Anim Finish
            yield return new WaitForSeconds(dieAnimLength);
            if(!onDrop)
                StartCoroutine(DropCoinCo());
            yield return new WaitUntil(() => !onDrop);
            questManager.CheckQuestProgress(monsterType);
            Destroy(gameObject);

        }
    }

    #endregion

    protected abstract void Attack(Player _Target);


    protected void RotateToPlayer()
    {
        float distance = Vector3.Distance(target.position, transform.position);
        Vector3 lookDir = Vector3.zero;
        
        //look Player in detectArea
        if (distance <= detectRange)
        {
            lookDir = target.transform.position - transform.position;
            lookDir = new Vector3(lookDir.x, 0f, lookDir.z).normalized;
   
            horizontal = 0f;
            vertical = 1f;
            
            //if Pushing each other now, transform.forward decide in PushOthers(), else, decide in here; 
            if(!overlap && !onOverlapPushing)
            {
                transform.parent.forward = lookDir;
            }
            else if(overlap && !onOverlapPushing)
            {
                StartCoroutine(PushOthers(overlappedMonster));
            }
        }
        else
        {
            lookDir = new Vector3(horizontal, 0f, vertical).normalized;
            transform.parent.forward = Vector3.Lerp(transform.parent.forward, lookDir, Time.deltaTime * rotateSpeed);
        }
    }


    #region Check Direction (Up&Down) / (R, L, B, F)

    #region Up & down

    private float checkDownSpeed(float downSpeed) // 플레이어 추락 시 접점 확인 중심점 기준으로 4분면 
    {
        if (world.CheckForVoxel(new Vector3(transform.position.x - monsterWidth, transform.position.y + downSpeed, transform.position.z - monsterWidth)) ||
           world.CheckForVoxel(new Vector3(transform.position.x + monsterWidth, transform.position.y + downSpeed, transform.position.z - monsterWidth)) ||
           world.CheckForVoxel(new Vector3(transform.position.x + monsterWidth, transform.position.y + downSpeed, transform.position.z + monsterWidth)) ||
           world.CheckForVoxel(new Vector3(transform.position.x - monsterWidth, transform.position.y + downSpeed, transform.position.z + monsterWidth)))
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
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

    #endregion  

    #region R, L, B, F

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

    #endregion

    #region Move

    private void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }


    protected virtual void Move()
    {
        transform.parent.localPosition += velocity;
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

        //만약 데미지 입는 에니메이션 상태면 뒤로 밀림
        if(_animState == EAnimState.DAMAGE)
        {
            Vector3 lookDir = (target.transform.position - transform.position).normalized;
            velocity += -lookDir * knockbackMomentum * Time.fixedDeltaTime;
        }

        // 앞뒤 좌우에 복셀이 있는 상태에서 해당 방향으로 진행하려고 할 경우 해당 진행 값을 0으로 하여 진행 불가
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

    private void RandomMove()
    {
        if (!target || _state != EState.IDLE) return;
        if(randomMoveTimer >= waitTime)
        {
            randomMoveTimer = 0f;
            Random.seed = (int)System.DateTime.Now.Ticks;
            waitTime = Random.Range(1f, 3f);
            
            horizontal = Random.Range(-1f, 1f);
            vertical = Random.Range(-1f, 1f);
            
            Vector3 dir = new Vector3(horizontal, 0f, vertical).normalized;
            
            horizontal = dir.x;
            vertical = dir.z;
        }
        else
        {
            randomMoveTimer += Time.fixedDeltaTime;

            if(!isDie && isIdle)
            {
                if(front || back || right || left)
                {
                    jumpRequest = true;
                }
            }
        }
    }


    #region push other Enemies

    //if position overlapped with other Enemy, they push each other
    private IEnumerator PushOthers(GameObject _curMonster)
    {
        onOverlapPushing = true;
        Vector3 monsterDir = _curMonster.transform.position - transform.position;
        monsterDir = Vector3.ProjectOnPlane(monsterDir, Vector3.up).normalized;

        float timer = 0.5f;
        //set dir to half vector
        while (timer >= 0f)
        {
            timer -= Time.deltaTime;
            transform.parent.forward = (transform.parent.forward - monsterDir).normalized;
            yield return null;
        }
        onOverlapPushing = false;
    }
    private GameObject OverlapCheck()
    {
        for (int i = 0; i < spawnManager.monsterList.Count; ++i)
        {
            GameObject curMonster = spawnManager.monsterList[i].gameObject;
            
            if (!curMonster.activeSelf || curMonster == gameObject) continue;

            if (Vector3.Distance(curMonster.transform.position, transform.position) <= fightRange)
            {
                overlap = true;
                return curMonster;
            }
        }
        return null;
    }

    #endregion

    #endregion

    #region monsterDrop

    protected struct Drop
    {
        public int number;
        public int exp;
        public DropTable kind;

        public Drop(int _number, int _exp, int _kind)
        {
            number = _number;
            number = Mathf.Clamp(number, 0, Mathf.Abs(number));
            exp = _exp;
            exp = Mathf.Clamp(exp, 0, Mathf.Abs(exp));
            kind = (DropTable)_kind;
            kind = (DropTable)Mathf.Clamp((int)kind, 0, System.Enum.GetValues(typeof(DropTable)).Length);
        }
    }

    protected enum DropTable
    {
        gold,
    }

    protected Drop _drop = new Drop(5, 1, 0);

    private IEnumerator DropCoinCo()
    {
        float dropTimer = 0.2f * _drop.number + 0.1f;
        float instanceTimer = 0.2f;

        onDrop = true;

        while(dropTimer >= 0f)
        {
            dropTimer -= Time.deltaTime;
            instanceTimer -= Time.deltaTime;
            if(instanceTimer <= 0f)
            {
                Instantiate(coin, transform.position + Vector3.up, Quaternion.identity);
                instanceTimer = 0.2f;
            }
            yield return null;
        }
        onDrop = false;
    }

    #endregion
}
