using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputControl : NetworkBehaviour
{

    private PlayerControlInputAction _playerControlInputAction;
    private Vector3 movementVector;

    public event Action<Vector3> OnMoveInput;
    public event Action OnMoveActionCancelled;
    public event Action OnShootInput;
    public event Action OnShootInputCancelled;
    public event Action<Vector2> OnShootAngelPerformed;

    public override void OnNetworkSpawn()
    {
        if(GetComponent<NetworkObject>().IsOwner)
        {
            _playerControlInputAction = new PlayerControlInputAction();
            _playerControlInputAction.Enable();

            _playerControlInputAction.PlayerControlMap.Move.performed += MoveActionPerformed;
            _playerControlInputAction.PlayerControlMap.Move.canceled += MoveActionCancelled;

            _playerControlInputAction.PlayerControlMap.Shoot.performed += ShootOnPerform;
            _playerControlInputAction.PlayerControlMap.Shoot.canceled += ShootOnCancelled;

            _playerControlInputAction.PlayerControlMap.ShootAngel.performed += ShootAngelPerform;
        }    
    }
    private void ShootAngelPerform(InputAction.CallbackContext context)
    {
        OnShootAngelPerformed?.Invoke(context.ReadValue<Vector2>());
    }
    private void ShootOnPerform(InputAction.CallbackContext context)
    {
        Debug.Log("Shoot button pressed");
        OnShootInput?.Invoke();
        //Debug.Log(OnShootInput == null);
    }
    private void ShootOnCancelled(InputAction.CallbackContext context)
    {
        Debug.Log("Shoot button released");
        OnShootInputCancelled?.Invoke();
    }

    private void MoveActionCancelled(InputAction.CallbackContext context)
    {
        Debug.Log("Move button released");
        movementVector = Vector3.zero;
        OnMoveActionCancelled?.Invoke();
    }    
    private void MoveActionPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Move button pressed");
        Vector2 v2Movement = context.ReadValue<Vector2>();
        movementVector = new Vector3(v2Movement.x, 0, v2Movement.y);
    }    

    // Update is called once per frame
    void Update()
    {
        if(movementVector != Vector3.zero)
        {
            OnMoveInput?.Invoke(movementVector);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerControlInputAction.PlayerControlMap.Move.performed -= MoveActionPerformed;
            _playerControlInputAction.PlayerControlMap.Move.canceled -= MoveActionCancelled;

            _playerControlInputAction.PlayerControlMap.Shoot.performed -= ShootOnPerform;
            _playerControlInputAction.PlayerControlMap.Shoot.canceled -= ShootOnCancelled;
        }    
    }
}
