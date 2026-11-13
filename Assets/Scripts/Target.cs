using UnityEngine;

/// <summary>
/// A floating target. Bobs and spins, and blows up (VFX + score) when a Projectile hits it.
/// Put this on the Target prefab (a Sphere with a SphereCollider and the energy Shader Graph material).
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Target : MonoBehaviour
{
    [SerializeField] GameObject explosionPrefab;

    [Header("Idle motion")]
    [SerializeField] float bobHeight = 0.03f;
    [SerializeField] float bobSpeed = 2f;
    [SerializeField] float spinSpeed = 60f;

    Vector3 _startLocalPos;
    float _phase;
    bool _destroyed;

    void Awake()
    {
        // Kinematic: we move it ourselves, but physics still reports collisions with the projectile.
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Start()
    {
        _startLocalPos = transform.localPosition;
        _phase = Random.value * 10f; // so the targets don't bob in sync
    }

    void Update()
    {
        transform.localPosition = _startLocalPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed + _phase) * bobHeight;
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.Self);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (_destroyed) return;
        if (collision.collider.GetComponentInParent<Projectile>() == null) return;

        _destroyed = true;

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        GameManager.Instance.TargetDestroyed();
        Destroy(gameObject);
    }
}
