using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TerrainLayerData))]
public class TerrainLayerDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.indentLevel = 0;

        Rect foldPos = new(position.x, position.y, position.width, 18);
        property.isExpanded = EditorGUI.Foldout(foldPos, property.isExpanded, property.FindPropertyRelative("terrainLayerName").stringValue, true);
        if (property.isExpanded)
        {
            string[] properties = new string[] { "HeightPosition", "BlendPercent" };
            string[] propertyLabels = new string[] { "Height position", "Blend percent" };

            Rect[] propertyPos = new Rect[properties.Length];
            float xPos = 60;
            float yPos = EditorGUIUtility.singleLineHeight + 2;
            float width = EditorGUIUtility.currentViewWidth - 70;
            float height = 18;

            propertyPos[0] = new(xPos, position.y + yPos, width, height);
            propertyPos[1] = new(xPos, position.y + (yPos * 2), width, height);

            int arrayIndex = property.FindPropertyRelative("terrainLayerArrayIndex").intValue;
            int arrayLength = property.FindPropertyRelative("terrainLayerArrayLength").intValue;

            for (int i = 0; i < properties.Length; i++)
            {
                if (arrayIndex + 1 == arrayLength && i + 1 == properties.Length) break;
                EditorGUI.PropertyField(propertyPos[i], property.FindPropertyRelative(properties[i]), new GUIContent(propertyLabels[i]));
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
        {
            int arrayIndex = property.FindPropertyRelative("terrainLayerArrayIndex").intValue;
            int arrayLength = property.FindPropertyRelative("terrainLayerArrayLength").intValue;

            int yPosOffset = 5;
            int lineCount = arrayIndex + 1 == arrayLength ? 2 : 3;
            return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * lineCount + yPosOffset;
        }
        else return base.GetPropertyHeight(property, label);
    }
}
