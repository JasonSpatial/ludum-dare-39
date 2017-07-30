namespace Assets.Scripts.Infrastructure
{
    using UnityEngine;

    /// <summary>
    ///     Spawns an object.
    /// </summary>
    public class Spawn : MonoBehaviour
    {
        [SerializeField]
        private Transform parent;

        [SerializeField]
        private GameObject prefab;

        [SerializeField]
        private int sortingLayerID;

        [SerializeField]
        private int sortingOrder;

        private bool spawned = false;

        private void Awake()
        {
            if (!spawned)
            {
                if (prefab != null)
                {
                    GameObject newGameObject =  Instantiate(prefab, transform.position, Quaternion.identity, parent);
                    newGameObject.name = prefab.name;

                    SpriteRenderer[] renderers = newGameObject.GetComponentsInChildren<SpriteRenderer>();
                    foreach (SpriteRenderer renderer in renderers)
                    {
                        renderer.sortingLayerID = sortingLayerID;
                        renderer.sortingOrder = sortingOrder;
                    }
                }
                spawned = true;
            }
        }

        public void Construct(Transform parent, GameObject prefab, int sortingLayerID, int sortingOrder)
        {
            this.parent = parent;
            this.prefab = prefab;
            this.sortingLayerID = sortingLayerID;
            this.sortingOrder = sortingOrder;
        }
    }
}
