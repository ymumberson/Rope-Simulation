using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stick : MonoBehaviour
{
    public Hinge hingeA, hingeB;
    public float length;

    LineRenderer lineRenderer;
    EdgeCollider2D edgeCollider;
    public float width_offset;

    private void Awake()
    {
        this.lineRenderer = GetComponent<LineRenderer>();
        this.edgeCollider = GetComponent<EdgeCollider2D>();
        this.lineRenderer.positionCount = 2;
        this.width_offset = transform.localScale.x / 2f; // default
    }

    public void SetWidthOffset(float offset)
    {
        this.width_offset = offset;
    }

    public void UpdateStick(Hinge hingeA, Hinge hingeB)
    {
        this.hingeA = hingeA;
        this.hingeB = hingeB;
        this.length = Vector3.Distance(hingeA.GetPosition(), hingeB.GetPosition());
        Vector3 direction = (hingeA.GetPosition() - hingeB.GetPosition()).normalized;
        this.edgeCollider.points = new Vector2[] {
                hingeA.GetPosition() - direction * width_offset,
                hingeB.GetPosition() + direction * width_offset
            };
        lineRenderer.SetPosition(0, hingeA.GetPosition() - direction * width_offset);
        lineRenderer.SetPosition(1, hingeB.GetPosition() + direction * width_offset);
    }

    private void LateUpdate()
    {
        if (this.hingeA != null && this.hingeB != null)
        {
            Vector3 direction = (hingeA.GetPosition() - hingeB.GetPosition()).normalized;
            this.edgeCollider.points = new Vector2[] {
                hingeA.GetPosition() - direction * width_offset,
                hingeB.GetPosition() + direction * width_offset
            };
            lineRenderer.SetPosition(0, hingeA.GetPosition() - direction * width_offset);
            lineRenderer.SetPosition(1, hingeB.GetPosition() + direction * width_offset);
        }
    }
}
