using System.Collections.Generic;
using UnityEngine;

namespace Doofus.Pulpits
{
    // Grid placement math: pulpits occupy a 9x9-unit grid so adjacent cells sit flush
    // against each other with no gap and no overlap.
    public static class PulpitGrid
    {
        private static readonly Vector2Int[] Directions =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
        };

        // Picks a random cell adjacent to 'from' that isn't already occupied by a
        // currently-alive pulpit, and - if avoid is given - isn't that cell either, even
        // if the pulpit that used to be there has since despawned. That keeps the next
        // pulpit from reappearing at the spot Doofus just walked off of. Relaxes each
        // constraint in turn (avoid, then occupancy) rather than throwing, so a
        // pathological state never crashes the spawner.
        public static Vector2Int GetRandomAdjacent(Vector2Int from, List<Pulpit> occupied, Vector2Int? avoid = null)
        {
            List<Vector2Int> candidates = new List<Vector2Int>();
            List<Vector2Int> candidatesIgnoringAvoid = new List<Vector2Int>();

            foreach (Vector2Int dir in Directions)
            {
                Vector2Int candidate = from + dir;
                if (IsOccupied(candidate, occupied)) continue;

                candidatesIgnoringAvoid.Add(candidate);
                if (!avoid.HasValue || candidate != avoid.Value)
                {
                    candidates.Add(candidate);
                }
            }

            if (candidates.Count > 0)
            {
                return candidates[Random.Range(0, candidates.Count)];
            }

            if (candidatesIgnoringAvoid.Count > 0)
            {
                return candidatesIgnoringAvoid[Random.Range(0, candidatesIgnoringAvoid.Count)];
            }

            return from + Directions[Random.Range(0, Directions.Length)];
        }

        private static bool IsOccupied(Vector2Int cell, List<Pulpit> occupied)
        {
            foreach (Pulpit p in occupied)
            {
                if (p != null && p.IsAlive && p.GridPosition == cell) return true;
            }
            return false;
        }

        public static Vector3 ToWorldPosition(Vector2Int gridPosition, float cellSize, float height)
        {
            return new Vector3(gridPosition.x * cellSize, height, gridPosition.y * cellSize);
        }
    }
}
