namespace Assets.Scripts.Infrastructure
{
    using System.Collections.Generic;
    using Tiled2Unity;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.UI;

    /// <summary>
    ///     Manages a scrolling background in the horizontal direction.
    /// </summary>
    public class ScrollingBackground : MonoBehaviour
    {
        [SerializeField]
        private float speed = 10;

        [SerializeField]
        private List<string> parts;

        [SerializeField]
        private int nextPart = 0;

        [SerializeField]
        private string partsDirectory = "";

        private TiledMap back;

        private TiledMap front;

        private Camera mainCamera;

        private float CameraHalfWidth
        {
            get { if (mainCamera == null) return 0; return mainCamera.orthographicSize*mainCamera.aspect;  }
        }

		private TiledMap LoadNextMapPart(float baseX)
        {
            if (parts.Count == 0) return null;

            if (nextPart >= parts.Count)
            {
                nextPart = 0;
            }
            else if (nextPart < 0)
            {
                nextPart = 0;
            }

            string partPath = partsDirectory + "/" + parts[nextPart];
            GameObject partPrefabGO = Resources.Load(partPath) as GameObject;
            Assert.IsTrue(partPrefabGO != null, "The part prefab does not exist! Path:" + partPath);
            TiledMap partPrefab = partPrefabGO.GetComponent<TiledMap>();
            Assert.IsTrue(partPrefab != null, "The part prefab does not have a TiledMap component.");

            nextPart++;
            if (nextPart >= parts.Count)
            {
                nextPart = 0;
            }

            TiledMap part = Instantiate(partPrefab, new Vector3(baseX, partPrefab.GetMapHeightInPixelsScaled()/2), Quaternion.identity, transform);

            Rigidbody2D physicsBody = part.GetComponent<Rigidbody2D>();
			Assert.IsTrue(physicsBody != null, "The map '" + partPath + "' needs to have a Rigidbody2D.");
            physicsBody.velocity = speed*Vector2.left;

			return part;
        }

        private void FixedUpdate()
        {
            if (front.transform.position.x + front.GetMapWidthInPixelsScaled() <= mainCamera.transform.position.x - CameraHalfWidth)
            {
                front.transform.position = back.transform.position + new Vector3(back.GetMapWidthInPixelsScaled(), 0);
                Destroy(front.gameObject);
                front = back;
                back = LoadNextMapPart(front.transform.position.x + front.GetMapWidthInPixelsScaled());
            }
        }

        /// <summary>
        ///     Gets the current scrolling speed.
        /// </summary>
        /// <returns>The current scroling speed.</returns>
        public float GetSpeed()
        {
            return speed;
        }

        private void Start()
        {
            GameObject mainCameraGO = GameObject.FindGameObjectWithTag("MainCamera");
            Assert.IsTrue(mainCameraGO != null, "Could not find the main camera.");
            mainCamera = mainCameraGO.GetComponent<Camera>();
            Assert.IsTrue(mainCamera != null, "Main camera does not have a Camera component.");

            front = LoadNextMapPart(-CameraHalfWidth);
            back = LoadNextMapPart(front.transform.position.x + front.GetMapWidthInPixelsScaled());
        }

        private void UpdateVelocities()
        {
			Rigidbody2D physicsBodyFront = front.GetComponent<Rigidbody2D>();
            Assert.IsTrue(physicsBodyFront != null, "Front part does not have a RigidBody2D.");
			Rigidbody2D physicsBodyBack = back.GetComponent<Rigidbody2D>();
            Assert.IsTrue(physicsBodyBack != null, "Back part does not have a RigidBody2D.");
			physicsBodyFront.velocity = speed*Vector2.left;
			physicsBodyBack.velocity = speed*Vector2.left;
        }
    }
}