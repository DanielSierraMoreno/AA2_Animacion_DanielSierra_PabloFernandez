using OctopusController;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using UnityEngine;

internal class MyTentacleController
{
    TentacleMode tentacleMode;
    Transform[] _bones;
    public Transform _endEffectorSphere;
    bool debug = false;

    public Transform[] Bones { get => _bones; }

    public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
    {
        tentacleMode = mode;

        switch (tentacleMode)
        {
            case TentacleMode.LEG:
                _bones = GetAllJointsFromRoot(root, "Joint");
                _endEffectorSphere = GetTransformByName(root, "EndEffector");
                break;

            case TentacleMode.TAIL:

                _bones = GetAllJointsFromRoot(root, "joint_");
                _endEffectorSphere = GetTransformByName(root, "EndEffector");

                break;

            case TentacleMode.TENTACLE:

                _bones = GetAllJointsFromRoot(root, "Bone.", "end");
                _endEffectorSphere = GetTransformByName(root, "Bone.001_end");

                break;
        }
        return Bones;
    }
    Transform GetTransformByName(Transform root, string name)
    {
        Transform child = root.GetComponentsInChildren<Transform>(true)
                             .FirstOrDefault(t => t.name.StartsWith(name));
        if (debug)
            if (child != null)
                Debug.Log("Loaded " + name);
            else
                Debug.Log("Loaded null instead of " + name);

        return child;
    }

    Transform[] GetAllJointsFromRoot(Transform root, string name, string end = null)
    {
        Transform[] allJoints = root.GetComponentsInChildren<Transform>(true);

        IEnumerable<Transform> joints = allJoints.Where(t => t.name.StartsWith(name));
        if (end != null)
        {
            joints = joints.Where(t => !t.name.EndsWith(end));
        }
        if (debug)
            Debug.Log("Loaded transforms with name: " + string.Join(", ", joints.Select(t => t.name)));

        return joints.ToArray();
    }
}