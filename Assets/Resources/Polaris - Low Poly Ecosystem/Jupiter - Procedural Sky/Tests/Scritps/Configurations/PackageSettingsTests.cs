using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Pinwheel.Jupiter;
using System.Reflection;

public class PackageSettingsTests
{
    [Test]
    public static void _SettingsAssetExists()
    {
        JJupiterSettings s = Resources.Load<JJupiterSettings>("JupiterSettings");
        Assert.IsNotNull(s);
    }

    [Test]
    public static void DefaultProfilesAssigned()
    {
        JJupiterSettings s = JJupiterSettings.Instance;
        Assert.IsNotNull(s.DefaultProfileSunnyDay);
        Assert.IsNotNull(s.DefaultProfileStarryNight);
    }

    [Test]
    public static void DefaultTexturesAssigned()
    {
        JJupiterSettings s = JJupiterSettings.Instance;
        Assert.IsNotNull(s.NoiseTexture);
        Assert.IsNotNull(s.CloudTexture);
    }

    [Test]
    public static void DefaultSkyMaterialAssigned()
    {
        JJupiterSettings s = JJupiterSettings.Instance;
        Assert.IsNotNull(s.DefaultSkybox);
    }

    [Test]
    public static void InternalShadersAssigned()
    {
        JInternalShaderSettings s = JJupiterSettings.Instance.InternalShaders;
        FieldInfo[] fields = s.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        for (int i=0;i<fields.Length;++i)
        {
            Assert.IsNotNull(fields[i].GetValue(s), fields[i].Name + " is null.");
        }
    }
}
