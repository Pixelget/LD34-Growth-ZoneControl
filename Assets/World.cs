using UnityEngine;
using System.Collections;

public class World : MonoBehaviour {
    // public 
    public int unoccupiedTiles = 100;
    public int Player1Score = 0;
    public int Player2Score = 0;

    public GameObject tile;
    public int height = 15;
    public int width = 15;
    Tile[,] world;

    float heightOffset = -49.5f;
    float randomHeightOffset = -0.2f;
    int range = 2;

    // Camera
    public GameObject CameraRotationPoint;
    float rotateSpeed = 15f;

    // Input
    Vector3 LastFramePosition;
    float lastScreenX;
    Vector3 CurrentFramePosition;
    float currentScreenX;
    Vector3 Difference;
    Vector3 DownPoint;
    float Distance;

    float timer = 0f;
    GameObject hitGO;

    bool colorSwap = true;
    public Color PlayerOneColor;
    public Color PlayerTwoColor;

    public int turnLimitPlayerOne = 20;
    public int turnLimitPlayerTwo = 20;

    void Start () {
        world = new Tile[width, height];
        CreateWorld();
    }
	
	void Update () {
        timer += Time.deltaTime;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            hitGO = hit.collider.gameObject;

            if (Input.GetMouseButtonDown(0)) {
                timer = 0f;
            }
        }
        CurrentFramePosition = Input.mousePosition;

        UpdateCameraMovement();
        if ((timer < 0.2f) && (Input.GetMouseButtonUp(0))) {
                Clicked(hitGO);
        }
        
