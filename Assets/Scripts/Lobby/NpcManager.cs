using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
    public delegate bool CallWorld(Vector3 _pos);
    private CallWorld CheckInVoxel = null;
    public delegate BuildingManager CallBuildingMG();
    private CallBuildingMG GetBuildingMg = null;
    public void AddFuncToNPCMg(CallWorld _chkVoxel, CallBuildingMG _buildingMg)
    {
        CheckInVoxel += _chkVoxel;
        GetBuildingMg += _buildingMg;
    }

    [SerializeField] private Player_Lobby player = null;
    private NpcController npcCtrl = null;

    private GameObject NpcGo = null;
    private GameObject portalPrefab = null;
    private GameObject portalGo = null;

    private void Start()
    {
        npcCtrl = Resources.Load<NpcController>("Prefabs/Npc/Npc");
        portalPrefab = Resources.Load<GameObject>("prefabs/Portal");
    }
    private void Update()
    {
        RotateToPlayer();
        ActivePotal();
    }

    public void NpcInit()
    {
        NpcGo = Instantiate(npcCtrl.gameObject);
        NpcPosCheck(NpcGo.transform, 0);
    }

    private void RotateToPlayer()
    {
        float dis = Vector3.Distance(NpcGo.transform.position, player.transform.position);
        Vector3 dir = player.transform.position - NpcGo.transform.position;
        dir = new Vector3(dir.x, 0f, dir.z).normalized;

        float detectRange = 3;

        if (dis <=detectRange)
        {
            NpcGo.transform.forward = Vector3.Lerp(NpcGo.transform.forward, dir, 0.1f);
        }
    }


    private void NpcPosCheck(Transform _npcPos, int _count)
    {
        _npcPos.position = SetNpcPos(GetBuildingMg().Getbuilding().GetTransform().position, player.transform.position) ;
    }


    private Vector3 SetNpcPos(Vector3 _buildingPos, Vector3 _playerPos)
    {
        Vector3 Lerppos = Vector3.Lerp(_buildingPos, _playerPos, 0.7f);
        Vector3 pos = new Vector3(Lerppos.x, Lerppos.y - 6f, Lerppos.z);
        return pos;
    }

    private void ActivePotal()
    {
        Ray ray = new Ray();
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit = new RaycastHit();
        if(Physics.Raycast(ray, out raycastHit))
        {
            if (Input.GetMouseButtonDown(0) && raycastHit.collider.CompareTag("Npc"))
            {

                if (portalGo == null)
                {
                    portalGo = Instantiate(portalPrefab);

                    float height = portalGo.GetComponent<BoxCollider>().bounds.size.y * 0.5f;
                    Debug.Log(height);
                    portalGo.transform.position = player.transform.position - NpcGo.transform.forward * 5f + Vector3.up * height;
                    portalGo.transform.rotation = NpcGo.transform.rotation;
                }
            }
        }
        
    }

    

}
