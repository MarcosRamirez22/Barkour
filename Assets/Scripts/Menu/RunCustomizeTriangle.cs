using UnityEngine;

public sealed class RunCustomizeTriangle : UnityEngine.UI.MaskableGraphic
{
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
    {
        mesh.Clear();
        Rect bounds = GetPixelAdjustedRect();
        mesh.AddVert(new Vector2(bounds.xMin, bounds.yMin), color, Vector2.zero);
        mesh.AddVert(new Vector2(bounds.xMin, bounds.yMax), color, Vector2.zero);
        mesh.AddVert(new Vector2(bounds.xMax, bounds.center.y), color, Vector2.zero);
        mesh.AddTriangle(0, 1, 2);
    }
}
