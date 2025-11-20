using UnityEngine;

public class GunTipVisuals : MonoBehaviour
{
    [SerializeField] private CharacterMediator characterMediator;
    [SerializeField] private GameObject[] muzzleFlashes;
    [field: SerializeField] public GameObject HeadShotVisuals { get; private set; }

    [SerializeField] private SoundPlayer soundPlayer;
    private GunConfig gunConfig;
    public void SetConfig(GunConfig config) => gunConfig = config;

    public void Shoot()
    {
        Instantiate(
            muzzleFlashes[Random.Range(0, muzzleFlashes.Length)],
            transform
        );
        soundPlayer.RequestPlaySound(characterMediator.GetTransform(), gunConfig.shootSounds, true);
    }

    public void ShowHit(Vector2 position)
    {
        var hit = Instantiate(
            muzzleFlashes[Random.Range(0, muzzleFlashes.Length)],
            position, QuaternionUtilities.Random()
        );
        hit.transform.localScale *= 2f;
    }
}
