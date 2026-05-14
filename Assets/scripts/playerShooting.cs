using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 12f;
    [SerializeField] private float bulletLifeTime = 2f;
    [SerializeField] private float fireCooldown = 0.12f;
    [SerializeField] private float spawnOffset = 0.35f;

    private float nextFireTime;
    private bool warnedAboutMissingPrefab;
    private Camera mainCamera;
    private Collider2D[] ownerColliders;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (firePoint == null)
        {
            GameObject point = new GameObject("FirePoint");
            point.transform.SetParent(transform);
            point.transform.localPosition = Vector3.right * 0.6f;
            point.transform.localRotation = Quaternion.identity;
            firePoint = point.transform;
        }

        ownerColliders = GetComponentsInChildren<Collider2D>(true);
    }

    private void Update()
    {
        AimAtMouse();

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireCooldown;
            Shoot();
        }
    }

    private void AimAtMouse()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = transform.position.z;

        Vector2 direction = mouseWorld - transform.position;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Shoot()
    {
        if (bulletPrefab == null)
        {
            if (!warnedAboutMissingPrefab)
            {
                Debug.LogWarning("PlayerShooting: bulletPrefab is not assigned.");
                warnedAboutMissingPrefab = true;
            }
            return;
        }

        Vector2 shootDirection = firePoint.right;
        Vector3 spawnPosition = firePoint.position + (Vector3)(shootDirection * spawnOffset);
        GameObject bulletObject = Instantiate(bulletPrefab, spawnPosition, firePoint.rotation);
        IgnoreOwnerCollision(bulletObject);

        BulletMovement bulletMovement = bulletObject.GetComponent<BulletMovement>();
        if (bulletMovement == null)
        {
            bulletMovement = bulletObject.AddComponent<BulletMovement>();
        }
        bulletMovement.Launch(shootDirection, bulletSpeed, bulletLifeTime);
    }

    private void IgnoreOwnerCollision(GameObject bullet)
    {
        if (ownerColliders == null || ownerColliders.Length == 0)
        {
            return;
        }

        Collider2D[] bulletColliders = bullet.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < bulletColliders.Length; i++)
        {
            Collider2D bulletCollider = bulletColliders[i];
            if (bulletCollider == null)
            {
                continue;
            }

            for (int j = 0; j < ownerColliders.Length; j++)
            {
                Collider2D ownerCollider = ownerColliders[j];
                if (ownerCollider == null)
                {
                    continue;
                }
                Physics2D.IgnoreCollision(bulletCollider, ownerCollider, true);
            }
        }
    }
}
