using System.Collections;
using UnityEngine;

public class Mushroom : Monster
{

    protected override void Start()
    {
        atkDmg = MonsterData.CuteMushroomAtkDmg;
        atkSpd = MonsterData.CuteMushroomAtkSpd;
        maxHp = MonsterData.CuteMushroomMaxHp;
        attackAnimLength = 0.667f;
        dieAnimLength = 1.333f;

        base.Start();
    }

    protected override void Attack(Player _Target)
    {
        attack = true;
        StartCoroutine(AttackCo(_Target));
        StartCoroutine(AttackAnimCo());
    }

    private IEnumerator AttackCo(Player _Target)
    {
        float attackCooldown = 5f / (1 + atkSpd);
        _Target.TakeDamage(atkDmg);
        Debug.Log("Attack! (" + _Target.GetHp() + " / " + _Target.GetMaxHp() + ")");
        yield return new WaitForSeconds(attackCooldown);
        attack = false;
    }
    private IEnumerator AttackAnimCo()
    {
        attackAnim = true;
        yield return new WaitForSeconds(attackAnimLength);
        attackAnim = false;
    }
}
