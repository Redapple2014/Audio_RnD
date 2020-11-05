using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TargetMoverFor3d : MonoBehaviour
{
    private float RotateSpeed = 5f;
    private float radius = 0.25f;

    private Vector3 _centre;
    private float _angle;
    void Start()
    {
        _centre = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (SoundManager.Instance.isSurroundOn) 
        {
            _angle += RotateSpeed * Time.deltaTime;

            var offset = new Vector3(Mathf.Cos(_angle) * radius, 0, Mathf.Sin(_angle) * radius);
            transform.position = _centre + offset;
        }
    }
    public void ResetAudioSource()
    {
        transform.position = _centre;
    }
}
