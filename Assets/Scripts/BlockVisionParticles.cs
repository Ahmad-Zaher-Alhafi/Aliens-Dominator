using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockVisionParticles : MonoBehaviour
{
    [SerializeField] private Constants.ObjectsColors blockVisionParticlesColor;
    public Constants.ObjectsColors BlockVisionParticlesColor
    {
        get
        {
            return blockVisionParticlesColor;
        }
    }
}
