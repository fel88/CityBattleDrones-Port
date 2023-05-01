using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CityBattleDrones_Port
{
    public class Polygon
    {


        public Vector3d normal;
        public List<Vector3d> verts = new List<Vector3d>();
        public bool isFrontFacing;

        public Polygon() { }
        public void calculateNormal()
        {
            if (verts.Count >= 3)
            {
                Vector3d v1 = Vector3d.Subtract(verts[0], verts[1]);
                Vector3d v2 = Vector3d.Subtract(verts[0], verts[2]);
                if (isFrontFacing)
                {
                    normal = Vector3d.Cross(v1, v2);
                }
                else
                {
                    normal = Vector3d.Cross(v2, v1);
                }
                normal.Normalize();
            }
        }

        public void draw(int texID, Vector2d[] stCoordinates, bool hasAlpha)
        {
            GL.BindTexture(TextureTarget.Texture2D, texID);
            GL.Enable(EnableCap.Texture2D);

            if (hasAlpha)
            {
                GL.Disable(EnableCap.Lighting);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }

            GL.Begin(PrimitiveType.Polygon);
            GL.Normal3(normal.X, normal.Y, normal.Z);
            if (isFrontFacing)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    GL.TexCoord2(stCoordinates[i].X, stCoordinates[i].Y);
                    GL.Vertex3(verts[i].X, verts[i].Y, verts[i].Z);
                }
            }
            else
            {
                for (int i = (int)verts.Count; i > 0; i--)
                {
                    GL.TexCoord2(stCoordinates[i - 1].X, stCoordinates[i - 1].Y);
                    GL.Vertex3(verts[i - 1].X, verts[i - 1].Y, verts[i - 1].Z);
                }
            }
            GL.End();

            if (hasAlpha)
            {
                GL.Enable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Blend);
                GL.Enable(EnableCap.Lighting);
            }

            GL.Disable(EnableCap.Texture2D);
        }

        public void draw()
        {
            GL.Begin(PrimitiveType.Polygon);
            GL.Normal3(normal.X, normal.Y, normal.Z);
            if (isFrontFacing)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    GL.Vertex3(verts[i].X, verts[i].Y, verts[i].Z);
                }
            }
            else
            {
                for (int i = (int)verts.Count; i > 0; i--)
                {
                    GL.Vertex3(verts[i - 1].X, verts[i - 1].Y, verts[i - 1].Z);
                }
            }
            GL.End();
        }
    }
}