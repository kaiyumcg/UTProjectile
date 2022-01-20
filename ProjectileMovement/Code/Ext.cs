using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KProjectile
{
    public static class Ext
    {
        public static void CalculateProjectilePath(this Transform transform, Transform destination, ref Vector3[] points, int steps,
            float launchingAngle, float gravity = -9.81f)
        {
            Vector3 direction = destination.position - transform.position;
            direction = direction.normalized;
            float range = Vector3.Distance(transform.position, destination.position);
            CalculateProjectilePath(transform, ref points, steps, launchingAngle, direction, range, gravity);
        }

        public static void CalculateProjectilePath(this Transform transform, ref Vector3[] points, int steps,
            float launchingAngle, Vector3 direction, float range, float gravity = -9.81f)
        {
            if (steps <= 0 || launchingAngle < 1f || range < 1f) { return; }
            Vector3 origin = transform.position;
            if (points == null || points.Length != steps) { points = new Vector3[steps]; }
            var initialVelocity = GetInitialVelocity();
            var totalTime = (2f * initialVelocity * Mathf.Sin(launchingAngle * Mathf.Deg2Rad)) / Mathf.Abs(gravity);
            totalTime *= 1.1f;
            for (int i = 0; i < steps; i++)
            {
                float t = ((float)i / (float)steps) * totalTime;
                float x = initialVelocity * t * Mathf.Cos(launchingAngle * Mathf.Deg2Rad);
                float y = initialVelocity * t * Mathf.Sin(launchingAngle * Mathf.Deg2Rad) - 0.5f * Mathf.Abs(gravity) * t * t;
                var pos = origin + direction.normalized * x + Vector3.up * y;
                points[i] = pos;
            }

            float GetInitialVelocity()
            {
                var rg = range * Mathf.Abs(gravity);
                var twoSinTheta = Mathf.Sin(2 * launchingAngle * Mathf.Deg2Rad);
                return Mathf.Sqrt(rg / twoSinTheta);
            }
        }

        public static void CalculateProjectilePath(this Rigidbody rigidbody, Transform destination, ref Vector3[] points, int steps,
           float launchingAngle, float gravity = -9.81f)
        {
            CalculateProjectilePath(rigidbody.transform, destination, ref points, steps, launchingAngle, gravity);
        }

        public static void CalculateProjectilePath(this Rigidbody rigidbody, ref Vector3[] points, int steps,
            float launchingAngle, Vector3 direction, float range, float gravity = -9.81f)
        {
            CalculateProjectilePath(rigidbody.transform, ref points, steps, launchingAngle, direction, range, gravity);
        }

        public static float MoveTowards(this float from, float target, float speed)
        {
            var v1 = new Vector3(from, 0.0f, 0.0f);
            var v2 = new Vector3(target, 0.0f, 0.0f);
            var resultVec = Vector3.zero;
            resultVec = Vector3.MoveTowards(v1, v2, speed);
            return resultVec.x;
        }

        public static float SmoothDamp(this float from, float target, float smoothTime, ref Vector3 velocity)
        {
            var v1 = new Vector3(from, 0.0f, 0.0f);
            var v2 = new Vector3(target, 0.0f, 0.0f);
            var resultVec = Vector3.zero;
            resultVec = Vector3.SmoothDamp(v1, v2, ref velocity, smoothTime);
            return resultVec.x;
        }

        static float RemapTo01(this float val)
        {
            var OldRange = (1f - (-1f));
            var NewValue = 0f;
            if (Mathf.Approximately(OldRange, 0f))
            {
                NewValue = 0f;
            }
            else
            {
                var NewRange = (1f - 0f);
                NewValue = (((val - (-1f)) * NewRange) / OldRange) + 0f;
            }
            return NewValue;
        }

        public static float RemapMinusOneToOne(this float from, float to, float currentNormalizedValueInMinusOneAndOne)
        {
            var t = currentNormalizedValueInMinusOneAndOne.RemapTo01();
            return Mathf.Lerp(from, to, t);
        }

        public static Quaternion RemapMinusOneToOne(this Quaternion from, Quaternion to, float currentNormalizedValueInMinusOneAndOne)
        {
            var t = currentNormalizedValueInMinusOneAndOne.RemapTo01();
            return Quaternion.Lerp(from, to, t);
        }
    }
}