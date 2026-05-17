using UnityEngine;

public class GunCardSetUp : CardSetUp
{
    [SerializeField] private GunCardIcon damage, headShotDamage, capacity, reloadTime;
    protected override string SpecificInit(CharacterToolkit toolkit, ScriptableObjectBase SOBase)
    {
        if (SOBase is GunConfig config)
        {
            damage.SetValue(config.damage);
            headShotDamage.SetValue(config.headshotDamage);
            capacity.SetValue(config.capacity);
            reloadTime.SetValue(config.reloadDuration);

            return $"{config.Description}\n\n{config._GetSpecificAttributes()}";
        }
        var errorMessage = $"{this} received ScriptableObjectBase not of type GunConfig!";
        Debug.LogError(errorMessage);
        return errorMessage;
    }
}
