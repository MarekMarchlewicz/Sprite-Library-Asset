using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.U2D.Animation;

public static class SpriteSkinHelpers
{
    const string k_AnimationRuntimeNamespace = "UnityEngine.U2D.Animation";
    const string k_AnimationRuntimeAssembly = "Unity.2D.Animation.Runtime";

    static readonly Assembly k_RuntimeAssembly = Assembly.Load(k_AnimationRuntimeAssembly);
    static readonly Type k_SpriteSkinType = k_RuntimeAssembly.GetType($"{k_AnimationRuntimeNamespace}.SpriteSkin");
    static readonly PropertyInfo k_SpriteSkinGetAutoRebind = k_SpriteSkinType.GetProperty("autoRebind", BindingFlags.Instance | BindingFlags.NonPublic);

    /// <summary>
    /// Gets the value of Auto Rebind property.
    /// </summary>
    /// <param name="spriteSkin">Sprite Skin component.</param>
    /// <returns>True if the Auto Rebind is enabled, otherwise false.</returns>
    public static bool GetAutoRebind(this SpriteSkin spriteSkin) => (bool)k_SpriteSkinGetAutoRebind.GetValue(spriteSkin);

    /// <summary>
    /// Sets the value of Auto Rebind property.
    /// </summary>
    /// <param name="spriteSkin">Sprite Skin component.</param>
    /// <param name="autoRebind">True if the Auto Rebind is enabled, otherwise false.</param>
    public static void SetAutoRebind(this SpriteSkin spriteSkin, bool autoRebind) => k_SpriteSkinGetAutoRebind.SetValue(spriteSkin, autoRebind);
}
