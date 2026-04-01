using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperCat : MonoBehaviour
{
    private Vector3 initPosition;

    private float speed = 20f;
    private SuperCatState currentState = SuperCatState.Idel;
    private WavyLineRenderer wavyLineRenderer;

    public List<GameObject> targetBlocks = new List<GameObject>();
    private Animator animator;

    private void OnEnable()
    {
        wavyLineRenderer = GetComponentInChildren<WavyLineRenderer>();
        initPosition = transform.position;
        animator = GetComponent<Animator>();
        animator.SetInteger("Status",0);
    }

    public void AddAllTarget(List<GameObject> blocks, Color color)
    {
        targetBlocks = new List<GameObject>(blocks);
        wavyLineRenderer.InitializeLineRenderer(0.15f, 0.15f,0.015f);
        wavyLineRenderer.SetColor(color);
        animator.SetInteger("Status", 1);
        StartCoroutine(ShootingRoutine());
        currentState = SuperCatState.Shoot;
        animator.enabled = true;
    }

    private IEnumerator ShootingRoutine()
    {
        wavyLineRenderer.UpdateStartPoint(transform.position);

        for (int i = 0; i < targetBlocks.Count; i++)
        {
            if (targetBlocks[i] == null) continue;

            wavyLineRenderer.UpdateStartPoint(transform.position);
            wavyLineRenderer.AddTarget(targetBlocks[i]);

            yield return new WaitUntil(() => !wavyLineRenderer.IsProcessing);
        }

        targetBlocks.Clear();
        animator.SetInteger("Status", 0);
        currentState = SuperCatState.Rotate;
        animator.enabled = false;
    }

    void Update()
    {
        if (currentState == SuperCatState.Rotate)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(90f, 0, 0), Time.deltaTime * 10f);
            if (Quaternion.Angle(transform.rotation, Quaternion.Euler(90f, 0, 0)) < 0.1f)
            {
                currentState = SuperCatState.Move;
            }
        }
        if (currentState == SuperCatState.Move)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);
            if (Vector3.Distance(initPosition, transform.position) >= 10f)
            {
                currentState = SuperCatState.Idel;
                transform.position = initPosition;
                transform.rotation = Quaternion.identity;
                gameObject.SetActive(false);
            }
        }
    }

    public enum SuperCatState
    {
        Idel,
        Move,
        Shoot,
        Rotate
    }
}
