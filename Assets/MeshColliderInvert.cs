using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshColliderInvert : MonoBehaviour
{
    public MeshCollider meshCollider;

    private void Awake()
    {
        //if (!meshCollider) meshCollider = GetComponent<MeshCollider>();

        var mesh = meshCollider.sharedMesh;
        Debug.Log(mesh);

        if (mesh.normals[0].x > 0)
        {
            // Reverse the triangles
            mesh.triangles = mesh.triangles.Reverse().ToArray();
            // also invert the normals
            mesh.normals = mesh.normals.Select(n => -n).ToArray();
            Debug.Log((mesh.normals[0], mesh.normals[1], mesh.normals[2]));
        }
    }
}
