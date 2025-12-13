using UnityEngine;
using UnityEditor;
using FuzzyLogic;
using FuzzyLogic.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

// ===== WINDOW CLASS =====
public class FuzzySystemEditorWindow : EditorWindow
{
	public FuzzyInferenceSystemSO currentSystem;
	private Vector2 scrollPosition;
	private bool showInputVariables = true;
	private bool showOutputVariable = true;
	private List<bool> showRuleGroups = new List<bool>();

	[MenuItem("Window/Fuzzy Logic/Fuzzy System Editor")]
	public static void ShowWindow()
	{
		GetWindow<FuzzySystemEditorWindow>("Fuzzy System Editor");
	}

	private void OnGUI()
	{
		EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

		// Header
		EditorGUILayout.Space(10);
		EditorGUILayout.LabelField("Fuzzy Inference System Creator", EditorStyles.boldLabel);
		EditorGUILayout.Separator();

		// Create new or load existing
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("New System", GUILayout.Width(100)))
		{
			CreateNewSystem();
		}

		if (GUILayout.Button("Load System", GUILayout.Width(100)))
		{
			LoadSystem();
		}

		if (currentSystem != null && GUILayout.Button("Save", GUILayout.Width(100)))
		{
			SaveSystem();
		}
		EditorGUILayout.EndHorizontal();

		if (currentSystem == null)
		{
			EditorGUILayout.HelpBox("Create a new system or load an existing one.", MessageType.Info);
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			return;
		}

		EditorGUILayout.Space(20);
		EditorGUILayout.LabelField("System: " + currentSystem.name, EditorStyles.boldLabel);

		// Input Variables
		showInputVariables = EditorGUILayout.Foldout(showInputVariables, "Input Variables", true);
		if (showInputVariables)
		{
			EditorGUI.indentLevel++;
			for (int i = 0; i < currentSystem.InputVariables.Count; i++)
			{
				DrawVariableEditor(currentSystem.InputVariables[i], i, true);
			}

			if (GUILayout.Button("+ Add Input Variable", GUILayout.Width(150)))
			{
				currentSystem.InputVariables.Add(new FuzzyVariableDef
				{
					Name = $"Input_{currentSystem.InputVariables.Count + 1}",
					Min = 0,
					Max = 1,
					Sets = new List<FuzzySetDef>
					{
						CreateDefaultFuzzySet("Low"),
						CreateDefaultFuzzySet("Medium"),
						CreateDefaultFuzzySet("High")
					}
				});
			}
			EditorGUI.indentLevel--;
		}

		EditorGUILayout.Space(10);

		// Output Variable
		showOutputVariable = EditorGUILayout.Foldout(showOutputVariable, "Output Variable", true);
		if (showOutputVariable)
		{
			EditorGUI.indentLevel++;
			DrawVariableEditor(currentSystem.OutputVariable, -1, false);
			EditorGUI.indentLevel--;
		}

		EditorGUILayout.Space(10);

		// Rule Groups
		EditorGUILayout.LabelField("Rule Groups", EditorStyles.boldLabel);

		while (showRuleGroups.Count < currentSystem.RuleGroups.Count)
		{
			showRuleGroups.Add(true);
		}

