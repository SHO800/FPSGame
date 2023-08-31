using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public float volume = 0.5f;
    
    [SerializeField] private AudioClip reloadStartSound;
    [SerializeField] private AudioClip reloadEndSound;
    [SerializeField] private AudioClip pickupGunSound;
    [SerializeField] private AudioClip pickupAmmoSound;
    [SerializeField] private AudioClip getDamageSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip deadSound;
    [SerializeField] private AudioClip hitSound;
    
    private AudioSource _audioSource;
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    public void PlayReloadStartSound()
    {
        _audioSource.volume = volume;
        _audioSource.PlayOneShot(reloadStartSound);
    }
    
    public void PlayReloadEndSound()
    {
        _audioSource.volume = volume;
        _audioSource.PlayOneShot(reloadEndSound);
    }
    
    public void PlayPickupGunSound()
    {
        _audioSource.volume = volume;
        _audioSource.PlayOneShot(pickupGunSound);
    }
    
    public void PlayPickupAmmoSound()
    {
        _audioSource.volume = volume;
        _audioSource.PlayOneShot(pickupAmmoSound);
    }
    
    public void PlayGetDamageSound()
    {
        _audioSource.volume = volume;
        _audioSource.PlayOneShot(getDamageSound);
    }
    
    public void PlayHealSound()
    {
        _audioSource.volume = volume;
        _audioSource.PlayOneShot(healSound);
    }
    
    public void PlayDeadSound()
    {
        _audioSource.volume = volume;
        _audioSource.PlayOneShot(deadSound);
    }
    
    public void PlayHitSound()
    {
        _audioSource.volume = volume;
        _audioSource.PlayOneShot(hitSound);
    }
    

}
