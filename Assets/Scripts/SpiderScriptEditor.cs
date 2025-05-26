using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpiderScript))]
public class SpiderScriptEditor : Editor {
    public override void OnInspectorGUI() {
        SpiderScript spiderScript = (SpiderScript)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate Legs")) {
            spiderScript.GenerateLegs();
        }
    }
}
