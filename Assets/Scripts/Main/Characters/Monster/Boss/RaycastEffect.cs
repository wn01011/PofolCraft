using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastEffect : MonoBehaviour
{
    #region variables
    [SerializeField]
    private GameObject rayBody = null;
    [SerializeField]
    private GameObject scaleDistance = null;
    [SerializeField]
    private GameObject rayResult = null;
    [SerializeField]
    private GameObject cube = null;

    private World world = null;
    private AudioSource audioSource = null;
    
    public ECubeType _cubeType = new ECubeType();

    private GameObject flameThrower = null;
    private GameObject flameEffect = null;

    private GameObject grenade = null;
    private float grenadeTimer = 1.0f;

    private float rayRotateSpeed = 0.3f;
    private float fireDmgTimer = 0.05f;
    private float laserDmgTimer = 0.05f;
    public RaycastHit grassHitResult = new RaycastHit();

    private Transform focus = null;
    private Player player = null;
    private Boss boss = null;

    private MyGeometryGrassPainter grassPainter = null;
    public bool grassDraw = false;

    private AudioClip grassFire = null;
    private int grassFireCount = 0;

    #region Cube Attack Damage
    //Ground and Grass have not direct Dmg to Player
    //Ground => shoot Grenade(grenade has damage not cube)
    //Grass  => grass has damage when it burn by fireatk
    private float fireDamage = 0.04f;
    private float laserDamage = 0.08f;
    #endregion
    #endregion

    public enum ECubeType
    {
        FIRE,
        LASER,
        GROUND,
        GRASS,
    }

    #region CubeAttack

    private void CubeAttack(RaycastHit _hitResult)
    {
        switch (_cubeType)
        {
            case ECubeType.FIRE:
                {
                    rayRotateSpeed = 0.2f;
                    fireDmgTimer -= Time.deltaTime;
                    FireAttack(_hitResult);
                }
                break;
            case ECubeType.LASER:
                {
                    rayRotateSpeed = 0.3f;
                    laserDmgTimer -= Time.deltaTime;
                    LaserAttack(_hitResult);
                }
                break;
            case ECubeType.GROUND:
                GroundAttack();
                break;
            case ECubeType.GRASS:
                rayRotateSpeed = 0.1f;
                GrassAttack(_hitResult);
                break;
        }
    }

    private void FireAttack(RaycastHit _hitResult)
    {
        float distance = Vector3.Distance(_hitResult.point, player.transform.position);
        if ((distance <= 0.5f || _hitResult.collider.CompareTag("Player")) && fireDmgTimer <= 0f)
        {
            player.TakeDamage(fireDamage);
        }
        else if(_hitResult.collider.CompareTag("Tree"))
        {
            boss.DestroyTree(_hitResult.collider);
        }
        else
        {
            for(int i = 0; i < grassPainter.positions.Count; ++i)
            {
                float grassDist = Vector3.Distance(grassPainter.positions[i], _hitResult.point);
                Vector3 destroyPos = Vector3.zero;

                if(grassDist <= 5f)
                {
                    destroyPos = grassPainter.positions[i];

                    grassPainter.positions.RemoveAt(i);
                    grassPainter.colors.RemoveAt(i);
                    grassPainter.normals.RemoveAt(i);
                    grassPainter.length.RemoveAt(i);
                    grassPainter.indicies.RemoveAt(i);
                    grassPainter.i--;
                    for (int j = 0; j < grassPainter.indicies.Count; j++)
                    {
                        grassPainter.indicies[j] = j;
                    }

                    //Instantiate flame Effect where Destroy Grass
                    Instantiate(flameEffect, destroyPos, Quaternion.identity);
                    ++grassFireCount;
                    StartCoroutine(BurnGrass(destroyPos));
                }
            }
        }

        if(fireDmgTimer <= 0f)
        {
            fireDmgTimer = 0.05f;
        }
    }
    private void LaserAttack(RaycastHit _hitResult)
    {
        float distance = Vector3.Distance(_hitResult.point, player.transform.position);
        if ((distance <= 0.5f || _hitResult.collider.CompareTag("Player")) && laserDmgTimer <= 0f)
        {
            player.TakeDamage(laserDamage);
        }
        if(laserDmgTimer <= 0f)
        {
            laserDmgTimer = 0.0f;
        }
    }
    private void GroundAttack()
    {
        grenadeTimer -= Time.deltaTime;
        if(grenadeTimer <= 0f)
        {
            grenadeTimer = 1.0f;
            Instantiate(grenade, transform.position, Quaternion.identity);
        }
    }
    private void GrassAttack(RaycastHit _hitResult)
    {
        if(world.GetVoxel(_hitResult.point + 0.1f * transform.forward) == 6 || Vector3.Distance(_hitResult.point,boss.transform.position) <= 5f)
        {
            grassHitResult = new RaycastHit();
            grassDraw = false;
        }
        else
        {
            grassHitResult = _hitResult;
            grassDraw = true;
        }
    }
    #endregion

    private void Start()
    {
        #region initialization
        player = FindObjectOfType<Player>();
        world = FindObjectOfType<World>();
        audioSource = gameObject.AddComponent<AudioSource>();
        focus = transform.parent;
        boss = transform.parent.parent.GetComponent<Boss>();
        flameThrower = Resources.Load<GameObject>("Prefabs/Effect/FlameThrowerEffect");
        flameThrower = Instantiate(flameThrower, focus.position, Quaternion.identity, focus);
        flameThrower.SetActive(false);

        flameEffect = Resources.Load<GameObject>("Prefabs/Effect/FlamesEffects");

        grenade = Resources.Load<GameObject>("Prefabs/Grenade/Weapon Grenade");

        grassPainter = FindObjectOfType<MyGeometryGrassPainter>();
        grassFire = SoundManager.Instance.TreeFire;

        fireDamage = MonsterData.fireDamage;
        laserDamage = MonsterData.laserDamage;
        #endregion
    }
    private void Update()
    {
        if (boss._state != Boss.EState.CAST) return;

       
        if(_cubeType == ECubeType.LASER || _cubeType == ECubeType.GRASS)
        {
            ShootRay();
        }
        else if(_cubeType == ECubeType.FIRE || _cubeType == ECubeType.GROUND)
        {
            ShootEffect();
        }
        else
        {
            rayBody.SetActive(false);
            scaleDistance.SetActive(false);
            rayResult.SetActive(false);
        }
    }

    //Laser Effect Laser = rayBody(start) + scaleDistance(middle"actual ray") + rayResult(rayEndpoint)
    private void ShootRay()
    {
        if (!cube || !boss)
        {
            Destroy(flameThrower);
            Destroy(gameObject);
            return;
        }

        transform.position = cube.transform.position;
        Vector3 lookDir = Vector3.zero;
        
        if(_cubeType != ECubeType.GRASS)
        {
            lookDir = ((player.transform.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f))) - transform.position).normalized;
            focus.transform.forward = Vector3.Lerp(transform.forward, lookDir, Time.deltaTime * rayRotateSpeed);
        }
        else if(_cubeType == ECubeType.GRASS)
        {
            lookDir = (player.transform.position + player.transform.forward * 2f - transform.position).normalized;
            focus.transform.forward = Vector3.Lerp(transform.forward, lookDir, Time.deltaTime * rayRotateSpeed * 2f);
        }


        if (Vector3.Dot((transform.position - boss.transform.position).normalized, boss.transform.forward) <= -0.2f)
        {
            rayBody.SetActive(false);
            scaleDistance.SetActive(false);
            rayResult.SetActive(false);
            grassDraw = false;
            return;
        }
        else
        {
            rayBody.SetActive(true);
            scaleDistance.SetActive(true);
            rayResult.SetActive(true);
        }

        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(transform.position, transform.forward, out hit, 40f))
        {
            rayBody.SetActive(true);
            scaleDistance.SetActive(true);
            rayResult.SetActive(true);
            scaleDistance.transform.localScale = new Vector3(0.1f, hit.distance * 0.5f, 0.1f);
            scaleDistance.transform.position = (rayBody.transform.position + rayResult.transform.position) * 0.5f;
            rayResult.transform.position = hit.point;
            rayResult.transform.rotation = Quaternion.LookRotation(hit.normal);

            CubeAttack(hit);
        }
        else
        {
            rayBody.SetActive(false);
            scaleDistance.SetActive(false);
            rayResult.SetActive(false);
            grassDraw = false;
            return;
        }
    }

    private void ShootEffect()
    {
        transform.position = cube.transform.position;
        Vector3 lookDir = ((player.transform.position + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f))) - transform.position).normalized;
        focus.transform.forward = Vector3.Lerp(transform.forward, lookDir, Time.deltaTime * rayRotateSpeed);
        flameThrower.transform.position = cube.transform.position;

        rayBody.SetActive(false);
        scaleDistance.SetActive(false);
        rayResult.SetActive(false);


        if (Vector3.Dot((transform.position - boss.transform.position).normalized, boss.transform.forward) <= -0.2f)
        {
            flameThrower.SetActive(false);
            return;
        }
        else if (_cubeType == ECubeType.FIRE)
        {
            flameThrower.SetActive(true);
        }
        else
        {
            flameThrower.SetActive(false);
        }


        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(transform.position, transform.forward, out hit, 40f))
        {
            scaleDistance.transform.localScale = new Vector3(0.1f, hit.distance * 0.5f, 0.1f);
            scaleDistance.transform.position = (rayBody.transform.position + rayResult.transform.position) * 0.5f;
            rayResult.transform.position = hit.point;
            rayResult.transform.rotation = Quaternion.LookRotation(hit.normal);

            CubeAttack(hit);
        }
        else
        {
            flameThrower.SetActive(false);
            return;
        }
    }

    private IEnumerator BurnGrass(Vector3 _pos)
    {
        audioSource.maxDistance = 20f;
        audioSource.spatialBlend = 1f;
        audioSource.volume *= SoundManager.Instance.SFXVolume;
        if(grassFireCount <= 3)
            AudioSource.PlayClipAtPoint(grassFire, _pos, audioSource.volume);
        yield return new WaitForSeconds(grassFire.length);
        --grassFireCount;
    }
}