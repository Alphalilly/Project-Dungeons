using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    public Joystick joystick;

    private void Start()
    {
        //joystick = GameObject.FindWithTag("Joystick").GetComponent<FixedJoystick>();
    }
    private void OnGUI()
    {
        GUIStyle header = new GUIStyle();
        GUIStyle normal = new GUIStyle();

        header.normal.textColor = Color.cyan;
        normal.normal.textColor = Color.cyan;

        header.fontSize = 20;
        normal.fontSize = 12;

        header.fontSize = 20;
        normal.fontSize = 12;

        float x = 10;
        float y = 5;
        float width = 400;
        float height = 40;

        float margin = 1;
        float yLocation = y;

        GUI.Label(new Rect(x, yLocation, width, height), "DEBUGCONSOLE", header);
        yLocation += height + margin;
        //GUI.Label(new Rect(x, yLocation, width, height), "FPS" + 1.0f / Time.deltaTime, normal);
        //yLocation += height + margin;
        //GUI.Label(new Rect(x, yLocation, width, height), "Date/Time: " + System.DateTime.Now, normal);
        //yLocation += height + margin;
        GUI.Label(new Rect(x, yLocation, width, height), "JoystickHorizontal " + joystick.Horizontal + " JoystickVertical " + joystick.Vertical, normal);
    }
}
