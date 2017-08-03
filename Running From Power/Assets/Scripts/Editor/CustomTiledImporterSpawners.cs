namespace Assets.Scripts
{
    using Infrastructure;
    using System;
    using System.Collections.Generic;
    using Tiled2Unity;
    using UnityEngine;
    using Utilities;

    /// <summary>
    ///     Custom Tiled map import code.
    /// </summary>
    [CustomTiledImporter]
    public class CustomTiledImporterSpawners : ICustomTiledImporter
    {
        private const float pixelsPerUnit = 100;

        private const string spawnPrefabPath = "Assets/Prefabs/Systems/Spawn.prefab";

        private const string prefabDirectory = "Assets/Prefabs";

        private Spawn spawnPrefab;

        private Spawn SpawnPrefab
        {
            get
            {
                if (spawnPrefab == null)
                {
                    GameObject prefabGO = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(spawnPrefabPath, typeof(GameObject));
                    if (prefabGO == null)
                    {
                        Debug.Log("Could not load the spawn prefab from '" + spawnPrefabPath + "'");
                        return null;
                    }
                    spawnPrefab = prefabGO.GetComponent<Spawn>();
                    if (spawnPrefab == null)
                    {
                        Debug.Log("Spawn prefab does not have Spawn component.");
                        return null;
                    }
                }
                return spawnPrefab;
            }
        }

        public void CustomizePrefab(GameObject prefab)
        {
            Rigidbody2D physicsBody = prefab.AddComponent<Rigidbody2D>();
            physicsBody.constraints = RigidbodyConstraints2D.FreezeRotation;
            physicsBody.bodyType = RigidbodyType2D.Kinematic;
        }

        private bool GetEnumProperty<TEnum>(string propertyName, IDictionary<string, string> props, out TEnum result, string objectType, int objectID)
        {
            if (!props.ContainsKey(propertyName))
            {
                Debug.Log(objectType + " #" + objectID + ": '" + propertyName + "' property missing!");
                result = (TEnum)Activator.CreateInstance(typeof(TEnum));
                return false;
            }

            string value = props[propertyName];
            if (!EnumUtilities.TryParse(value, out result))
            {
                Debug.Log(objectType + " #" + objectID + ": '" + propertyName + "' value of '" + value + "' is not supported!");
                return false;
            }

            return true;
        }

        private bool GetFloatProperty(string propertyName, IDictionary<string, string> props, out float result, string objectType, int objectID)
        {
            if (!props.ContainsKey(propertyName))
            {
                Debug.Log(objectType + " #" + objectID + ": '" + propertyName + "' property missing!");
                result = 0;
                return false;
            }

            string value = props[propertyName];
            if (!float.TryParse(value, out result))
            {
                Debug.Log(objectType + " #" + objectID + ": '" + propertyName + "' value of '" + value + "' could not be parsed as a float!");
                return false;
            }

            return true;
        }

        private bool GetIntProperty(string propertyName, IDictionary<string, string> props, out int result, string objectType, int objectID)
        {
            if (!props.ContainsKey(propertyName))
            {
                Debug.Log(objectType + " #" + objectID + ": '" + propertyName + "' property missing!");
                result = 0;
                return false;
            }

            string value = props[propertyName];
            if (!int.TryParse(value, out result))
            {
                Debug.Log(objectType + " #" + objectID + ": '" + propertyName + "' value of '" + value + "' could not be parsed as an int!");
                return false;
            }

            return true;
        }

        private bool GetStringProperty(string propertyName, IDictionary<string, string> props, out string result, string objectType, int objectID)
        {
            if (!props.ContainsKey(propertyName))
            {
                Debug.Log(objectType + " #" + objectID + ": '" + propertyName + "' property missing!");
                result = "";
                return false;
            }

            result = props[propertyName];
            return true;
        }

        public void HandleCustomProperties(GameObject gameObject, IDictionary<string, string> props)
        {
            if (props.ContainsKey("prefab"))
            {
                SetupGameObject(gameObject, props);
            }
        }

        private void SetupGameObject(GameObject gameObject, IDictionary<string, string> props)
        {
            TileObject tileObject = gameObject.GetComponent<TileObject>();
            if (tileObject == null)
            {
                Debug.Log("'TileObject' component does not exist on the game object!");
                return;
            }

            string prefabName;
            if (!GetStringProperty("prefab", props, out prefabName, "Tile Object", tileObject.TmxId)) return;

            bool floating = false;
            if (props.ContainsKey("floating"))
            {
                bool.TryParse(props["floating"], out floating);
            }

            string prefabPath = prefabDirectory + "/" + prefabName + ".prefab";
            GameObject prefab = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
            if (prefab == null)
            {
                Debug.Log("Tile Object #" + tileObject.TmxId + ": Could not load '" + prefabPath + "'");
                return;
            }

            Vector2 convertedSize = tileObject.TmxSize/pixelsPerUnit;
            Vector2 halfSize = convertedSize/2;
            Spawn spawn = GameObject.Instantiate(SpawnPrefab, gameObject.transform.position + new Vector3(halfSize.x, halfSize.y, 0), Quaternion.identity, gameObject.transform);
            if (spawn == null)
            {
                Debug.Log("Tile Object #" + tileObject.TmxId + ": Could not instantiate game object from prefab '" + prefabPath + "'!");
                return;
            }

            Vector3 newPosition = pixelsPerUnit*gameObject.transform.position;
            newPosition.x = Mathf.Round(newPosition.x);
            newPosition.y = Mathf.Round(newPosition.y);
            newPosition = newPosition/pixelsPerUnit;
            gameObject.transform.position = newPosition;
            
            Transform parent = null;
            MeshRenderer renderer = gameObject.GetComponentInChildren<MeshRenderer>();
            if (!floating)
            {
                parent = gameObject.transform;
            }
            spawn.Construct(parent, prefab, renderer.sortingLayerID, renderer.sortingOrder);

            Transform oldTileObject = gameObject.transform.Find("TileObject");
            if (oldTileObject != null)
            {
                GameObject.DestroyImmediate(oldTileObject.gameObject);
            }
        }
    }
}