		for (int i = 0; i < currentSystem.RuleGroups.Count; i++)
		{
			var group = currentSystem.RuleGroups[i];
			showRuleGroups[i] = EditorGUILayout.Foldout(showRuleGroups[i], group.GroupName, true);

			if (showRuleGroups[i])
			{
				EditorGUI.indentLevel++;

				EditorGUILayout.BeginHorizontal();
				group.GroupName = EditorGUILayout.TextField("Group Name:", group.GroupName);
				if (GUILayout.Button("Remove", GUILayout.Width(80)))
				{
					currentSystem.RuleGroups.RemoveAt(i);
					showRuleGroups.RemoveAt(i);
					EditorGUI.indentLevel--;
					continue;
				}
				EditorGUILayout.EndHorizontal();

				// Rules in group
				for (int j = 0; j < group.Rules.Count; j++)
				{
					DrawRuleEditor(group.Rules[j], j, group);
				}

				if (GUILayout.Button("+ Add Rule to Group", GUILayout.Width(150)))
				{
					group.Rules.Add(CreateDefaultRule());
				}

				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Space(5);
		}

		if (GUILayout.Button("+ Add Rule Group", GUILayout.Width(150)))
		{
			currentSystem.RuleGroups.Add(new FuzzyRuleGroupDef
			{
				GroupName = $"Group_{currentSystem.RuleGroups.Count + 1}",
				Rules = new List<FuzzyRuleDef>()
			});
		}

		EditorGUILayout.Space(20);

		// Preview and Create Buttons
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Preview Rules", GUILayout.Height(30)))
		{
			PreviewRules();
		}

		if (GUILayout.Button("Create Asset", GUILayout.Height(30)))
		{
			CreateAsset();
		}

		if (GUILayout.Button("Save Asset", GUILayout.Height(30)))
		{
			SaveSystem();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
	}

	private void DrawVariableEditor(FuzzyVariableDef variable, int index, bool isInput)
	{
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);

		EditorGUILayout.BeginHorizontal();
		variable.Name = EditorGUILayout.TextField("Name:", variable.Name);
		if (isInput && GUILayout.Button("Remove", GUILayout.Width(80)))
		{
			currentSystem.InputVariables.RemoveAt(index);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			return;
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		variable.Min = EditorGUILayout.FloatField("Min:", variable.Min);
		variable.Max = EditorGUILayout.FloatField("Max:", variable.Max);
		EditorGUILayout.EndHorizontal();

		// Fuzzy Sets
		EditorGUILayout.LabelField("Fuzzy Sets:", EditorStyles.boldLabel);
		for (int i = 0; i < variable.Sets.Count; i++)
		{
			var set = variable.Sets[i];
			EditorGUILayout.BeginHorizontal();

			set.Name = EditorGUILayout.TextField("Set Name:", set.Name);

			if (GUILayout.Button("Edit Curve", GUILayout.Width(80)))
			{
				AnimationCurveEditorWindow.ShowWindow(set);
			}

			if (GUILayout.Button("X", GUILayout.Width(25)))
			{
				variable.Sets.RemoveAt(i);
				EditorGUILayout.EndHorizontal();
				continue;
			}

			EditorGUILayout.EndHorizontal();
		}

		if (GUILayout.Button("+ Add Fuzzy Set", GUILayout.Width(120)))
		{
			variable.Sets.Add(new FuzzySetDef
			{
				Name = $"Set_{variable.Sets.Count + 1}",
				Curve = CreateDefaultCurve()
			});
		}

		EditorGUILayout.EndVertical();
		EditorGUILayout.Space(5);
	}

	private void DrawRuleEditor(FuzzyRuleDef rule, int index, FuzzyRuleGroupDef group)
	{
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);

		EditorGUILayout.BeginHorizontal();
		rule.Name = EditorGUILayout.TextField("Rule Name:", rule.Name);
		if (GUILayout.Button("X", GUILayout.Width(25)))
		{
			group.Rules.RemoveAt(index);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			return;
		}
		EditorGUILayout.EndHorizontal();

		// Weight
		rule.Weight = EditorGUILayout.Slider("Weight:", rule.Weight, 0, 1);

		// Antecedents (IF conditions)
		EditorGUILayout.LabelField("IF (AND):", EditorStyles.boldLabel);
		for (int i = 0; i < rule.Antecedents.Count; i++)
		{
			var antecedent = rule.Antecedents[i];
			EditorGUILayout.BeginHorizontal();

			// Variable dropdown
			string[] varNames = currentSystem.InputVariables.Select(v => v.Name).ToArray();
			int varIndex = System.Array.IndexOf(varNames, antecedent.VariableName);
			if (varIndex < 0) varIndex = 0;

			varIndex = EditorGUILayout.Popup(varIndex, varNames, GUILayout.Width(120));
			if (varIndex >= 0 && varIndex < varNames.Length)
			{
				antecedent.VariableName = varNames[varIndex];

				// Set dropdown
				var variable = currentSystem.InputVariables[varIndex];
				string[] setNames = variable.Sets.Select(s => s.Name).ToArray();
				int setIndex = System.Array.IndexOf(setNames, antecedent.SetName);
				if (setIndex < 0) setIndex = 0;

				EditorGUILayout.LabelField("is", GUILayout.Width(20));
				setIndex = EditorGUILayout.Popup(setIndex, setNames, GUILayout.Width(100));
				if (setIndex >= 0 && setIndex < setNames.Length)
				{
					antecedent.SetName = setNames[setIndex];
				}
			}

			if (GUILayout.Button("-", GUILayout.Width(25)))
			{
				rule.Antecedents.RemoveAt(i);
				EditorGUILayout.EndHorizontal();
				continue;
			}

			EditorGUILayout.EndHorizontal();
		}

		if (GUILayout.Button("+ Add Condition", GUILayout.Width(120)))
		{
			if (currentSystem.InputVariables.Count > 0)
			{
				var firstVar = currentSystem.InputVariables[0];
				rule.Antecedents.Add(new FuzzyAntecedentDef
				{
					VariableName = firstVar.Name,
					SetName = firstVar.Sets.Count > 0 ? firstVar.Sets[0].Name : "Set"
				});
			}
		}

		// Consequent (THEN)
		EditorGUILayout.LabelField("THEN Output is:", EditorStyles.boldLabel);
		string[] outputSetNames = currentSystem.OutputVariable.Sets.Select(s => s.Name).ToArray();
		int consequentIndex = System.Array.IndexOf(outputSetNames, rule.ConsequentSetName);
		if (consequentIndex < 0) consequentIndex = 0;

		consequentIndex = EditorGUILayout.Popup(consequentIndex, outputSetNames);
		if (consequentIndex >= 0 && consequentIndex < outputSetNames.Length)
		{
			rule.ConsequentSetName = outputSetNames[consequentIndex];
		}

		EditorGUILayout.EndVertical();
		EditorGUILayout.Space(5);
	}

