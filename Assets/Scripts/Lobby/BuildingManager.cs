using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private List<Building> buildings = null;
    private Building building = null;

    [SerializeField] private List<Vector3> preBuildingPosList = new List<Vector3>();

    public delegate Chk CallWorld(Vector3 _pos);
    private CallWorld getChunkFromVector3 = null;
    public void AddFuncToBuildingMg(CallWorld _cal)
    {
        getChunkFromVector3 += _cal;
    }


    public Building Getbuilding() => building;

    private void Start()
    {
        LoadBuildingsAtList();
    }


    private void LoadBuildingsAtList()
    {
        foreach (Building bd in Resources.LoadAll<Building>("Prefabs/Structures/Buildings"))
        {
            buildings.Add(bd);
        }
    }
    public void AddPreBuildPosList(Vector3 _pos)
    {
        preBuildingPosList.Add(_pos);
    }

    public void SetBuildings(int _buildingIdx)
    {
        building = Instantiate<Building>(buildings[_buildingIdx]);
        for(int i = 0; i < preBuildingPosList.Count; i++)
        {
            int num = Random.Range(0, preBuildingPosList.Count);
            Vector3 pos = preBuildingPosList[num];
            if((pos.x > 12 && pos.x <36) && (pos.z >12 && pos.z <36))
            {
                if(!((pos.x > 20 && pos.x <30) && (pos.z >20 && pos.z <30)))
                {

                    building.SetPosition(pos + Vector3.up);
                    if (pos.x < (VD.ChunkWidth*VD.WorldSizeInChunks)*0.5f)
                    {
                        building.GetTransform().forward = Vector3.right;
                    }
                    else
                    {
                        building.GetTransform().forward = Vector3.left;
                    }
                }
            }
        }
        SetBuildingSolid(building);
    }

   

    private void SetBuildingSolid(Building _bd)
    {
        //좌하단 / 영점
        int LBX = (int)_bd.GetBuildingPos()[0].x;
        int LBY = (int)_bd.GetBuildingPos()[0].y;
        int LBZ = (int)_bd.GetBuildingPos()[0].z;

        //우상단 
        int RTX = (int)_bd.GetBuildingPos()[3].x;
        int RTY = (int)_bd.GetBuildingPos()[3].y;
        int RTZ = (int)_bd.GetBuildingPos()[3].z;



        for (int z = LBZ ; z <= RTZ-1; z += (RTZ-LBZ-1))
        {
            for(int x = LBX; x <= RTX; x++)
            { 
                for(int y = (int)_bd.transform.position.y ; y <= _bd.transform.position.y+LBY; y++)
                {
                    getChunkFromVector3(new Vector3(x, y, z)).EditVoxelMenuely(new Vector3(x, y, z), 8);
                }
            }
        }

        for(int x = LBX; x<=RTX;x += (RTX-LBX))
        {
            for(int z = LBZ; z<=RTZ; z++)
            {
                for(int y = (int)_bd.transform.position.y;y<=_bd.transform.position.y+RTY;y++)
                {
                    getChunkFromVector3(new Vector3(x, y, z)).EditVoxelMenuely(new Vector3(x, y, z), 8);
                }
            }
        }
    }


}
