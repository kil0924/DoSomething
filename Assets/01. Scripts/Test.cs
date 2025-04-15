using UnityEngine;

public class Test : MonoBehaviour
{
    [ContextMenu( "Test" )]
    public void TestMethod()
    {
        var v = Vector3.zero;
        
        Debug.Log(v);

        v.x = 10;
        Debug.Log(v);
    }
}
