namespace Assets.Scripts.UI
{
    using Assets.Scripts.Infrastructure;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

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

        [SerializeField]
        private float speed = 8;

		[SerializeField]
		private float maxPower = 30;

        private int groundIntersectionsCount = 0;

        private Slider guiPowerBar;

        private bool jumpRequested = false;

        private bool jumpHandled = false;

        private float jumpStartTimer;

        private Rigidbody2D physicsBody;

        private SpriteRenderer spriteRenderer;

        private void FixedUpdate()
        {
            physicsBody.AddForce(-physicsBody.velocity.x*Vector2.right, ForceMode2D.Impulse);
            physicsBody.AddForce(speed*Vector2.right, ForceMode2D.Impulse);
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

            if (Mathf.Approximately(speed, 0))
            {
                GameOver();
            }

            SlowDown(slowDownRate*Time.fixedDeltaTime);
        }

        private void GameOver()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        ///     Gets the players current speed.
        /// </summary>
        /// <returns>The speed.</returns>
        public float GetSpeed()
        {
            return speed;
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
                SpeedUp(2);
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

        private void SlowDown(float delta)
        {
            speed = Mathf.Max(0, speed - delta);
            SpeedChanged();
        }

        private void SpeedChanged()
        {
            guiPowerBar.value = Mathf.Min(speed/maxPower, 1);
        }

        private void SpeedUp(float delta)
        {
            speed += delta;
            SpeedChanged();
        }

        private void Start()
        {
            physicsBody = GetComponent<Rigidbody2D>();
            Assert.IsTrue(physicsBody != null, "PlayerController requires a RigidBody2D component and there was none.");

            spriteRenderer = GetComponent<SpriteRenderer>();
            Assert.IsTrue(spriteRenderer != null, "PlayerController requires a SpriteRenderer component and there was none.");

			GameObject powerBarGO = GameObject.Find("GUI_PowerBar");
			Assert.IsTrue(powerBarGO != null, "No power bar found in the GUI.");
			guiPowerBar = powerBarGO.GetComponent<Slider>();
			Assert.IsTrue(guiPowerBar != null, "Power bar does not have a Slider component.");
		}
    }
}