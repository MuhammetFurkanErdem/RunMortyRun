using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Ses Efektleri (Clips)")]
    [SerializeField] private AudioClip positiveGateSFX;  // Kapıdan + veya x ile geçince
    [SerializeField] private AudioClip negativeGateSFX;  // Kapıdan - veya ÷ ile geçince
    [SerializeField] private AudioClip characterDeathSFX; // Karakter tuzağa değip ölünce
    [SerializeField] private AudioClip levelWinSFX;      // Bitiş çizgisini geçince
    [SerializeField] private AudioClip gameOverSFX;      // Oyun kaybedilince

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayPositiveGate()
    {
        PlaySFX(positiveGateSFX);
    }

    public void PlayNegativeGate()
    {
        PlaySFX(negativeGateSFX);
    }

    public void PlayCharacterDeath()
    {
        PlaySFX(characterDeathSFX);
    }

    public void PlayLevelWin()
    {
        PlaySFX(levelWinSFX);
    }

    public void PlayGameOver()
    {
        PlaySFX(gameOverSFX);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            // PlayOneShot seslerin üst üste binerek kesilmeden çalmasını sağlar
            sfxSource.PlayOneShot(clip);
        }
    }
}