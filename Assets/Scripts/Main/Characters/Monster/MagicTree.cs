using System.Collections;
using UnityEngine;

public class MagicTree : Monster
{

    protected override void Start()
    {
        atkDmg = MonsterData.MagictreeAtkDmg;
        atkSpd = MonsterData.MagictreeAtkSpd;
        maxHp = MonsterData.MagictreeMaxHp;
        attackAnimLength = 1.5f;
        dieAnimLength = 2.0f;

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
