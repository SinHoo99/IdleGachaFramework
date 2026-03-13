using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGBtn : MonoBehaviour
{
    public void Exit()
    {
        transform.parent.gameObject.SetActive(false);
        GameManager.Instance.PlaySFX(SFX.Click);
    }
}
