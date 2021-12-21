using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "PopolCraft/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    public string biomeName;
    public int solidGroundHeight;
    public int terrainHeight;
    public float terrainScale;

    public Lode[] lodes;

    [Header("Trees")]
    public float treeZoneScale = 1.0f;

    [Range(0.1f, 1f)]
    public float treeZoneThreshold = 0.6f;
    public float treePlacementScale = 15f;

    [Range(0.1f, 1f)]
    public float treePlacemantThreshold = 0.8f;


    public int maxTreeHeight = 12;
    public int minTreeHeight = 5;

}

[System.Serializable]
public class Lode
{
    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float nouseOffset;
}



