using UnityEngine;

[ExecuteInEditMode]
public class ScaleCapsuleWithBlendShape : MonoBehaviour
{
    public SkinnedMeshRenderer meshRenderer;
    public CapsuleCollider capsuleCollider;

    public Vector3 startCenter = new Vector3(0, 0.015f, 0);
    public float startHeight = 0.03f;

    public Vector3 endCenter = new Vector3 (0, 0.15f, 0);
    public float endHeight = 0.3f;

    private void OnValidate()
    {
        SetCapsuleSize();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetCapsuleSize();
    }

    public void SetCapsuleSize()
    {
        capsuleCollider.center = new Vector3(Mathf.Lerp(startCenter.x, endCenter.x, meshRenderer.GetBlendShapeWeight(0) / 100f), Mathf.Lerp(startCenter.y, endCenter.y, meshRenderer.GetBlendShapeWeight(0) / 100f), Mathf.Lerp(startCenter.z, endCenter.z, meshRenderer.GetBlendShapeWeight(0) / 100f));
        capsuleCollider.height = Mathf.Lerp(startHeight, endHeight, meshRenderer.GetBlendShapeWeight(0) / 100f);
    }
}
