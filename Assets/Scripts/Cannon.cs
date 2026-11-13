using UnityEngine;

/// <summary>
/// Turns the AR camera into a cannon. Hook the FIRE button's OnClick() to Cannon.Fire().
/// Put this on the GameManager object (or anywhere in the scene).
/// </summary>
public class Cannon : MonoBehaviour
{
    [SerializeField] Projectile projectilePrefab;
    [SerializeField] Camera arCamera;

    [SerializeField] float speed = 4f;            // metres per second
    [SerializeField] float spawnOffset = 0.15f;   // spawn a bit in front of the lens (past the near clip plane)
    [SerializeField] float fireCooldown = 0.2f;

    float _lastFireTime = -999f;

    void Awake()
    {
        if (arCamera == null) arCamera = Camera.main;
    }

    public void Fire()
    {
        if (Time.time - _lastFireTime < fireCooldown) return;
        _lastFireTime = Time.time;

        Transform cam = arCamera.transform;
        Projectile projectile = Instantiate(projectilePrefab, cam.position + cam.forward * spawnOffset, cam.rotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.AddForce(cam.forward * speed, ForceMode.VelocityChange);
    }
}
