using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Player : MonoBehaviour
{

    #region variables

    [SerializeField] private Transform cam;
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float _gravity = -9.8f;
    [SerializeField] private World world = null;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isSprinting = false;

    [SerializeField] private float playerWidth = 0.3f; // 플레이어 직경
    
    [SerializeField] private float checkIncrement = 0.1f;
    [SerializeField] private float reach = 8f;
    [SerializeField] private Image healthOrb = null;
    [SerializeField] private Slider expSlider = null;

    private Animator animator = null;
    private AudioSource audioSource = null;
    private AudioClip jumpClip = null;

    private bool jumpRequest = false;
    
    private float verticalMomentum = 0f;
    private float horizontal = 0f;
    private float vertical = 0f;
    private Vector3 velocity = Vector3.zero;

    private float hp = 10f;
    private float maxHp = 10f;
    private float regen = 1f;

    private float BorderMin = 0f;
    private float BorderMax = 0f;

    public bool isDie = false;
    private bool isTakeDmg = false;
    public bool isAtk = false;

    private float walkSoundTimer = 0f;
    private int hitSoundCount = 0;

    #endregion

    public enum EState
    {
        IDLE = 1,
        MOVE,
        ATK,
        BUFF,
        DAMAGE,
        DIE,
    }
    public EState eState = EState.IDLE;

    public void Init()
    {
        #region Initialization

        if (!audioSource)
            audioSource = FindObjectOfType<SoundManager>().SFXSource;

        maxHp = (GameManager.level * 5 + 5) * GameManager.HpUp;
        walkSpeed += GameManager.SpeedUp * 0.1f;
        jumpForce += GameManager.JumpUp * 0.3f;
        hp = maxHp;
        regen = 0.01f + GameManager.level * 0.01f;
        isDie = false;
        isTakeDmg = false;
        //Set healthOrb Hp value Full
        healthOrb.material.SetFloat("_Value", 1f);
        GetBorders();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        jumpClip = SoundManager.Instance.Jump;

        #endregion
    }

    private void FixedUpdate()
    {
        CalculateVelocity();
        GetPlayerInputs();
        PlayerMove();
      
        if (jumpRequest)
        {
            Jump();
            audioSource.PlayOneShot(jumpClip, SoundManager.Instance.SFXVolume);
        }

        Cursor.lockState = CursorLockMode.Locked;
    }



    private void Update()
    {
        UpdateHealthOrb();
        UpdateExpSlider();
        HpRegen();
        AnimationStateUpdate();
    }

    private void AnimationStateUpdate()
    {
        if (!animator) return;

        switch (eState)
        {
            case EState.IDLE:
                animator.SetInteger("animation", (int)EState.IDLE);
                break;
            case EState.MOVE:
                animator.SetInteger("animation", (int)EState.MOVE);
                break;
            case EState.ATK:
                animator.SetInteger("animation", (int)EState.ATK);
                break;
            case EState.BUFF:
                animator.SetInteger("animation", (int)EState.BUFF);
                break;
            case EState.DAMAGE:
                animator.SetInteger("animation", (int)EState.DAMAGE);
                break;
            case EState.DIE:
                animator.SetInteger("animation", (int)EState.DIE);
                break;
        }

    }

    private void GetBorders()
    {
        BorderMin = VoxelData.ChunkWidth + 0.5f;
        BorderMax = VoxelData.ChunkWidth * (VoxelData.WorldSizeInChunks - 1) - 0.5f;
    }

    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        Vector2 dir = new Vector2(horizontal, vertical).normalized;
        horizontal = dir.x;
        vertical = dir.y;

        if(Input.GetKey(KeyCode.LeftShift))
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        if(isGrounded && Input.GetKey(KeyCode.Space))
        {
            jumpRequest = true;
        }
        
    }
    
    #region Move

    private void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }


    private void PlayerMove()
    {
        if (transform.position.x >= BorderMax)
            velocity = new Vector3(Mathf.Clamp(velocity.x, -10f, 0f), velocity.y, velocity.z);
        else if(transform.position.x <= BorderMin)
            velocity = new Vector3(Mathf.Clamp(velocity.x, 0f, 10f), velocity.y, velocity.z);

        if (transform.position.z >= BorderMax)
            velocity = new Vector3(velocity.x, velocity.y, Mathf.Clamp(velocity.z, -10f, 0f));
        else if(transform.position.z <= BorderMin)
            velocity = new Vector3(velocity.x, velocity.y, Mathf.Clamp(velocity.z, 0f, 10f));

        transform.Translate(velocity, Space.World);

        //if player on walk, print walk Sound
        if((velocity.x != 0 || velocity.z != 0 ) && walkSoundTimer <= 0 && isGrounded)
        {
            walkSoundTimer = 2f / walkSpeed;
            audioSource.PlayOneShot(SoundManager.Instance.Blop, SoundManager.Instance.SFXVolume * 0.8f);
        }
        walkSoundTimer -= Time.deltaTime;
    } 

    private void CalculateVelocity()
    {
        if(verticalMomentum > _gravity) //중력 모멘텀 관성 수직 운동량이 중력보다 크면 운동량에서 중력만큼 감소시킴 -> 내려감 
        {
            verticalMomentum += Time.fixedDeltaTime * _gravity;
        }


        if(isSprinting) // bool 값의 여부에 따라 이동속도를 달리기 혹은 걷기로 적용 
        {
            walkSpeed = 12.0f;
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        }
        else
        {
            walkSpeed = 6.0f;
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        }

        // 항상 수직운동량*픽스시간 만큼 속도의 상승값에 더해줌 -> 항싱 위로 올리고 있으나 중력보다 모멘텀이 낮아서 안뜨는 상태
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;


        // 앞뒤 좌우에 복셀이 있는 상태에서 해당 방향으로 진행하려고 할 경우 해당 진행 값을 0으로 하여 진행 불가
        if((velocity.z > 0 && front) || (velocity.z < 0 && back))
        {
            velocity.z = 0;
        }
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
        {
            velocity.x = 0;
        }
        // 내려갈 때 추락 검사 실행
        if(velocity.y < 0) 
        {
            velocity.y = checkDownSpeed(velocity.y);
        }
        else if(velocity.y > 0) //올라갈 때 검사 실행 
        {
            velocity.y = checkUpSpeed(velocity.y);
        }

        //Choose Animation State
        if (velocity == Vector3.zero && !isTakeDmg && !isAtk)
        {
            eState = EState.IDLE;
        }
        else if (velocity != Vector3.zero && !isTakeDmg && !isAtk)
        {
            eState = EState.MOVE;
        }

    }
    #endregion


    #region Check Direction (Up&Down) / (R, L, B, F)

    private float checkDownSpeed(float downSpeed) // 플레이어 추락 시 접점 확인 중심점 기준으로 4분면 
    {
        if(world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
           world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
           world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
           world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) )
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
        if (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y +1f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y +1f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y +1f + upSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y +1f + upSpeed, transform.position.z + playerWidth)))
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    public bool front
    {
        get { if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y+1f, transform.position.z + playerWidth)))
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
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
              world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth)))
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
            if (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
              world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y+1f, transform.position.z)))
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
            if (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
              world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y+1f, transform.position.z)))
            {
                return true;
            }
            else
                return false;
        }
    }

    #endregion


    #region Get,Set Hp, TakeDmg, DieCheck

    public void TakeDamage(float _dmg)
    {
        hp -= _dmg;
        eState = EState.DAMAGE;
        isTakeDmg = true;
        //dmg animation and sound Coroutine
        StartCoroutine(TakeDmgCo());

        if (DieCheck())
        {
            Die();
        }

        
    }

    private IEnumerator TakeDmgCo()
    {
        float dieLength = 0.667f;
        //Damage Sound
        if (hitSoundCount < 1)
        {
            audioSource.PlayOneShot(SoundManager.Instance.Hit, SoundManager.Instance.SFXVolume);
            ++hitSoundCount;
            dieLength -= SoundManager.Instance.Hit.length;
            yield return new WaitForSeconds(SoundManager.Instance.Hit.length);
            --hitSoundCount;
        }
        yield return new WaitForSeconds(dieLength);
        isTakeDmg = false;
        eState = EState.IDLE;
    }
    public void SetHp(float _hp)
    {
        hp = _hp;
    }
    public void SetMaxHp(float _maxHp)
    {
        maxHp = _maxHp;
    }
    public float GetHp()
    {
        return hp;
    }
    public float GetMaxHp()
    {
        return maxHp;
    }
    public void UpdateHealthOrb()
    {
        float value = Mathf.Lerp(healthOrb.material.GetFloat("_Value"), hp / maxHp, Time.deltaTime * 2f);
        healthOrb.material.SetFloat("_Value", value);
    }
    private void HpRegen()
    {
        if (hp < maxHp)
            hp += regen * Time.deltaTime;
    }
    public bool DieCheck()
    {
        if(hp <= 0f)
        {
            hp = 0f;
            isDie = true;
        }
        return isDie;
    }
    private void Die()
    {
        eState = EState.DIE;
    }

    #endregion

    public void LevelUp()
    {
        SetMaxHp(5 + GameManager.level * 5);
        Shoot.atkDmg = 1f + GameManager.level * 0.5f;
        regen = 0.01f + GameManager.level * regen;
        SetHp(GetMaxHp());
    }

    private void UpdateExpSlider()
    {
        float value = expSlider.value;
        float finalValue = (float)GameManager.exp / GameManager.expRequirement;
        expSlider.value = Mathf.Lerp(value, finalValue, Time.deltaTime);
    }

}
