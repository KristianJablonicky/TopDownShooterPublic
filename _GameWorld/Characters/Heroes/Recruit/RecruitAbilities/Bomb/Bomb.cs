using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SoundPlayer soundPlayer;
    [SerializeField] private AudioClip[] explosionSounds;
    [field: SerializeField] public GameObject Explosion { get; private set; }

    [Header("Bomb physics")]
    [SerializeField] private float velocityThreshold = 12f;
    [SerializeField] private float duration = 2f;
    [SerializeField] private float collisionDamping = 2f;

    [Header("Bomb values")]
    [SerializeField] private int maxDamage = 80;
    [SerializeField] private int minDamage = 30;
    [field: SerializeField] public float ExplosionRadius { get; private set; } = 3f;
    [field: SerializeField] public float RecoilMultiplierOnHit { get; private set; } = 2f;


    private CharacterMediator mediator;
    private RecruitAbilityRPCs networkAbilities;

    public void Init(RecruitAbilityRPCs networkAbilities, CharacterMediator mediator, Vector2 force)
    {
        rb.AddForce(force, ForceMode2D.Impulse);

        if (mediator.IsLocalPlayer)
        {
            this.mediator = mediator;
            this.networkAbilities = networkAbilities;
        }
        StartCoroutine(ExplodeAfterDelay(duration));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.magnitude > velocityThreshold)
        {
            Explode();
            return;
        }

        var reflect = Vector2.Reflect(rb.linearVelocity, collision.contacts[0].normal);
        rb.linearVelocity = reflect * collisionDamping;
    }

    private void Explode()
    {
        soundPlayer.RequestPlaySound(transform, explosionSounds, false);
        // locally rendered bomb, explosion will be handled by its owner
        if (mediator == null)
        {
            CleanUp();
            return;
        }

        networkAbilities.RequestExplosionRPC(transform.position);
        var hitCharacters = new List<CharacterMediator>();


        var manager = CharacterManager.Instance;
        foreach(var player in manager.Mediators.Values)
        {
            var distance = Vector2.Distance(player.GetPosition(), transform.position);
            if (distance <= ExplosionRadius)
            {
                var damage = Mathf.Lerp(maxDamage, minDamage, distance / ExplosionRadius);

                mediator.NetworkInput.DealDamage((int)damage, DamageTag.Ability, player, mediator);
                hitCharacters.Add(player);
            }
        }

        networkAbilities.RequestExplosionHitRPC(
            networkAbilities.MediatorsToIds(hitCharacters)
        );



        CleanUp();
    }

    private void CleanUp()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

    private IEnumerator ExplodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    public string DamageRangeString => $"{maxDamage} - {minDamage}";
}