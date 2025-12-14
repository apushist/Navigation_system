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
        private SerializedProperty ruleGroupsProp; 

        private void OnEnable()
        {
            inputVariablesProp = serializedObject.FindProperty("InputVariables");
            outputVariableProp = serializedObject.FindProperty("OutputVariable");
            ruleGroupsProp = serializedObject.FindProperty("RuleGroups"); 
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

            // --- RULES GROUPS ---
            DrawRuleGroupsEditor();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRuleGroupsEditor()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rule Groups", EditorStyles.boldLabel);

            FuzzyInferenceSystemSO system = (FuzzyInferenceSystemSO)target;
            
            string[] inputVarNames = system.InputVariables.Select(v => v.Name).ToArray();
            string[] outputSetNames = system.OutputVariable.Sets.Select(s => s.Name).ToArray();

            for (int g = 0; g < ruleGroupsProp.arraySize; g++)
            {
                SerializedProperty groupProp = ruleGroupsProp.GetArrayElementAtIndex(g);
                SerializedProperty groupNameProp = groupProp.FindPropertyRelative("GroupName");
                SerializedProperty groupRulesProp = groupProp.FindPropertyRelative("Rules");

                EditorGUILayout.BeginVertical(EditorStyles.helpBox); 

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Group:", GUILayout.Width(50));
                EditorGUILayout.PropertyField(groupNameProp, GUIContent.none); 
                if (GUILayout.Button("Remove Group", GUILayout.Width(100)))
                {
                    ruleGroupsProp.DeleteArrayElementAtIndex(g);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue; 
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                for (int i = 0; i < groupRulesProp.arraySize; i++)
                {
                    SerializedProperty ruleProp = groupRulesProp.GetArrayElementAtIndex(i);
					
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20); 
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox); 

                    EditorGUILayout.BeginHorizontal();
					
                    SerializedProperty ruleNameProp = ruleProp.FindPropertyRelative("Name");
                    EditorGUILayout.LabelField("Rule Name:", GUILayout.Width(70));
                    EditorGUILayout.PropertyField(ruleNameProp, GUIContent.none);
                    
                    if (GUILayout.Button("X", GUILayout.Width(25))) 
                    {
                        groupRulesProp.DeleteArrayElementAtIndex(i);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        continue;
                    }
                    EditorGUILayout.EndHorizontal();

                    // --- THEN (Consequent) ---
                    SerializedProperty consequentProp = ruleProp.FindPropertyRelative("ConsequentSetName");
                    int currentConsequentIndex = System.Array.IndexOf(outputSetNames, consequentProp.stringValue);
                    
                    int newConsequentIndex = EditorGUILayout.Popup("THEN Output is", currentConsequentIndex, outputSetNames);
                    if (newConsequentIndex >= 0 && newConsequentIndex < outputSetNames.Length)
                    {
                        consequentProp.stringValue = outputSetNames[newConsequentIndex];
                    }
                    else if (outputSetNames.Length > 0 && currentConsequentIndex == -1)
                    {
                        consequentProp.stringValue = outputSetNames[0]; 
                    }

                    // --- Weight ---
                    SerializedProperty weightProp = ruleProp.FindPropertyRelative("Weight");
                    EditorGUILayout.PropertyField(weightProp);
                    
                    // --- IF (Antecedents) ---
                    EditorGUILayout.LabelField("IF Conditions (AND):", EditorStyles.boldLabel);
                    SerializedProperty antecedentsProp = ruleProp.FindPropertyRelative("Antecedents");

                    for (int j = 0; j < antecedentsProp.arraySize; j++)
                    {
                        SerializedProperty antecedentProp = antecedentsProp.GetArrayElementAtIndex(j);
                        EditorGUILayout.BeginHorizontal();

                        SerializedProperty varNameProp = antecedentProp.FindPropertyRelative("VariableName");
                        SerializedProperty setNameProp = antecedentProp.FindPropertyRelative("SetName");

                        int currentVarIndex = System.Array.IndexOf(inputVarNames, varNameProp.stringValue);
                        int newVarIndex = EditorGUILayout.Popup(currentVarIndex, inputVarNames, GUILayout.Width(100));
                        
                        if (newVarIndex >= 0 && newVarIndex < inputVarNames.Length)
                        {
                            varNameProp.stringValue = inputVarNames[newVarIndex];
                            
                            var variableDef = system.InputVariables[newVarIndex];
                            string[] currentSetNames = variableDef.Sets.Select(s => s.Name).ToArray();
                            
                            int currentSetIndex = System.Array.IndexOf(currentSetNames, setNameProp.stringValue);
                            
                            EditorGUILayout.LabelField("is", GUILayout.Width(20));

                            int newSetIndex = EditorGUILayout.Popup(currentSetIndex, currentSetNames);
                            if (newSetIndex >= 0 && newSetIndex < currentSetNames.Length)
                            {
                                setNameProp.stringValue = currentSetNames[newSetIndex];
                            }
                        } 
                        else 
                        {
                             EditorGUILayout.LabelField("Invalid Var");
                        }

                        if (GUILayout.Button("-", GUILayout.Width(25)))
                        {
                            antecedentsProp.DeleteArrayElementAtIndex(j);
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (GUILayout.Button("+ Condition"))
                    {
                        antecedentsProp.InsertArrayElementAtIndex(antecedentsProp.arraySize);
                    }

                    EditorGUILayout.EndVertical(); 
                    EditorGUILayout.EndHorizontal(); 
                    EditorGUILayout.Space(5);
                }

                // Кнопка добавления правила ВНУТРИ группы
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                if (GUILayout.Button("Add Rule to Group"))
                {
                    groupRulesProp.InsertArrayElementAtIndex(groupRulesProp.arraySize);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical(); 
                EditorGUILayout.Space(10);
            }

            if (GUILayout.Button("Add New Rule Group", GUILayout.Height(30)))
            {
                ruleGroupsProp.InsertArrayElementAtIndex(ruleGroupsProp.arraySize);
            }
        }
    }
}