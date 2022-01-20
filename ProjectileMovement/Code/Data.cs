using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KProjectile
{
    public enum LineVisibileMode { LineRenderer, SegmentedPrefab, Both }
    public enum Interpolation { Movetowards, SmoothDamp }

    [System.Serializable]
    public class SegmentDescription
    {
        [SerializeField] Transform prefab = null;
        [SerializeField] int count = 100;
        [SerializeField] bool shouldOrient = false;
        [SerializeField] bool scaleOverride = false;
        [SerializeField] Vector3 overridenScale = Vector3.one;
        List<GameObject> segments = null;
        public Transform SegmentPrefab { get { return prefab; } set { prefab = value; } }
        public int SegmentCount { get { return count; } set { count = value; } }
        public bool SegmentShouldOrient { get { return shouldOrient; } set { shouldOrient = value; } }
        public bool SegmentScaleOverride { get { return scaleOverride; } set { scaleOverride = value; } }
        public Vector3 SegmentOverridenScale { get { return overridenScale; } set { overridenScale = value; } }
        public List<GameObject> Segments { get { return segments; } set { segments = value; } }
    }

    [System.Serializable]
    public class ProjectileLine
    {
        [SerializeField] LineVisibileMode mode = LineVisibileMode.LineRenderer;
        [SerializeField] SegmentDescription segment;
        [SerializeField] LineRenderer renderer;
        Vector3[] lineLocalPoints = null;
        Transform _tr = null;
        bool enabled = true; 

        void CheckLineRendererData(Vector3[] worldPositions)
        {
            if (lineLocalPoints == null || lineLocalPoints.Length != worldPositions.Length || ReferenceEquals(_tr, null))
            {
                lineLocalPoints = new Vector3[worldPositions.Length];
                renderer.positionCount = lineLocalPoints.Length;
                _tr = renderer.transform;
            }
        }

        void SetEnableSegments(bool enable)
        {
            if (segment != null)
            {
                var segments = segment.Segments;
                if (segments != null && segments.Count > 0)
                {
                    for (int i = 0; i < segments.Count; i++)
                    {
                        var seg = segments[i];
                        if (seg == null) { continue; }
                        seg.SetActive(enable);
                    }
                }
            }
        }

        void CheckSegmentData()
        {
            if (segment != null && segment.SegmentPrefab != null)
            {
                var segmentCount = segment.SegmentCount;
                var segmentPrefab = segment.SegmentPrefab;
                if (segment.Segments == null || segment.Segments.Count == 0 || segmentCount != segment.Segments.Count)
                {
                    if (segment.Segments != null && segment.Segments.Count > 0)
                    {
                        for (int i = 0; i < segment.Segments.Count; i++)
                        {
                            var seg = segment.Segments[i];
                            if (seg == null) { continue; }
                            GameObject.Destroy(seg);
                        }
                    }
                    segment.Segments = new List<GameObject>();
                    for (int i = 0; i < segmentCount; i++)
                    {
                        var seg = GameObject.Instantiate(segmentPrefab) as Transform;
                        seg.SetParent(_tr, true);
                        seg.localPosition = Vector3.zero;
                        seg.localRotation = segmentPrefab.localRotation;
                        seg.localScale = segmentPrefab.localScale;
                        seg.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                        segment.Segments.Add(seg.gameObject);
                    }
                }
            }
        }

        public SegmentDescription Segment { get { return segment; } }

        public void SetMode(LineVisibileMode mode)
        {
            this.mode = mode;
            CheckSegmentData();
        }

        public void SetSegmentCount(int count)
        {
            if (segment != null)
            {
                segment.SegmentCount = count;
            }
            CheckSegmentData();
        }

        public ProjectileLine(LineRenderer renderer, Transform segmentPrefab, int segmentCount)
        {
            _tr = renderer.transform;
            this.renderer = renderer;
            this.segment = new SegmentDescription()
            {
                SegmentCount = segmentCount,
                SegmentPrefab = segmentPrefab
            };
        }

        public void SetEnable(bool enable)
        {
            if (mode == LineVisibileMode.SegmentedPrefab || mode == LineVisibileMode.Both)
            {
                CheckSegmentData();
                SetEnableSegments(enable);
            }

            if (mode == LineVisibileMode.LineRenderer || mode == LineVisibileMode.Both)
            {
                renderer.enabled = enable;
            }
            this.enabled = enable;
        }

        public void UpdateLine(Vector3[] worldPositions)
        {
            if (!enabled || worldPositions == null || worldPositions.Length == 0) { return; }
            CheckLineRendererData(worldPositions);
            CheckSegmentData();

            if (mode == LineVisibileMode.LineRenderer || mode == LineVisibileMode.Both)
            {
                if (lineLocalPoints.Length > 0)
                {
                    for (int i = 0; i < lineLocalPoints.Length; i++)
                    {
                        lineLocalPoints[i] = _tr.worldToLocalMatrix.MultiplyPoint(worldPositions[i]);
                    }
                }
                renderer.SetPositions(lineLocalPoints);
            }

            renderer.enabled = mode == LineVisibileMode.LineRenderer || mode == LineVisibileMode.Both;
            SetEnableSegments(mode == LineVisibileMode.Both || mode == LineVisibileMode.SegmentedPrefab);
            if (mode == LineVisibileMode.SegmentedPrefab || mode == LineVisibileMode.Both)
            {
                if (segment != null && segment.SegmentCount > 0)
                {
                    var segmentCount = segment.SegmentCount;
                    int interval = worldPositions.Length / segmentCount;
                    if (interval < 1) { interval = 1; }
                    interval++;
                    int id = 0;
                    for (int i = 0; i < worldPositions.Length; i++)
                    {
                        if (i % interval != 0) { continue; }
                        if (id < segment.Segments.Count && segment.Segments[id] != null)
                        {
                            var s = segment.Segments[id];
                            s.transform.position = worldPositions[i];
                            if (segment.SegmentScaleOverride)
                            {
                                s.transform.localScale = segment.SegmentOverridenScale;
                            }

                            if (segment.SegmentShouldOrient && (i + 1) < worldPositions.Length)
                            {
                                var p1 = worldPositions[i];
                                var p2 = worldPositions[i + 1];
                                var dir = p2 - p1;
                                if (Mathf.Approximately(dir.magnitude, 0.0f) == false)
                                {
                                    var qot = Quaternion.LookRotation(dir);
                                    s.transform.rotation = qot;
                                }
                            }
                        }
                        id++;
                    }
                }
            }
        }
    }
}