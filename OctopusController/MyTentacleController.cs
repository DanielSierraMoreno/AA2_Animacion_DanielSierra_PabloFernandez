using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;




namespace OctopusController
{


    internal class MyTentacleController

    {

        TentacleMode tentacleMode;
        Transform[] _bones;
        public Transform endEffector;

        public Transform[] Bones { get => _bones; }

        void LoadLeg(Transform root)
        {
            List<Transform> joints = new List<Transform>();

            Transform current = root;

            current = root.GetChild(0);
            joints.Add(current);
            while (current.name != "Joint2")
            {
                current = current.GetChild(1);
                joints.Add(current);

            }
            //Load endEffector inside bones array to make update calculations easier
            current = current.GetChild(1);
            joints.Add(current);

            _bones = joints.ToArray();

        }
        void LoadTail(Transform root)
        {
            List<Transform> joints = new List<Transform>();

            Transform current = root;
            joints.Add(current);
            while (current.GetChild(1).name != "EndEffector")
            {
                current = current.GetChild(1);
                joints.Add(current);
            }
            endEffector = current.GetChild(1);
            _bones = joints.ToArray();

        }
        void LoadTentacle(Transform root)
        {
            List<Transform> joints = new List<Transform>();

            Transform current = root;

            while (current.name != "Bone.001_end")
            {
                current = current.GetChild(0);
                joints.Add(current);
            }

            _bones = joints.ToArray();

        }
        //Exercise 1.
        public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
        {
            //TODO: add here whatever is needed to find the bones forming the tentacle for all modes
            tentacleMode = mode;
            switch (tentacleMode)
            {
                case TentacleMode.LEG:
                    //TODO: in _endEffectorsphere you keep a reference to the base of the leg
                    LoadLeg(root);

                    break;
                case TentacleMode.TAIL:
                    //TODO: in _endEffectorsphere you keep a reference to the red sphere 
                    LoadTail(root);
                    break;
                case TentacleMode.TENTACLE:
                    //TODO: in _endEffectorphere you  keep a reference to the sphere with a collider attached to the endEffector
                    LoadTentacle(root);


                    break;
            }
            return Bones;
        }
    }
}
