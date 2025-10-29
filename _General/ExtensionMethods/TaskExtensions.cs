using System;
using System.Threading.Tasks;

public static class TaskExtensions
{
    public static Task Delay(float seconds)
    {
        return Task.Delay(TimeSpan.FromSeconds(seconds));
    }
}