	private void CreateNewSystem()
	{
		currentSystem = ScriptableObject.CreateInstance<FuzzyInferenceSystemSO>();
		currentSystem.name = "NewFuzzySystem";
		currentSystem.InputVariables = new List<FuzzyVariableDef>
		{
			CreateDefaultInputVariable("Distance", 0, 10),
			CreateDefaultInputVariable("Angle", -180, 180)
		};

		currentSystem.OutputVariable = new FuzzyVariableDef
		{
			Name = "Output",
			Min = -1,
			Max = 1,
			Sets = new List<FuzzySetDef>
			{
				CreateDefaultFuzzySet("Negative"),
				CreateDefaultFuzzySet("Zero"),
				CreateDefaultFuzzySet("Positive")
			}
		};

		currentSystem.RuleGroups = new List<FuzzyRuleGroupDef>();
		showRuleGroups.Clear();
	}

	private void LoadSystem()
	{
		string path = EditorUtility.OpenFilePanel("Select Fuzzy System", "Assets/", "asset");
		if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
		{
			string assetPath = "Assets" + path.Substring(Application.dataPath.Length);
			currentSystem = AssetDatabase.LoadAssetAtPath<FuzzyInferenceSystemSO>(assetPath);
			if (currentSystem != null)
			{
				showRuleGroups = new List<bool>(currentSystem.RuleGroups.Count);
				for (int i = 0; i < currentSystem.RuleGroups.Count; i++)
				{
					showRuleGroups.Add(true);
				}
			}
		}
	}

	private void SaveSystem()
	{
		if (currentSystem != null)
		{
			EditorUtility.SetDirty(currentSystem);
			AssetDatabase.SaveAssets();
			Debug.Log("Fuzzy system saved.");
		}
	}

