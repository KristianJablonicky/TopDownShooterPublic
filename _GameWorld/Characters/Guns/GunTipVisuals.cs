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
        var z = characterMediator.RotationController.GetRotationAngle + 90f + Random.Range(-35, 35f);
        var hit = Instantiate(
            muzzleFlashes[Random.Range(0, muzzleFlashes.Length)],
            position, Quaternion.Euler(0f, 0f, z)
        );
        hit.transform.localScale *= Random.Range(2f, 3f);
    }
}
