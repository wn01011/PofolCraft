using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderInteractor : MonoBehaviour
{
    private void Update()
    {
        Shader.SetGlobalVector("_PositionMoving", transform.position);
    }
}
