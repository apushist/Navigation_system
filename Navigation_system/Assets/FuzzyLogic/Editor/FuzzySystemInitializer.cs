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

    [MenuItem("Tools/Fuzzy Logic/Initialize Default FIS Assets")]
    public static void InitDefaultFISAssets()
    {
        CreateDefaultSpeedFIS();
        CreateDefaultTurnFIS();
        EditorUtility.DisplayDialog("Fuzzy System Initializer", "Default SpeedFIS and TurnFIS assets have been created in " + assetsPath, "OK");
    }

    private static void CreateDefaultSpeedFIS()
    {
        FuzzyInferenceSystemSO speedFisSO = ScriptableObject.CreateInstance<FuzzyInferenceSystemSO>();
        speedFisSO.name = "SpeedFIS";

        // --- Input Variables ---
        // FrontDist
        var frontDistVar = new FuzzyVariableDef { Name = "FrontDist", Min = 0f, Max = 5f }; // Assuming MaxSensorDist is 5
        frontDistVar.Sets.Add(CreateFuzzySetDef("VeryNear", 0f, 0f, 0.1f, 0.2f));
        frontDistVar.Sets.Add(CreateFuzzySetDef("Near", 0.1f, 0.2f, 0.3f, 0.4f));
        frontDistVar.Sets.Add(CreateFuzzySetDef("Mid", 0.3f, 0.4f, 0.6f, 0.8f));
        frontDistVar.Sets.Add(CreateFuzzySetDef("Far", 0.7f, 0.8f, 1f, 1f));
        speedFisSO.InputVariables.Add(frontDistVar);

        // AngleAbs
        var angleAbsVar = new FuzzyVariableDef { Name = "AngleAbs", Min = 0f, Max = 180f };
        angleAbsVar.Sets.Add(CreateFuzzySetDef("Small", 0f, 0f, 10f / 180f, 30f / 180f));
        angleAbsVar.Sets.Add(CreateFuzzySetDef("Mid", 10f / 180f, 30f / 180f, 70f / 180f, 100f / 180f));
        angleAbsVar.Sets.Add(CreateFuzzySetDef("Large", 70f / 180f, 100f / 180f, 1f, 1f));
        speedFisSO.InputVariables.Add(angleAbsVar);

        // TargetDist
        var targetDistVar = new FuzzyVariableDef { Name = "TargetDist", Min = 0f, Max = 20f };
        targetDistVar.Sets.Add(CreateFuzzySetDef("VeryNear", 0f, 0f, 0.05f, 0.1f));
        targetDistVar.Sets.Add(CreateFuzzySetDef("Near", 0.05f, 0.1f, 0.15f, 0.2f));
        targetDistVar.Sets.Add(CreateFuzzySetDef("Mid", 0.15f, 0.2f, 0.3f, 0.4f));
        targetDistVar.Sets.Add(CreateFuzzySetDef("Far", 0.3f, 0.4f, 1f, 1f));
        speedFisSO.InputVariables.Add(targetDistVar);

        // --- Output Variable ---
        speedFisSO.OutputVariable = new FuzzyVariableDef { Name = "Speed", Min = 0f, Max = 1f };
        speedFisSO.OutputVariable.Sets.Add(CreateFuzzySetDef("Stop", 0f, 0f, 0.05f, 0.1f));
        speedFisSO.OutputVariable.Sets.Add(CreateFuzzySetDef("Slow", 0.05f, 0.1f, 0.2f, 0.3f));
        speedFisSO.OutputVariable.Sets.Add(CreateFuzzySetDef("Mid", 0.2f, 0.3f, 0.5f, 0.6f));
        speedFisSO.OutputVariable.Sets.Add(CreateFuzzySetDef("Fast", 0.5f, 0.6f, 1f, 1f));

        // --- Rules ---
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Slow").AddAntecedent("FrontDist", "VeryNear").AddAntecedent("TargetDist", "VeryNear"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Slow").AddAntecedent("FrontDist", "VeryNear").AddAntecedent("TargetDist", "Near"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Slow").AddAntecedent("FrontDist", "VeryNear").AddAntecedent("TargetDist", "Mid"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Slow").AddAntecedent("FrontDist", "VeryNear").AddAntecedent("TargetDist", "Far"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Slow").AddAntecedent("FrontDist", "Near").AddAntecedent("TargetDist", "VeryNear"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Slow").AddAntecedent("FrontDist", "Near").AddAntecedent("TargetDist", "Near"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Slow").AddAntecedent("FrontDist", "Near").AddAntecedent("TargetDist", "Mid"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Slow").AddAntecedent("FrontDist", "Near").AddAntecedent("TargetDist", "Far"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Slow").AddAntecedent("FrontDist", "Mid").AddAntecedent("TargetDist", "VeryNear"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Slow").AddAntecedent("FrontDist", "Mid").AddAntecedent("TargetDist", "Near"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Mid").AddAntecedent("FrontDist", "Mid").AddAntecedent("TargetDist", "Mid"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Mid").AddAntecedent("FrontDist", "Mid").AddAntecedent("TargetDist", "Far"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Slow").AddAntecedent("FrontDist", "Far").AddAntecedent("TargetDist", "VeryNear"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Slow").AddAntecedent("FrontDist", "Far").AddAntecedent("TargetDist", "Near"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Mid").AddAntecedent("FrontDist", "Far").AddAntecedent("TargetDist", "Mid"));
        speedFisSO.Rules.Add(CreateFuzzyRuleDef("Fast").AddAntecedent("FrontDist", "Far").AddAntecedent("TargetDist", "Far"));

        SaveAsset(speedFisSO, "SpeedFIS.asset");
    }

    private static void CreateDefaultTurnFIS()
    {
        FuzzyInferenceSystemSO turnFisSO = ScriptableObject.CreateInstance<FuzzyInferenceSystemSO>();
        turnFisSO.name = "TurnFIS";

        // --- Input Variables ---
        float maxSensor = 5f; // Assuming MaxSensorDist is 5

        // LeftDist
        var leftDistVar = new FuzzyVariableDef { Name = "LeftDist", Min = 0f, Max = maxSensor };
        leftDistVar.Sets.Add(CreateFuzzySetDef("Near", 0f, 0f, 0.1f, 0.3f));
        leftDistVar.Sets.Add(CreateFuzzySetDef("Mid", 0.1f, 0.3f, 0.5f, 0.7f));
        leftDistVar.Sets.Add(CreateFuzzySetDef("Far", 0.5f, 0.7f, 1f, 1f));
        turnFisSO.InputVariables.Add(leftDistVar);

        // RightDist
        var rightDistVar = new FuzzyVariableDef { Name = "RightDist", Min = 0f, Max = maxSensor };
        rightDistVar.Sets.Add(CreateFuzzySetDef("Near", 0f, 0f, 0.1f, 0.3f));
        rightDistVar.Sets.Add(CreateFuzzySetDef("Mid", 0.1f, 0.3f, 0.5f, 0.7f));
        rightDistVar.Sets.Add(CreateFuzzySetDef("Far", 0.5f, 0.7f, 1f, 1f));
        turnFisSO.InputVariables.Add(rightDistVar);

        // FrontDist
        var frontDistVar = new FuzzyVariableDef { Name = "FrontDist", Min = 0f, Max = maxSensor };
        frontDistVar.Sets.Add(CreateFuzzySetDef("Near", 0f, 0f, 0.1f, 0.3f));
        frontDistVar.Sets.Add(CreateFuzzySetDef("Mid", 0.1f, 0.3f, 0.5f, 0.7f));
        frontDistVar.Sets.Add(CreateFuzzySetDef("Far", 0.5f, 0.7f, 1f, 1f));
        turnFisSO.InputVariables.Add(frontDistVar);

        // Angle
        var angleVar = new FuzzyVariableDef { Name = "Angle", Min = -180f, Max = 180f };
        angleVar.Sets.Add(CreateFuzzySetDef("LeftLarge", 0f, 0f, 135f / 360f, 150f / 360f)); // Normalized from -180 to 180
        angleVar.Sets.Add(CreateFuzzySetDef("LeftSmall", 135f / 360f, 150f / 360f, 170f / 360f, 180f / 360f));
        angleVar.Sets.Add(CreateFuzzySetDef("Center", 170f / 360f, 180f / 360f, 180f / 360f, 190f / 360f));
        angleVar.Sets.Add(CreateFuzzySetDef("RightSmall", 180f / 360f, 190f / 360f, 210f / 360f, 225f / 360f));
        angleVar.Sets.Add(CreateFuzzySetDef("RightLarge", 210f / 360f, 225f / 360f, 1f, 1f));
        turnFisSO.InputVariables.Add(angleVar);

        // --- Output Variable ---
        turnFisSO.OutputVariable = new FuzzyVariableDef { Name = "Steer", Min = -1f, Max = 1f };
        turnFisSO.OutputVariable.Sets.Add(CreateFuzzySetDef("LL", 0f, 0f, 0.15f, 0.25f));
        turnFisSO.OutputVariable.Sets.Add(CreateFuzzySetDef("L", 0.15f, 0.25f, 0.35f, 0.45f));
        turnFisSO.OutputVariable.Sets.Add(CreateFuzzySetDef("F", 0.35f, 0.45f, 0.55f, 0.65f));
        turnFisSO.OutputVariable.Sets.Add(CreateFuzzySetDef("R", 0.55f, 0.65f, 0.75f, 0.85f));
        turnFisSO.OutputVariable.Sets.Add(CreateFuzzySetDef("RR", 0.75f, 0.85f, 1f, 1f));

        // --- Rules ---
        // Angle rules
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR", 0.2f).AddAntecedent("Angle", "RightLarge"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("R", 0.2f).AddAntecedent("Angle", "RightSmall"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("LL", 0.2f).AddAntecedent("Angle", "LeftLarge"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("L", 0.2f).AddAntecedent("Angle", "LeftSmall"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("F", 0.2f).AddAntecedent("Angle", "Center"));

        // Obstacle avoidance rules
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Near"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Mid"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Far"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Near"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Mid"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Far"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("F").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Near"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Mid"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Far"));

        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Near"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Mid"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("LL").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Far"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Near"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Mid"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("LL").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Far"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Near"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("F").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Mid"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("LL", 0.5f).AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Far"));

        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR").AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Near"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR").AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Mid"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR").AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Far"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR").AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Near"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR").AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Mid"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR", 0.5f).AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Far"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR").AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Near"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("RR", 0.5f).AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Mid"));
        turnFisSO.Rules.Add(CreateFuzzyRuleDef("F", 0.5f).AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Far"));

        SaveAsset(turnFisSO, "TurnFIS.asset");
    }

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

