using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public float updateInterval = 0.5f;
    private float accum = 0;
    private int frames = 0;
    private float timeleft;
    private float fps;

    void Start()
    {
        timeleft = updateInterval;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        if (timeleft <= 0.0)
        {
            fps = accum / frames;
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 100, 20), "FPS: " + fps.ToString("F1"), style);
    }
}