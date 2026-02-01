using UnityEngine;
using static AimDirection;

public class Gun : MonoBehaviour, IResettable
{
    
    [SerializeField] private AbilityManager abilityManager;
    public GunConfig GunConfig { get; private set; }

    [SerializeField] private float headShotRangeBuffer = 1f;
    [SerializeField] private Transform gunTip;
    [SerializeField] private MovementController characterMovementController;
    [SerializeField] private BulletVisualization bulletVisualization;
    [SerializeField] private CharacterMediator mediator;
    [SerializeField] private LayerMask shotLayers;


    [Header("Prevention of self-damage")]
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private Hitbox playerHead;
    [SerializeField] private HealthComponent playerHealth;

    [Header("Visual tweaks")]
    [SerializeField] private GunTipVisuals gunTipVisuals;

    private static readonly RaycastHit2D[] _raycastHits = new RaycastHit2D[15];

    public ShootManager ShootManager { get; private set; }
    public ChannelingManager ChannelingManager { get; private set; }
    private RecoilManager recoilManager;
    private IUpdatable[] managers;
    private void Awake()
    {
        GunConfig = abilityManager.Toolkit.GunConfig;
        ChannelingManager = new (mediator.AnimationController);
        ShootManager = new (mediator.NetworkInput, GunConfig, ChannelingManager);
        recoilManager = new (GunConfig, characterMovementController);
        managers = new IUpdatable[]
        {
            recoilManager,
            ShootManager,
            ChannelingManager,
        };

        gunTipVisuals.SetConfig(GunConfig);
    }

    private void Update()
    {
        foreach (var manager in managers)
        {
            manager.IUpdate(Time.deltaTime);
        }
    }

    public bool CanReload() => ShootManager.CanReload();
    public void Reload() => ShootManager.InvokedReload();
    public float GetAngle() => recoilManager.GetRecoilAngle();
    public float GetRecoil() => recoilManager.CurrentRecoil;

    public int ShotCount => GunConfig.shotCount;
    public bool CanShoot(bool pressedDown)
    {
        if (!GunConfig.isAutomatic && !pressedDown) return false;

        var didShoot = ShootManager.CanShoot();
        if (didShoot == ShootManager.ShootResult.DidNotShoot) return false;

        return true;
    }

    private void DealDamage(int damage, DamageTag tag, HealthComponent targetHealthComponent)
        => mediator.NetworkInput.DealDamage(damage, tag, targetHealthComponent.Mediator, mediator);

    // ran locally
    public bool CanHeadShot(bool pressedDown, AimDirection direction)
    {
        return pressedDown
            && recoilManager.CanHeadshot()
            && direction == Straight;
    }

    public void ApplyRecoil(float shotCount = 1f) => recoilManager.ApplyRecoil(shotCount);
    public void ApplyRecoilMouseMovement(float recoil) => recoilManager.ApplyRecoilMouseMovement(recoil);

    public Vector2 GetShootDirection(Vector2 targetPosition) => GetTarget(targetPosition);

    public bool SingleFloorShot(AimDirection direction, float sourceToDestinationDistance)
    {
        return direction == Straight
            || !IsInRange(sourceToDestinationDistance);
    }

    public bool IsInRange(float distance) => distance <= GunConfig.bulletRange;

    public void ShowShotVisuals()
    {
        gunTipVisuals.Shoot();
        mediator.AnimationController.PlayAnimation(Animations.Shoot);
    }

    public Vector2? RaycastForDamage(Vector2 source, Vector2 direction, Vector2? destination, float damageMultiplier = 1f)
    {
        Vector2? returnValue = null;
        float damage = GunConfig.damage * damageMultiplier;

        var raycastCount = GetRaycastHits(source, direction, destination);
        RaycastHit2D hit;
        for (int i = 0; i < raycastCount; i++)
        {
            hit = _raycastHits[i];
            var collider = _raycastHits[i].collider;

            // avoid shooting yourself.
            if (collider == null || collider == playerCollider) continue;

            if (collider.TryGetComponent<HealthComponent>(out var health))
            {
                DealDamage((int)damage, DamageTag.Shot, health);
                returnValue = hit.point;
                break;
            }

            else if (collider.TryGetComponent<Obstacle>(out var wall))
            {
                damage *= wall.DamageMultiplier;
                if (damage == 0)
                {
                    returnValue = hit.point;
                    break;
                }
            }
        }
        
        return returnValue;
    }

