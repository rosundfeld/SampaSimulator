using UnityEngine;

// Reusable Gizmos helpers for visualizing casts/checks used by movement scripts. Editor-only, call from OnDrawGizmos(Selected).
public static class GizmosUtils
{
    public static void DrawWireBox(Vector3 center, Vector3 size, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, size);
    }

    public static void DrawWireSphere(Vector3 center, float radius, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(center, radius);
    }

    public static void DrawRay(Vector3 origin, Vector3 direction, float length, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(origin, direction.normalized * length);
    }

    // Draws a SphereCast as two spheres (start/end) connected by a line, matching how Physics.SphereCast travels.
    public static void DrawSphereCast(Vector3 origin, float radius, Vector3 direction, float distance, Color color)
    {
        Gizmos.color = color;
        Vector3 endPos = origin + direction.normalized * distance;
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawWireSphere(endPos, radius);
        Gizmos.DrawLine(origin, endPos);
    }

    // Draws a BoxCast as two boxes (start/end) connected by a line, matching how Physics.BoxCast travels.
    public static void DrawBoxCast(Vector3 origin, Vector3 halfExtents, Vector3 direction, float distance, Color color)
    {
        Gizmos.color = color;
        Vector3 endPos = origin + direction.normalized * distance;
        Gizmos.DrawWireCube(origin, halfExtents * 2f);
        Gizmos.DrawWireCube(endPos, halfExtents * 2f);
        Gizmos.DrawLine(origin, endPos);
    }
}
