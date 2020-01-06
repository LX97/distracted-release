using UnityEngine;
using System.Collections;
using UnityEditor;

//[InitializeOnLoad]
public class ReplaceAgentPrefabMenu : Editor
{

	static ReplaceAgentPrefabMenu ()
    {
		SceneView.onSceneGUIDelegate += OnSceneGUI;
	}


	static void OnSceneGUI (SceneView sceneview)
    {
		
		if (Event.current.button == 1)
		{
			if (Event.current.type == EventType.MouseDown)
			{              
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Replace Agent/Normative Agent"), false, ReplaceAgent, 1);
				menu.AddItem(new GUIContent("Replace Agent/Distracted Agent"), false, ReplaceAgent, 2);
				menu.ShowAsContext();
			}
		}

	}

	static void ReplaceAgent (object obj)
    {
		GameObject selectedObj = Selection.activeGameObject;


		if (selectedObj.CompareTag("Agent")){
			Agent oldAgent = selectedObj.GetComponent<Agent> ();
			Transform oldTransform = selectedObj.transform;
			Transform parent = oldAgent.transform.parent;
			GameObject agent;
			if ((int)obj == 2)
				agent = (GameObject)PrefabUtility.InstantiatePrefab ((GameObject) AssetDatabase.LoadAssetAtPath<Object>("Assets/Prefabs/DistractedAgent.prefab") as GameObject);
			else //obj == 1
				agent = (GameObject)PrefabUtility.InstantiatePrefab ((GameObject) AssetDatabase.LoadAssetAtPath<Object>("Assets/Prefabs/NormativeAgent.prefab") as GameObject);
			
			agent.transform.position = oldTransform.position;
			agent.transform.rotation = oldTransform.rotation;
			agent.transform.localScale = oldTransform.localScale;
			if (parent != null)
				agent.transform.SetParent (parent);
			SetTargetOnLoad setTargetScript = agent.GetComponent<SetTargetOnLoad> ();
			SetTargetOnLoad oldTargetScript = selectedObj.GetComponent<SetTargetOnLoad> ();
			if (oldTargetScript.enabled == true) {
				Vector3 goalPosition = oldTargetScript.target;

				setTargetScript.enabled = true;
				setTargetScript.target = goalPosition;
			}

			DestroyImmediate(selectedObj);
		}
	}
}
