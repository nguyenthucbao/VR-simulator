using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimation : NetworkBehaviour
{
    private Animator _playerAnimator;
    private PlayerInputControl _playerInputControl;
    private AnimatorControllerParameter[] allParams;

    public override void OnNetworkSpawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerAnimator = GetComponentInChildren<Animator>();
            _playerInputControl = GetComponent<PlayerInputControl>();

            allParams = _playerAnimator.parameters;
            

            _playerInputControl.OnMoveInput += PlayerInputControlsOnOnMoveInput;
            _playerInputControl.OnMoveActionCancelled += PlayerInputControlsOnOnMoveActionCancelled;
            _playerInputControl.OnShootInput += PlayerInputControlsOnOnShootInput;
            _playerInputControl.OnShootInputCancelled += PlayerInputControlsOnOnShootInputCancelled;


        }
    }

    private void PlayerInputControlsOnOnShootInputCancelled()
    {
        SetOneParameterToTrue("isIdle");
    }

    private void PlayerInputControlsOnOnShootInput()
    {
        SetOneParameterToTrue("isShooting");
    }

    private void PlayerInputControlsOnOnMoveActionCancelled()
    {
        SetOneParameterToTrue("isIdle");
    }

    private void PlayerInputControlsOnOnMoveInput(Vector3 context)
    {
        if (context.magnitude > 0)
        {
            SetOneParameterToTrue("isRuning");
        }
    }


    void SetOneParameterToTrue(string param)
    {
        foreach (var parameter in allParams)
        {
            if (parameter.name == param)
            {
                _playerAnimator.SetBool(parameter.nameHash, true);
            }
            else
            {
                _playerAnimator.SetBool(parameter.nameHash, false);

            }

        }
    }

    public override void OnNetworkDespawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControl.OnMoveInput -= PlayerInputControlsOnOnMoveInput;
            _playerInputControl.OnMoveActionCancelled -= PlayerInputControlsOnOnMoveActionCancelled;
            _playerInputControl.OnShootInput -= PlayerInputControlsOnOnShootInput;
            _playerInputControl.OnShootInputCancelled -= PlayerInputControlsOnOnShootInputCancelled;
        }
    }


}
