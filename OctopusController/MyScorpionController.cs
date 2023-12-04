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
            distancesBetweenJoints = new float[_legs[0].Bones.Length];
            jointsController = new Vector3[_legs[0].Bones.Length+1];
        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation

            tailEndEffector = _tail._endEffectorSphere;
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
                //Play animation only for 5 seconds
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
            for (int j = 0; j < 6; j++)
            {
                if (Vector3.Distance(_legs[j].Bones[0].position, legFutureBases[j].position) > distanceBetweenFutureBases)
                {
                    _legs[j].Bones[0].position = Vector3.Lerp(_legs[j].Bones[0].position, legFutureBases[j].position, 1f);
                }
                updateLegs(j);
            }

        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {
            //Only if tail end position is far away from target and if scorpion arrive to the ball
            if (Vector3.Distance(tailEndEffector.transform.position, tailTarget.transform.position) > threeshold && StartTail)
            {
                for (int i = 0; i < _tail.Bones.Length - 2; i++)
                {
                    float rotation = 0;
                    if (i == 0)
                    {
                        //Rotate first tail joint in x and z axis
                        rotation = CalculateRotation(_tail.Bones[i], new Vector3(0, 0, 1));
                        _tail.Bones[i].transform.Rotate((new Vector3(0, 0, 1) * -rotation) * tailRate);
                        rotation = CalculateRotation(_tail.Bones[i], new Vector3(1, 0, 0));
                        _tail.Bones[i].transform.Rotate((new Vector3(1, 0, 0) * -rotation) * tailRate);
                    }
                    else
                    {
                        //Rotate the other joints in only x axis

                        rotation = CalculateRotation(_tail.Bones[i], new Vector3(1, 0, 0));
                        _tail.Bones[i].transform.Rotate((new Vector3(1, 0, 0) * -rotation) * tailRate);

                    }

                }
            }
        }
        private float CalculateRotation(Transform actualJoint, Vector3 axis)
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

            SetParameters(idPata);

            float targetToRoot = Vector3.Distance(jointsController[0], legTargets[idPata].position);
            if (targetToRoot < distancesBetweenJoints.Sum())
            {

                CalculateRotations(idPata);

                //Set leg bones rotation 

                RotateLegs(idPata);
            }
            #endregion
        }

        void SetParameters(int id)
        {
            for (int i = 0; i <= _legs[id].Bones.Length - 1; i++)
            {
                jointsController[i] = _legs[id].Bones[i].position;
            }
            jointsController[_legs[id].Bones.Length] = _legs[id]._endEffectorSphere.position;

            for (int i = 0; i <= _legs[id].Bones.Length - 2; i++)
            {
                distancesBetweenJoints[i] = Vector3.Distance(_legs[id].Bones[i].position, _legs[id].Bones[i + 1].position);
            }
            distancesBetweenJoints[_legs[id].Bones.Length - 1] = Vector3.Distance(_legs[id].Bones[_legs[id].Bones.Length - 1].position, _legs[id]._endEffectorSphere.position);
        }
        void CalculateRotations(int id)
        {
            while (Vector3.Distance(jointsController[jointsController.Length - 1], legTargets[id].position) != 0 || Vector3.Distance(jointsController[0], _legs[id].Bones[0].position) != 0)
            {
                //From target to base
                jointsController[jointsController.Length - 1] = legTargets[id].position;
                for (int i = jointsController.Length - 2; i >= 0; i--)
                {
                    Vector3 vectorDirector = (jointsController[i + 1] - jointsController[i]).normalized;
                    Vector3 movementVector = vectorDirector * distancesBetweenJoints[i];
                    jointsController[i] = jointsController[i + 1] - movementVector;
                }

                //From base to target
                jointsController[0] = _legs[id].Bones[0].position;
                for (int i = 1; i < jointsController.Length - 1; i++)
                {
                    Vector3 vectorDirector = (jointsController[i - 1] - jointsController[i]).normalized;
                    Vector3 movementVector = vectorDirector * distancesBetweenJoints[i - 1];
                    jointsController[i] = jointsController[i - 1] - movementVector;

                }
            }
        }

        void RotateLegs(int id)
        {
            for (int i = 0; i <= jointsController.Length - 2; i++)
            {
                Vector3 direction = (jointsController[i + 1] - jointsController[i]).normalized;
                Vector3 antDir;
                if (i + 1 >= _legs[id].Bones.Length)
                    antDir = (_legs[id]._endEffectorSphere.position - _legs[id].Bones[i].position).normalized;
                else
                    antDir = (_legs[id].Bones[i + 1].position - _legs[id].Bones[i].position).normalized;

                Quaternion rot = Quaternion.FromToRotation(antDir, direction);
                _legs[id].Bones[i].rotation = rot * _legs[id].Bones[i].rotation;
            }
        }
    }

}
