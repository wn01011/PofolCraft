using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class StageController : MonoBehaviour
{
    public void SetStage(int _idx)
    {
        if(_idx == 0)
        {
            StageData.stageNum = StageData.StageNum.STAGE_1;
            SceneController.Instance().StartLoadScene(2);
        }
        else if(_idx == 1)
        {
            StageData.stageNum = StageData.StageNum.STAGE_2;
            SceneController.Instance().StartLoadScene(2);
        }
        else
        {
            SceneController.Instance().StartLoadScene(4);
        }
    }
    
}
