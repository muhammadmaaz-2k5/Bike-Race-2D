using System.Collections.Generic;
using UnityEngine;
using DirtDash.Core;

namespace DirtDash.Track
{
    /// <summary>
    /// Builds dynamic track collision (EdgeCollider2D), visual surface line (LineRenderer),
    /// and solid filled terrain mesh underneath the track.
    /// Can load from LevelData or programmatically generated point lists.
    /// </summary>
    [RequireComponent(typeof(EdgeCollider2D))]
    public class TrackBuilder : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private LevelData activeLevelData;
        [SerializeField] private PhysicsMaterial2D trackPhysicsMaterial;
        [SerializeField] private float lineWidth = 0.22f;
        [SerializeField] private Color trackLineColor = new Color(0.95f, 0.60f, 0.15f, 1f);
        [SerializeField] private Color groundFillColor = new Color(0.18f, 0.22f, 0.28f, 1f);
        [SerializeField] private float groundBaselineY = -30f;

        [Header("Prefabs")]
        [SerializeField] private GameObject finishLinePrefab;
        [SerializeField] private GameObject coinPrefab;

        [Header("Runtime References")]
        [SerializeField] private EdgeCollider2D edgeCollider;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private MeshFilter groundMeshFilter;
        [SerializeField] private MeshRenderer groundMeshRenderer;

        public LevelData ActiveLevelData => activeLevelData;

        private void Awake()
        {
            EnsureComponents();
        }

        private void EnsureComponents()
        {
            if (edgeCollider == null) edgeCollider = GetComponent<EdgeCollider2D>();
            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

            if (groundMeshFilter == null)
            {
                var meshGo = new GameObject("GroundFillMesh");
                meshGo.transform.SetParent(transform, false);
                groundMeshFilter = meshGo.AddComponent<MeshFilter>();
                groundMeshRenderer = meshGo.AddComponent<MeshRenderer>();
                groundMeshRenderer.material = new Material(Shader.Find("Sprites/Default")) { color = groundFillColor };
            }
        }

        /// <summary>
        /// Builds the entire level from a LevelData asset.
        /// </summary>
        public void BuildLevel(LevelData levelData)
        {
            EnsureComponents();
            activeLevelData = levelData;

            if (levelData == null || levelData.trackPoints == null || levelData.trackPoints.Count < 2)
            {
                Debug.LogWarning("[Dirt Dash] TrackBuilder: Insufficient track points to build.");
                return;
            }

            BuildTrack(levelData.trackPoints);
            SpawnFinishLine(levelData);
            SpawnCoins(levelData);
        }

        /// <summary>
        /// Builds collision, line renderer outline, and solid ground mesh from a coordinate list.
        /// </summary>
        public void BuildTrack(List<Vector2> points)
        {
            EnsureComponents();
            if (points == null || points.Count < 2) return;

            // 1. Collision
            edgeCollider.points = points.ToArray();
            if (trackPhysicsMaterial != null)
            {
                edgeCollider.sharedMaterial = trackPhysicsMaterial;
            }

            // 2. LineRenderer Outline
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                lineRenderer.SetPosition(i, new Vector3(points[i].x, points[i].y, 0f));
            }
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;

            if (lineRenderer.material == null || lineRenderer.material.shader.name == "")
            {
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
            lineRenderer.startColor = trackLineColor;
            lineRenderer.endColor = trackLineColor;

            // 3. Solid Ground Mesh Fill
            bool showSolidGround = SaveSystem.Instance == null || SaveSystem.Instance.Data.solidGround;
            if (showSolidGround)
            {
                GenerateGroundMesh(points);
            }
            else if (groundMeshRenderer != null)
            {
                groundMeshRenderer.enabled = false;
            }
        }

