using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Player_Lobby : MonoBehaviour
{

    #region variables

    [SerializeField] private Transform cam; //���� ī�޶�
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private readonly float _gravity = -9.8f;
    [SerializeField] private World_Lobby world = null;
    private float verticalMomentum = 0f;
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private bool isSprinting = false;
    private bool isCanMove = true;
    [SerializeField] private float playerWidth = 0.3f; // �÷��̾� ����
    

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
        if(verticalMomentum > _gravity) //�߷� ����� ���� ���� ����� �߷º��� ũ�� ������� �߷¸�ŭ ���ҽ�Ŵ -> ������ 
        {
            verticalMomentum += Time.fixedDeltaTime * _gravity;
        }


        if(isSprinting) // bool ���� ���ο� ���� �̵��ӵ��� �޸��� Ȥ�� �ȱ�� ���� 
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        }
        else
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        }

        // �׻� �������*�Ƚ��ð� ��ŭ �ӵ��� ��°��� ������ -> �׽� ���� �ø��� ������ �߷º��� ������� ���Ƽ� �ȶߴ� ����
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;


        // �յ� �¿쿡 ������ �ִ� ���¿��� �ش� �������� �����Ϸ��� �� ��� �ش� ���� ���� 0���� �Ͽ� ���� �Ұ�
        if((velocity.z > 0 && front) || (velocity.z < 0 && back))
        {
            velocity.z = 0;
        }
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
        {
            velocity.x = 0;
        }
        // ������ �� �߶� �˻� ����
        if(velocity.y < 0) 
        {
            velocity.y = checkDownSpeed(velocity.y);
        }
        else if(velocity.y > 0) //�ö� �� �˻� ���� 
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

    private float checkDownSpeed(float downSpeed) // �÷��̾� �߶� �� ���� Ȯ�� �߽��� �������� 4�и� 
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

    private float checkUpSpeed(float upSpeed) // �÷��̾� ��� �� ���� Ȯ�� �߽��� �������� 4�и� 
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
