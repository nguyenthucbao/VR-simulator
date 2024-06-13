using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerInputControl))]
public class PlayerMovement : NetworkBehaviour
{
    private PlayerInputControl _playerInputControl;
    private const float MOVE_SPEED = .03f;
    private const float MOVE_THRESHOLD = 0.01f;
    private const float LOOKATPOINT_Delta = 2f;
    private GameObject lookatPoint;

    public override void OnNetworkSpawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            lookatPoint = new GameObject();
            lookatPoint.transform.position = transform.position;
            lookatPoint.transform.rotation = transform.rotation;

            _playerInputControl = GetComponent<PlayerInputControl>();
            _playerInputControl.OnMoveInput += PlayerInputControlOnOnMoveInput;
        }    
    }

    private void PlayerInputControlOnOnMoveInput(Vector3 inputMovement)
    {
        if (inputMovement.magnitude < MOVE_THRESHOLD) return;
        transform.position += inputMovement * MOVE_SPEED;

        PlayerLookInMovementDirection(inputMovement); 
    }    

    void PlayerLookInMovementDirection(Vector3 inputVector)
    {
        Vector3 point = transform.position + inputVector.normalized * LOOKATPOINT_Delta;
        lookatPoint.transform.position = point;
        transform.LookAt(lookatPoint.transform);
    }

    public override void OnNetworkDespawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControl.OnMoveInput -= PlayerInputControlOnOnMoveInput;
        }
    }
}
