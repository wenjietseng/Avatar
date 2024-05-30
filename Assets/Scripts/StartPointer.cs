using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class StartPointer : MonoBehaviour
{
    private ExperimentController experimentController;
    private Transform rightIndexTip;

    private void Start()
    {
        experimentController = GameObject.Find("ScriptsHandler").GetComponent<ExperimentController>();
    }

    private void Update()
    {
        if (rightIndexTip == null)
        {
            if (experimentController.syncAvatar != null && experimentController.syncAvatar.transform.FindChildRecursive("FullBody_RightHandIndexTip") != null)
                rightIndexTip = experimentController.syncAvatar.transform.FindChildRecursive("FullBody_RightHandIndexTip").transform;
        }
        else
        {
            this.transform.position = rightIndexTip.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "StartBox")
        {
            experimentController.isStartFlagOn = true;
        }
    }
}
