using UnityEngine;
using System.Collections.Generic;

namespace Roguelike
{
    [System.Serializable]
    public class WeightedEnemy
    {
        public GameObject enemyPrefab;
        [Range(1, 100)]
        public int weight = 1;
    }

    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<WeightedEnemy> enemyPool;
        private int totalWeight;

        private void Awake() => CalculateTotalWeight();

        private void CalculateTotalWeight()
        {
            totalWeight = 0;
            if (enemyPool == null) return;
            foreach (var enemy in enemyPool) totalWeight += enemy.weight;
        }

        public void SpawnAt(Vector3 worldPosition, Transform parent)
        {
            GameObject prefab = GetRandomEnemyPrefab();
            if (prefab) Instantiate(prefab, worldPosition, Quaternion.identity, parent);
        }

        private GameObject GetRandomEnemyPrefab()
        {
            if (totalWeight <= 0) CalculateTotalWeight();
            if (totalWeight <= 0) return null;
            int roll = Random.Range(0, totalWeight);
            int current = 0;
            foreach (var enemy in enemyPool) {
                current += enemy.weight;
                if (roll < current) return enemy.enemyPrefab;
            }
            return null;
        }
    }
}
