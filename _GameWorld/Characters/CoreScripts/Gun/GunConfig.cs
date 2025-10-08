using UnityEngine;

[CreateAssetMenu(fileName = "GunConfig", menuName = "Guns/GunConfig")]
public class GunConfig : ScriptableObject
{
    [Header("General")]
    public bool isAutomatic = true;

    [Header("Damage")]
    public float RPM = 600f;
    public int capacity = 30;
    public int damage = 30;
    public int headshotDamage = 80;
    public float reloadDuration = 3f;

    [Header("Accuracy")]
    public float maxRecoilDiameter = 3f;
    [Range(0f, 1f)] public float baseRecoilPerShot = 0.1f;
    [Range(0f, 1f)] public float recoilPerShotMultiplier = 0.2f;
    public float recoilRecoveryPerSecond = 1f;
    [Range(0f, 1f)] public float movementInaccuracyFactor = 0.5f;
    [Range(0f, 1f)] public float headshotAccuracyRequirement = 0.2f;

    [Header("Miscellaneous")]
    public float bulletRange = 12;

    [Header("Visuals")]
    public AudioClip[] shootSounds;

    [Header("Specific")]
    public int shotCount = 1;
}