	private void CreateAsset()
	{
		if (!AssetDatabase.IsValidFolder("Assets/FuzzyLogic/Data/"))
		{
			AssetDatabase.CreateFolder("Assets/FuzzyLogic", "Data");
		}

		string path = EditorUtility.SaveFilePanelInProject(
			"Save Fuzzy System",
			"NewFuzzySystem",
			"asset",
			"Save Fuzzy Inference System",
			"Assets/FuzzyLogic/Data/");

		if (!string.IsNullOrEmpty(path))
		{
			if (currentSystem == null)
			{
				CreateNewSystem();
			}

			AssetDatabase.CreateAsset(currentSystem, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			EditorUtility.FocusProjectWindow();
			Selection.activeObject = currentSystem;
		}
	}

	private void PreviewRules()
	{
		if (currentSystem == null) return;

		Debug.Log("=== FUZZY SYSTEM RULES PREVIEW ===");
		Debug.Log($"System: {currentSystem.name}");
		Debug.Log($"Input Variables: {currentSystem.InputVariables.Count}");
		Debug.Log($"Output Variable: {currentSystem.OutputVariable.Name}");
		Debug.Log($"Rule Groups: {currentSystem.RuleGroups.Count}");

		foreach (var group in currentSystem.RuleGroups)
		{
			Debug.Log($"\nGroup: {group.GroupName}");
			foreach (var rule in group.Rules)
			{
				string antecedents = string.Join(" AND ",
					rule.Antecedents.Select(a => $"{a.VariableName} is {a.SetName}"));
				Debug.Log($"  Rule: {rule.Name}");
				Debug.Log($"    IF {antecedents}");
				Debug.Log($"    THEN {currentSystem.OutputVariable.Name} is {rule.ConsequentSetName}");
				Debug.Log($"    Weight: {rule.Weight}");
			}
		}
	}

	private FuzzyVariableDef CreateDefaultInputVariable(string name, float min, float max)
	{
		return new FuzzyVariableDef
		{
			Name = name,
			Min = min,
			Max = max,
			Sets = new List<FuzzySetDef>
			{
				CreateDefaultFuzzySet("Low"),
				CreateDefaultFuzzySet("Medium"),
				CreateDefaultFuzzySet("High")
			}
		};
	}

	private FuzzySetDef CreateDefaultFuzzySet(string name)
	{
		return new FuzzySetDef
		{
			Name = name,
			Curve = CreateDefaultCurve()
		};
	}

	private AnimationCurve CreateDefaultCurve()
	{
		return new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(0.25f, 1f),
			new Keyframe(0.75f, 1f),
			new Keyframe(1f, 0f)
		});
	}

	private FuzzyRuleDef CreateDefaultRule()
	{
		return new FuzzyRuleDef
		{
			Name = "New Rule",
			Weight = 1f,
			Antecedents = new List<FuzzyAntecedentDef>(),
			ConsequentSetName = currentSystem.OutputVariable.Sets.Count > 0 ?
				currentSystem.OutputVariable.Sets[0].Name : "Set"
		};
	}
}

// ===== STATIC HELPER CLASS (ORIGINAL) =====
public static class FuzzySystemInitializer
{
	private static string assetsPath = "Assets/FuzzyLogic/Data/";

	[MenuItem("Assets/Create/Fuzzy Logic/Inference System")]
	public static void CreateNewFuzzySystem()
	{
		var system = ScriptableObject.CreateInstance<FuzzyInferenceSystemSO>();
		system.name = "NewFuzzySystem";

		// Create default structure
		system.InputVariables = new List<FuzzyVariableDef>
		{
			CreateDefaultInputVariable("Input1", 0, 10),
			CreateDefaultInputVariable("Input2", 0, 100)
		};

		system.OutputVariable = new FuzzyVariableDef
		{
			Name = "Output",
			Min = 0,
			Max = 1,
			Sets = new List<FuzzySetDef>
			{
				CreateDefaultFuzzySet("Low"),
				CreateDefaultFuzzySet("Medium"),
				CreateDefaultFuzzySet("High")
			}
		};

		// Save it
		string path = AssetDatabase.GenerateUniqueAssetPath(assetsPath + "NewFuzzySystem.asset");
		AssetDatabase.CreateAsset(system, path);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		// Open in editor
		var window = EditorWindow.GetWindow<FuzzySystemEditorWindow>("Fuzzy System Editor");
		window.currentSystem = system;
		window.Show();

		Selection.activeObject = system;
	}

