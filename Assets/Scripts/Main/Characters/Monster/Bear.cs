using System.Collections;
using UnityEngine;

public class Bear : Monster
{

    protected override void Start()
    {
        atkDmg = MonsterData.MinimonstersAtkDmg;
        atkSpd = MonsterData.MinimonstersAtkSpd;
        maxHp = MonsterData.MinimonstersMaxHp;
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
