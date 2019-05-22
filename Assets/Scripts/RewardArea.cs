﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// The poster, which is a mesh, requires a material to show a picture.
/// Therefore make sure that the cueImage (Sprite) in this script matches the
/// material unless the experiment requires it to be different.
/// </summary>
public class RewardArea : MonoBehaviour {
    //Drag in Unity Editor
    /// <summary>
    /// // image to display as cue
    /// </summary>
    public Sprite cueImage;

    /// <summary>
    /// Target of the Reward Area
    /// </summary> 
    public Transform target;

    /// <summary>
    /// optional blinkLight
    /// </summary>
    public Light blinkLight;

    /// <summary>
    /// viewing angle required to register if the target is in sight
    /// </summary>
    public static float requiredViewAngle = 110f;

    /// <summary>
    /// Minimum valid distance from the target.
    /// </summary>
    public static float requiredDistance = 2f;

    /// <summary>
    /// All rewards use the same trigger event. RewardArea script will be returned for extra processing
    /// </summary>
    /// <param name="rewardArea">Script of RewardArea that is triggered</param>
    public delegate void OnRewardTriggeredEvent(RewardArea rewardArea);
    public static event OnRewardTriggeredEvent OnRewardTriggered;

    private float lightIntensity = 1;
    private bool blinkState;
    private Coroutine blinkCoroutine; // reference to properly stop the coroutine
    private WaitForSecondsRealtime blinkHalfPeriod = new WaitForSecondsRealtime(1f);

    private const string Format_NoRewardAreaComponentFound = "{0} does not have a RewardAreaComponent but is tagged as a reward";

    private void Start() {
        if (blinkLight != null) {
            blinkLight.intensity = 0;// off at the start
            blinkState = false;
        }
    }

    private void OnTriggerStay(Collider other) {
        CheckFieldOfView(other.transform);
    }

    /// <summary>
    /// Checks if the poster is within the robot's field of view.
    /// </summary>
    /// <param name="robot">Current transform of the robot</param>
    private void CheckFieldOfView(Transform robot) {
        Vector3 direction = target.position - robot.position;
        direction.y = 0; // ignore y axis

        float angle = Vector3.Angle(direction, robot.forward);

        //1.588 is estimated by logging the distance of the target and robot 
        //position when the robot is pressing itself against the target
        float distanceWithOffset = requiredDistance + 1.588f; 

        //uncomment to see the required view in the scene tab
        Vector3 left = Quaternion.AngleAxis(-requiredViewAngle / 2f, Vector3.up) * robot.forward * distanceWithOffset;
        Vector3 right = Quaternion.AngleAxis(requiredViewAngle / 2f, Vector3.up) * robot.forward * distanceWithOffset;
        Debug.DrawRay(robot.position, left, Color.black);
        Debug.DrawRay(robot.position, right, Color.black);
        Debug.DrawRay(robot.position, direction.normalized * distanceWithOffset, Color.cyan);

        //check if in view angle
        if (angle < requiredViewAngle * 0.5f) {
            //checks if close enough
            if (Vector3.Distance(target.position, robot.position) <= distanceWithOffset) {
                Debug.Log("Reward!!!");
                OnRewardTriggered?.Invoke(this);
            }
            else {
                Debug.Log("inView!!!" + Vector3.Distance(target.position, robot.position) + " " + distanceWithOffset);
            }
        }
    }

    public RobotMovement.RobotMovementEvent Haha(Transform t) {
        return null;
    }

    /// <summary>
    /// Helper method to access GameObject.SetActive
    /// </summary>
    /// <param name="value"></param>
    public void SetActive(bool value) {
        gameObject.SetActive(value);
    }

    public void StartBlinking() {
        if (blinkLight != null) {
            blinkCoroutine = StartCoroutine(Blink());
        }
    }

    public void StopBlinking() {
        if (blinkCoroutine != null) {
            StopCoroutine(blinkCoroutine);
        }

        if (blinkLight != null) {
            blinkLight.intensity = 0;
        }
    }

    private IEnumerator Blink() {
        while (true) {
            if (blinkState) {
                blinkLight.intensity = 0;
            }
            else {
                blinkLight.intensity = lightIntensity;
            }
        }
    }

    public static RewardArea[] GetAllRewardsFromScene() {
        //Find all rewardAreas in scene and populate rewards[].
        GameObject[] objs = GameObject.FindGameObjectsWithTag(Tags.RewardArea);
        RewardArea[] tempArr = new RewardArea[objs.Length];
        for (int i = 0; i < objs.Length; i++) {
            RewardArea area = objs[i].GetComponent<RewardArea>();
            if (area != null) {
                tempArr[i] = area;

                // Deactivate all rewards at the start.
                area.SetActive(false);
            }
            else {
                Debug.LogWarning(string.Format(Format_NoRewardAreaComponentFound, objs[0].name));
            }
        }
        return tempArr;
    }
}