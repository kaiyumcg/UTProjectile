using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Jobs;

namespace KProjectile
{
    public class ProjectileControl : MonoBehaviour
    {
        [Tooltip("Gravity of the projectile path")]
        [SerializeField] float gravity = -9.81f;
        [Tooltip("How smooth the overall projectile trajectory would be")]
        [SerializeField] int steps = 1000;

        [Header("Player Control Section")]
        [SerializeField] bool enableSnappingFeature = false;
        [SerializeField] bool snapDetectWithTouchPosition = false;
        [SerializeField] LayerMask snapLayer;
        [SerializeField, Range(0f, 10f)] float horInputModulation = 4f;
        [SerializeField, Range(0f, 10f)] float vertInputModulation = 4f;
        [SerializeField] float maxRange = 200f, minRange = 5f;
        [SerializeField] float maxAngle = 80f, minAngle = 5f;
        [SerializeField] float leftRotation, rightRotation;
        
        [Header("View Section")]
        [SerializeField] ProjectileLine lineView;

        [SerializeField] Text dbView;

        NativeArray<RaycastHit> snap_physx_results;
        NativeArray<RaycastCommand> snap_physx_commands;
        const float snapRayDistance = 1.5f;
        const float snapTime = 0.5f;
        Camera cam;
        Transform _transform;
        bool isPossessedByPlayer = false;
        public delegate bool UnpossessCondition();

