using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionHandler : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] float levelLoadDelay = 2f;

    [Header("Audio")]
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip crash;

    private int currentSceneIndex;
    private AudioSource audioSource;

    void Start()
    {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision other)
    {
        switch (other.gameObject.tag)
        {
            case "Friendly":
                Debug.Log("Friendly");
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            case "Fuel":
                Debug.Log("Fuel");
                break;
            default:
                StartCrashSequence();
                break;
        }
    }

    void StartCrashSequence()
    {
        audioSource.PlayOneShot(crash);
        DisableMovement();
        Invoke(nameof(ReloadLevel), levelLoadDelay);
    }

    void StartSuccessSequence()
    {
        audioSource.PlayOneShot(success);
        DisableMovement();
        Invoke(nameof(LoadNextLevel), levelLoadDelay);
    }

    void DisableMovement()
    {
        GetComponent<Movement>().enabled = false;
    }

    void LoadNextLevel()
    {
        int nextScene = currentSceneIndex + 1;
        if (nextScene == SceneManager.sceneCountInBuildSettings)
        {
            nextScene = 0;
        }
        SceneManager.LoadScene(nextScene);
    }

    void ReloadLevel()
    {
        SceneManager.LoadScene(currentSceneIndex);
    }
}
