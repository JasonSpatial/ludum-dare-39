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
        private PlayerController player;

        private void Start()
        {
            GameObject playerGO = GameObject.Find("Player");
            Assert.IsTrue(playerGO != null, "CameraFollow could not find the player.");
            player = playerGO.GetComponent<PlayerController>();
            Assert.IsTrue(player != null, "No PlayerController found on Player.");
        }

        private void FixedUpdate()
        {
            Vector3 newPosition = transform.position;
            newPosition.x += player.GetSpeed()*Time.deltaTime;
            transform.position = newPosition;
        }
    }
}