using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample02 : MonoBehaviour
{
    public GameObject humanoid;
    public bool mirror = true;
    public bool move = true;

    NtUnity.Kinect nt;
    NtUnity.HumanoidSkeleton hs;

    void Start()
    {
        nt = new NtUnity.Kinect();
        hs = new NtUnity.HumanoidSkeleton(humanoid);
    }

    void Update()
    {
        nt.setRGB();
        nt.setSkeleton();
        nt.setFace();
        nt.imshowBlack();
        int n = nt.getSkeleton();
        if (n > 0)
        {
            hs.set(nt, 0, mirror, move);
        }
    }

    void OnApplicationQuit()
    {
        nt.stopKinect();
    }
}
