using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KProjectile
{
    internal static class Util
    {
        //current moving which direction in delegate callback so that we can process rotation in our way inside it
        //method for transform just like rigidbody as well
        /*
        internal static void TravelNonKinematicRigidbodyAtUniformSpeed(MonoBehaviour runner, Rigidbody rigidbody, float speed, Vector3[] pts,
            bool isLooping = false, System.Action OnCompleteLap = null)
        {
            runner.StopAllCoroutines();
            runner.StartCoroutine(Movement());
            IEnumerator Movement()
            {
                if (pts == null || pts.Length == 0) { yield break; }
                int progressID = 0;
                while (true)
                {
                    if (progressID > pts.Length - 1) { rigidbody.velocity = Vector3.zero; yield break; }
                    var target = pts[progressID];
                    var vel = target - rigidbody.position;
                    vel = vel.normalized * speed;
                    rigidbody.velocity = vel;
                    if (Vector3.Distance(rigidbody.position, target) < 0.1f)
                    {
                        progressID++;
                        if (progressID > pts.Length - 1)
                        {
                            if (isLooping)
                            {
                                progressID = 0;
                            }
                            OnCompleteLap?.Invoke();
                        }
                    }

                    yield return null;
                }
            }
        }
        */

        internal static void TravelKinematicRigidbodyAtUniformSpeed(MonoBehaviour runner, Transform transform, float speed, Vector3[] pts,
            bool isLooping = false, System.Action OnCompleteLap = null)
        {
            runner.StopAllCoroutines();
            runner.StartCoroutine(Movement());
            IEnumerator Movement()
            {
                if (pts == null || pts.Length == 0) { yield break; }
                int progressID = 0;
                while (true)
                {
                    if (progressID > pts.Length - 1) { yield break; }
                    var target = pts[progressID];
                    transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                    if (Vector3.Distance(transform.position, target) < 0.1f)
                    {
                        progressID++;
                        if (progressID > pts.Length - 1)
                        {
                            if (isLooping)
                            {
                                progressID = 0;
                            }
                            OnCompleteLap?.Invoke();
                        }
                    }
                    yield return null;
                }
            }
        }

        /*
        internal static void TravelRigidbodyWithForce(MonoBehaviour runner, Rigidbody rigidbody, float forceAmount, Vector3[] pts,
            bool isLooping = false, System.Action OnCompleteLap = null)
        {
            runner.StopAllCoroutines();
            runner.StartCoroutine(Movement());
            IEnumerator Movement()
            {
                if (pts == null || pts.Length == 0) { yield break; }
                int progressID = 0;
                while (true)
                {
                    if (progressID > pts.Length - 1) { rigidbody.velocity = Vector3.zero; yield break; }
                    var target = pts[progressID];
                    var force = target - rigidbody.position;
                    force = force.normalized * forceAmount;
                    rigidbody.AddForce(force, ForceMode.Force);
                    if (Vector3.Distance(rigidbody.position, target) < 0.1f)
                    {
                        progressID++;
                        if (progressID > pts.Length - 1)
                        {
                            if (isLooping)
                            {
                                progressID = 0;
                            }
                            OnCompleteLap?.Invoke();
                        }
                    }
                    yield return null;
                }
            }
        }
        */
    }
}