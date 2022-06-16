using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Diagnostics;

public class LevelObject : Core
{
    #region [ PROPERTIES ]

    [HideInInspector] public Vector3 gridPos = new Vector3();
    [HideInInspector] public bool isMoving = false;
    [HideInInspector] public float facingDir = 0.0f;

    public ObjectTypes type;

    [HideInInspector] public GameObject pivot;

    public bool movable = false;
    public bool rotatable = false;

    protected Coroutine movement;

    protected int animFramesPassed;
    Stopwatch sw = new Stopwatch();

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    void Awake()
    {
        OnAwake();
    }

    void Start()
    {
        OnStart();
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {

    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    protected void OnAwake()
    {
        GetComponents();
    }
    
    protected void OnStart()
    {
        GetFacing();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    protected void GetComponents()
    {
        if (GetChildrenWithTag(gameObject, "Pivot").Count > 0)
        {
            pivot = GetChildrenWithTag(gameObject, "Pivot")[0];
        }
    }

    public Vector3 GetGridPos()
    {
        gridPos = transform.position / GameManager.LevelController.gridCellScale;
        return gridPos;
    }

    public float GetFacing()
    {
        if (pivot != null)
        {
            facingDir = WrapClamp(pivot.transform.eulerAngles.y, 0.0f, 360.0f);
        }
        return facingDir;
    }

    public float GetGridBearing(Vector3 dir)
    {
        if (dir.x >= 0.0f)
        {
            return ToDeg(Mathf.Acos(dir.z / dir.magnitude));
        }
        else
        {
            return 360.0f - ToDeg(Mathf.Acos(dir.z / dir.magnitude));
        }
    }
    
    public Vector3 GetGridDir(float bearing)
    {
        Vector3 dir = new Vector3();
        dir.x = Mathf.Sin(ToRad(bearing));
        dir.z = Mathf.Cos(ToRad(bearing));
        return dir;
    }
    
    public void Move(Vector3 gridMovement)
    {
        if (!isMoving)
        {
            movement = StartCoroutine(MoveAnim(gridMovement, 1.0f));
        }
    }
    
    public void Move(Vector3 gridMovement, bool interrupt)
    {
        if (!isMoving)
        {
            movement = StartCoroutine(MoveAnim(gridMovement, 1.0f));
        }
        else if (interrupt)
        {
            if (movement != null)
            {
                StopCoroutine(movement);
                movement = StartCoroutine(MoveAnim(gridMovement, 1.0f));
            }
        }
    }

    public void Move(Vector3 gridMovement, float animTime)
    {
        if (!isMoving)
        {
            movement = StartCoroutine(MoveAnim(gridMovement, animTime));
        }
    }
    
    public void Move(Vector3 gridMovement, float animTime, bool interrupt)
    {
        if (!isMoving)
        {
            movement = StartCoroutine(MoveAnim(gridMovement, animTime));
        }
        else if (interrupt)
        {
            if (movement != null)
            {
                StopCoroutine(movement);
                movement = StartCoroutine(MoveAnim(gridMovement, animTime));
            }
        }
    }
    
    protected IEnumerator MoveAnim(Vector3 gridMovement, float animTime)
    {
        isMoving = true;

        int animFrames = (int)(animTime * 200.0f);
        float animFrameTime = animTime / (float)animFrames;
        Vector3 posStart = transform.position;
        Vector3 posEnd = posStart + gridMovement * GameManager.LevelController.gridCellScale;

        for (int i = 1; i <= animFrames; i++)
        {
            float delta = (float)i / (float)animFrames;

            yield return new WaitForSeconds(animFrameTime);

            transform.position = Vector3.Lerp(posStart, posEnd, delta);

            animFramesPassed = i;

            sw.Stop();
            GameManager.UIController.coroutineFrameTime = (float)sw.Elapsed.TotalSeconds;
            sw.Restart();
        }

        GetGridPos();

        if (gameObject.GetComponent<Player>() != null)
        {
            string info = animFrames + " | " + animFrameTime;
            GameManager.UIController.moveAnimInfo = info;
        }

        isMoving = false;
    }
    
    public void Move(Vector3 gridMovement, Vector3 jumpOffset)
    {
        if (!isMoving)
        {
            movement = StartCoroutine(MoveAnim(gridMovement, jumpOffset, 1.0f));
        }
    }
    
    public void Move(Vector3 gridMovement, Vector3 jumpOffset, bool interrupt)
    {
        if (!isMoving)
        {
            movement = StartCoroutine(MoveAnim(gridMovement, jumpOffset, 1.0f));
        }
        else if (interrupt)
        {
            if (movement != null)
            {
                StopCoroutine(movement);
                movement = StartCoroutine(MoveAnim(gridMovement, jumpOffset, 1.0f));
            }
        }
    }

    public void Move(Vector3 gridMovement, Vector3 jumpOffset, float animTime)
    {
        if (!isMoving)
        {
            movement = StartCoroutine(MoveAnim(gridMovement, jumpOffset, animTime));
        }
    }
    
    public void Move(Vector3 gridMovement, Vector3 jumpOffset, float animTime, bool interrupt)
    {
        if (!isMoving)
        {
            movement = StartCoroutine(MoveAnim(gridMovement, jumpOffset, animTime));
        }
        else if (interrupt)
        {
            if (movement != null)
            {
                StopCoroutine(movement);
                movement = StartCoroutine(MoveAnim(gridMovement, jumpOffset, animTime));
            }
        }
    }

    protected IEnumerator MoveAnim(Vector3 gridMovement, Vector3 jumpOffset, float animTime)
    {
        isMoving = true;

        int animFrames = (int)(animTime * 200.0f);
        float animFrameTime = animTime / (float)animFrames;
        Vector3 posStart = transform.position;
        Vector3 posEnd = posStart + gridMovement * GameManager.LevelController.gridCellScale;

        for (int i = 1; i <= animFrames; i++)
        {
            float delta = (float)i / (float)animFrames;

            yield return new WaitForSeconds(animFrameTime);

            Vector3 pos = Vector3.Lerp(posStart, posEnd, delta);
            pos += jumpOffset * Mathf.Sin(Mathf.PI * delta);
            transform.position = pos;
        }

        GetGridPos();

        isMoving = false;
    }

    public void Rotate(float gridRot, float animTime)
    {
        if (!isMoving)
        {
            movement = StartCoroutine(RotateAnim(gridRot, animTime));
        }
    }
    
    public void Rotate(float gridRot, float animTime, bool interrupt)
    {
        if (!isMoving)
        {
            movement = StartCoroutine(RotateAnim(gridRot, animTime));
        }
        else if (interrupt)
        {
            if (movement != null)
            {
                StopCoroutine(movement);
                movement = StartCoroutine(RotateAnim(gridRot, animTime));
            }
        }
    }

    protected IEnumerator RotateAnim(float gridRot, float animTime)
    {
        isMoving = true;

        int animFrames = (int)(animTime * 200.0f);
        float animFrameTime = animTime / (float)animFrames;
        float rotAngle = gridRot * 90.0f;
        float rotStep = rotAngle / (float)animFrames;

        for (int i = 1; i <= animFrames; i++)
        {
            yield return new WaitForSeconds(animFrameTime);
            if (pivot != null)
            {
                pivot.transform.Rotate(Vector3.up, rotStep);
            }
            facingDir += rotStep;
        }
        facingDir = WrapClamp(facingDir, 0.0f, 360.0f);

        isMoving = false;
    }
    
    public void RotateAround(Vector3 gridPivot, float gridRot, float animTime)
    {
        if (!isMoving)
        {
            movement = StartCoroutine(RotateAroundAnim(gridPivot, gridRot, animTime));
        }
    }
    
    public void RotateAround(Vector3 gridPivot, float gridRot, float animTime, bool interrupt)
    {
        if (!isMoving)
        {
            movement = StartCoroutine(RotateAroundAnim(gridPivot, gridRot, animTime));
        }
        else if (interrupt)
        {
            if (movement != null)
            {
                StopCoroutine(movement);
                movement = StartCoroutine(RotateAroundAnim(gridPivot, gridRot, animTime));
            }
        }
    }

    protected IEnumerator RotateAroundAnim(Vector3 gridPivot, float gridRot, float animTime)
    {
        isMoving = true;
        
        int animFrames = (int)(animTime * 200.0f);
        float animFrameTime = animTime / (float)animFrames;

        float rotAngle = gridRot * 90.0f;
        float rotStep = rotAngle / (float)animFrames;
        float rotAngleRad = ToRad(rotAngle);
        float rotStepRad = ToRad(rotStep);

        Vector3 pivotPoint = gridPivot * GameManager.LevelController.gridCellScale;

        float startAngleRad = ToRad(GetGridBearing(transform.position - gridPivot));
        float rotRadius = Vector3.Distance(transform.position, gridPivot);

        // pos x = pvt x + r * sin(angle)
        // pos z = pvt z + r * cos(angle)

        for (int i = 1; i <= animFrames; i++)
        {
            float delta = (float)i / (float)animFrames;
            float angle = startAngleRad + rotAngleRad * delta;
            float x = pivotPoint.x + rotRadius * Mathf.Sin(angle);
            float z = pivotPoint.z + rotRadius * Mathf.Cos(angle);
            yield return new WaitForSeconds(animFrameTime);
            if (pivot != null)
            {
                pivot.transform.Rotate(Vector3.up, rotStep);
            }
            facingDir += rotStep;
            transform.position = new Vector3(x, transform.position.y, z);
        }
        facingDir = WrapClamp(facingDir, 0.0f, 360.0f);

        GetGridPos();
        isMoving = false;
    }

}
