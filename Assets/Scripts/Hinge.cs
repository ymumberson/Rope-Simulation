using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hinge : MonoBehaviour
{
    public Vector3 _position, _previous_position;
    public bool locked;

    private SpriteRenderer sprite_renderer;

    private void Awake()
    {
        this.sprite_renderer = GetComponent<SpriteRenderer>();
    }

    public void SetPosition(Vector3 pos)
    {
        _position = pos;
        this.transform.position = pos;
    }

    public Vector3 GetPosition()
    {
        return _position;
    }

    public void SetPreviousPosition(Vector3 pos)
    {
        _previous_position = pos;
    }

    public Vector3 GetPreviousPosition()
    {
        return _previous_position;
    }

    public void ToggleLocked()
    {
        if (this.locked)
        {
            sprite_renderer.color = Color.white;
            this.locked = false;
        }
        else
        {
            sprite_renderer.color = Color.red;
            this.locked = true;
        }
    }
}
