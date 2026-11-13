using UnityEngine;

/// <summary>
/// Part 3 - Spawns 3 floating targets above the base.
/// Put this on the ROOT of the GameBase prefab (root scale must stay 1,1,1 so distances are in metres).
/// Targets are parented to the base, so they inherit its AR anchor.
/// </summary>
public class GameBase : MonoBehaviour
{
    [SerializeField] Target targetPrefab;
    [SerializeField] int targetCount = 3;
    [Tooltip("Horizontal distance of targets from the centre of the base (metres).")]
    [SerializeField] float radius = 0.18f;
    [Tooltip("Height of targets above the base (metres).")]
    [SerializeField] float height = 0.3f;
    [Tooltip("Extra height added to every other target so they're staggered.")]
    [SerializeField] float heightStagger = 0.08f;

    void Start()
    {
        GameManager.Instance.SetGameBase(this);
        SpawnWave();
    }

    public void SpawnWave()
    {
        for (int i = 0; i < targetCount; i++)
        {
            float angle = i * 360f / targetCount;
            Vector3 localPos = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius
                             + Vector3.up * (height + (i % 2) * heightStagger);

            Target target = Instantiate(targetPrefab, transform);
            target.transform.localPosition = localPos;
            target.transform.localRotation = Quaternion.identity;

            GameManager.Instance.RegisterTarget();
        }
    }
}
