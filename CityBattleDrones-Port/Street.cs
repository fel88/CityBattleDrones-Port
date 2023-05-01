using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.Intrinsics;

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
            scaleX = 1;
            scaleZ = 15;
            rotationY = 0;
            position = new Vector3d(0, 0, 0);
            build();
        }

        public void draw()
        {
            GL.PushMatrix();
            GL.Translate(position.X, 0.0, position.Z);
            GL.Rotate(rotationY, 0.0, 1.0, 0.0);
            GL.Scale(scaleX, 1.0, scaleZ);
            streetPoly.draw();
            GL.PopMatrix();
        }
        public void build()
        {
            streetPoly = new Polygon();
            Vector3d[] verts = new Vector3d[] { new Vector3d(-1, 0, -1), new Vector3d(-1, 0, 1), new Vector3d(1, 0, 1),
                new Vector3d(1, 0, -1) };
            streetPoly.verts = verts.ToList();
            streetPoly.calculateNormal();
        }
        public void rotateY(float deltaY)
        {
            rotationY += deltaY;
        }
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
        public void processMetaData(string md)
        {
            StringReader iss = new StringReader(md);

            int i = 0;
            while (true)
            {
                var line = iss.ReadLine();
                if (line == null) break;
                if (i == 0) scaleX = Helpers.ParseFloat(line);
                else if (i == 1) scaleZ = Helpers.ParseFloat(line);
                else if (i == 2) rotationY = Helpers.ParseFloat(line);
                else if (i == 3)
                {
                    // Vector of string to save tokens
                    string[] tokens = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    
                    position.X = Helpers.ParseFloat(tokens[0]);
                    position.Y = Helpers.ParseFloat(tokens[1]);
                    position.Z = Helpers.ParseFloat(tokens[2]);                   
                    
                }
                i++;
            }

            build();

        }
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