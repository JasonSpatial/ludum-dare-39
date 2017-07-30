namespace Assets.Scripts.Behaviors
{
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    ///     Handles ending the game if you fall into a pit.
    /// </summary>
    public class TheVoid : MonoBehaviour
    {
        private void OnTriggerExit2D(Collider2D collision)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}