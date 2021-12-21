using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] GameObject imagineWater = null;
    private float imagineWaterHeight = 3f;
    private float seaLevel = 0f;
    private bool inWater = false;

    private MeshRenderer camFilter = null;
    private World world = null;
    private float distortionPower = 0.02f;

    private Color waterFilterColor = Color.white;

    private void Start()
    {
        #region Initialization

        camFilter = GetComponentInChildren<MeshRenderer>();
        world = FindObjectOfType<World>();
        imagineWater.SetActive(false);
        waterFilterColor = camFilter.material.color;
        camFilter.material.color = Color.white;
        camFilter.material.SetFloat("_Strength", 0f);
        seaLevel = world.biome.solidGroundHeight * 0.3f;

        #endregion

        StartCoroutine(UnderWaterCheck());
        StartCoroutine(UnderWaterSound());
    }


    private void FixedUpdate()
    {
        imagineWater.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        imagineWaterHeight = world.biome.solidGroundHeight + seaLevel + 0.5f - transform.position.y;
        if (inWater)
        {
            imagineWater.transform.localPosition = new Vector3(0f, imagineWaterHeight + 2f, 0f);
        }
        else
            imagineWater.transform.localPosition = Vector3.up * 3f;
    }
    private IEnumerator UnderWaterSound()
    {
        while(inWater)
        {
            SoundManager.Instance.BGMSource.PlayOneShot(SoundManager.Instance.InWater);
            yield return new WaitForSeconds(SoundManager.Instance.InWater.length * 0.8f);
        }
        yield return new WaitUntil(() => inWater);
        StartCoroutine(UnderWaterSound());
    }

    private IEnumerator UnderWaterCheck()
    {
        //wait for underWater
        yield return new WaitUntil(() => transform.position.y <= world.biome.solidGroundHeight + seaLevel + 0.5f && transform.position.y >= world.biome.solidGroundHeight);
        inWater = true;
        camFilter.material.color = waterFilterColor;
        camFilter.material.SetFloat("_Strength", distortionPower);
        foreach (GameObject water in world.waters)
        {
            water.SetActive(false);
        }
        imagineWater.SetActive(true);

        //wait for onGround
        yield return new WaitUntil(() => !(transform.position.y <= world.biome.solidGroundHeight + seaLevel +0.5f && transform.position.y >= world.biome.solidGroundHeight));
        inWater = false;
        camFilter.material.color = Color.white;
        camFilter.material.SetFloat("_Strength", 0f);
        foreach (GameObject water in world.waters)
        {
            water.SetActive(true);
        }
        imagineWater.SetActive(false);

        //Restart
        StartCoroutine(UnderWaterCheck());
    }
}