        LastFramePosition = Input.mousePosition;
    }

    void LateUpdate() {
        // calculate the player scores
        Player1Score = 0;
        Player2Score = 0;
        unoccupiedTiles = 0;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (world[x, y].owner_id == 1) {
                    Player1Score++;
                } else if (world[x, y].owner_id == 2) {
                    Player2Score++;
                } else {
                    unoccupiedTiles++;
                }
            }
        }

        // See if the game is done
        if ((unoccupiedTiles < 1) || ((turnLimitPlayerOne == 0) && (turnLimitPlayerTwo == 0))) {
            // Game is done
            Debug.Log("Game Complete");
        }
    }

    void Clicked(GameObject hit) {
        int x = Mathf.FloorToInt(hitGO.transform.position.x);
        int y = Mathf.FloorToInt(hitGO.transform.position.z);

        Debug.Log("Click at (" + x + ", " + y + ") on object " + hitGO.name);
        if (!((x < 0) || (y < 0))) {
            if (world[x, y].NotControlled()) {
                Debug.Log("Can Control " + hitGO.name);

                int currentRange = range;
                if (colorSwap) {
                    if (NextToControlledTile(x, y, 1)) {
                        currentRange++;
                    }
                } else {
                    if (NextToControlledTile(x, y, 2)) {
                        currentRange++;
                    }
                }

                if (world[x, y].CanControl(1f)) {
                    if (colorSwap) {
                        ControlZone(x, y, 1f, currentRange, PlayerOneColor, 1);
                        turnLimitPlayerOne--;
                    } else {
                        ControlZone(x, y, 1f, currentRange, PlayerTwoColor, 2);
                        turnLimitPlayerTwo--;
                    }
                    colorSwap = !colorSwap;
                }
            }
        }
    }

    bool NextToControlledTile(int x, int y, int id) {
        if (x > 0) {
            if (world[x - 1, y].owner_id == id) {
                return true;
            }
        }
        if (y > 0) {
            if (world[x, y - 1].owner_id == id) {
                return true;
            }
        }
        if (y < height - 1) {
            if (world[x, y + 1].owner_id == id) {
                return true;
            }
        }
        if (x < width - 1) {
            if (world[x + 1, y].owner_id == id) {
                return true;
            }
        }

        return false;
    }
    bool NextToEnemyTile(int x, int y, int id) {
        if (x > 0) {
            if ((world[x - 1, y].owner_id > 0) && (world[x - 1, y].owner_id != id)) {
                return true;
            }
        }
        if (y > 0) {
            if ((world[x, y - 1].owner_id > 0) && (world[x, y - 1].owner_id != id)) {
                return true;
            }
        }
        if (y < height - 1) {
            if ((world[x, y + 1].owner_id > 0) && (world[x, y + 1].owner_id != id)) {
                return true;
            }
        }
        if (x < width - 1) {
            if ((world[x + 1, y].owner_id > 0) && (world[x + 1, y].owner_id != id)) {
                return true;
            }
        }

        return false;
    }

    void ControlZone(int x, int y, float control, int reach, Color color, int id) {
        if (reach > 0) {
            // adjust the control value based on distance to source
            float controlAdjusted = ((reach * 1f) / (range * 1f));
            //Debug.Log("Adjusted Control: " + controlAdjusted + " from Control " + control);

            // Set the block
            world[x, y].SetController(color, control, id, 0.5f * (1f - controlAdjusted));
            
            // check bounds and if able
            if (x > 0) {
                if (world[x - 1, y].CanControl(controlAdjusted)) {
                    ControlZone(x - 1, y, controlAdjusted, reach - 1, color, id);
                }
            }
            if (y > 0) {
                if (world[x, y - 1].CanControl(controlAdjusted)) {
                    ControlZone(x, y - 1, controlAdjusted, reach - 1, color, id);
                }
            }
            if (y < height - 1) {
                if (world[x, y + 1].CanControl(controlAdjusted)) {
                    ControlZone(x, y + 1, controlAdjusted, reach - 1, color, id);
                }
            }
            if (x < width - 1) {
                if (world[x + 1, y].CanControl(controlAdjusted)) {
                    ControlZone(x + 1, y, controlAdjusted, reach - 1, color, id);
                }
            }
        }
    }

    void UpdateCameraMovement() {
        if (Input.GetMouseButton(0)) {
            Difference = LastFramePosition - CurrentFramePosition;
            
            CameraRotationPoint.transform.Rotate(new Vector3(0f, -(Difference.x) * rotateSpeed * Time.deltaTime, 0f));
        }
    }

    void CreateWorld() {
        Vector3 CameraPivotPosition = Vector3.zero;

        // Generate our gradiant
        /*int grad_width = Mathf.FloorToInt(width * 0.5f);
        int grad_center_x = grad_width / 2;
        int grad_height = Mathf.FloorToInt(height * 0.5f);
        int grad_center_y = grad_height / 2;
        float[,] gradient = new float[grad_width, grad_height];
        float largestValue = 1f;
        if (grad_width > grad_height) {
            largestValue = grad_width;
        } else {
            largestValue = grad_height;
        }

        for (int gx = 0; gx < grad_width; gx++) {
            for (int gy = 0; gy < grad_height; gy++) {
                float distanceX = ((float) grad_center_x - gx) * ((float) grad_center_x - gx);
                float distanceY = ((float) grad_center_y - gy) * ((float) grad_center_y - gy);
                gradient[gx, gy] = (Mathf.Sqrt(distanceX + distanceY) / largestValue);
            }
        }*/

        // Only needed for multiple gradients
        // Create a gradient map for our multiple gradients
        /*float[,] gradientMap = new float[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                gradientMap[x, y] = 1f;
            }
        }*/

        // determine x and y center locations for our gradients

        // Loop through the gradient and apply it with offset into the gradientMap

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                GameObject temp = Instantiate(tile);
                world[x, y] = temp.GetComponent<Tile>();
                temp.transform.position = new Vector3(x, -30f, y);
                CameraPivotPosition += temp.transform.position;
                temp.transform.parent = this.transform;
                temp.name = "Tile_" + x + "_" + y;
                
                world[x, y].Setup(heightOffset, true);
            }
        }

        world[0, 0].Setup(heightOffset, false);
        world[width-1, 0].Setup(heightOffset, false);
        world[0, height-1].Setup(heightOffset, false);
        world[width-1, height-1].Setup(heightOffset, false);

        CameraPivotPosition.y = 0f;
        CameraPivotPosition /= (width * height);

        // Set the pivots location
        //CameraRotationPoint.transform.position = new Vector3(width / 2f, 0f, height / 2f);
        CameraRotationPoint.transform.position = CameraPivotPosition;
    }
}
