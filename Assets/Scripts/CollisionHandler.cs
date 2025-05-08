using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionHandler : MonoBehaviour
{
    /* ────────── Inspector fields ────────── */
    [Header("Level")]
    [SerializeField] float levelLoadDelay = 2f;

    [Header("Audio")]
    [SerializeField] AudioClip successSFX;
    [SerializeField] AudioClip crashSFX;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem crashParticles;

    /* ────────── runtime refs ────────── */
    private int currentSceneIndex;
    private AudioSource audioSource;

    /* ────────── state flags ────────── */
    private bool isControlable = true;

    void Start()
    {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision other)
    {
        if (!isControlable)
        {
            return;
        }

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
        DisableControlAndAudio();
        audioSource.PlayOneShot(crashSFX);
        crashParticles.Play();
        DisableMovement();
        Invoke(nameof(ReloadLevel), levelLoadDelay);
    }

    void StartSuccessSequence()
    {
        DisableControlAndAudio();
        audioSource.PlayOneShot(successSFX);
        successParticles.Play();
        DisableMovement();
        Invoke(nameof(LoadNextLevel), levelLoadDelay);
    }

    void DisableControlAndAudio()
    {
        isControlable = false;
        audioSource.Stop();
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
