using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Grenade : MonoBehaviour
{
    #region variables

    private float shootForce = 10.0f;
    private float grenadeDamage = 4.0f;
    private ParticleSystem explosionParticle = null;
    private Player player = null;
    private GameObject explodeArea = null;
    private World world = null;

    #endregion

    private void Start()
    {
        #region initialization
        Rigidbody rigidbody = GetComponent<Rigidbody>();

        player = FindObjectOfType<Player>();
        world = FindObjectOfType<World>();

        explodeArea = GetComponentsInChildren<SphereCollider>()[1].gameObject;
        explosionParticle = GetComponentInChildren<ParticleSystem>();
        grenadeDamage = MonsterData.grenadeDamage;
        #endregion

        //����ź ��ô ���� ���� : Vector3.up �������� xz ���⿡ -0.5, 0.5 �� �������� �༭ ���� ����
        Vector3 forceDir = new Vector3(Random.Range(-0.5f, 0.5f), 1.0f, Random.Range(-0.5f, 0.5f)).normalized;
        rigidbody.AddForce(forceDir * shootForce, ForceMode.Impulse);

        StartCoroutine(ExplodeCo());
        StartCoroutine(ExplodeAreaCo());
    }

    #region explode Delay
    //����ź�� �߻�ǰ� �����ð� �ڷ�ƾ
    private IEnumerator ExplodeCo()
    {
        yield return new WaitForSeconds(4.0f);
        StopCoroutine(ExplodeAreaCo());
        Destroy(explodeArea);
        explosionParticle.Play();
        ExplodeGround();
        if(Vector3.Distance(player.transform.position, transform.position) <= 3.0f)
        {
            player.TakeDamage(grenadeDamage);
        }

        yield return new WaitForSeconds(explosionParticle.main.startLifetime.constantMax);
        Destroy(gameObject);
    }
    //���� �����ð����� ����ǥ�� �ڷ�ƾ
    private IEnumerator ExplodeAreaCo()
    {
        float timer = 4.0f;
        while(timer >= 0f)
        {
            timer -= Time.deltaTime;
            Vector3 areaScale = Vector3.one * 0.1f; 
            areaScale = Vector3.Lerp(areaScale, Vector3.one * 6f, (4- timer)/4.0f);
            explodeArea.transform.localScale = areaScale;
            yield return null;
        }
    }
    #endregion

    #region Ground Explosion
    // �ڱ� world.y���� x,z��� 3x3 �Ͷ߸���, (y - 1) ���� ��� 3x3�� �Ͷ߸��� - ��������
    private void ExplodeGround()
    {
        SoundManager.Instance.SFXSource.PlayOneShot(SoundManager.Instance.Explosion, (15 / Vector3.Distance(transform.position, player.transform.position)));
        
        #region SetDestructPos(3x3 + 3x3(downArea))
        SetDestructPos(transform.position, -0.5f * Vector3.up);
        SetDestructPos(transform.position, -0.5f * Vector3.up + new Vector3(-1f, 0f, -1f));
        SetDestructPos(transform.position, -0.5f * Vector3.up + new Vector3(-1f, 0f, 1f));
        SetDestructPos(transform.position, -0.5f * Vector3.up + new Vector3(1f, 0f, -1f));
        SetDestructPos(transform.position, -0.5f * Vector3.up + new Vector3(1f, 0f, 1f));
        SetDestructPos(transform.position, -0.5f * Vector3.up + new Vector3(-1f, 0f, 0f));
        SetDestructPos(transform.position, -0.5f * Vector3.up + new Vector3(1f, 0f, 0f));
        SetDestructPos(transform.position, -0.5f * Vector3.up + new Vector3(0f, 0f, -1f));
        SetDestructPos(transform.position, -0.5f * Vector3.up + new Vector3(0f, 0f, 1f));
        SetDestructPos(transform.position, new Vector3(-1f, 0f, -1f));
        SetDestructPos(transform.position, new Vector3(-1f, 0f, 1f));
        SetDestructPos(transform.position, new Vector3(1f, 0f, -1f));
        SetDestructPos(transform.position, new Vector3(1f, 0f, 1f));
        SetDestructPos(transform.position, new Vector3(-1f, 0f, 0f));
        SetDestructPos(transform.position, new Vector3(1f, 0f, 0f));
        SetDestructPos(transform.position, new Vector3(0f, 0f, -1f));
        SetDestructPos(transform.position, new Vector3(0f, 0f, 1f));
        #endregion

    }
    private void SetDestructPos(Vector3 _pos, Vector3 _dir)
    {
        Vector3 destructPos = _pos + _dir;
        if(world.GetChunkFromVector3(destructPos) != null)
            DestructVoxel(world.GetChunkFromVector3(destructPos).coord, destructPos);
    }

    #region Destruction
    private void DestructVoxel(ChunkCoord _thisChunk, Vector3 _pos)
    {
        if (world.chunks[_thisChunk.x, _thisChunk.z] == null) return;

        BlockType curBlockType = world.blockTypes[world.chunks[_thisChunk.x, _thisChunk.z].GetVoxelFromGlobalVector3(_pos)];
        string blockName = curBlockType.blockName;
        //Block Hit and hp-- => Destruct block
        if (blockName == "Air") return;

        
        --curBlockType.hp;
        

        if (curBlockType.hp <= 0)
        {
            curBlockType.hp = 1;
            

            CreateBrokenMesh(new Vector3((int)_pos.x, (int)_pos.y, (int)_pos.z), _thisChunk);
            world.chunks[_thisChunk.x, _thisChunk.z].EditVoxel(_pos, 0);

            return;
        }
        return ;
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
        brokenObject.GetComponent<DestructableObject>().ExplodeForce = 100f;
    }
    #endregion

    #endregion
}
