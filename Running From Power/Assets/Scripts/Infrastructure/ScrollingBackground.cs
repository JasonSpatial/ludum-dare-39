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
        private List<string> partsStart;

        [SerializeField]
        private List<string> partsEasy;

        [SerializeField]
        private List<string> partsMed;

        private int partsLoaded = 0;  // count of parts that have been loaded so far, used to progress the difficulty of the game

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
            string partFile;
            if (partsLoaded < partsStart.Count)
                partFile = partsStart[partsLoaded];
            else if (partsLoaded < 6)
                partFile = partsEasy[Random.Range(0, partsEasy.Count)];
            else if (partsLoaded < 12)
            {
                // 50/50 chance of getting an easy or medium part
                if(Random.Range(0, 1) == 1)
                    partFile = partsEasy[Random.Range(0, partsEasy.Count)];
                else
                    partFile = partsMed[Random.Range(0, partsMed.Count)];
            }
            else
                partFile = partsMed[Random.Range(0, partsMed.Count)];

            Debug.Log(partFile);
            string partPath = partsDirectory + "/" + partFile;
            GameObject partPrefabGO = Resources.Load(partPath) as GameObject;
            Assert.IsTrue(partPrefabGO != null, "The part prefab does not exist! Path:" + partPath);
            TiledMap partPrefab = partPrefabGO.GetComponent<TiledMap>();
            Assert.IsTrue(partPrefab != null, "The part prefab does not have a TiledMap component.");

            TiledMap part = Instantiate(partPrefab, new Vector3(baseX, partPrefab.GetMapHeightInPixelsScaled()/2), Quaternion.identity, transform);

            Rigidbody2D physicsBody = part.GetComponent<Rigidbody2D>();
			Assert.IsTrue(physicsBody != null, "The map '" + partPath + "' needs to have a Rigidbody2D.");
            physicsBody.velocity = speed*Vector2.left;

            partsLoaded += 1;

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