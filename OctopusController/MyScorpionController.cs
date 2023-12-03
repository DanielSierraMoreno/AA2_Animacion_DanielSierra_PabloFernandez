using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{

    public class MyScorpionController
    {
        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        MyTentacleController _tail;

        float animDuration;
        float currentTime = 0;
        bool isPlaying = false;
        bool StartTail = false;
        float distanceBetweenFutureBases = 1f;
        //LEGS
        Transform[] legTargets = new Transform[6];
        Transform[] legFutureBases = new Transform[6];
        MyTentacleController[] _legs = new MyTentacleController[6];


        private Vector3[] jointsController;
        private float[] distancesBetweenJoints;
        float threeshold = 0.05f;
        float tailRate = 120.0f;

        #region public
        public void InitLegs(Transform[] LegRoots, Transform[] LegFutureBases, Transform[] LegTargets)
        {

        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation

            tailEndEffector = _tail.endEffector;
        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {
            tailTarget = target;
        }

        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {
            isPlaying = true;
            animDuration = 5;
            currentTime = 0;
        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            updateTail();

        }
        #endregion

        #region private
        //TODO: Implement the leg base animations and logic

        private void updateLegPos()
        {

        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {
            if (Vector3.Distance(tailEndEffector.transform.position, tailTarget.transform.position) > threeshold)
            {
                for (int i = 0; i < _tail.Bones.Length - 2; i++)
                {
                    float slope = 0;
                    if (i == 0)
                    {
                        slope = CalculateSlope(_tail.Bones[i], new Vector3(0, 0, 1));
                        _tail.Bones[i].transform.Rotate((new Vector3(0, 0, 1) * -slope) * tailRate);
                        slope = CalculateSlope(_tail.Bones[i], new Vector3(1, 0, 0));
                        _tail.Bones[i].transform.Rotate((new Vector3(1, 0, 0) * -slope) * tailRate);
                    }
                    else
                    {
                        slope = CalculateSlope(_tail.Bones[i], new Vector3(1, 0, 0));
                        _tail.Bones[i].transform.Rotate((new Vector3(1, 0, 0) * -slope) * tailRate);

                    }

                }
            }
        }
        private float CalculateSlope(Transform actualJoint, Vector3 axis)
        {
            float distance = Vector3.Distance(tailEndEffector.transform.position, tailTarget.transform.position);
            actualJoint.transform.Rotate(axis * 0.01f);
            float distance2 = Vector3.Distance(tailEndEffector.transform.position, tailTarget.transform.position);
            actualJoint.transform.Rotate(axis * -0.01f);
            return (distance2 - distance) / 0.01f;
        }

        //TODO: implement fabrik method to move legs 
        private void updateLegs(int idPata)
        {
         
        }
    }

}
