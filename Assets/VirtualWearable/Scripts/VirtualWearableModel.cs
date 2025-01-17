﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Experimental.VFX;
using Leap;
using Leap.Unity;
using static Leap.Finger;

namespace VW
{
    public class VirtualWearableModel: MonoBehaviour
    {
        public GuidReference vwUI; //Virtual Wearable UI
        public GuidReference icons;
        public GuidReference particleExplosionVFX;

        private GameObject arm, armUI, palmUI, firstHandWingUI, secondHandWingUI;
        private GameObject palmUIIcons, firstAppIcons, secondAppIcons;

        public float HAND_AJUST__TOWARDS_FINGER = -0.058f;
        public float HAND_AJUST__TOWARDS_THUMB = 0.0045f;
        public const float ARM_WIDTH_METER_IN_BLENDER = 6.35024f * 0.01f; // = 6.35024cm
        public const float ARM_LENGTH_METER_IN_BLENDER = 25.6461f * 0.01f; // = 25.6461cm

        private HandUtil handUtil;
        public HandUtil handUtilAccess {
            get { return handUtil; }
        }
        public Transform playerHeadTransform;

        private bool isVisibleVirtualWearable;
        public bool IsVisibleVirtualWearable { get { return isVisibleVirtualWearable; } }

        public void Start() {
            this.arm = this.vwUI.gameObject.transform.Find("Arm").gameObject;
            this.armUI = this.vwUI.gameObject.transform.Find("ArmUI").gameObject;
            this.palmUI = this.vwUI.gameObject.transform.Find("PalmUI").gameObject;
            this.firstHandWingUI = this.vwUI.gameObject.transform.Find("FirstHandWingUI").gameObject;
            this.secondHandWingUI = this.vwUI.gameObject.transform.Find("SecondHandWingUI").gameObject;
            this.palmUIIcons = this.icons.gameObject.transform.Find("PalmUIIcons").gameObject;
            this.firstAppIcons = this.icons.gameObject.transform.Find("FirstAppIcons").gameObject;
            this.secondAppIcons = this.icons.gameObject.transform.Find("SecondAppIcons").gameObject;

            //Disable mesh
            Util.EnableMeshRendererRecursively(this.firstAppIcons, false);
            Util.EnableMeshRendererRecursively(this.secondAppIcons, false);

            /*
            Debug.Log("d1");
            foreach (Transform child in this.firstHandWingUI.transform) {
                if (child.GetChildCount() > 0) {
                    Transform grandChild = child.GetChild(0);
                    Debug.Log(grandChild.localRotation);
                }
            }
            */

            this.MoveChildren(this.firstAppIcons, this.firstHandWingUI);
            this.MoveChildren(this.secondAppIcons, this.secondHandWingUI);
            this.MoveChildren(this.palmUIIcons, this.palmUI);

            /*
            Debug.Log("d2");
            foreach (Transform child in this.firstHandWingUI.transform) {
                if (child.GetChildCount() > 0) {
                    Transform grandChild = child.GetChild(0);
                    Debug.Log(grandChild.localRotation);
                }
            }*/

            this.handUtil = new HandUtil(playerHeadTransform);
            Debug.Log("handUtil: " + handUtil);

            this.isVisibleVirtualWearable = false;
        }

        private void MoveChildren(GameObject sourceParent, GameObject targetParent)
        {
            //Debug.Log("num of children: " + sourceParent.gameObject.transform.childCount);
            for (int i = sourceParent.gameObject.transform.childCount - 1; i >= 0; i--)
            {
                Transform s = sourceParent.transform.GetChild(i);
                Transform t = targetParent.transform.GetChild(i);
                s.parent = t;
                s.localPosition = Vector3.zero;
                //s.localRotation = Quaternion.AngleAxis(270, Vector3.left);

                //s.localRotation = Quaternion.Euler(0, 0, 0);

                /*
                Debug.Log("Name: " + sourceParent.gameObject.name);
                Debug.Log("Child Name: " + s.gameObject.name);
                Debug.Log("Rotation1: " + s.localRotation);*/
            }
        }