        /// <summary>
        /// Constructs a 2D polygonal triangle-strip mesh filling the space beneath the track outline.
        /// </summary>
        private void GenerateGroundMesh(List<Vector2> points)
        {
            if (groundMeshFilter == null) return;

            int count = points.Count;
            Vector3[] vertices = new Vector3[count * 2];
            int[] triangles = new int[(count - 1) * 6];
            Vector2[] uvs = new Vector2[count * 2];

            for (int i = 0; i < count; i++)
            {
                // Top vertex on track line
                vertices[i * 2] = new Vector3(points[i].x, points[i].y, 0.05f); // slight Z offset behind line
                // Bottom vertex at baseline depth
                vertices[i * 2 + 1] = new Vector3(points[i].x, groundBaselineY, 0.05f);

                uvs[i * 2] = new Vector2((float)i / count, 1f);
                uvs[i * 2 + 1] = new Vector2((float)i / count, 0f);

                if (i < count - 1)
                {
                    int root = i * 2;
                    int triIndex = i * 6;

                    // Quad: (root, root+1, root+2) & (root+1, root+3, root+2)
                    triangles[triIndex + 0] = root;
                    triangles[triIndex + 1] = root + 2;
                    triangles[triIndex + 2] = root + 1;

                    triangles[triIndex + 3] = root + 1;
                    triangles[triIndex + 4] = root + 2;
                    triangles[triIndex + 5] = root + 3;
                }
            }

            Mesh mesh = new Mesh
            {
                name = "GeneratedGroundMesh",
                vertices = vertices,
                triangles = triangles,
                uv = uvs
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            groundMeshFilter.mesh = mesh;
            groundMeshRenderer.enabled = true;
        }

        private void SpawnFinishLine(LevelData levelData)
        {
            if (finishLinePrefab != null)
            {
                var fl = Instantiate(finishLinePrefab, levelData.finishPosition, Quaternion.identity, transform);
                var comp = fl.GetComponent<FinishLine>();
                if (comp != null) comp.SetLevelData(levelData);
            }
            else
            {
                // Procedural finish line trigger GameObject
                var flGo = new GameObject("FinishLineTrigger");
                flGo.transform.SetParent(transform);
                flGo.transform.position = levelData.finishPosition;
                var col = flGo.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1.5f, 8f);
                col.offset = new Vector2(0f, 4f);
                var comp = flGo.AddComponent<FinishLine>();
                comp.SetLevelData(levelData);

                // Visual checker marker
                var line = flGo.AddComponent<LineRenderer>();
                line.useWorldSpace = false;
                line.positionCount = 2;
                line.SetPosition(0, Vector3.zero);
                line.SetPosition(1, new Vector3(0f, 8f, 0f));
                line.startWidth = 0.35f;
                line.endWidth = 0.35f;
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = Color.white;
                line.endColor = Color.yellow;
            }
        }

        private void SpawnCoins(LevelData levelData)
        {
            if (levelData.coinPositions == null) return;

            foreach (var pos in levelData.coinPositions)
            {
                if (coinPrefab != null)
                {
                    Instantiate(coinPrefab, pos, Quaternion.identity, transform);
                }
                else
                {
                    // Procedural coin pickup
                    var coinGo = new GameObject("Coin");
                    coinGo.transform.SetParent(transform);
                    coinGo.transform.position = pos;
                    var col = coinGo.AddComponent<CircleCollider2D>();
                    col.isTrigger = true;
                    col.radius = 0.45f;
                    coinGo.AddComponent<Coin>();

                    // Visual gold ring
                    var line = coinGo.AddComponent<LineRenderer>();
                    line.useWorldSpace = false;
                    line.loop = true;
                    int segments = 16;
                    line.positionCount = segments;
                    for (int s = 0; s < segments; s++)
                    {
                        float rad = (float)s / segments * Mathf.PI * 2f;
                        line.SetPosition(s, new Vector3(Mathf.Cos(rad) * 0.35f, Mathf.Sin(rad) * 0.35f, 0f));
                    }
                    line.startWidth = 0.12f;
                    line.endWidth = 0.12f;
                    line.material = new Material(Shader.Find("Sprites/Default"));
                    line.startColor = new Color(1f, 0.85f, 0.1f, 1f);
                    line.endColor = new Color(1f, 0.75f, 0.05f, 1f);
                }
            }
        }

        public void SetThemeColors(Color lineCol, Color groundCol)
        {
            trackLineColor = lineCol;
            groundFillColor = groundCol;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = lineCol;
                lineRenderer.endColor = lineCol;
            }
            if (groundMeshRenderer != null && groundMeshRenderer.material != null)
            {
                groundMeshRenderer.material.color = groundCol;
            }
        }
    }
}
