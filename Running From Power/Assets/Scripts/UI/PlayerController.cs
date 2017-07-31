﻿namespace Assets.Scripts.UI
{
    using System;
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
        private float speedInitial = 10; // speed at start of game
        private float speed = 0; // current speed

        [SerializeField]
		private float maxPower = 30;

        private int groundIntersectionsCount = 0;

        private Slider guiPowerBar;

        private bool jumpRequested = false;

        private bool jumpHandled = false;

        private float jumpStartTimer;

        private Rigidbody2D physicsBody;

        private SpriteRenderer spriteRenderer;

        private Animator playerAnimator;

        private Text textDistance;
        private Text textBestDistance;

        private float distance;  // distance the player has moved in meters
        private float startX;   // the starting position of the Player, which is typically -2.33. Used to calculate distance from this point
        private float distanceBest = 0.0f; // The best/furthest distance the player reached this run of the game

        bool started = false;

        private void FixedUpdate()
        {
            if (!started)
            {
                if (Input.GetButton("Jump"))
                {
                    StartGame();
                }
                return;
            }
            
            physicsBody.AddForce(-physicsBody.velocity.x*Vector2.right, ForceMode2D.Impulse);
            physicsBody.AddForce(speed*Vector2.right, ForceMode2D.Impulse);
            if (Input.GetButton("Jump"))
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

            distance = (transform.position.x - startX) / 3;
            textDistance.text = "Meters " + distance.ToString("0.00");
            
            if (distance > distanceBest)
            {
                distanceBest = distance;
                textBestDistance.text = "Best " + distanceBest.ToString("0.00");
            }
        }

        private void GameOver()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            playerAnimator.StopPlayback();
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
            physicsBody.AddForce(-physicsBody.velocity.y * Vector2.up, ForceMode2D.Impulse);

            physicsBody.AddForce(jumpImpulse * Vector2.up, ForceMode2D.Impulse);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.name == "Collision_Wall" && collision.relativeVelocity.x < 0)
            {
                GameOver();
            }
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

            playerAnimator = GetComponent<Animator>();
            Assert.IsTrue(playerAnimator != null, "PlayerController requires a Animator component and there was none.");

            GameObject powerBarGO = GameObject.Find("GUI_PowerBar");
			Assert.IsTrue(powerBarGO != null, "No power bar found in the GUI.");

			guiPowerBar = powerBarGO.GetComponent<Slider>();
			Assert.IsTrue(guiPowerBar != null, "Power bar does not have a Slider component.");

            GameObject distanceGO = GameObject.Find("Distance");
            textDistance = distanceGO.GetComponent<Text>();

            GameObject bestDistanceGO = GameObject.Find("Best Distance");
            textBestDistance = bestDistanceGO.GetComponent<Text>();
            
            guiPowerBar.value = Mathf.Min(speedInitial / maxPower, 1);

            playerAnimator.StartPlayback();
        }

        private void StartGame()
        {
            started = true;
            speed = speedInitial;
            jumpHandled = true;
            playerAnimator.StopPlayback();
            startX = transform.position.x;
        }
    }
}