    /// <summary>
    /// int method, since the raycastHits get stored to the<b>_raycastHits</b> field.
    /// </summary>
    /// <returns>The number of raycast</returns>
    private int GetRaycastHits(Vector2 source, Vector2 direction, Vector2? destination)
    {
        float range;

        // classic shooting on the same floor
        if (!destination.HasValue)
        {
            range = GunConfig.bulletRange;
        }
        else
        {
            range = Vector2.Distance(source, destination.Value);
        }
        return Physics2D.RaycastNonAlloc(source, direction, _raycastHits, range, shotLayers);
    }


    // ran by the host, returns null if headshot was not successful
    public Vector2? TryToHeadShot(Vector2 target)
    {
        var distance = Vector2.Distance(target, (Vector2)transform.position);

        // can't headshot people outside of vision
        if (distance > GunConfig.bulletRange + headShotRangeBuffer) return null;

        // first, let's see if the player clicked on a head
        bool headHit = false;
        HealthComponent health = null;
        Vector3? returnValue = null;
        var hits = Physics2D.RaycastAll(target, Vector2.zero, shotLayers);
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            // TODO: reconsider utilizing Tags.
            if (hit.collider.TryGetComponent<Hitbox>(out var hitbox))
            {
                // Disallow shooting yourself in the head.
                if (hitbox == playerHead) continue;
                headHit = true;
                returnValue = hit.transform.position;
            }

            else if (hit.collider.TryGetComponent<HealthComponent>(out var healthComponent))
            {
                if (healthComponent == playerHealth) continue;
                health = healthComponent;
            }
        }
        
        var direction = (target - (Vector2)transform.position).normalized;
        // now, let's see if the head was at least half-visible on the player's screen
        foreach (var hit in Physics2D.RaycastAll(transform.position, direction, distance, shotLayers))
        {
            if (hit.collider == null) continue;

            if (hit.collider.TryGetComponent<Obstacle>(out var _))
            {
                // Can't headshot through walls.
                return null;
            }
        }

        // a head was clicked and also visible to the player
        if (headHit)
        {
            if (health == null)
            {
                Debug.LogWarning("A head was hit, but no health component was found.");
                return null;
            }

            DealDamage(GunConfig.headshotDamage, DamageTag.HeadShot, health);
        }

        return returnValue;
    }

    public float CanHeadShot() => 1f - (recoilManager.CurrentRecoil / GunConfig.headshotAccuracyRequirement);

    public void ShowHeadshotBulletTrail(Vector2 headshotPos)
    {
        ShowBulletTrailStopped(headshotPos);
        var rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        Instantiate(gunTipVisuals.HeadShotVisuals, headshotPos, rotation);
    }
    public void ShowBulletTrail(Vector2 direction)
    {
        var bulletTrail = Instantiate(bulletVisualization);//, gunTip.position, Quaternion.identity);
        bulletTrail.Shoot(gunTip.position, (Vector2)transform.position + (direction * GunConfig.bulletRange));
    }
    public void ShowBulletTrailStopped(Vector2 targetPosition)
    {
        ShowBulletTrailStopped(targetPosition, gunTip.position, false);
    }

    public void ShowBulletTrailStopped(Vector2 targetPosition, Vector2 source, bool showHitOnSource)
    {
        gunTipVisuals.ShowHit(targetPosition);
        if (showHitOnSource)
        {
            gunTipVisuals.ShowHit(source);
        }
        var bulletTrail = Instantiate(bulletVisualization, gunTip.position, Quaternion.identity);
        bulletTrail.Shoot(source, targetPosition);
    }

    private Vector2 GetTarget(Vector2 targetPosition)
    {
        var playerPos = mediator.GetPosition();

        var diff = targetPosition - playerPos;
        var distance = diff.magnitude;
        var direction = diff.normalized;

        var angleDeg = recoilManager.GetRecoilAngle() *
            Random.Range(-0.5f, 0.5f);
        var radians = angleDeg * Mathf.Deg2Rad;

        var cos = Mathf.Cos(radians);
        var sin = Mathf.Sin(radians);

        var rotatedDir = new Vector2 (
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        );

        return playerPos + rotatedDir.normalized * distance;
    }

    public void Reset()
    {
        ShootManager.Reset();
        ChannelingManager.Reset();
    }
}

public enum ShotType
{
    Normal,
    CurrentFloor,
    FloorShotTo
}