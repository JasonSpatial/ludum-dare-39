namespace Assets.Scripts.Infrastructure
{
    using Assets.Scripts.UI;
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary>
    ///     The camera follows the player.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        private PlayerController playerController;

        private Rigidbody2D playerPhysicsBody;

        private void Start()
        {
            GameObject playerGO = GameObject.Find("Player");
            Assert.IsTrue(playerGO != null, "CameraFollow could not find the player.");
            playerController = playerGO.GetComponent<PlayerController>();
            Assert.IsTrue(playerController != null, "No PlayerController found on Player.");

            playerPhysicsBody = playerGO.GetComponent<Rigidbody2D>();
            Assert.IsTrue(playerPhysicsBody != null, "No Rigidbody2D found on Player.");
        }

        private void FixedUpdate()
        {
            Vector3 newPosition = transform.position;
            newPosition.x += playerController.GetSpeed()*Time.deltaTime;
            transform.position = newPosition;
        }
    }
}