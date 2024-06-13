using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaceTank : NetworkBehaviour
{
    
    [SerializeField] private GameObject placementObject;
    private Camera mainCam;

    private void Start()
    {
        mainCam = GameObject.FindObjectOfType<Camera>();
    }

    void Update()
    {
        if (AllPlayerDataManager.Instance != default && AllPlayerDataManager.Instance.GetHasPlacePlayer(NetworkManager.Singleton.LocalClientId)) return;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("UI Hit was recognized");
                return;
            }
            TouchToRay(Input.mousePosition);
        }
#endif
#if UNITY_IOS || UNITY_ANDROID

        if (Input.touchCount > 0 && Input.touchCount < 2 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);

            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = touch.position;

            List<RaycastResult> results = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                // We hit a UI element
                Debug.Log("We hit an UI Element");
                return;
            }

            Debug.Log("Touch detected, fingerId: " + touch.fingerId);  // Debugging line


            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                Debug.Log("Is Pointer Over GOJ, No placement ");
                return;
            }
            TouchToRay(touch.position);
        }
#endif
    }

    void TouchToRay(Vector3 touch)
    {
        Ray ray = mainCam.ScreenPointToRay(touch);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            SpawnTankServerRPC(hit.point, rotation, NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnTankServerRPC(Vector3 position, Quaternion rotation, ulong callerID)
    {
        GameObject Tank = Instantiate(placementObject, position, rotation);
        NetworkObject tankNetworkObject = Tank.GetComponent<NetworkObject>();
        tankNetworkObject.SpawnWithOwnership(callerID);
        AllPlayerDataManager.Instance.AddPlacedPlayer(callerID);
    }
}
