using UnityEngine;

//[CreateAssetMenu(fileName = "ScriptableObjectBase", menuName = "Scriptable Objects/ScriptableObjectBase")]
public abstract class ScriptableObjectBase : ScriptableObject
{
    [field: SerializeField] public string Name {  get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }

    public abstract string _GetSpecificAttributes();
}
