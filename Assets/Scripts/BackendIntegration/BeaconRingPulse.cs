using UnityEngine;

public class BeaconRingPulse : MonoBehaviour
{
    public float StartDelay;
    public float BaseScale = 5f;
    float startTime;
    Renderer rend;

    void Start()
    {
        startTime = Time.time + StartDelay;
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float t = ((Time.time - startTime) % 2f) / 2f;
        float scale = BaseScale + Mathf.Lerp(0f, 5f, t);
        transform.localScale = new Vector3(scale, 0.05f, scale);
        Color c = rend.material.color;
        c.a = 1f - t;

        rend.material.color = c;
    }
}