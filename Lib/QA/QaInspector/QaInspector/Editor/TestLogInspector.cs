//  아래꺼 참고해서 잘쓰면됨
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(TestLog))]
public class TestLogInspector : Editor
{
    private int selectedTab = 0;
    private string[] tabs = { "General", "Event" };



    private int evet_selectedTab2 = 0;
    private string[] evet_tabs2 = { "Evnet_1", "Event_2" };
    public override void OnInspectorGUI()
    {

        selectedTab = GUILayout.Toolbar(selectedTab, tabs);

        EditorGUILayout.Space();
        var testLog = target as TestLog;

        var style = new GUIStyle();
        style.fontSize = 22;
        style.normal.textColor = new Color(67f / 255f, 160f / 255f, 250f / 255f, 1.0f);
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;




        switch (selectedTab)
        {
            case 0:
                GUILayout.Label("---------- General ----------", style);
                GUILayout.Label("---------- Resources----------", style);
                SerializedProperty GoldValue = serializedObject.FindProperty("GoldValue");
                testLog.GoldValue = EditorGUILayout.IntField("GoldValue", GoldValue.intValue);
                if (GUILayout.Button("AddGold"))
                {
                    //gold+=testlog.GoldValue;

                }
                SerializedProperty DiaValue = serializedObject.FindProperty("DiaValue");
                testLog.DiaValue = EditorGUILayout.IntField("DiaValue", DiaValue.intValue);
                if (GUILayout.Button("AddDia"))
                {
                    //dia+=testlog.DiaValue;
                }
                GUILayout.Label("---------- Enemy----------", style);
                if (GUILayout.Button("AllDie"))
                {
                    //dia+=testlog.DiaValue;
                }
                GUILayout.Label("---------- Stage----------", style);
                if (GUILayout.Button("StageClear"))
                {
                    //dia+=testlog.DiaValue;
                }
                EditorGUILayout.Space();


                break;
            case 1:
                evet_selectedTab2 = GUILayout.Toolbar(evet_selectedTab2, evet_tabs2);
                switch (evet_selectedTab2)
                {
                    case 0:
                        GUILayout.Label("---------- Event_1----------", style);
                        if (GUILayout.Button("Trigger_1"))
                        {

                        }
                        if (GUILayout.Button("Trigger_2"))
                        {

                        }

                        break;
                    case 1:
                        GUILayout.Label("---------- Event_ 2----------", style);
                        SerializedProperty Trigger_1Value = serializedObject.FindProperty("Trigger_1Value");
                        testLog.Trigger_1Value = EditorGUILayout.IntField("Trigger_1Value", Trigger_1Value.intValue);
                        if (GUILayout.Button("Trigger_1"))
                        {
                            
                        }
                        if (GUILayout.Button("Trigger_2"))
                        {

                        }


                        break;

                        
                }


                break;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(testLog);
        }
    }
}
