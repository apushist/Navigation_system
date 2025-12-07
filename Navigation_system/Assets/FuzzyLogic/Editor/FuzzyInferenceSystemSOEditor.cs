using UnityEngine;
using UnityEditor;
using FuzzyLogic.Data;
using System.Linq;

namespace FuzzyLogic.EditorCode
{
    [CustomEditor(typeof(FuzzyInferenceSystemSO))]
    public class FuzzyInferenceSystemSOEditor : Editor
    {
        private SerializedProperty inputVariablesProp;
        private SerializedProperty outputVariableProp;
        private SerializedProperty rulesProp;

        private void OnEnable()
        {
            inputVariablesProp = serializedObject.FindProperty("InputVariables");
            outputVariableProp = serializedObject.FindProperty("OutputVariable");
            rulesProp = serializedObject.FindProperty("Rules");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Fuzzy Inference System", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // --- INPUT VARIABLES ---
            EditorGUILayout.PropertyField(inputVariablesProp, new GUIContent("Input Variables"), true);
            
            // --- OUTPUT VARIABLE ---
            EditorGUILayout.PropertyField(outputVariableProp, new GUIContent("Output Variable"), true);

            // --- RULES ---
            DrawRulesEditor();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRulesEditor()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rules", EditorStyles.boldLabel);

            FuzzyInferenceSystemSO system = (FuzzyInferenceSystemSO)target;

            string[] inputVarNames = system.InputVariables.Select(v => v.Name).ToArray();
            string[] outputSetNames = system.OutputVariable.Sets.Select(s => s.Name).ToArray();

            for (int i = 0; i < rulesProp.arraySize; i++)
            {
                SerializedProperty ruleProp = rulesProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Rule #{i + 1}", EditorStyles.boldLabel);
                if (GUILayout.Button("Remove Rule", GUILayout.Width(100)))
                {
                    rulesProp.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }
                EditorGUILayout.EndHorizontal();

                // Consequent
                SerializedProperty consequentProp = ruleProp.FindPropertyRelative("ConsequentSetName");
                int currentConsequentIndex = System.Array.IndexOf(outputSetNames, consequentProp.stringValue);
                int newConsequentIndex = EditorGUILayout.Popup("THEN Output is", currentConsequentIndex, outputSetNames);
                if (newConsequentIndex >= 0 && newConsequentIndex < outputSetNames.Length)
                {
                    consequentProp.stringValue = outputSetNames[newConsequentIndex];
                }

                // Weight
                SerializedProperty weightProp = ruleProp.FindPropertyRelative("Weight");
                EditorGUILayout.PropertyField(weightProp);
                
                // Antecedents
                EditorGUILayout.LabelField("IF", EditorStyles.boldLabel);
                SerializedProperty antecedentsProp = ruleProp.FindPropertyRelative("Antecedents");

                for (int j = 0; j < antecedentsProp.arraySize; j++)
                {
                    SerializedProperty antecedentProp = antecedentsProp.GetArrayElementAtIndex(j);
                    EditorGUILayout.BeginHorizontal();

                    SerializedProperty varNameProp = antecedentProp.FindPropertyRelative("VariableName");
                    SerializedProperty setNameProp = antecedentProp.FindPropertyRelative("SetName");

                    int currentVarIndex = System.Array.IndexOf(inputVarNames, varNameProp.stringValue);
                    int newVarIndex = EditorGUILayout.Popup(currentVarIndex, inputVarNames);
                    
                    if (newVarIndex >= 0 && newVarIndex < inputVarNames.Length)
                    {
                        varNameProp.stringValue = inputVarNames[newVarIndex];
                        
                        string[] currentSetNames = system.InputVariables[newVarIndex].Sets.Select(s => s.Name).ToArray();
                        int currentSetIndex = System.Array.IndexOf(currentSetNames, setNameProp.stringValue);
                        int newSetIndex = EditorGUILayout.Popup("is", currentSetIndex, currentSetNames);
                        if (newSetIndex >= 0 && newSetIndex < currentSetNames.Length)
                        {
                            setNameProp.stringValue = currentSetNames[newSetIndex];
                        }
                    } else {
                         EditorGUILayout.Popup("is", -1, new string[0]);
                    }


                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        antecedentsProp.DeleteArrayElementAtIndex(j);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Condition (AND)"))
                {
                    antecedentsProp.InsertArrayElementAtIndex(antecedentsProp.arraySize);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Add New Rule"))
            {
                rulesProp.InsertArrayElementAtIndex(rulesProp.arraySize);
            }
        }
    }
}
