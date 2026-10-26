using System.Collections.Generic;
using UnityEngine;

namespace DirtDash.Track
{
    /// <summary>
    /// Procedural mathematical track segment generators.
    /// Used by TrackBuilder and the Track Editor tool to construct tracks from point lists.
    /// </summary>
    public static class TrackGenerators
    {
        /// <summary>
        /// Generates a flat straight road segment.
        /// </summary>
        public static List<Vector2> Straight(Vector2 start, float length, int pointsCount = 4)
        {
            var list = new List<Vector2>();
            for (int i = 0; i <= pointsCount; i++)
            {
                float t = (float)i / pointsCount;
                list.Add(new Vector2(start.x + (length * t), start.y));
            }
            return list;
        }

        /// <summary>
        /// Generates an angled slope (incline or decline).
        /// </summary>
        public static List<Vector2> Slope(Vector2 start, float deltaX, float deltaY, int resolution = 8)
        {
            var list = new List<Vector2>();
            for (int i = 0; i <= resolution; i++)
            {
                float t = (float)i / resolution;
                // Smooth cubic hermite S-curve for gentle start and end transitions
                float smoothT = t * t * (3f - 2f * t);
                list.Add(new Vector2(start.x + (deltaX * t), start.y + (deltaY * smoothT)));
            }
            return list;
        }

        /// <summary>
        /// Generates a smooth sine-wave hill or crest.
        /// </summary>
        public static List<Vector2> SmoothHill(Vector2 start, float width, float height, int resolution = 24)
        {
            var list = new List<Vector2>();
            for (int i = 0; i <= resolution; i++)
            {
                float t = (float)i / resolution;
                // 0 to PI sine arch
                float y = start.y + Mathf.Sin(t * Mathf.PI) * height;
                list.Add(new Vector2(start.x + (width * t), y));
            }
            return list;
        }

        /// <summary>
        /// Generates an accelerating takeoff ramp with smooth concave curvature.
        /// </summary>
        public static List<Vector2> Ramp(Vector2 start, float length, float height, float curvature = 1.6f, int resolution = 16)
        {
            var list = new List<Vector2>();
            for (int i = 0; i <= resolution; i++)
            {
                float t = (float)i / resolution;
                float y = start.y + Mathf.Pow(t, curvature) * height;
                list.Add(new Vector2(start.x + (length * t), y));
            }
            return list;
        }

        /// <summary>
        /// Generates a rhythmic series of bumps / whoops.
        /// </summary>
        public static List<Vector2> Bumps(Vector2 start, int count, float bumpWidth, float bumpHeight, int pointsPerBump = 8)
        {
            var list = new List<Vector2>();
            Vector2 current = start;
            for (int b = 0; b < count; b++)
            {
                for (int i = (b == 0 ? 0 : 1); i <= pointsPerBump; i++)
                {
                    float t = (float)i / pointsPerBump;
                    float y = current.y + Mathf.Sin(t * Mathf.PI) * bumpHeight;
                    list.Add(new Vector2(current.x + (bumpWidth * t), y));
                }
                current.x += bumpWidth;
            }
            return list;
        }

        /// <summary>
        /// Generates a 360-degree loop-the-loop track section.
        /// Uses an offset spiral entry and exit so the 2D edge collider does not intersect itself.
        /// </summary>
        public static List<Vector2> LoopTheLoop(Vector2 start, float radius = 4.5f, int resolution = 40)
        {
            var list = new List<Vector2>();
            Vector2 center = new Vector2(start.x + radius + 1.0f, start.y + radius);

            // Entry approach curve
            int approachPoints = 8;
            for (int i = 0; i <= approachPoints; i++)
            {
                float t = (float)i / approachPoints;
                list.Add(Vector2.Lerp(start, new Vector2(center.x - radius, start.y), t));
            }

            // Loop circle (from angle -90 deg / bottom around 360 deg)
            for (int i = 0; i <= resolution; i++)
            {
                float t = (float)i / resolution;
                // Angle from -PI/2 to 3*PI/2
                float angle = -Mathf.PI * 0.5f + (t * Mathf.PI * 2f);
                // Slight longitudinal shift along X so exit passes smoothly under/over entry
                float xOffset = t * (radius * 0.5f);
                float x = center.x + Mathf.Cos(angle) * radius + xOffset;
                float y = center.y + Mathf.Sin(angle) * radius;
                list.Add(new Vector2(x, y));
            }

            return list;
        }
    }
}
