// PlaterShooting.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaterShooting : MonoBehaviour
{
    public GameObject weapon1ProjectilePrefab;
    public GameObject weapon2ProjectilePrefab;

    public Transform firePoint;
    Camera cam;

    public enum WeaponType { Weapon1, Weapon2 }
    public WeaponType currentWeapon = WeaponType.Weapon1;
    private float weapon1Speed = 20.0f;
    private int weapon1Damage = 1;

    private float weapon2Speed = 40.0f;
    private int weapon2Damage = 3;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SwitchWeapon();
        }
    }

    void SwitchWeapon()
    {
        if (currentWeapon == WeaponType.Weapon1)
        {
            currentWeapon = WeaponType.Weapon2;
        }
        else
        {
            currentWeapon = WeaponType.Weapon1;
        }
    }

    void Shoot()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPoint = ray.GetPoint(50f);
        Vector3 direction = (targetPoint - firePoint.position).normalized;

        GameObject selectedPrefab = null;
        float currentSpeed;
        int currentDamage;
        if (currentWeapon == WeaponType.Weapon1)
        {
            selectedPrefab = weapon1ProjectilePrefab;
            currentSpeed = weapon1Speed;
            currentDamage = weapon1Damage;
        }
        else 
        {
            selectedPrefab = weapon2ProjectilePrefab;
            currentSpeed = weapon2Speed;
            currentDamage = weapon2Damage;
        }
        if (selectedPrefab == null) return;
        GameObject projObject = Instantiate(selectedPrefab, firePoint.position, Quaternion.LookRotation(direction));
        PlayerProjectile proj = projObject.GetComponent<PlayerProjectile>();
        if (proj != null)
        {
            proj.speed = currentSpeed;
            proj.damage = currentDamage;
        }
    }
}