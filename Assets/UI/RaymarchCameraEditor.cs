#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;


[CustomEditor(typeof(RaymarchCamera))]
public class RaymarchCameraEditor : Editor
{
    public VisualTreeAsset m_InspectorXML;
    public override VisualElement CreateInspectorGUI()
    { 
        // Create a new VisualElement to be the root of our Inspector UI.
        VisualElement myInspector = new VisualElement();

        // Add a simple label.
        myInspector.Add(new Label("This is a custom Inspector"));

        // Load the UXML file.
        m_InspectorXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/RaymarchingCameraUI.uxml");

        // Instantiate the UXML.
        myInspector = m_InspectorXML.Instantiate();

        // Return the finished Inspector UI.
        return myInspector;
    }
}
#endif