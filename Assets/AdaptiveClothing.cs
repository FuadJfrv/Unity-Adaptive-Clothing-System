using UnityEngine;

public class AdaptiveClothing : MonoBehaviour
{
    [Range(0.001f, 0.1f)]
    [SerializeField] private float shrinkAmount = 0.01f;
    [Range(0.001f, 0.5f)]
    [SerializeField] private float rayDistance = 0.1f;

    [SerializeField] private SkinnedMeshRenderer underGarmentRenderer;
    [SerializeField] private SkinnedMeshRenderer overGarmentRenderer;

    [ContextMenu("Adapt Clothing")]
    public void Adapt()
    {
        if (underGarmentRenderer == null || overGarmentRenderer == null)
        {
            Debug.LogError("Both renderers must be assigned.");
            return;
        }

        var adaptedMesh = GenerateAdaptedMesh();

        CreateObjectWithAdaptedMesh(adaptedMesh);

        underGarmentRenderer.gameObject.SetActive(false);
    }

    private Mesh GenerateAdaptedMesh()
    {
        // Temporary collider at origin using the outer garment's bind-pose mesh
        GameObject tempOverGarmentObj = new GameObject("Temp_OverGarment_Collider")
        {
            transform =
            {
                position = Vector3.zero,
                rotation = Quaternion.identity,
                localScale = Vector3.one
            }
        };

        MeshCollider overGarmentCollider = tempOverGarmentObj.AddComponent<MeshCollider>();
        overGarmentCollider.sharedMesh = overGarmentRenderer.sharedMesh;

        Physics.SyncTransforms();

        // Clone bind-pose mesh
        Mesh adaptedMesh = Instantiate(underGarmentRenderer.sharedMesh);
        Vector3[] origVerts = adaptedMesh.vertices;
        Vector3[] normals = adaptedMesh.normals;
        int[] triangles = adaptedMesh.triangles;

        //Pre-shrink the mesh
        Vector3[] workingVerts = new Vector3[origVerts.Length];
        for (int i = 0; i < origVerts.Length; i++)
        {
            workingVerts[i] = origVerts[i] - (normals[i] * shrinkAmount);
        }

        bool prevBackface = Physics.queriesHitBackfaces;
        Physics.queriesHitBackfaces = true;
        
        try
        {
            // Raycast outward per triangle from the shrunken mesh
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int i0 = triangles[i];
                int i1 = triangles[i + 1];
                int i2 = triangles[i + 2];

                Vector3 center = (workingVerts[i0] + workingVerts[i1] + workingVerts[i2]) / 3f;
                Vector3 normal = (normals[i0] + normals[i1] + normals[i2]).normalized;

                Ray ray = new Ray(center, normal);

                if (overGarmentCollider.Raycast(ray, out var hit, rayDistance))
                { }
                else
                {
                    //Not covered - un-shrink it. 
                    workingVerts[i0] = origVerts[i0];
                    workingVerts[i1] = origVerts[i1];
                    workingVerts[i2] = origVerts[i2];
                }
            }
        }
        finally
        {
            Physics.queriesHitBackfaces = prevBackface;
            DestroyImmediate(tempOverGarmentObj);
        }

        adaptedMesh.vertices = workingVerts;
        adaptedMesh.RecalculateBounds();
        adaptedMesh.RecalculateTangents();
        return adaptedMesh;
    }

    private void CreateObjectWithAdaptedMesh(Mesh adaptedMesh)
    {
        GameObject newObject = new GameObject($"{underGarmentRenderer.gameObject.name}_Adapted");
        newObject.transform.SetPositionAndRotation(underGarmentRenderer.transform.position, underGarmentRenderer.transform.rotation);
        newObject.transform.localScale = underGarmentRenderer.transform.localScale;

        SkinnedMeshRenderer newRenderer = newObject.AddComponent<SkinnedMeshRenderer>();
        newRenderer.sharedMesh = adaptedMesh;
        newRenderer.sharedMaterials = underGarmentRenderer.sharedMaterials;
        newRenderer.bones = underGarmentRenderer.bones;
        newRenderer.rootBone = underGarmentRenderer.rootBone;
    }
}