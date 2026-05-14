using UnityEngine;
using System.Collections.Generic;

namespace Roguelike
{
    public class Room : MonoBehaviour
    {
        public Vector2Int size = new Vector2Int(20, 12);
        [SerializeField] private BoxCollider2D walkableArea;

        private void Awake()
        {
            if (walkableArea == null) walkableArea = GetComponent<BoxCollider2D>();
            if (walkableArea != null) walkableArea.isTrigger = true;
        }

        public Rect GetWorldRect()
        {
            return new Rect(transform.position.x, transform.position.y, size.x, size.y);
        }
    }
}
