using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRaycastDebug : MonoBehaviour
{
    public GraphicRaycaster raycaster;   // Canvas에 붙은 GraphicRaycaster
    public EventSystem eventSystem; 
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            foreach (var result in results)
            {
                Debug.Log("Raycast hit: " + result.gameObject.name);
            }

            if (results.Count == 0)
            {
                Debug.Log("Raycast hit nothing.");
            }
        }
    }
}
