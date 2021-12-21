using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGeometryGrassPainter : MonoBehaviour
{
    #region variables
    private World world = null;
    private Boss boss = null;
    private Player player = null;
    private RaycastEffect bossGrass = null;

    private Mesh mesh = null;
    MeshFilter filter = null;

    public Color AdjustedColor = Color.clear;

    [Range(1, 600000)]
    public int grassLimit = 50000;

    private Vector3 lastPosition = Vector3.zero;

    public int toolbarInt = 0;

    public List<Vector3> positions = new List<Vector3>();
    public List<Color> colors = new List<Color>();
    public List<int> indicies = new List<int>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Vector2> length = new List<Vector2>();

    public bool painting = false;
    public bool removing = false;
    public bool editing = false;

    public int i = 0;

    public float sizeWidth = 1f;
    public float sizeLength = 1f;
    public float density = 1f;

    public float normalLimit = 1;

    public float rangeR, rangeG, rangeB = 0f;
    public LayerMask hitMask = 1;
    public LayerMask paintMask = 1;
    public float brushSize = 0f;

    Vector3 mousePos = Vector3.zero;

    [HideInInspector]
    public Vector3 hitPosGizmo = Vector3.zero;

    Vector3 hitPos = Vector3.zero;

    [HideInInspector]
    public Vector3 hitNormal = Vector3.zero;

    int[] indi;
    #endregion

    private void Start()
    {
        world = FindObjectOfType<World>();
        player = FindObjectOfType<Player>();
        filter = GetComponent<MeshFilter>();
        mesh = GetComponent<MeshFilter>().mesh;

        boss = FindObjectOfType<Boss>();
        for(int i=0; i < boss.GetComponentsInChildren<RaycastEffect>().Length; ++i)
        {
            if(boss.GetComponentsInChildren<RaycastEffect>()[i]._cubeType == RaycastEffect.ECubeType.GRASS)
            {
                bossGrass = boss.GetComponentsInChildren<RaycastEffect>()[i];
            }
        }
        
        //painting = true;
        //StartCoroutine(IsGroundThereCheck());

    }
    private void LateUpdate()
    {
        OnDrag();
    }

    private void OnDrag()
    {
        if(Input.GetMouseButton(1))
        {
            DrawMesh(Input.mousePosition);
        }
        else if(boss && boss._state == Boss.EState.CAST && bossGrass.grassHitResult.collider && bossGrass.grassDraw && bossGrass.grassHitResult.collider.GetComponent<MeshRenderer>()
            && (bossGrass.grassHitResult.collider.GetComponent<MeshRenderer>().material.name == "M_Blocks (Instance)")
            && Vector3.Dot(bossGrass.grassHitResult.point - player.transform.position, bossGrass.transform.position - player.transform.position) >= 0f)
        {
            Vector3 mousePos = Camera.main.WorldToScreenPoint(bossGrass.grassHitResult.point);
            DrawMesh(mousePos);
        }
        
    }

    private IEnumerator IsGroundThereCheck()
    {
        while(true)
        {
            for (int j = 0; j < positions.Count; j++)
            {
                Vector3 pos = positions[j];

                pos += this.transform.position;
                ChunkCoord chunkCoord = world.GetChunkFromVector3(pos - 0.1f * Vector3.up).coord;

                //if position is in air, removeIt
                if (world.chunks[chunkCoord.x, chunkCoord.z].GetVoxelFromGlobalVector3(pos + 0.1f * Vector3.up) == 0)
                {
                    positions.RemoveAt(j);
                    colors.RemoveAt(j);
                    normals.RemoveAt(j);
                    length.RemoveAt(j);
                    indicies.RemoveAt(j);
                    i--;
                    for (int i = 0; i < indicies.Count; i++)
                    {
                        indicies[i] = i;
                    }
                }
            }
            yield return new WaitForSeconds(3.0f);
        }
    }

    private void DrawMesh(Vector3 mousePos)
    {
        Ray ray = new Ray();
        RaycastHit raycastHit = new RaycastHit();
        ray = Camera.main.ScreenPointToRay(mousePos);

        if(Physics.Raycast(ray, out raycastHit , 20f))
        {
            hitPos = raycastHit.point;
        }
        if(toolbarInt == 0)
        {
            for (int k = 0; k < density; ++k)
            {
                float t = 2f * Mathf.PI * UnityEngine.Random.Range(0f, brushSize);
                float u = UnityEngine.Random.Range(0f, brushSize) + UnityEngine.Random.Range(0f, brushSize);
                float r = (u > 1 ? 2 - u : u);
                Vector3 origin = Vector3.zero;

                if (k != 0)
                {
                    origin.x += r * Mathf.Cos(t);
                    origin.y += r * Mathf.Sin(t);
                }
                else
                {
                    origin = Vector3.zero;
                }

                Ray randomRay = Camera.main.ScreenPointToRay(mousePos);
                randomRay.origin += origin;
                if (Physics.Raycast(randomRay, out raycastHit, 20f) && i < grassLimit && raycastHit.normal.y < (1 + normalLimit) && raycastHit.normal.y >= (1 - normalLimit))
                {
                    if ((1 << raycastHit.transform.gameObject.layer) > 0)
                    {
                        hitPos = raycastHit.point;
                        hitNormal = raycastHit.normal;
                        if (k != 0)
                        {
                            var grassPosition = hitPos;
                            grassPosition -= this.transform.position;

                            positions.Add(grassPosition);
                            indicies.Add(i);
                            length.Add(new Vector2(sizeWidth, sizeLength));
                            colors.Add(new Color(AdjustedColor.r + (UnityEngine.Random.Range(0, 1.0f) * rangeR), AdjustedColor.g + (UnityEngine.Random.Range(0, 1.0f) * rangeG), AdjustedColor.b + (UnityEngine.Random.Range(0, 1.0f) * rangeB), 1));

                            normals.Add(raycastHit.normal);
                            ++i;
                        }
                        else
                        {// to not place everything at once, check if the first placed point far enough away from the last placed first one
                            if (Vector3.Distance(raycastHit.point, lastPosition) > brushSize)
                            {
                                var grassPosition = hitPos;
                                grassPosition -= this.transform.position;
                                positions.Add((grassPosition));
                                indicies.Add(i);
                                length.Add(new Vector2(sizeWidth, sizeLength));
                                colors.Add(new Color(AdjustedColor.r + (UnityEngine.Random.Range(0, 1.0f) * rangeR), AdjustedColor.g + (UnityEngine.Random.Range(0, 1.0f) * rangeG), AdjustedColor.b + (UnityEngine.Random.Range(0, 1.0f) * rangeB), 1));
                                normals.Add(raycastHit.normal);
                                i++;

                                if (origin == Vector3.zero)
                                {
                                    lastPosition = hitPos;
                                }
                            }
                        }
                    }
                }
            }
        }
        else if(toolbarInt == 1)
        {
            hitPos = raycastHit.point;
            hitPosGizmo = hitPos;
            hitNormal = raycastHit.normal;
            for (int j = 0; j < positions.Count; j++)
            {
                Vector3 pos = positions[j];

                pos += this.transform.position;
                float dist = Vector3.Distance(raycastHit.point, pos);

                // if its within the radius of the brush, remove all info
                if (dist <= brushSize)
                {
                    positions.RemoveAt(j);
                    colors.RemoveAt(j);
                    normals.RemoveAt(j);
                    length.RemoveAt(j);
                    indicies.RemoveAt(j);
                    i--;
                    for (int i = 0; i < indicies.Count; i++)
                    {
                        indicies[i] = i;
                    }
                }
            }
        }
        else if(toolbarInt == 2)
        {
            hitPos = raycastHit.point;
            hitPosGizmo = hitPos;
            hitNormal = raycastHit.normal;
            for (int j = 0; j < positions.Count; j++)
            {
                Vector3 pos = positions[j];

                pos += this.transform.position;
                float dist = Vector3.Distance(raycastHit.point, pos);

                // if its within the radius of the brush, remove all info
                if (dist <= brushSize)
                {

                    colors[j] = (new Color(AdjustedColor.r + (UnityEngine.Random.Range(0, 1.0f) * rangeR), AdjustedColor.g + (UnityEngine.Random.Range(0, 1.0f) * rangeG), AdjustedColor.b + (UnityEngine.Random.Range(0, 1.0f) * rangeB), 1));

                    sizeWidth = UnityEngine.Random.Range(sizeWidth * 0.5f, sizeWidth);
                    sizeLength = UnityEngine.Random.Range(sizeLength * 0.5f, sizeWidth * 2f);
                    length[j] = new Vector2(sizeWidth, sizeLength);

                }
            }
        }

        mesh = new Mesh();
        mesh.SetVertices(positions);
        indi = indicies.ToArray();
        mesh.SetIndices(indi, MeshTopology.Points, 0);
        mesh.SetUVs(0, length);
        mesh.SetColors(colors);
        mesh.SetNormals(normals);
        filter.mesh = mesh;
    }


    public void ClearMesh()
    {
        i = 0;
        positions = new List<Vector3>();
        indicies = new List<int>();
        colors = new List<Color>();
        normals = new List<Vector3>();
        length = new List<Vector2>();
    }

}
