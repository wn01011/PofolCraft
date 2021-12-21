using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private float timer = 0f;
    private float rotateSpeed = 30f;
    private float forceAmount = 8f;
    private float eatDistance = 5f;
    private Transform player = null;
    private bool eat = false;

    private void Start()
    {
        player = FindObjectOfType<Player>().transform;
        Vector3 forceDir = new Vector3(Random.Range(-0.3f, 0.3f), 1f, Random.Range(-0.3f, 0.3f)).normalized;
        GetComponent<Rigidbody>().AddForce(forceDir * forceAmount, ForceMode.Impulse);
        SoundManager.Instance.SFXSource.PlayOneShot(SoundManager.Instance.Click);
    }

    private void Update()
    {
        timer += Time.deltaTime * rotateSpeed;
        transform.rotation = Quaternion.AngleAxis(timer % 360, Vector3.up);
        if(timer >= 60f)
            Eat();
    }

    private void Eat()
    {
        if(Vector3.Distance(player.position, transform.position) <= eatDistance && !eat)
        {
            ++GameManager.gold;

            eat = true;
            rotateSpeed += 500;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().AddForce(Vector3.up * forceAmount, ForceMode.Impulse);
            SoundManager.Instance.SFXSource.PlayOneShot(SoundManager.Instance.GoldDrop);
            StartCoroutine(SpinningCo());
        }

    }
    private IEnumerator SpinningCo()
    {
        while(rotateSpeed <= 1500f)
        {
            rotateSpeed += 12f;
            yield return null;
        }
        Destroy(gameObject);
    }
}
