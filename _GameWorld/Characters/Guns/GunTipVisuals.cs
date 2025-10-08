using UnityEngine;

public class GunTipVisuals : MonoBehaviour
{
    [SerializeField] private GameObject[] muzzleFlashes;
    [field: SerializeField] public GameObject HeadShotVisuals { get; private set; }

    [SerializeField] private float lightIntensity;
    [SerializeField] private float lightIntensityRange;

    [SerializeField] private AudioSource audioSource;
    private GunConfig gunConfig;
    public void SetConfig(GunConfig config) => gunConfig = config;

    public void Shoot()
    {
        Instantiate(
            muzzleFlashes[Random.Range(0, muzzleFlashes.Length)],
            transform
        );
        var clip = gunConfig.shootSounds[Random.Range(0, gunConfig.shootSounds.Length)];
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip);
    }

    public void ShowHit(Vector2 position)
    {
        Quaternion randomZ = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        var hit = Instantiate(
            muzzleFlashes[Random.Range(0, muzzleFlashes.Length)],
            position, randomZ
        );
        hit.transform.localScale *= 2f;
    }
}
