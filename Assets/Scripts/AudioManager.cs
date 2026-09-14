using Unity.Netcode;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{
    [SerializeField] private AudioClip[] audioClips;


    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        // 2. Ensure only one instance exists in the scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroys duplicates
            return;
        }

        Instance = this;
    }


    public enum Sounds
    {
        shootCrunch = 0,

    }


    public void PlaySoundAtPoint(Sounds soundEnum, Vector3 soundLocation)
    {
        AudioSource.PlayClipAtPoint(audioClips[(int)soundEnum], soundLocation);
    }

    public void PlaySoundFromSource(Sounds soundEnum, AudioSource audioSource)
    {
        audioSource.PlayOneShot(audioClips[(int)soundEnum]);
    }
/*
    [Rpc(SendTo.Everyone)]
    public void PlaySoundFromSourceForAllRpc(Sounds soundEnum, AudioSource audioSource)
    {
        audioSource.PlayOneShot(audioClips[(int)soundEnum]);
    }*/
    
}
