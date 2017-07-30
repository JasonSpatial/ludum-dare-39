namespace Assets.Scripts.UI
{
    using Assets.Scripts.Infrastructure;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.SceneManagement;

    /// <summary>
    ///     Handles controlling the player.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private float jumpImpulse = 10;

        [SerializeField]
        private bool grounded = false;

        [SerializeField]
        private float bottomOfTheGame = -3;

        private int groundIntersectionsCount = 0;

        private Rigidbody2D physicsBody;

        private SpriteRenderer spriteRenderer;

        private void FixedUpdate()
        {
            
            if (transform.position.y + spriteRenderer.sprite.bounds.extents.y <= bottomOfTheGame)
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void Jump()
        {
            physicsBody.AddForce(jumpImpulse * Vector2.up, ForceMode2D.Impulse);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Powerup")
            {
                ScrollingBackground background = GameObject.Find("Scrolling Background").GetComponent<ScrollingBackground>();
                background.SpeedUp(2);
            }
            else if (collision.name == "Collision_Ground")
            {
                groundIntersectionsCount++;
                grounded = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.name == "Collision_Ground")
            {
                groundIntersectionsCount--;
                Assert.IsTrue(groundIntersectionsCount >= 0, "Ground intersections counter has become negative!");
                if (groundIntersectionsCount == 0)
                {
                    grounded = false;
                }
            }
        }

        private void Start()
        {
            physicsBody = GetComponent<Rigidbody2D>();
            Assert.IsTrue(physicsBody != null, "PlayerController requires a RigidBody2D component and there was none.");

            spriteRenderer = GetComponent<SpriteRenderer>();
            Assert.IsTrue(spriteRenderer != null, "PlayerController requires a SpriteRenderer component and there was none.");
        }

        private void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (grounded) Jump();
            }
        }
    }
}