	[MenuItem("Fuzzy Logic/Create Example/Turn System")]
	public static void CreateTurnSystem()
	{
		var system = ScriptableObject.CreateInstance<FuzzyInferenceSystemSO>();
		system.name = "TurnFIS";

		// Input: Left Distance
		var leftDist = new FuzzyVariableDef
		{
			Name = "LeftDist",
			Min = 0,
			Max = 10,
			Sets = new List<FuzzySetDef>
			{
				CreateFuzzySetDef("Near", 0f, 0f, 0.1f, 0.4f),
				CreateFuzzySetDef("Far", 0.6f, 0.9f, 1f, 1f)
			}
		};

		// Input: Right Distance
		var rightDist = new FuzzyVariableDef
		{
			Name = "RightDist",
			Min = 0,
			Max = 10,
			Sets = new List<FuzzySetDef>
			{
				CreateFuzzySetDef("Near", 0f, 0f, 0.1f, 0.4f),
				CreateFuzzySetDef("Far", 0.6f, 0.9f, 1f, 1f)
			}
		};

		// Input: Front Distance
		var frontDist = new FuzzyVariableDef
		{
			Name = "FrontDist",
			Min = 0,
			Max = 5,
			Sets = new List<FuzzySetDef>
			{
				CreateFuzzySetDef("Near", 0f, 0f, 0.1f, 0.3f),
				CreateFuzzySetDef("Mid", 0.1f, 0.3f, 0.5f, 0.7f),
				CreateFuzzySetDef("Far", 0.5f, 0.7f, 1f, 1f)
			}
		};

		// Input: Angle
		var angle = new FuzzyVariableDef
		{
			Name = "Angle",
			Min = -180,
			Max = 180,
			Sets = new List<FuzzySetDef>
			{
				CreateFuzzySetDef("Left", 0f, 0f, 0.3f, 0.5f),
				CreateFuzzySetDef("Right", 0.5f, 0.7f, 1f, 1f)
			}
		};

		// Output: Steer
		var steer = new FuzzyVariableDef
		{
			Name = "Steer",
			Min = -1,
			Max = 1,
			Sets = new List<FuzzySetDef>
			{
				CreateFuzzySetDef("L", 0f, 0f, 0.1f, 0.6f),
				CreateFuzzySetDef("R", 0.4f, 0.9f, 1f, 1f)
			}
		};

		system.InputVariables = new List<FuzzyVariableDef> { leftDist, rightDist, frontDist, angle };
		system.OutputVariable = steer;

		// Safety Rules
		var safetyGroup = new FuzzyRuleGroupDef
		{
			GroupName = "Safety",
			Rules = new List<FuzzyRuleDef>
			{
				new FuzzyRuleDef
				{
					Name = "Avoid Left",
					Antecedents = new List<FuzzyAntecedentDef>
					{
						new FuzzyAntecedentDef { VariableName = "LeftDist", SetName = "Near" }
					},
					ConsequentSetName = "R",
					Weight = 1f
				},
				new FuzzyRuleDef
				{
					Name = "Avoid Right",
					Antecedents = new List<FuzzyAntecedentDef>
					{
						new FuzzyAntecedentDef { VariableName = "RightDist", SetName = "Near" }
					},
					ConsequentSetName = "L",
					Weight = 1f
				}
			}
		};

		// Navigation Rules
		var navigationGroup = new FuzzyRuleGroupDef
		{
			GroupName = "Navigation",
			Rules = new List<FuzzyRuleDef>
			{
				new FuzzyRuleDef
				{
					Name = "Seek Target Left",
					Antecedents = new List<FuzzyAntecedentDef>
					{
						new FuzzyAntecedentDef { VariableName = "Angle", SetName = "Left" }
					},
					ConsequentSetName = "L",
					Weight = 0.4f
				},
				new FuzzyRuleDef
				{
					Name = "Seek Target Right",
					Antecedents = new List<FuzzyAntecedentDef>
					{
						new FuzzyAntecedentDef { VariableName = "Angle", SetName = "Right" }
					},
					ConsequentSetName = "R",
					Weight = 0.4f
				}
			}
		};

		system.RuleGroups = new List<FuzzyRuleGroupDef> { safetyGroup, navigationGroup };

		SaveAsset(system, "TurnFIS.asset");
	}

	[MenuItem("Fuzzy Logic/Create Example/Speed System")]
	public static void CreateSpeedSystem()
	{
		var system = ScriptableObject.CreateInstance<FuzzyInferenceSystemSO>();
		system.name = "SpeedFIS";

		// Add your SpeedFIS implementation here...
		// (Similar structure to CreateTurnSystem but with speed-related variables)

		SaveAsset(system, "SpeedFIS.asset");
	}

	private static FuzzySetDef CreateFuzzySetDef(string name, float p1, float p2, float p3, float p4)
	{
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

	private static void SaveAsset(ScriptableObject so, string fileName)
	{
		if (!AssetDatabase.IsValidFolder(assetsPath.TrimEnd('/')))
		{
			AssetDatabase.CreateFolder("Assets/FuzzyLogic", "Data");
		}
		string path = AssetDatabase.GenerateUniqueAssetPath(assetsPath + fileName);
		AssetDatabase.CreateAsset(so, path);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Debug.Log($"Created fuzzy system: {path}");
	}

	private static FuzzyVariableDef CreateDefaultInputVariable(string name, float min, float max)
	{
		return new FuzzyVariableDef
		{
			Name = name,
			Min = min,
			Max = max,
			Sets = new List<FuzzySetDef>
			{
				CreateDefaultFuzzySet("Low"),
				CreateDefaultFuzzySet("Medium"),
				CreateDefaultFuzzySet("High")
			}
		};
	}

	private static FuzzySetDef CreateDefaultFuzzySet(string name)
	{
		return new FuzzySetDef
		{
			Name = name,
			Curve = CreateDefaultCurve()
		};
	}

	private static AnimationCurve CreateDefaultCurve()
	{
		return new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(0.25f, 1f),
			new Keyframe(0.75f, 1f),
			new Keyframe(1f, 0f)
		});
	}
}

