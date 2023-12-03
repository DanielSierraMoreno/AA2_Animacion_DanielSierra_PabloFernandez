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
            _legs = new MyTentacleController[LegRoots.Length];
            for (int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                legFutureBases[i] = LegFutureBases[i];
                legTargets[i] = LegTargets[i];
                //TODO: initialize anything needed for the FABRIK implementation
            }
            distancesBetweenJoints = new float[_legs[0].Bones.Length - 1];
            jointsController = new Vector3[_legs[0].Bones.Length];
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

            MovementAnimation();
        }
        #endregion

        #region private
        //TODO: Implement the leg base animations and logic
        private void MovementAnimation()
        {
            if (isPlaying == true)
            {
                currentTime += Time.deltaTime;
                if (currentTime < animDuration)
                {
                    updateLegPos();

                }
                else
                {
                    StartTail = true;
                    isPlaying = false;
                }
            }
        }
        private void updateLegPos()
        {
            //check for the distance to the futureBase, then if it's too far away start moving the leg towards the future base position
            for (int j = 0; j < 6; j++)
            {
                if (Vector3.Distance(_legs[j].Bones[0].position, legFutureBases[j].position) > distanceBetweenFutureBases)
                {
                    _legs[j].Bones[0].position = Vector3.Lerp(_legs[j].Bones[0].position, legFutureBases[j].position, 1.4f);
                }
                updateLegs(j);
            }

        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {
            if (Vector3.Distance(tailEndEffector.transform.position, tailTarget.transform.position) > threeshold && StartTail)
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
            // Save the position of the bones in copy
            for (int i = 0; i <= _legs[idPata].Bones.Length - 1; i++)
            {
                jointsController[i] = _legs[idPata].Bones[i].position;
            }
            for (int i = 0; i <= _legs[idPata].Bones.Length - 2; i++)
            {
                distancesBetweenJoints[i] = Vector3.Distance(_legs[idPata].Bones[i].position, _legs[idPata].Bones[i + 1].position);
            }

            float targetToRoot = Vector3.Distance(jointsController[0], legTargets[idPata].position);
            if (targetToRoot < distancesBetweenJoints.Sum())
            {

                while (Vector3.Distance(jointsController[jointsController.Length - 1], legTargets[idPata].position) != 0 || Vector3.Distance(jointsController[0], _legs[idPata].Bones[0].position) != 0)
                {
                    jointsController[jointsController.Length - 1] = legTargets[idPata].position;
                    for (int i = _legs[idPata].Bones.Length - 2; i >= 0; i--)
                    {
                        Vector3 vectorDirector = (jointsController[i + 1] - jointsController[i]).normalized;
                        Vector3 movementVector = vectorDirector * distancesBetweenJoints[i];
                        jointsController[i] = jointsController[i + 1] - movementVector;
                    }

                    jointsController[0] = _legs[idPata].Bones[0].position;
                    for (int i = 1; i < _legs[idPata].Bones.Length - 1; i++)
                    {
                        Vector3 vectorDirector = (jointsController[i - 1] - jointsController[i]).normalized;
                        Vector3 movementVector = vectorDirector * distancesBetweenJoints[i - 1];
                        jointsController[i] = jointsController[i - 1] - movementVector;

                    }
                }

                for (int i = 0; i <= _legs[idPata].Bones.Length - 2; i++)
                {
                    Vector3 direction = (jointsController[i + 1] - jointsController[i]).normalized;
                    Vector3 antDir = (_legs[idPata].Bones[i + 1].position - _legs[idPata].Bones[i].position).normalized;
                    Quaternion rot = Quaternion.FromToRotation(antDir, direction);
                    _legs[idPata].Bones[i].rotation = rot * _legs[idPata].Bones[i].rotation;
                }
            }
            #endregion
        }
    }

}
