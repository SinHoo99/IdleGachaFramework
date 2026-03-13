using System.Collections;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public bool IsWalk = true;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    private void Start()
    {
        FilpUnit();
    }

    private void FilpUnit()
    {
        animator.SetBool("IsWalk", true);
        if (this.transform.position.x < 0)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }
}
