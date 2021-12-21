using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VD
{ /*
        front       back
          7-------6      0(0,0,0) , 1(1,0,0) , 2(1,1,0) , 3(0,1,0)
         /|      /|      4(0,0,1) , 5(1,0,1) , 6(1,1,1) , 7(0,1,1)   
        3-------2 |      �ݽð���� ȸ��
        | |     | |      �����ϴ� ����
        | 4-----|-5  
        |/      |/
        0-------1       

    */

    public static readonly int ChunkHeight = 20;           //ûũ�� ����
    public static readonly int ChunkWidth = 4;             //ûũ�� ����/���� ����
    public static readonly int WorldSizeInChunks = 12;     //ûũ�� ���� �� ũ��

    public static int WorldSizeInVoxels              
    {
        get { return WorldSizeInChunks * ChunkWidth; }
    }


    public static readonly int TextureAtlasSizeInBlocks = 4; //�ؽ�ó�� ��� ũ�� 
    public static float NormalizeBlockTextureSize        // ��Ʋ�󽺿��� �ؽ�ó �ϳ��� ũ��     
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks;}
    }

    //���� ����Ʈ 
    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        //front                         //idx
        new Vector3(0.0f, 0.0f, 0.0f),  //0
        new Vector3(1.0f, 0.0f, 0.0f),  //1
        new Vector3(1.0f, 1.0f, 0.0f),  //2
        new Vector3(0.0f, 1.0f, 0.0f),  //3

        //back
        new Vector3(0.0f, 0.0f, 1.0f),  //4
        new Vector3(1.0f, 0.0f, 1.0f),  //5
        new Vector3(1.0f, 1.0f, 1.0f),  //6
        new Vector3(0.0f, 1.0f, 1.0f),  //7

    };

    //�� ���� ��ǥ ����Ʈ
    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0.0f,0.0f,-1.0f),   //back
        new Vector3(0.0f,0.0f,1.0f),    //front
        new Vector3(0.0f,1.0f,0.0f),    //top
        new Vector3(0.0f,-1.0f,0.0f),   //bottom
        new Vector3(-1.0f,0.0f,0.0f),   //left
        new Vector3(1.0f,0.0f,0.0f)    //right

    };

    //�� ���� �׸� �� �ʿ��� ������ �ε��� ����Ʈ  N �������� ������ 
    public static readonly int[,] voxelTris = new int[6, 4]
    {

        {0,3,1,2}, // BackFace
        {5,6,4,7}, // FrotFace
        {3,7,2,6}, // TopFace
        {1,5,0,4}, // BottomFace
        {4,7,0,3}, // LeftFace
        {1,2,5,6}  // RightFace
    };

    //uv�� �׸��� ���� ��� ��ǥ�� : ���� �׸� ���� �� ��
    public static readonly Vector2[] voxelUvs = new Vector2[4]
    {
        new Vector2(0.0f,0.0f),
        new Vector2(0.0f,1.0f),
        new Vector2(1.0f,0.0f),
        new Vector2(1.0f,1.0f),
    };
 




}
