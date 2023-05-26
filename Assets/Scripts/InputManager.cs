using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private GameObject HINGE_PREFAB;
    [SerializeField]
    private GameObject STICK_PREFAB;

    [SerializeField]
    public float GRAVITY = 9.81f;
    public bool CREATE_GRID;
    public int GRID_WIDTH, GRID_HEIGHT;
    public int NUM_ITTERATIONS;

    private List<Hinge> hingeList;
    private List<Stick> stickList;

    LineRenderer lineRenderer;
    private Hinge hinge_clicked;
    private float previous_timescale;
    private bool is_paused;
    public bool cut_mode;

    private void Awake()
    {
        is_paused = false;
        previous_timescale = 1f;
        TogglePause(); // start off paused

        hinge_clicked = null;
        this.hingeList = new List<Hinge>();
        this.stickList = new List<Stick>();

        this.lineRenderer = GetComponent<LineRenderer>();
        this.lineRenderer.positionCount = 2;
        cut_mode = false;
    }

    private void Start()
    {
        if (CREATE_GRID) CreateGrid(GRID_WIDTH,GRID_HEIGHT);
    }

    private void FixedUpdate()
    {
        Simulate();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = GetHitAtClick();

        if (hinge_clicked != null)
        {
            Vector3 p0 = hinge_clicked.GetPosition();
            Vector3 p1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = (p0 - p1).normalized;
            lineRenderer.SetPosition(0, p0 - direction*0.25f);
            lineRenderer.SetPosition(1, p1 + direction * 0.25f);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
        else if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (hit.collider == null)
            {
                CreateHinge();
            }
            else if (hit.collider.gameObject.tag == "Hinge")
            {
                if (hinge_clicked != null)
                {
                    // Check if clicking the same hinge
                    if (hinge_clicked != hit.collider.gameObject.GetComponent<Hinge>())
                    {
                        CreateStick(hinge_clicked, hit.collider.gameObject.GetComponent<Hinge>());
                    }
                    hinge_clicked = null;
                    lineRenderer.SetPosition(0, Vector3.zero);
                    lineRenderer.SetPosition(1, Vector3.zero);
                }
                else
                {
                    hinge_clicked = hit.collider.gameObject.GetComponent<Hinge>();
                }
            }

        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.tag == "Hinge")
                {
                    hit.collider.gameObject.GetComponent<Hinge>().ToggleLocked();
                }
                else if (hit.collider.gameObject.tag == "Stick")
                {
                    DestroyStick(hit.collider.gameObject.GetComponent<Stick>());
                    cut_mode = true;
                }
            }
            else
            {
                cut_mode = true;
            }
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            cut_mode = false;
        }

        if (cut_mode && hit.collider != null && hit.collider.gameObject.tag == "Stick")
        {
            DestroyStick(hit.collider.gameObject.GetComponent<Stick>());
        }
    }

    private void TogglePause()
    {
        //if (is_paused)
        //{
        //    Time.timeScale = previous_timescale;
        //}
        //else
        //{
        //    previous_timescale = Time.timeScale;
        //    Time.timeScale = 0f;
        //}
        is_paused = !is_paused;
    }

    private RaycastHit2D GetHitAtClick()
    {
        Vector3 mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return Physics2D.Raycast(mouse_position, Camera.main.transform.forward);
    }

    private void CreateHinge()
    {
        Vector3 mouse_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse_position.z = 0;
        Hinge h = Instantiate(HINGE_PREFAB).GetComponent<Hinge>();
        h.SetPosition(mouse_position);
        h.SetPreviousPosition(mouse_position);
        hingeList.Add(h);
    }

    private void CreateHinge(Vector3 position)
    {
        Hinge h = Instantiate(HINGE_PREFAB).GetComponent<Hinge>();
        h.SetPosition(position);
        h.SetPreviousPosition(position);
        hingeList.Add(h);
    }

    private void CreateStick(Hinge hingeA, Hinge hingeB)
    {
        Stick s = Instantiate(STICK_PREFAB).GetComponent<Stick>();
        s.SetWidthOffset(HINGE_PREFAB.transform.localScale.x/2f);
        s.UpdateStick(hingeA, hingeB);
        stickList.Add(s);
    }

    private void DestroyStick(Stick s)
    {
        for (int i=0; i<stickList.Count; i++)
        {
            if (stickList[i] == s)
            {
                stickList.RemoveAt(i);
                Destroy(s.gameObject);
                return;
            }
        }
    }

    private void Simulate()
    {
        if (is_paused) return;

        //for (int i=0; i<hingeList.Count; ++i)
        //{
        //    int randomIndex = Random.Range(0, hingeList.Count);
        //    Hinge temp = hingeList[randomIndex];
        //    hingeList[randomIndex] = hingeList[i];
        //    hingeList[i] = temp;
        //}

        //for (int i=0; i<stickList.Count; ++i)
        //{
        //    int randomIndex = Random.Range(0, stickList.Count);
        //    Stick temp = stickList[randomIndex];
        //    stickList[randomIndex] = stickList[i];
        //    stickList[i] = temp;
        //}

        foreach (Hinge h in hingeList)
        {
            if (!h.locked)
            {
                Vector3 before_move = h.GetPosition();
                Vector3 pos = h.GetPosition();
                pos += (h.GetPosition() - h.GetPreviousPosition())
                    + Vector3.down * GRAVITY * Time.fixedDeltaTime * Time.fixedDeltaTime;
                h.SetPosition(pos);
                h.SetPreviousPosition(before_move);
            }
        }

        for (int i = 0; i < NUM_ITTERATIONS; ++i)
        {
            foreach (Stick s in stickList)
            {
                Vector2 stick_centre = (s.hingeA.GetPosition() + s.hingeB.GetPosition()) / 2f;
                Vector2 stick_direction = (s.hingeA.GetPosition() - s.hingeB.GetPosition()).normalized;
                if (!s.hingeA.locked) s.hingeA.SetPosition(stick_centre + stick_direction * s.length / 2f);
                if (!s.hingeB.locked) s.hingeB.SetPosition(stick_centre - stick_direction * s.length / 2f);
            }
        }
    }

    private void CreateGrid(int x, int y)
    {
        float offsetX = 0.5f;
        float offsetY = 0.5f;
        Vector3 start = Vector3.zero - new Vector3(x/2f * offsetX, y/2f * offsetY, 0f);
        for (int j=0; j<y; ++j)
        {
            for (int i=0; i<x; ++i)
            {
                CreateHinge(start + new Vector3(i * offsetX, j * offsetY, 0));
            }
        }

        int count = 0;
        for (int j = 0; j < y; ++j)
        {
            for (int i = 0; i < x; ++i)
            {
                if (i < x - 1)
                {
                    CreateStick(hingeList[count], hingeList[count + 1]);
                }

                if (j < y - 1)
                {
                    CreateStick(hingeList[i + (x * j)], hingeList[i + (x * j) + (x)]);
                }

                ++count;
            }
        }
    }
}
