using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class StageData
{
    public enum StageNum
    {
        LOBBY,
        STAGE_1,
        STAGE_2,
    }

    public struct SetValue
    {
        public byte topBlock;
        public byte middleBlock;
    }

    public static StageNum stageNum = StageNum.STAGE_1;

    public static SetValue SetValues;


    public static void SetStage()
    {
        if(stageNum == StageNum.STAGE_1)
        {
            StageValue_1();
        }
        else if(stageNum == StageNum.STAGE_2)
        {
            StageValue_2();
        }
    }

    //초원
    private static void StageValue_1()
    {
        SetValues.topBlock = 3;
        SetValues.middleBlock = 5;
    }
    //사막
    private static void StageValue_2()
    {
        SetValues.topBlock = 4;
        SetValues.middleBlock = 5;
    }



}
