using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Numerics;

namespace CityBattleDrones_Port
{
    public class Street
    {
        public float scaleX;
        public float scaleZ;
        public float rotationY;
        public Vector3d position;
        public Polygon streetPoly = new Polygon();

        public Street()
        {

        }
        public void draw()
        {

        }
        public void build()
        {

        }
        public void rotateY(float deltaY) { }
        public void moveAlongGround(float deltaX, float deltaZ)
        {
            position.X += deltaX * 0.5;
            position.Z += deltaZ * 0.5;
        }
        public void changeScaleFactors(float deltaX, float deltaZ)
        {
            scaleX += deltaX;
            scaleZ += deltaZ;

            if (scaleX < 0)
            {
                scaleX = 0;
            }

            if (scaleZ < 0)
            {
                scaleZ = 0;
            }
        }
        public string getMetaData()
        {
            string md = "+++++\n";
            md += (scaleX) + "\n";
            md += (scaleZ) + "\n";
            md += (rotationY) + "\n";
            md += (position.X) + " " + (position.Y) + " " + (position.Z) + "\n";
            return md;
        }
        public void processMetaData(string md) { }
        public void draw(int textID)
        {
            float streetLength = scaleX * 2;
            float streetWidth = scaleZ * 2;
            float ratio = 3; // street texture should be 3 times longer than its width

            float numRepeat = streetWidth / (ratio * streetLength);
            Vector2d[] stCoordinates = { new Vector2d(0, 0), new Vector2d(0, numRepeat), new Vector2d(1, numRepeat), new Vector2d(1, 0) };

            GL.PushMatrix();
            GL.Translate(position.X, 0.0, position.Z);
            GL.Rotate(rotationY, 0.0, 1.0, 0.0);
            GL.Scale(scaleX, 1.0, scaleZ);
            streetPoly.draw(textID, stCoordinates, false);
            GL.PopMatrix();
        }

    }
}