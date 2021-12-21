using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MonsterData 
{
    private static int stageNum = (int)StageData.stageNum;
    #region Boss
    public static float hp = 100f * stageNum;
    public static float fireDamage = 0.04f * stageNum;
    public static float laserDamage = 0.08f * stageNum;
    public static float flameDamage = 0.005f * stageNum;
    public static float grenadeDamage = 4.0f * stageNum;
    #endregion

    #region Monster

    #region Cactus
    public static float CactusAtkDmg = 1.5f * stageNum;
    public static float CactusAtkSpd = 0.6f * stageNum;
    public static float CactusMaxHp = 8f * stageNum;
    #endregion

    #region CuteMushroom
    public static float CuteMushroomAtkDmg = 0.5f * stageNum;
    public static float CuteMushroomAtkSpd = 2f * stageNum;
    public static float CuteMushroomMaxHp = 4f * stageNum;
    #endregion

    #region Dragon
    public static float DragonAtkDmg = 2f * stageNum;
    public static float DragonAtkSpd = 1f * stageNum;
    public static float DragonMaxHp = 12f * stageNum;
    #endregion

    #region Littleboar
    public static float LittleboarAtkDmg = 1f * stageNum;
    public static float LittleboarAtkSpd = 1f * stageNum;
    public static float LittleboarMaxHp = 10f * stageNum;
    #endregion

    #region Magictree
    public static float MagictreeAtkDmg = 3f * stageNum;
    public static float MagictreeAtkSpd = 0.5f * stageNum;
    public static float MagictreeMaxHp = 20f * stageNum;
    #endregion

    #region Minimonsters
    public static float MinimonstersAtkDmg = 0.4f * stageNum;
    public static float MinimonstersAtkSpd = 0.8f * stageNum;
    public static float MinimonstersMaxHp = 6f * stageNum;
    #endregion

    #region Momo
    public static float MomoAtkDmg = 1f * stageNum;
    public static float MomoAtkSpd = 1f * stageNum;
    public static float MomoMaxHp = 8f * stageNum;
    #endregion

    #region MonsterFlower
    public static float MonsterFlowerAtkDmg = 0.5f * stageNum;
    public static float MonsterFlowerAtkSpd = 1f * stageNum;
    public static float MonsterFlowerMaxHp = 6f * stageNum;
    #endregion

    #region Penguin
    public static float PenguinAtkDmg = 1.5f * stageNum;
    public static float PenguinAtkSpd = 0.8f * stageNum;
    public static float PenguinMaxHp = 12f * stageNum;
    #endregion

    #region Sheep
    public static float SheepAtkDmg = 0.8f * stageNum;
    public static float SheepAtkSpd = 0.5f * stageNum;
    public static float SheepMaxHp = 8f * stageNum;
    #endregion

    #region Snake
    public static float SnakeAtkDmg = 0.4f * stageNum;
    public static float SnakeAtkSpd = 3f * stageNum;
    public static float SnakeMaxHp = 4f * stageNum;
    #endregion

    #region Teruteru
    public static float TeruteruAtkDmg = 2.5f * stageNum;
    public static float TeruteruAtkSpd = 0.5f * stageNum;
    public static float TeruteruMaxHp = 4f * stageNum;
    #endregion

    #region Tonton
    public static float TontonAtkDmg = 1f * stageNum;
    public static float TontonAtkSpd = 1f * stageNum;
    public static float TontonMaxHp = 10f * stageNum;
    #endregion

    #endregion
}
