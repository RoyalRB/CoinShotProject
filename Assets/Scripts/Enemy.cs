using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] Material deathMaterial;

    public void Die()
    {
        GetComponent<MeshRenderer>().material = deathMaterial;
    }
}
