using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Numerics;

namespace CityBattleDrones_Port
{
    public class PrismMesh
    {
        protected int numBaseEdges;
        protected float rotationY;
        protected float initialHeight;
        protected float currentHeight;
        protected Vector3d scaleFactors;
        const double minScaleFactor = 0.05;

        public Polygon baseBottom = new Polygon();
        public Polygon baseTop = new Polygon();
        public Vector3d position;
        public List<Polygon> quads = new List<Polygon>();

        public PrismMesh() { }
        public PrismMesh(int numEdges) { }
        public PrismMesh(int numEdges, float height, float rotY, float posX, float posZ, Vector3d scale)
        {

        }

        // Draw the prism without any textures
        public void draw()
        {
            GL.PushMatrix();
            GL.Translate(position.X, position.Y, position.Z);
            GL.Rotate(rotationY, 0.0, 1.0, 0.0);
            GL.Scale(scaleFactors.X, scaleFactors.Y, scaleFactors.Z);
            baseBottom.draw();
            baseTop.draw();
            foreach (var quad in quads)
            {
                quad.draw();
            }
            GL.PopMatrix();
        }
        public void draw(int quadTexID, Vector2d[] stQuadCoords, bool baseTex, int baseTexID) {
            GL.PushMatrix();
            GL.Translate(position.X, position.Y, position.Z);
            GL.Rotate(rotationY, 0.0, 1.0, 0.0);
            GL.Scale(scaleFactors.X, scaleFactors.Y, scaleFactors.Z);

            foreach (var  quad in quads)
            {
                quad.draw(quadTexID, stQuadCoords, false);
            }

            if (baseTex)
            {
                List<Vector2d> stBaseCoords = new List<Vector2d>();

                for (int i = 0; i < numBaseEdges; i++)
                {
                    float angle = (360.0f / numBaseEdges) * i;

                    float norm = 0.5f;
                    float x = (float)(Math.Sin(angle * Math.PI / 180.0) * norm + 0.5);
                    float z = (float)(Math.Cos(angle * Math.PI / 180.0) * norm + 0.5);

                    stBaseCoords.Add(new Vector2d(z, x));
                }
                
                //if the base is a square/rectangle, fit it perfectly to the texture
                if (numBaseEdges == 4)
                {
                    stBaseCoords.Clear();
                    stBaseCoords = new List<Vector2d>(){ new Vector2d(0, 0), new Vector2d(0, 1),
                        new Vector2d(1, 1), new Vector2d(1, 0)};
                }
                baseBottom.draw(baseTexID, stBaseCoords.ToArray(), false);
                baseTop.draw(baseTexID, stBaseCoords.ToArray(), false);
            }
            else
            {
                baseBottom.draw();
                baseTop.draw();
            }
            GL.PopMatrix();
        }
        public void draw(int textID, Vector2d[][] stSideCoords, Vector2d[] stTopCoords, Vector2d[] stBottomCoords) { }
        public void build() { }
        public void changeNumSides(int changeNum) { }
        public void rotateY(float deltaY) { }
        public void moveAlongGround(float deltaX, float deltaY) { }
        public void changeScaleFactors(Vector3d scaleDeltas) { }
        public int getNumSides()
        {
            return numBaseEdges;
        }
    }
}