        void Awake()
        {
            cam = Camera.main;
            _transform = transform;
            snap_physx_results = new NativeArray<RaycastHit>(steps - 1, Allocator.Persistent);
            snap_physx_commands = new NativeArray<RaycastCommand>(steps - 1, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            // Dispose the buffers
            snap_physx_results.Dispose();
            snap_physx_commands.Dispose();
        }

        public void StartPlayerInput(System.Action<Vector3[]> OnComplete)
        {
            StopAllCoroutines();
            isPossessedByPlayer = true;
            Vector3[] points = null;
            bool willSnap = true;
            bool snapSwitch1 = false;
            bool snapSwitch2 = false;
            float snapTimer = 0.0f;

            Quaternion leftRot = Quaternion.Euler(new Vector3(0.0f, leftRotation, 0.0f));
            Quaternion rightRot = Quaternion.Euler(new Vector3(0.0f, rightRotation, 0.0f));
            float range = 0.0f;
            float angle = 0.0f;
            Vector3 originalMPos = Input.mousePosition;
            StartCoroutine(InputCycler());
            IEnumerator InputCycler()
            {
                while (true)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        OnComplete?.Invoke(points);
                        break;
                    }

                    if (snapSwitch1)
                    {
                        snapTimer += Time.deltaTime;
                        if (snapTimer > snapTime)
                        {
                            willSnap = false;
                            snapSwitch1 = false;
                            snapSwitch2 = true;
                            snapTimer = 0.0f;
                        }
                    }

                    if (snapSwitch2)
                    {
                        snapTimer += Time.deltaTime;
                        if (snapTimer > snapTime)
                        {
                            willSnap = true;
                            snapSwitch2 = false;
                            snapTimer = 0.0f;
                        }
                    }

                    if (enableSnappingFeature && snapDetectWithTouchPosition)
                    {
                        willSnap = true;
                        snapSwitch1 = snapSwitch2 = false;
                        snapTimer = 0.0f;
                    }

                    Transform snapTarget = null;
                    if (enableSnappingFeature && points != null && points.Length > 1 && steps > 1 && steps == points.Length && willSnap)
                    {
                        if (snapDetectWithTouchPosition)
                        {
                            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                            RaycastHit hit;
                            if (Physics.Raycast(ray, out hit, Mathf.Infinity, snapLayer))
                            {
                                snapTarget = hit.transform;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < points.Length; i++)
                            {
                                if (i == points.Length - 1) { continue; }
                                var ray_origin = points[i];
                                var ray_direction = (points[i + 1] - points[i]).normalized;
                                snap_physx_commands[i] = new RaycastCommand(ray_origin, ray_direction, snapRayDistance, snapLayer);
                                snap_physx_results[i] = new RaycastHit();
                            }

                            JobHandle handle = RaycastCommand.ScheduleBatch(snap_physx_commands, snap_physx_results, 1, default(JobHandle));
                            handle.Complete();
                            for (int i = 0; i < snap_physx_results.Length; i++)
                            {
                                var res = snap_physx_results[i];
                                if (res.collider != null)
                                {
                                    snapTarget = res.transform;
                                    if (snapSwitch1 == false)
                                    {
                                        snapSwitch1 = true;
                                        snapTimer = 0.0f;
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    var curMPos = Input.mousePosition;
                    var MDir = curMPos - originalMPos;
                    var vertInput = Mathf.Clamp01(Mathf.Abs(MDir.y) / (float)Screen.height * vertInputModulation) * (curMPos.y >= originalMPos.y ? 1f : -1f); 
                    var horInput = Mathf.Clamp01(Mathf.Abs(MDir.x) / (float)Screen.width * horInputModulation) * (curMPos.x >= originalMPos.x ? 1f : -1f);

                    dbView.text = "h: " + horInput.ToString("0.0") + " and v: " + vertInput.ToString("0.0");
                    range = minRange.RemapMinusOneToOne(maxRange, vertInput);
                    angle = minAngle.RemapMinusOneToOne(maxAngle, vertInput);
                    if (enableSnappingFeature && snapTarget != null)
                    {
                        _transform.CalculateProjectilePath(snapTarget, ref points, steps, angle, gravity);
                    }
                    else
                    {
                        _transform.CalculateProjectilePath(ref points, steps, angle, transform.forward, range, gravity);
                    }

                    var targetRot = leftRot.RemapMinusOneToOne(rightRot, horInput);
                    _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRot, 30f * Time.deltaTime);
                    lineView.SetEnable(true);
                    lineView.UpdateLine(points);
                    yield return null;
                }
            }
        }

        public void TurnOffAll()
        {
            StopAllCoroutines();
            lineView.SetEnable(false);
        }

        public Vector3[] NPCPossessTarget(Transform target, float angle, UnpossessCondition unpossessCondition = null)
        {
            StopAllCoroutines();
            isPossessedByPlayer = false;
            Vector3[] points = null;
            _transform.CalculateProjectilePath(target, ref points, steps, angle, gravity);
            StartCoroutine(PossesCycler());
            IEnumerator PossesCycler()
            {
                while (true)
                {
                    var stop = false;
                    if (unpossessCondition != null) { stop = unpossessCondition.Invoke(); }
                    if (stop)
                    {
                        break;
                    }
                    lineView.SetEnable(true);
                    lineView.UpdateLine(points);
                    yield return null;
                }
            }
            return points;
        }

        public ProjectileLine GetView() { return lineView; }

        public bool IsPossessedByPlayer { get { return isPossessedByPlayer; } }

        #region Movement
        /// <summary>
        /// Does not work now
        /// </summary>
        /// <param name="rigidbody"></param>
        /// <param name="speed"></param>
        /// <param name="pts"></param>
        /// <param name="isLooping"></param>
        /// <param name="OnCompleteLap"></param>
        internal void TravelNonKinematicRigidbodyAtUniformSpeed(Rigidbody rigidbody, float speed, Vector3[] pts,
            bool isLooping = false, System.Action OnCompleteLap = null)
        {
            //var runner = gameObject.AddComponent<AsyncRunner>();
            //runner.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            //Util.TravelNonKinematicRigidbodyAtUniformSpeed(runner, rigidbody, speed, pts, isLooping, () =>
            //{
            //    Destroy(runner);
            //    OnCompleteLap?.Invoke();
            //});
        }

        public void TravelAlongProjectilePath(Transform rigidbody, float speed, Vector3[] pts,
            bool isLooping = false, System.Action OnCompleteLap = null)
        {
            var runner = gameObject.AddComponent<AsyncRunner>();
            runner.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            Util.TravelKinematicRigidbodyAtUniformSpeed(runner, rigidbody, speed, pts, isLooping, () =>
            {
                Destroy(runner);
                OnCompleteLap?.Invoke();
            });
        }

        /// <summary>
        /// Does not work now
        /// </summary>
        /// <param name="rigidbody"></param>
        /// <param name="forceAmount"></param>
        /// <param name="pts"></param>
        /// <param name="isLooping"></param>
        /// <param name="OnCompleteLap"></param>
        internal void TravelRigidbodyWithForce(Rigidbody rigidbody, float forceAmount, Vector3[] pts,
            bool isLooping = false, System.Action OnCompleteLap = null)
        {
            //var runner = gameObject.AddComponent<AsyncRunner>();
            //runner.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            //Util.TravelRigidbodyWithForce(runner, rigidbody, forceAmount, pts, isLooping, () =>
            //{
            //    Destroy(runner);
            //    OnCompleteLap?.Invoke();
            //});
        }
        #endregion
    }
}