using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("SFX Clips")]
    public AudioClip plantClip;
    public AudioClip harvestClip;
    public AudioClip enemySpawnClip;
    public AudioClip damageClip;
    public AudioClip gameOverClip;
    public AudioClip phaseClip;
    public AudioClip buttonClip;

    [Header("Music")]
    public AudioClip backgroundMusic;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        PlayMusic();
    }

    public void PlayPlant()
    {
        PlaySFX(plantClip);
    }

    public void PlayHarvest()
    {
        PlaySFX(harvestClip);
    }

    public void PlayEnemySpawn()
    {
        PlaySFX(enemySpawnClip);
    }

    public void PlayDamage()
    {
        PlaySFX(damageClip);
    }

    public void PlayGameOver()
    {
        PlaySFX(gameOverClip);
    }

    public void PlayPhase()
    {
        PlaySFX(phaseClip);
    }

    public void PlayButton()
    {
        PlaySFX(buttonClip);
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    void PlayMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
}
