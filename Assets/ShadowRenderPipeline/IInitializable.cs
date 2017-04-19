using UnityEngine;

public interface IInitializable
{
    void Init();
}

public static class InitializableExtensions
{
    public static T GetOrCreate<T>(this T initializable) where T : ScriptableObject, IInitializable
    {
        return initializable ?? Initializable.Create<T>();
    }
}

public static class Initializable
{
    public static T Create<T>() where T : ScriptableObject, IInitializable
    {
        var initializable = ScriptableObject.CreateInstance<T>();
        initializable.Init();
        return initializable;
    }

    public static T GetOrCreate<T>(ref T initializable) where T : ScriptableObject, IInitializable
    {
        return initializable ?? (initializable = Create<T>());
    }
}
