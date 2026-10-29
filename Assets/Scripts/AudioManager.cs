using Unity.Netcode;
using UnityEngine;

public class AudioManager : NetworkBehaviour
{
    [SerializeField] public AudioClip[] audioClips;


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
        glassHit = 1,
        glassHeadshot = 2,
    }


    [Rpc(SendTo.Everyone)]
    public void PlaySoundAtPointRPC(Sounds soundEnum, Vector3 soundLocation)
    {
        AudioSource.PlayClipAtPoint(audioClips[(int)soundEnum], soundLocation);
    }

    public void PlaySoundAtPoint(Sounds soundEnum, Vector3 soundLocation, float pitch)
    {
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = soundLocation;

        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = audioClips[(int)soundEnum];
        audioSource.pitch = pitch;
        audioSource.Play();
        Object.Destroy(tempGO, audioClips[(int)soundEnum].length);
    }

    public void PlaySoundFromSource(Sounds soundEnum, AudioSource audioSource)
    {
        audioSource.PlayOneShot(audioClips[(int)soundEnum]);
    }
}
