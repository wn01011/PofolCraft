using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
    [SerializeField] private World world = null;
    Text text;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    private void Start()
    {
        text = GetComponent<Text>();

        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
        halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;

    }

    private void Update()
    {
        #region stringText 내용 모음임
        string debugText = "debuginginginging";
        debugText += "\n";
        debugText += frameRate + "fps";
        debugText += "\n";
        debugText += "X/Y/Z : " + (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels)+ " / "
                                + Mathf.FloorToInt(world.player.transform.position.y) + " / "
                                + (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels);
        debugText += "\n";
        debugText += "Chunk : " + (world.playerLastChunkCoord.x - halfWorldSizeInChunks) + " / "
                                + (world.playerChunkCoord.z - halfWorldSizeInChunks);

        #endregion

        text.text = debugText;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }


    }


}
