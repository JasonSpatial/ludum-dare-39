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

        [SerializeField]
        private float slowDownRate = 1;

        [SerializeField]
        private float jumpButtonTimeWindow = 0.1f;

        ScrollingBackground background;

        private int groundIntersectionsCount = 0;

        private bool jumpRequested = false;

        private bool jumpHandled = false;

        private float jumpStartTimer;

        private Rigidbody2D physicsBody;

        private SpriteRenderer spriteRenderer;

        private void FixedUpdate()
        {
            if (Input.GetButton("Fire1"))
            {
                if (!jumpRequested)
                {
                    jumpRequested = true;
                    jumpStartTimer = 0;
                }
                else
                {
                    jumpStartTimer += Time.fixedDeltaTime;
                }

				if (jumpStartTimer <= jumpButtonTimeWindow)
				{
					if (grounded && !jumpHandled)
					{
						jumpHandled = true;
						Jump();
					}
				}
			}
            else
            {
                jumpHandled = false;
                jumpRequested = false;
            }

			if (transform.position.y + spriteRenderer.sprite.bounds.extents.y <= bottomOfTheGame)
            {
                GameOver();
            }

            background.SlowDown(slowDownRate*Time.fixedDeltaTime);
        }

        private void GameOver()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void Jump()
        {
            // Negate any existing vertical velocity before jumping.
            physicsBody.AddForce(-physicsBody.velocity.y*Vector2.up, ForceMode2D.Impulse);

            physicsBody.AddForce(jumpImpulse*Vector2.up, ForceMode2D.Impulse);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Powerup")
            {
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

            GameObject backgroundGO = GameObject.Find("Scrolling Background");
            Assert.IsTrue(backgroundGO != null, "Could not find the scrolling background.");
            background = backgroundGO.GetComponent<ScrollingBackground>();
            Assert.IsTrue(background != null, "The scrolling background game object does not have a ScrollingBackground component.");
        }
    }
}