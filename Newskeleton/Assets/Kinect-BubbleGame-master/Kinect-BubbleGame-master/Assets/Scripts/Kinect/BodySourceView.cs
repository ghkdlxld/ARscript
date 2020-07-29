using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

public class BodySourceView : MonoBehaviour
{
    public BodySourceManager mBodySourceManager;
    public GameObject mJointObject;

    private Dictionary<ulong, GameObject> mBodies = new Dictionary<ulong, GameObject>();
    private List<JointType> _joints = new List<JointType>
    {
        JointType.FootLeft, JointType.AnkleLeft,          //{왼발,왼발목}
        JointType.AnkleLeft, JointType.KneeLeft,          //{왼발목,왼무릎}
        JointType.KneeLeft, JointType.HipLeft,            //{왼무릎,왼엉덩이}
        JointType.HipLeft, JointType.SpineBase,           //{왼엉덩이,가운데엉덩이}
        
        JointType.FootRight, JointType.AnkleRight,        //{오른발,오른발목}
        JointType.AnkleRight, JointType.KneeRight,        //{오른발목,오른무릎}
        JointType.KneeRight, JointType.HipRight,          //{오른무릎,오른엉덩이}
        JointType.HipRight, JointType.SpineBase,         //{오른엉덩이,가운데엉덩이}

        JointType.HandTipLeft, JointType.HandLeft,        //{왼손끝,왼손등}
        JointType.ThumbLeft, JointType.HandLeft,          //{왼엄지,왼손등}
        JointType.HandLeft, JointType.WristLeft,          //{왼손등,왼손목}
        JointType.WristLeft, JointType.ElbowLeft,         //{왼손목,왼팔꿈치}
        JointType.ElbowLeft, JointType.ShoulderLeft,      //{왼팔꿈치,왼어깨}
        JointType.ShoulderLeft, JointType.SpineShoulder,  //{왼어깨,가운데어깨}
        
        JointType.HandTipRight, JointType.HandRight,      //{오른손끝,오른손등}
        JointType.ThumbRight, JointType.HandRight,        //{오른엄지,오른손등}
        JointType.HandRight, JointType.WristRight,        //{오른손등,오른손목}
        JointType.WristRight, JointType.ElbowRight,       //{오른손목,오른팔꿈치}
        JointType.ElbowRight, JointType.ShoulderRight,    //{오른팔꿈치,오른어깨}
        JointType.ShoulderRight, JointType.SpineShoulder, //{오른어깨,가운데어깨}
        
        JointType.SpineBase, JointType.SpineMid,          //{가운데어깨,허리}
        JointType.SpineMid, JointType.SpineShoulder,      //{허리,가운데어깨}
        JointType.SpineShoulder, JointType.Neck,          //{가운데어깨,목}
        JointType.Neck, JointType.Head
    };

    void Update()
    {
        #region Get Kinect data
        Body[] data = mBodySourceManager.GetData();
        if (data == null)
            return;

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
                continue;

            if (body.IsTracked)
                trackedIds.Add(body.TrackingId);
        }
        #endregion

        #region Delete Kinect bodies
        List<ulong> knownIds = new List<ulong>(mBodies.Keys);
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                // Destroy body object
                Destroy(mBodies[trackingId]);

                // Remove from list
                mBodies.Remove(trackingId);
            }
        }
        #endregion

        #region Create Kinect bodies
        foreach (var body in data)
        {
            // If no body, skip
            if (body == null)
                continue;

            if (body.IsTracked)
            {
                // If body isn't tracked, create body
                if (!mBodies.ContainsKey(body.TrackingId))
                    mBodies[body.TrackingId] = CreateBodyObject(body.TrackingId);

                // Update positions
                UpdateBodyObject(body, mBodies[body.TrackingId]);
            }
        }
        #endregion
    }

    private GameObject CreateBodyObject(ulong id)
    {
        // Create body parent
        GameObject body = new GameObject("Body:" + id);

        // Create joints
        foreach (JointType joint in _joints)
        {
            // Create Object
            GameObject newJoint = Instantiate(mJointObject);
            newJoint.name = joint.ToString();

            // Parent to body
            newJoint.transform.parent = body.transform;
        }

        return body;
    }

    private void UpdateBodyObject(Body body, GameObject bodyObject)
    {
        // Update joints
        foreach (JointType _joint in _joints)
        {
            // Get new target position
            Joint sourceJoint = body.Joints[_joint];
            Vector3 targetPosition = GetVector3FromJoint(sourceJoint);
            targetPosition.z = 0;

            // Get joint, set new position
            Transform jointObject = bodyObject.transform.Find(_joint.ToString());
            jointObject.position = targetPosition;
        }
    }

    private Vector3 GetVector3FromJoint(Joint joint)
    {
        return new Vector3(joint.Position.X * 14, joint.Position.Y * 14, joint.Position.Z * 14);
    }
}
