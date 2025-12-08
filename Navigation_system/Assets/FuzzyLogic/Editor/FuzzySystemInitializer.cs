using UnityEngine;
using UnityEditor;
using FuzzyLogic;
using FuzzyLogic.Data;
using System.Collections.Generic;

/// <summary>
/// Contains the extension method for cleanly adding antecedents in the initializer script.
/// C# requires extension methods to be in a top-level, non-generic, static class.
/// </summary>
public static class FuzzySystemInitializerExtensions
{
    public static FuzzyRuleDef AddAntecedent(this FuzzyRuleDef rule, string variableName, string setName)
    {
        rule.Antecedents.Add(new FuzzyAntecedentDef { VariableName = variableName, SetName = setName });
        return rule;
    }
}

public class FuzzySystemInitializer : EditorWindow
{
    private static string assetsPath = "Assets/FuzzyLogic/Data/";

    private static FuzzySetDef CreateFuzzySetDef(string name, float p1, float p2, float p3, float p4)
    {
        // Replicate FuzzySet.Trapezoid logic, but without setting tangents to linear
        // to match the original runtime behavior.
        Keyframe[] kf = new Keyframe[]
        {
            new Keyframe(0f, 0f),
            new Keyframe(p1, 0f),
            new Keyframe(p2, 1f),
            new Keyframe(p3, 1f),
            new Keyframe(p4, 0f),
            new Keyframe(1f, 0f)
        };
        AnimationCurve curve = new AnimationCurve(kf);
        
        return new FuzzySetDef { Name = name, Curve = curve };
    }

    private static FuzzyRuleDef CreateFuzzyRuleDef(string consequentSetName, float weight = 1f)
    {
        return new FuzzyRuleDef { ConsequentSetName = consequentSetName, Weight = weight, Antecedents = new List<FuzzyAntecedentDef>() };
    }

    private static void SaveAsset(ScriptableObject so, string fileName)
    {
        if (!AssetDatabase.IsValidFolder(assetsPath.TrimEnd('/')))
        {
            AssetDatabase.CreateFolder("Assets/FuzzyLogic", "Data");
        }
        string path = AssetDatabase.GenerateUniqueAssetPath(assetsPath + fileName);
        AssetDatabase.CreateAsset(so, path);
    }
}

