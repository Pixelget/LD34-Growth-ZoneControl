using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {
    public float ControlAmount = 0f;
    public Color owner = new Color(0.8863f,0.8745f,0.6353f);
    public float startHeight = 0f;
    public int owner_id = 0;
    float heightScale = -0.5f;
    Material mat;

    bool clickable = true;

    float delay = 0f;

    void Start() {
        mat = GetComponent<MeshRenderer>().material;
    }

    void Update() {
        MoveTowardsHeight();
        AdjustColor();
    }

    public bool CanControl(float amount) {
        if (ControlAmount <= amount)
            return true;

        return false;
    }

    public bool NotControlled() {
        if (ControlAmount == 0f)
            return true;

        return false;
    }

	public void SetController(Color c, float amount, int id, float delay) {
        ControlAmount = amount;

        // Set the color and adjust height
        owner = c;
        owner_id = id;

        this.delay = delay;
    }

    public void Setup(float height, bool clickable) {
        startHeight = height;
        this.clickable = clickable;

        if (!clickable) {
            ControlAmount = 100f;
        }
    }

    void MoveTowardsHeight() {
        delay -= Time.deltaTime;

        if (delay < 0f) {
            if (transform.position.y != startHeight) {
                // Move towards the correct height
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, (startHeight + (ControlAmount * heightScale)), 0.01f), transform.position.z);
            }
            delay = 0f;
        }
    }

    void AdjustColor() {
        Vector3 color = Vector3.Lerp(new Vector3(mat.color.r, mat.color.g, mat.color.b), new Vector3(owner.r, owner.g, owner.b), 0.05f);
        mat.color = new Color(color.x, color.y, color.z);
    }
}
