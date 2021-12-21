using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shoot : MonoBehaviour
{

    #region variables

    public static float rayLength = 20f;
    private float bulletSpeed = 10f;
    public static float atkDmg = 1f;
    private float atkSpd = 1f;
    private AimController aimController = null;
    private World world = null;

    private bool somethingHit = false;
    private bool isBazooka = false;

    private GameObject rocketTrail = null;
    private GameObject muzzle = null;
    private GameObject metalHitEffect = null;
    private GameObject sandHitEffect = null;
    private GameObject stoneHitEffect = null;
    private GameObject woodHitEffect = null;
    [SerializeField]
    private GameObject[] fleshHitEffects = null;
    private SpawnManager spawnManager = null;

    private float splashRange = 5f;

    private AudioClip treeBurn = null;

    [SerializeField]
    private Image weaponType = null;
    private Sprite pistol = null;
    private Sprite bazooka = null;
    #endregion

    private void Start()
    {

        #region Initialization

        aimController = FindObjectOfType<AimController>();
        spawnManager = FindObjectOfType<SpawnManager>();
        world = FindObjectOfType<World>();
        rocketTrail = Resources.Load<GameObject>("Prefabs/Effect/Weapon/RocketTrailEffect");
        muzzle = Resources.Load<GameObject>("Prefabs/Effect/Weapon/MuzzleFlashEffect");
        metalHitEffect = Resources.Load<GameObject>("Prefabs/Effect/Weapon/BulletImpactMetalEffect");
        sandHitEffect = Resources.Load<GameObject>("Prefabs/Effect/Weapon/BulletImpactSandEffect");
        stoneHitEffect = Resources.Load<GameObject>("Prefabs/Effect/Weapon/BulletImpactStoneEffect");
        woodHitEffect = Resources.Load<GameObject>("Prefabs/Effect/Weapon/BulletImpactWoodEffect");
        pistol = Resources.Load<Sprite>("Textures/Weapon/pistol");
        bazooka = Resources.Load<Sprite>("Textures/Weapon/bazooka");

        treeBurn = SoundManager.Instance.TreeFire;

        atkDmg = (GameManager.AtkUp + GameManager.level) * 0.5f + 1f;
        atkSpd = 1 + GameManager.AtkSpeedUp * 0.2f;

        #endregion

        StartCoroutine(ShootRayCoroutine());
    }

    private void Update()
    {
        Aiming();
    }

    private IEnumerator ShootRayCoroutine()
    {
        while(true)
        {
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            Player player = transform.parent.GetComponent<Player>();
            player.eState = Player.EState.ATK;
            player.isAtk = true;

            //WaitFor Shoot delay for two shots
            yield return new WaitForSeconds((1.133f / 2f) / atkSpd);
            StartCoroutine(ShootRay());
            StartCoroutine(WeaponImgAnimate());
            yield return new WaitForSeconds((1.133f / 4f) / atkSpd);
            StartCoroutine(ShootRay());
            StartCoroutine(WeaponImgAnimate());
            yield return new WaitForSeconds((1.133f / 4f) / atkSpd);
            SoundManager.Instance.SFXSource.PlayOneShot(SoundManager.Instance.Reload);
            player.isAtk = false;
        }
    }

    private IEnumerator ShootRay()
    {
        #region Initialization
        Ray shootRay = new Ray();
        RaycastHit shootRaycastHit = new RaycastHit();
        float timer = 0f;

        shootRay.origin = transform.position + transform.forward;
        shootRay.direction = transform.forward;
        Vector3 bulletPos = shootRay.origin;

        GameObject myRocket = null;
        GameObject myMuzzle = null;
        
        #endregion

        //BazookaMode
        if (isBazooka)
        {
            #region Instantiate rocketTrail

            myRocket = Instantiate(rocketTrail, bulletPos, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.parent.eulerAngles.y, 0f));
            List<ParticleSystem> rocketParticles = new List<ParticleSystem>();
            
            ParticleSystem myRocketSystem = myRocket.GetComponent<ParticleSystem>();
            ParticleSystem myRocketTrail = myRocket.transform.Find("RocketTrail").GetComponent<ParticleSystem>();
            ParticleSystem myRocketWarhead = myRocket.transform.Find("RocketWarhead").GetComponent<ParticleSystem>();

            rocketParticles.Add(myRocketSystem);
            rocketParticles.Add(myRocketTrail);
            rocketParticles.Add(myRocketWarhead);

            for(int i=0; i< rocketParticles.Count; ++i)
            {
                rocketParticles[i].startSpeed = bulletSpeed;
            }

            #endregion
            //Sound
            SoundManager.Instance.SFXSource.PlayOneShot(SoundManager.Instance.FireImpact);
        } 
        // Normal Shooting
        else
        {
            myMuzzle = Instantiate(muzzle, bulletPos, Quaternion.LookRotation(transform.right));
            //Sound
            SoundManager.Instance.SFXSource.PlayOneShot(SoundManager.Instance.GunSilencer);
        }

        //s = vt; length = bulletSpeed * Time;
        while(rayLength >= bulletSpeed * timer)
        {
            timer += Time.deltaTime;
            bulletPos += shootRay.direction * bulletSpeed * Time.deltaTime;

            //bullet Particle position sync to imagine bullet when using normal Shoot;
            //Bazooka particles move forward on their own;
            if(myMuzzle) myMuzzle.transform.position = bulletPos;


            if(Physics.Raycast(shootRay, out shootRaycastHit, rayLength))
            {
                Collider curCollider = shootRaycastHit.collider;

                if((bulletPos - shootRaycastHit.point).magnitude <= 1f)
                {
                    if(curCollider.CompareTag("Enemy"))
                    {
                        //PlayerAttack ±¸Çö
                        curCollider.GetComponent<Monster>().GetDmg(atkDmg);
                        SpawnDecal(shootRaycastHit, fleshHitEffects[Random.Range(0, fleshHitEffects.Length)]);
                        break;
                    }
                    else if(curCollider.CompareTag("Tree"))
                    {
                        //vertex Color Black to Red
                        BlacktoRed(curCollider);
                        for(int i=0; i< curCollider.transform.parent.GetComponentsInChildren<MeshRenderer>().Length; ++i)
                        {
                            MeshRenderer curMeshRenderer = curCollider.transform.parent.GetComponentsInChildren<MeshRenderer>()[i];
                            StartCoroutine(Burning(curMeshRenderer));
                        }
                        break;
                    }
                    else if(curCollider.CompareTag("Boss"))
                    {
                        curCollider.GetComponent<Boss>().Damage(atkDmg);
                        SpawnDecal(shootRaycastHit, fleshHitEffects[Random.Range(0, fleshHitEffects.Length)]);
                        break;
                    }
                    else
                    {
                        HandleHit(shootRaycastHit, shootRay.direction);

                        //Splash when you use BAZZOOKKKAAAA
                        if(!curCollider.CompareTag("Enemy") && isBazooka)
                        {
                            for(int i=0; i<spawnManager.monsterList.Count; ++i)
                            {
                                if (spawnManager.monsterList[i] && Vector3.Distance(shootRaycastHit.point, spawnManager.monsterList[i].transform.position) <= splashRange)
                                {
                                    spawnManager.monsterList[i].GetDmg(GameManager.SplashDmgUp * 0.3f);
                                    Debug.Log(GameManager.SplashDmgUp * 0.3f);
                                }
                            }
                        }
                        else if(curCollider.CompareTag("Enemy") && isBazooka)
                        {
                            curCollider.gameObject.GetComponent<Monster>().GetDmg(atkDmg + GameManager.SplashDmgUp * 0.3f);
                        }
                        break;
                    }
                }
                somethingHit = true;
            }
            yield return new WaitForEndOfFrame();
        }

        if (shootRaycastHit.collider && isBazooka)
        {
            StartCoroutine(Boom(shootRaycastHit.point));
            //Bazooka Sound
            SoundManager.Instance.SFXSource.PlayOneShot(SoundManager.Instance.Explosion, 5 / Vector3.Distance(shootRaycastHit.point, transform.position));
        }
        else if(shootRaycastHit.collider)
        {
            //Normal shoot hitSound
            SoundManager.Instance.SFXSource.PlayOneShot(SoundManager.Instance.Hit, (5 / Vector3.Distance(shootRaycastHit.point, transform.position)) * 0.7f);
        }

        yield return new WaitUntil(() => rayLength <= bulletSpeed * timer || somethingHit);

        if(myRocket)
            Destroy(myRocket);
        else if(myMuzzle)
            Destroy(myMuzzle);

        somethingHit = false;
    }

    #region Handle RayCastHit (GetBlockType and Instantiate differnt effect by Type)

    //Get VoxelValue(blockType) for diffrent effect which use hit something
    private void HandleHit(RaycastHit hit, Vector3 _dir)
    {
        if (hit.collider)
        {
            string blockName = "";
            ChunkCoord thisChunk = new ChunkCoord(hit.point + _dir * 0.1f);
            blockName = DestructVoxel(thisChunk, hit, _dir);

            switch (blockName)
            {
                case "Grass":
                    SpawnDecal(hit, metalHitEffect);
                    break;
                case "Dirt":
                    SpawnDecal(hit, sandHitEffect);
                    break;
                case "Stone":
                    SpawnDecal(hit, stoneHitEffect);
                    break;
                case "Sand":
                    SpawnDecal(hit, sandHitEffect);
                    break;
                case "Tree":
                    SpawnDecal(hit, woodHitEffect);
                    break;
            }
        }
    }

    #region Voxel Destruct

    private string DestructVoxel(ChunkCoord _thisChunk, RaycastHit _hit, Vector3 _dir)
    {

        BlockType curBlockType = world.blockTypes[world.chunks[_thisChunk.x, _thisChunk.z].GetVoxelFromGlobalVector3(_hit.point + _dir * 0.01f)];
        string blockName = curBlockType.blockName;

        //if normalAttack, don't destroy 
        if (!isBazooka) return blockName;

        //Block Hit and hp-- => Destruct block
        if (blockName == "Air")
        {
            world.chunks[_thisChunk.x, _thisChunk.z].CreateMesh();
            return blockName;
        }
        else
        {
            --curBlockType.hp;
        }

        if (curBlockType.hp <= 0)
        {
            curBlockType.hp = 4;
            Vector3 hitDirPos = _hit.point + _dir * 0.1f;

            CreateBrokenMesh(new Vector3((int)hitDirPos.x, (int)hitDirPos.y, (int)hitDirPos.z), _thisChunk);
            world.chunks[_thisChunk.x, _thisChunk.z].EditVoxel(_hit.point + _dir * 0.01f, 0);

            return blockName;
        }
        return blockName;
    }

    //Create New Mesh when destroy curChunk's voxel with EditVoxel func
    //This new Mesh position is same as old voxel and exactly same other values
    //But new one is seperate from chunkMesh. then new Mesh destroyEffect start! BOOM!
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
            if (blockID == 0) return;

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

    private void SpawnDecal(RaycastHit hit, GameObject prefab)
    {
        GameObject spawnedDecal = Instantiate(prefab, hit.point, Quaternion.LookRotation(hit.normal));
        spawnedDecal.transform.SetParent(hit.collider.transform);
    }

    #endregion

    #endregion

    #region Rocket Boom

    private IEnumerator Boom(Vector3 _pos)
    {
        GameObject myBoom = Resources.Load<GameObject>("Prefabs/Effect/SmallExplosionEffect");
        myBoom = Instantiate(myBoom, _pos, Quaternion.identity);
        yield return new WaitForSeconds(myBoom.GetComponent<ParticleSystem>().main.startLifetime.constantMax);
        Destroy(myBoom);
    }

    #endregion

    #region Burnning Tree

    private IEnumerator Burning(MeshRenderer _meshRenderer)
    {
        Material _material = _meshRenderer.material;
        GameObject fireEmbers = _meshRenderer.transform.parent.Find("FireEmbers").gameObject;
        GameObject smoke = _meshRenderer.transform.parent.Find("Smoke").gameObject;
        fireEmbers.SetActive(true);
        smoke.SetActive(true);
        fireEmbers.GetComponent<ParticleSystem>().Play();
        
        float cutOffValue = _material.GetFloat("_Cutoff");
        float timer = Mathf.Pow((cutOffValue - 0.1f), 0.5f);

        //Burn Sound
        AudioSource treeAudio = _meshRenderer.gameObject.AddComponent<AudioSource>();
        treeAudio.spatialBlend = 1f;
        treeAudio.maxDistance = rayLength;
        treeAudio.volume *= SoundManager.Instance.SFXVolume;
        treeAudio.PlayOneShot(treeBurn);

        while(_material.GetFloat("_Cutoff") <= 0.9f)
        {
            timer += Time.deltaTime * 0.05f;
            cutOffValue = 0.1f + Mathf.Pow(timer, 2);
            _material.SetFloat("_Cutoff", cutOffValue);

            yield return null;
        }
        if(_meshRenderer)
        {
            Destroy(_meshRenderer.gameObject);
            Destroy(fireEmbers);
            Destroy(smoke);
        }
    }
    private void BlacktoRed(Collider _collider)
    {
        for(int i =0; i < _collider.transform.parent.GetComponentsInChildren<MeshFilter>().Length; ++i)
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
    
    //when aimed monster, cursor img change to red
    private void Aiming()
    {

        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            isBazooka = !isBazooka;
            WeaponImgChange();
        }

        Ray aimRay = new Ray();
        RaycastHit aimRaycastHit = new RaycastHit();

        aimRay.origin = transform.position;
        aimRay.direction = transform.forward;

        Physics.Raycast(aimRay, out aimRaycastHit, rayLength * 2f);
        if(aimRaycastHit.collider && aimRaycastHit.collider.CompareTag("Enemy"))
        {
            float distance = Vector3.Distance(transform.position, aimRaycastHit.collider.transform.position);
            if(rayLength >= distance)
            {
                for(int i=0; i<aimController.rectImg.Length; ++i)
                {
                    aimController.rectImg[i].color = Color.red;
                }
            }
        }
        else
        {
            for (int i = 0; i < aimController.rectImg.Length; ++i)
            {
                aimController.rectImg[i].color = Color.blue;
            }
        }
    }

    private void WeaponImgChange()
    {
        if(isBazooka)
        {
            weaponType.sprite = bazooka;
        }
        else
        {
            weaponType.sprite = pistol;
        }
    }
    private IEnumerator WeaponImgAnimate()
    {
        float xPos = 0f;

        for(int i = 1; i < 5; ++i)
        {
            xPos = -i * 5f;
            weaponType.GetComponent<RectTransform>().anchoredPosition = new Vector3(xPos, 0f, 0f);
            yield return new WaitForSeconds(Time.deltaTime * 0.5f);
        }
        
        float moveSpeed = 50f;
        while(xPos <= 0f)
        {
            xPos += moveSpeed * Time.deltaTime;
            weaponType.GetComponent<RectTransform>().anchoredPosition = new Vector3(xPos, 0f, 0f);
            yield return null;
        }

    }
}

