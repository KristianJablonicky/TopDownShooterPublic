using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private Image bulletImage;
    [SerializeField] private Transform ammoRadiusCenter;

    [Header("Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField, Range(0f, 90f)] private float arcAngle = 90f;

    [SerializeField] private Color unavailableColor = Color.gray;
    
    [SerializeField] private int capacityForExtraScaleCeiling = 10;
    [SerializeField] private float maxScale = 2f;

    private ShootManager manager;
    private Image[] bullets;

    private void Awake()
    {
        PlayerNetworkInput.PlayerSpawned += OnOwnerSpawn;
    }

    private void OnOwnerSpawn(CharacterMediator mediator)
    {
        var capacity = mediator.Gun.GunConfig.capacity;
        if (capacity == 1)
        {
            Destroy(gameObject);
            return;
        }


        manager = mediator.Gun.ShootManager;
        SetUpAmmoImages(mediator.Gun);

        if (capacity < 10)
        {
            Destroy(ammoText.gameObject);
        }
        else
        {
            manager.CurrentAmmo.OnValueSet += UpdateAmmoText;
        }
        manager.CurrentAmmo.OnValueSet += UpdateAmmoImages;
        UpdateAmmoText(manager.CurrentAmmo);
    }

    private void SetUpAmmoImages(Gun gun)
    {
        var capacity = gun.GunConfig.capacity;
        bullets = new Image[capacity];

        var capacityScale = Mathf.Clamp01((float)capacity / capacityForExtraScaleCeiling);
        var scale = Mathf.Lerp(maxScale, 1f, capacityScale);

        for (int i = 0; i < capacity; i++)
        {
            var t = 1f - (i + 1f) / (capacity + 1f);
            var angleGap = (90f - arcAngle) * 0.5f;
            var angle = Mathf.Lerp(-angleGap, angleGap - 90f, t);
            var rad = angle * Mathf.Deg2Rad;


            var img = Instantiate(bulletImage, transform);
            img.gameObject.transform.localPosition = new Vector2
                (Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

            img.transform.localRotation = Quaternion.Euler(0, 0, angle);

            if (scale > 1f)
            {
                img.transform.localScale = Vector2.one * scale;
            }

            bullets[i] = img;
        }
    }

    private void UpdateAmmoText(int newAmmo)
    {
        if (newAmmo > 0)
        {
            ammoText.text = newAmmo.ToString();
        }
        else
        {
            ammoText.text = string.Empty;
        }
    }

    public void UpdateAmmoImages(int newAmmo)
    {
        var capacity = bullets.Length;
        var ammoRemaining = (float)newAmmo / capacity;
        ammoRemaining = Mathf.Clamp01(2f * ammoRemaining);

        var availableColor = new Color(1f, ammoRemaining, ammoRemaining);
        for (int i = 0; i < bullets.Length; i++)
        {
            bullets[i].color = (i < newAmmo) ? availableColor : unavailableColor;
        }
    }
}
