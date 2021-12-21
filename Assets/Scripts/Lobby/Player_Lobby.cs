using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Player_Lobby : MonoBehaviour
{

    #region variables

    [SerializeField] private Transform cam; //메인 카메라
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private readonly float _gravity = -9.8f;
    [SerializeField] private World_Lobby world = null;
    private float verticalMomentum = 0f;
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private bool isSprinting = false;
    private bool isCanMove = true;
    [SerializeField] private float playerWidth = 0.3f; // 플레이어 직경
    

    private bool jumpRequest;
    
    private float horizontal;
    private float vertical;
    private Vector3 velocity;

    public Text selectedBlockText;
    public byte selectedBlockIndex = 1;


    private float BorderMin = 0f;
    private float BorderMax = 0f;

    public bool GetIsCanMove() => isCanMove;

    #endregion

    private void Start()
    {
        GetBorders();
    }

    private void FixedUpdate()
    {
        if (isCanMove)
        {
            CalculateVelocity();
            GetPlayerInputs();
            PlayerMove();
        }
        if (jumpRequest)
            Jump();
        //Cursor.lockState = CursorLockMode.Locked;
        selectedBlockText.text = world.blockTypes[selectedBlockIndex].blockName + "block selected";
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
    } 

    private void CalculateVelocity()
    {
        if(verticalMomentum > _gravity) //중력 모멘텀 관성 수직 운동량이 중력보다 크면 운동량에서 중력만큼 감소시킴 -> 내려감 
        {
            verticalMomentum += Time.fixedDeltaTime * _gravity;
        }


        if(isSprinting) // bool 값의 여부에 따라 이동속도를 달리기 혹은 걷기로 적용 
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        }
        else
        {
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


    }
    #endregion

    private void GetBorders()
    {
        BorderMin = VD.ChunkWidth + 0.5f;
        BorderMax = VD.ChunkWidth * (VD.WorldSizeInChunks - 1)- 0.5f;
    }


    public void SetIsMove(bool _bool)
    {
        isCanMove = _bool;
    }
    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        

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

    

   
}
