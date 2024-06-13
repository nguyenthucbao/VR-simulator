using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class PlayerShootBullets : NetworkBehaviour
{
    private PlayerInputControl _playerInputControl;

    private const float BULLET_DELAY = .2f;
    private const float SHOOTING_DELAY = .2f;
    private const float BULLET_SPEED = 10f;
    private const float BULLET_ANGLE_AMPLIFY = .25f;
    private const float BULLETSHOOTAMPLIFYMAX = 25;

    private Transform bulletSpawnTransform;
    [SerializeField] private GameObject bulletPrefab;
    private float bulletShootAngle;

    private Coroutine ShootAutoCoroutine;

    public override void OnNetworkSpawn()
    {
        bulletSpawnTransform = GetComponentInChildren<ShootBulletTransformReference>().transform;

        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControl = GetComponent<PlayerInputControl>();
            _playerInputControl.OnShootInput += StartShooting;
            _playerInputControl.OnShootInputCancelled += StopShooting;
            _playerInputControl.OnShootAngelPerformed += OnShootAngle;
        }
    }
    
    private void StopShooting()
    {
        if (ShootAutoCoroutine != null)
        {
            StopCoroutine(ShootAutoCoroutine);
            ShootAutoCoroutine = null;
        }
    }

    private void StartShooting()
    {
        if (ShootAutoCoroutine == null)
        {
            
            ShootAutoCoroutine = StartCoroutine(ShootCoroutine());
        }
    }


    private void OnShootAngle(Vector2 angleValue)
    {
        float newAngle;

        if (angleValue == Vector2.zero)
        {
            newAngle = 0f;
        }
        else
        {
            newAngle = bulletShootAngle + angleValue.y * -BULLET_ANGLE_AMPLIFY;
            newAngle = Mathf.Clamp(newAngle, -BULLETSHOOTAMPLIFYMAX, BULLETSHOOTAMPLIFYMAX);
        }

        bulletShootAngle = newAngle;
    }


    IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(SHOOTING_DELAY);
        
        while (true)
        {
            StartShootBulletServerRpc(bulletShootAngle, NetworkManager.Singleton.LocalClientId);
            yield return new WaitForSeconds(BULLET_DELAY);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void StartShootBulletServerRpc(float bulletShootAngle, ulong CallerID)
    {
        Quaternion rotation = Quaternion.Euler(bulletShootAngle, 0f, 1f);

        bulletSpawnTransform.localRotation = rotation;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.LookRotation(bulletSpawnTransform.up));

        NetworkObject bulletNetworkObject = bullet.GetComponent<NetworkObject>();

        bulletNetworkObject.Spawn();
        bullet.GetComponent<BulletData>().SetOwnershipServerRPC(CallerID);

        Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();

        bulletRigidbody.AddForce(bulletSpawnTransform.forward * BULLET_SPEED, ForceMode.VelocityChange);
    }

    public override void OnNetworkDespawn()
    {

        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControl.OnShootInput -= StartShooting;
            _playerInputControl.OnShootInputCancelled -= StopShooting;
            _playerInputControl.OnShootAngelPerformed -= OnShootAngle;
        }
    }


}
