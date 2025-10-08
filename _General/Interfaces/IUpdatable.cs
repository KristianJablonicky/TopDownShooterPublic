public interface IUpdatable
{
    /// <summary>
    /// A bit of a silly name to make sure Unity knows what Update() method to invoke on MonoBehaviour child classes
    /// </summary>
    /// <param name="dt">Time.deltaTime</param>
    public void IUpdate(float dt);
}