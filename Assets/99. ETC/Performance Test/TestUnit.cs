using Spine.Unity;
using UnityEngine;

public class TestUnit : MonoBehaviour
{
    public GameObject vfx;
    public SkeletonAnimation spine;
    
    void Start()
    {
        // spine.initialSkinName = "Normal";
        // spine.Initialize(true);
        // spine.AnimationState.SetAnimation(0, "Idle", true);
        
    }

    public void ActiveUnit(bool isActive)
    {
        spine.gameObject.SetActive(isActive);
    }
    public void ActiveVFX(bool isActive)
    {
        vfx.SetActive(isActive);
    }
}
