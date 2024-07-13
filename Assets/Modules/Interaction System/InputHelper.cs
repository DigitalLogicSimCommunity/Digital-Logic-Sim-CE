using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;

public static class InputHelper
{
    static Camera _mainCamera;

    // Constructor
    static Camera MainCamera
    {
        get
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            return _mainCamera;
        }
    }

    public static Vector2 MouseWorldPos => MainCamera.ScreenToWorldPoint(Input.mousePosition);

    public static bool MouseOverUIObject()
    {
        var e = IsPointerOverUIElement(GetEventSystemRaycastResults());
        //Debug.Log($"MouseOverUIObject: {e}");
        return e;
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        return eventSystemRaysastResults.Any(curRaycastResult => curRaycastResult.gameObject.layer == ProjectLayer.UI);
    }

    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    public static GameObject GetObjectUnderMouse2D(LayerMask mask)
    {
        Vector2 mouse = MouseWorldPos;
        var hit = Physics2D.GetRayIntersection(
            new Ray(new Vector3(mouse.x, mouse.y, -100), Vector3.forward),
            float.MaxValue, mask);

        return hit.collider ? hit.collider.gameObject : null;
    }

    public static bool CompereTagObjectUnderMouse2D(string tag, LayerMask mask)
    {
        var e = GetObjectUnderMouse2D(mask);
        return e != null && e.CompareTag(tag);
    }

    public static List<GameObject> GetUIObjectsUnderMouse()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        return results.Select(result => result.gameObject).ToList();
    }


    public static bool AnyOfTheseKeysDown(params KeyCode[] keys)
    {
        return keys.Any(Input.GetKeyDown);
    }

    public static bool AnyOfTheseKeysHeld(params KeyCode[] keys)
    {
        return keys.Any(Input.GetKey);
    }
}