        public void AdjustVirtualWearable(Hand hand)
        {
            Vector3 palmPosition = HandUtil.ToVector3(hand.PalmPosition);
            Vector3 directionTowardsFinger = HandUtil.ToVector3(hand.Direction);
            Vector3 handNormal = HandUtil.ToVector3(hand.PalmNormal);

            // Hand position & rotation
            //https://docs.unity3d.com/ScriptReference/Vector3.Cross.html
            Vector3 directionTowardsThumb = Vector3.Cross(handNormal, directionTowardsFinger).normalized;
            this.vwUI.gameObject.transform.position = palmPosition + directionTowardsFinger * HAND_AJUST__TOWARDS_FINGER;
            this.vwUI.gameObject.transform.position += directionTowardsThumb * HAND_AJUST__TOWARDS_THUMB;
            this.vwUI.gameObject.transform.rotation = HandUtil.ToQuaternion(hand.Rotation) *
                Quaternion.AngleAxis(180, Vector3.forward) *
                Quaternion.AngleAxis(180, Vector3.up);

            // Arm position, rotation, and scale
            this.arm.transform.rotation = HandUtil.ToQuaternion(hand.Arm.Rotation) * Quaternion.AngleAxis(270, Vector3.left);
            this.armUI.transform.rotation = HandUtil.ToQuaternion(hand.Arm.Rotation) * Quaternion.AngleAxis(270, Vector3.left);
            this.arm.transform.localScale = new Vector3(
                hand.Arm.Width / ARM_WIDTH_METER_IN_BLENDER,
                hand.Arm.Length / ARM_LENGTH_METER_IN_BLENDER,
                1);
            //Debug.Log("width: " + hand.Arm.Width);
            //Debug.Log("length: " + hand.Arm.Length);

            /*
            // Icons
            for (int i = 0; i < this.palmUIIcons.gameObject.transform.childCount; i++)
            {
                this.palmUIIcons.gameObject.transform.GetChild(i).position = this.palmUI.gameObject.transform.GetChild(i).position;
                this.palmUIIcons.gameObject.transform.GetChild(i).rotation = this.palmUI.gameObject.transform.GetChild(i).rotation * Quaternion.AngleAxis(270, Vector3.left); // * this.vwUI.gameObject.transform.rotation;
            }
            for (int i = 0; i < this.firstAppIcons.gameObject.transform.childCount; i++)
            {
                this.firstAppIcons.gameObject.transform.GetChild(i).position = this.firstHandWingUI.gameObject.transform.GetChild(i).position;
                this.firstAppIcons.gameObject.transform.GetChild(i).rotation = this.firstHandWingUI.gameObject.transform.GetChild(i).rotation * Quaternion.AngleAxis(270, Vector3.left); // * this.vwUI.gameObject.transform.rotation;
            }
            for (int i = 0; i < this.secondAppIcons.gameObject.transform.childCount; i++)
            {
                this.secondAppIcons.gameObject.transform.GetChild(i).position = this.secondHandWingUI.gameObject.transform.GetChild(i).position;
                this.secondAppIcons.gameObject.transform.GetChild(i).rotation = this.secondHandWingUI.gameObject.transform.GetChild(i).rotation * Quaternion.AngleAxis(270, Vector3.left); // * this.vwUI.gameObject.transform.rotation;
            }
            */

            //VFX
            this.particleExplosionVFX.gameObject.transform.position = this.vwUI.gameObject.transform.position;
            this.particleExplosionVFX.gameObject.transform.rotation = this.vwUI.gameObject.transform.rotation * Quaternion.AngleAxis(90, Vector3.left);
        }

        public void VisibleVirtualWearable(bool isVisible) {
            Util.EnableMeshRendererRecursively(this.vwUI.gameObject, isVisible);
            Util.EnableMeshRendererRecursively(this.icons.gameObject, isVisible);
            this.isVisibleVirtualWearable = isVisible;
        }

    }
}
