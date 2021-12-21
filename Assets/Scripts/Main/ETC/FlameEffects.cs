using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameEffects : MonoBehaviour
{
    private Player player = null;
    private float flameDamage = 0.005f;
    private void Start()
    {
        flameDamage = MonsterData.flameDamage;

        player = FindObjectOfType<Player>();
        StartCoroutine(LifeTimeCo());
        StartCoroutine(AttackCo());
    }
    private IEnumerator LifeTimeCo()
    {
        SoundManager.Instance.SFXSource.PlayOneShot(SoundManager.Instance.Reload, 5 / Vector3.Distance(player.transform.position, transform.position));
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    private IEnumerator AttackCo()
    {
        while(!player.isDie)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if(dist <= 0.5f)
            {
                player.TakeDamage(flameDamage);
            }
            yield return null;
        }
    }
}
