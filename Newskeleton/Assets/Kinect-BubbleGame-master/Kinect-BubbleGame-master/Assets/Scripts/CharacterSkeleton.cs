/*
 * Copyright (c) 2017 Yoshihisa Nitta
 * Released under the MIT license
 * http://opensource.org/licenses/mit-license.php
 */

/* http://nw.tsuda.ac.jp/lec/kinect2/ */
/* version 1.3: 2017/08/11 */
/* version 1.2: 2017/08/10 */
/* version 1.1: 2017/08/07 */
/* version 1.0: 2017/08/06 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class CharacterSkeleton
{
    public const int
      // JointType
      JointType_SpineBase = 0,
      JointType_SpineMid = 1,
      JointType_Neck = 2,
      JointType_Head = 3,
      JointType_ShoulderLeft = 4,
      JointType_ElbowLeft = 5,
      JointType_WristLeft = 6,
      JointType_HandLeft = 7,
      JointType_ShoulderRight = 8,
      JointType_ElbowRight = 9,
      JointType_WristRight = 10,
      JointType_HandRight = 11,
      JointType_HipLeft = 12,
      JointType_KneeLeft = 13,
      JointType_AnkleLeft = 14,
      JointType_FootLeft = 15,
      JointType_HipRight = 16,
      JointType_KneeRight = 17,
      JointType_AnkleRight = 18,
      JointType_FootRight = 19,
      JointType_SpineShoulder = 20,
      JointType_HandTipLeft = 21,
      JointType_ThumbLeft = 22,
      JointType_HandTipRight = 23,
      JointType_ThumbRight = 24,
      // TrackingState
      TrackingState_NotTracked = 0,
      TrackingState_Inferred = 1,
      TrackingState_Tracked = 2,
      // Number
      bodyCount = 6,
      jointCount = 25;

    private static int[] jointSegment = new int[] {
    JointType_SpineBase, JointType_SpineMid,             // Spine
    JointType_Neck, JointType_Head,                      // Neck
    // left
    JointType_ShoulderLeft, JointType_ElbowLeft,         // LeftUpperArm
    JointType_ElbowLeft, JointType_WristLeft,            // LeftLowerArm
    JointType_WristLeft, JointType_HandLeft,             // LeftHand
    JointType_HipLeft, JointType_KneeLeft,               // LeftUpperLeg
    JointType_KneeLeft, JointType_AnkleLeft,             // LeftLowerLeg6
    JointType_AnkleLeft, JointType_FootLeft,             // LeftFoot
    // right
    JointType_ShoulderRight, JointType_ElbowRight,       // RightUpperArm
    JointType_ElbowRight, JointType_WristRight,          // RightLowerArm
    JointType_WristRight, JointType_HandRight,           // RightHand
    JointType_HipRight, JointType_KneeRight,             // RightUpperLeg
    JointType_KneeRight, JointType_AnkleRight,           // RightLowerLeg
    JointType_AnkleRight, JointType_FootRight,           // RightFoot
  };
    public Vector3[] joint = new Vector3[jointCount];
    public int[] jointState = new int[jointCount];

    Dictionary<HumanBodyBones, Vector3> trackingSegment = null;
    Dictionary<HumanBodyBones, int> trackingState = null;

    private static HumanBodyBones[] humanBone = new HumanBodyBones[] {
    HumanBodyBones.Hips,
    HumanBodyBones.Spine,
    HumanBodyBones.UpperChest,
    HumanBodyBones.Neck,
    HumanBodyBones.Head,
    HumanBodyBones.LeftUpperArm,
    HumanBodyBones.LeftLowerArm,
    HumanBodyBones.LeftHand,
    HumanBodyBones.LeftUpperLeg,
    HumanBodyBones.LeftLowerLeg,
    HumanBodyBones.LeftFoot,
    HumanBodyBones.RightUpperArm,
    HumanBodyBones.RightLowerArm,
    HumanBodyBones.RightHand,
    HumanBodyBones.RightUpperLeg,
    HumanBodyBones.RightLowerLeg,
    HumanBodyBones.RightFoot,
  };

    private static HumanBodyBones[] targetBone = new HumanBodyBones[] {
    HumanBodyBones.Spine,
    HumanBodyBones.Neck,
    HumanBodyBones.LeftUpperArm,
    HumanBodyBones.LeftLowerArm,
    HumanBodyBones.LeftHand,
    HumanBodyBones.LeftUpperLeg,
    HumanBodyBones.LeftLowerLeg,
    HumanBodyBones.LeftFoot,
    HumanBodyBones.RightUpperArm,
    HumanBodyBones.RightLowerArm,
    HumanBodyBones.RightHand,
    HumanBodyBones.RightUpperLeg,
    HumanBodyBones.RightLowerLeg,
    HumanBodyBones.RightFoot,
  };

    public GameObject humanoid;
    private Dictionary<HumanBodyBones, RigBone> rigBone = null;
    private bool isSavedPosition = false;
    private Vector3 savedPosition;
    private Quaternion savedHumanoidRotation;

    public CharacterSkeleton(GameObject h)
    {
        humanoid = h;
        rigBone = new Dictionary<HumanBodyBones, RigBone>();
        foreach (HumanBodyBones bone in humanBone)
        {
            rigBone[bone] = new RigBone(humanoid, bone);
        }
        savedHumanoidRotation = humanoid.transform.rotation;
        trackingSegment = new Dictionary<HumanBodyBones, Vector3>(targetBone.Length);
        trackingState = new Dictionary<HumanBodyBones, int>(targetBone.Length);
    }
    private void swapJoint(int a, int b)
    {
        Vector3 tmp = joint[a]; joint[a] = joint[b]; joint[b] = tmp;
        int t = jointState[a]; jointState[a] = jointState[b]; jointState[b] = t;
    }
    public void set(float[] jt, int[] st, int offset, bool mirrored, bool move)
    {
        if (isSavedPosition == false && jointState[JointType_SpineBase] != TrackingState_NotTracked)
        {
            isSavedPosition = true;
            int j = offset * jointCount + JointType_SpineBase;
            savedPosition = new Vector3(jt[j * 3], jt[j * 3 + 1], jt[j * 3 + 2]);
        }
        for (int i = 0; i < jointCount; i++)
        {
            int j = offset * jointCount + i;
            if (mirrored)
            {
                joint[i] = new Vector3(-jt[j * 3], jt[j * 3 + 1], -jt[j * 3 + 2]);
            }
            else
            {
                joint[i] = new Vector3(jt[j * 3], jt[j * 3 + 1], savedPosition.z * 2 - jt[j * 3 + 2]);
            }
            jointState[i] = st[j];
        }
        if (mirrored)
        {
            swapJoint(JointType_ShoulderLeft, JointType_ShoulderRight);
            swapJoint(JointType_ElbowLeft, JointType_ElbowRight);
            swapJoint(JointType_WristLeft, JointType_WristRight);
            swapJoint(JointType_HandLeft, JointType_HandRight);
            swapJoint(JointType_HipLeft, JointType_HipRight);
            swapJoint(JointType_KneeLeft, JointType_KneeRight);
            swapJoint(JointType_AnkleLeft, JointType_AnkleRight);
            swapJoint(JointType_FootLeft, JointType_FootRight);
            swapJoint(JointType_HandTipLeft, JointType_HandTipRight);
            swapJoint(JointType_ThumbLeft, JointType_ThumbRight);
        }
        for (int i = 0; i < targetBone.Length; i++)
        {
            int s = jointSegment[2 * i], e = jointSegment[2 * i + 1];
            trackingSegment[targetBone[i]] = joint[e] - joint[s];
            trackingState[targetBone[i]] = System.Math.Min(jointState[e], jointState[s]);
        }

        Vector3 waist = joint[JointType_HipRight] - joint[JointType_HipLeft];
        waist = new Vector3(waist.x, 0, waist.z);
        Quaternion rot = Quaternion.FromToRotation(Vector3.right, waist);
        Quaternion rotInv = Quaternion.Inverse(rot);

        Vector3 shoulder = joint[JointType_ShoulderRight] - joint[JointType_ShoulderLeft];
        shoulder = new Vector3(shoulder.x, 0, shoulder.z);
        Quaternion srot = Quaternion.FromToRotation(Vector3.right, shoulder);
        Quaternion srotInv = Quaternion.Inverse(srot);

        humanoid.transform.rotation = Quaternion.identity;
        foreach (HumanBodyBones bone in targetBone)
        {
            rigBone[bone].transform.rotation = rotInv * Quaternion.FromToRotation(Vector3.up, trackingSegment[bone]);
        }
        rigBone[HumanBodyBones.UpperChest].offset(srot);
        Quaternion bodyRot = rot;
        if (mirrored)
        {
            bodyRot = Quaternion.AngleAxis(180, Vector3.up) * bodyRot;
        }
        humanoid.transform.rotation = bodyRot;
        if (move == true)
        {
            Vector3 m = joint[JointType_SpineBase];
            if (mirrored) m = new Vector3(-m.x, m.y, -m.z);
            humanoid.transform.position = m;
        }
    }
}

