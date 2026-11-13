using UnityEngine;

/// <summary>
/// Physics projectile. Destroys itself on any collision, or once it has travelled
/// more than maxDistance from where it was fired (the "miss" rule / memory management).
/// Put this on the Projectile prefab (small Sphere + SphereCollider + Rigidbody).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField] float maxDistance = 5f;
    [Tooltip("Safety net in case it somehow stops moving.")]
    [SerializeField] float maxLifetime = 4f;

    Vector3 _origin;
    float _age;

    void Start()
    {
        _origin = transform.position;
    }

    void Update()
    {
        _age += Time.deltaTime;

        if ((transform.position - _origin).sqrMagnitude > maxDistance * maxDistance || _age > maxLifetime)
            Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