// Helper window for editing curves
public class AnimationCurveEditorWindow : EditorWindow
{
	private FuzzySetDef targetSet;
	private AnimationCurve workingCurve; // Рабочая копия для редактирования
	private bool hasChanges = false;

	public static void ShowWindow(FuzzySetDef set)
	{
		var window = GetWindow<AnimationCurveEditorWindow>("Curve Editor");
		window.targetSet = set;
		window.workingCurve = new AnimationCurve(set.Curve.keys); // Создаем копию для редактирования
		window.hasChanges = false;
		window.minSize = new Vector2(400, 300);
	}

	private void OnGUI()
	{
		if (targetSet == null || workingCurve == null)
		{
			Close();
			return;
		}

		// Заголовок
		EditorGUILayout.LabelField($"Editing: {targetSet.Name}", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Set name: " + targetSet.Name, EditorStyles.helpBox);

		EditorGUILayout.Space(10);

		// Редактор кривой
		EditorGUI.BeginChangeCheck();
		workingCurve = EditorGUILayout.CurveField("Curve:", workingCurve, GUILayout.Height(200));
		if (EditorGUI.EndChangeCheck())
		{
			hasChanges = true;
		}

		// Информация о ключевых кадрах
		EditorGUILayout.Space(10);
		EditorGUILayout.LabelField("Keyframes:", EditorStyles.boldLabel);

		for (int i = 0; i < workingCurve.keys.Length; i++)
		{
			var key = workingCurve.keys[i];
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"Frame {i}:", GUILayout.Width(60));
			EditorGUILayout.LabelField($"Time: {key.time:F3}", GUILayout.Width(80));
			EditorGUILayout.LabelField($"Value: {key.value:F3}", GUILayout.Width(80));
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.Space(20);

		// Кнопки управления
		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Apply", GUILayout.Height(30)))
		{
			// Применяем изменения к оригиналу
			targetSet.Curve = new AnimationCurve(workingCurve.keys);
			hasChanges = false;
			Debug.Log($"Curve '{targetSet.Name}' updated");
		}

		if (GUILayout.Button("Reset", GUILayout.Height(30)))
		{
			// Сбрасываем к оригиналу
			workingCurve = new AnimationCurve(targetSet.Curve.keys);
			hasChanges = false;
		}

		if (GUILayout.Button("Set Default", GUILayout.Height(30)))
		{
			// Устанавливаем дефолтную кривую
			workingCurve = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0f, 0f),
				new Keyframe(0.25f, 1f),
				new Keyframe(0.75f, 1f),
				new Keyframe(1f, 0f)
			});
			hasChanges = true;
		}

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space(10);

		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Close", GUILayout.Height(25)))
		{
			if (hasChanges)
			{
				// Спрашиваем подтверждение при закрытии с несохраненными изменениями
				if (EditorUtility.DisplayDialog("Unsaved Changes",
					"You have unsaved changes to the curve. Close without saving?",
					"Close", "Cancel"))
				{
					Close();
				}
			}
			else
			{
				Close();
			}
		}

		// Индикатор изменений
		if (hasChanges)
		{
			EditorGUILayout.LabelField("● Unsaved changes",
				new GUIStyle(EditorStyles.label) { normal = { textColor = Color.yellow } });
		}

		EditorGUILayout.EndHorizontal();
	}

	private void OnDestroy()
	{
		// При закрытии окна спрашиваем о сохранении
		if (hasChanges && targetSet != null)
		{
			if (EditorUtility.DisplayDialog("Save Changes?",
				$"Save changes to curve '{targetSet.Name}'?",
				"Save", "Discard"))
			{
				targetSet.Curve = new AnimationCurve(workingCurve.keys);
			}
		}
	}
}