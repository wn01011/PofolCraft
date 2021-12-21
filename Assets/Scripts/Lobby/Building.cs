using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Building : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI text = null;
    private Player_Lobby player = null;
    
    public TextMeshProUGUI GetTextMesh()
    {
        return text;
    }
    public Transform GetTransform()
    {
        return transform;
    }

    private void Start()
    {
        player = FindObjectOfType<Player_Lobby>();
    }

    private void Update()
    {
        text.transform.forward = -player.transform.forward;
    }

    public void SetPosition(Vector3 _pos)
    {
        transform.position = _pos;
    }

    public List<Vector3> GetBuildingPos()
    {
        List<Vector3> SolidBuildingPos = new List<Vector3>();

        BoxCollider mc = GetComponent<BoxCollider>();
        
        Vector3 LB = new Vector3(transform.position.x - mc.bounds.extents.x + 0.5f, transform.position.y - mc.bounds.size.y * 0.5f, transform.position.z - mc.bounds.extents.z + 0.5f);
        Vector3 RB = new Vector3(transform.position.x + mc.bounds.extents.x + 0.5f, transform.position.y - mc.bounds.size.y * 0.5f, transform.position.z - mc.bounds.extents.z + 0.5f);
        Vector3 LT = new Vector3(transform.position.x - mc.bounds.extents.x + 0.5f, transform.position.y - mc.bounds.size.y * 0.5f, transform.position.z + mc.bounds.extents.z + 0.5f);
        Vector3 RT = new Vector3(transform.position.x + mc.bounds.extents.x + 0.5f, transform.position.y - mc.bounds.size.y * 0.5f, transform.position.z + mc.bounds.extents.z + 0.5f);
        SolidBuildingPos.Add(LB);
        SolidBuildingPos.Add(RB);
        SolidBuildingPos.Add(LT);
        SolidBuildingPos.Add(RT);


        return SolidBuildingPos;
    }
    
    
    
}
