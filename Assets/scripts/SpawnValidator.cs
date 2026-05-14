using UnityEngine;

namespace Roguelike
{
    public class SpawnValidator : MonoBehaviour
    {
        private void Awake()
        {
            Vector2 spawnPos = transform.position;
            float checkRadius = 0.4f;
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(spawnPos, checkRadius);

            foreach (var hit in hitColliders)
            {
                if (hit.gameObject == gameObject) continue;
                if (!hit.isTrigger)
                {
                    Debug.LogError($"[SpawnValidator] BLOCKING COLLIDER AT SPAWN: {hit.gameObject.name}");
                }
            }
        }
    }
}
