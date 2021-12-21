using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    public static float Get2DPerlin(Vector2 _position, float _offset,float _scale)
    {
        return Mathf.PerlinNoise((_position.x + 0.1f) / VoxelData.ChunkWidth * _scale + _offset,
                                 (_position.y + 0.1f) / VoxelData.ChunkWidth * _scale + _offset);
    }

    public static bool Get3DPerlin (Vector3 position, float _offset, float _scale, float _threshold)
    {
        float x = (position.x + _offset + 0.1f) * _scale;
        float y = (position.y + _offset + 0.1f) * _scale;
        float z = (position.z + _offset + 0.1f) * _scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        if((AB+BC+AC+BA+CB+CA)/6f > _threshold)
        {
            return true;
        }
        else
        {
            return false;
        }

    }


}
