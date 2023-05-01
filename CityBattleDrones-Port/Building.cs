using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Numerics;
using System.Runtime.Intrinsics;

namespace CityBattleDrones_Port
{
    public class Building : PrismMesh
    {
        float floorHeight;
        const double numControlPoints = 6;
        List<double> cpSplineScales = new List<double>();
        List<double> cpBaseScales = new List<double>();
        Spline verticalSpline;
        int selectedSplineCP;
        int selectedBaseCP;
        public void processMetaData(string md)
        {
            StringReader sr = new StringReader(md);
            string line = null;
            int i = 0;
            while (true)
            {
                line = sr.ReadLine();
                if (line == null)
                    break;

                if (i == 0) numBaseEdges = int.Parse(line);
                else if (i == 1) initialHeight = Helpers.ParseFloat(line);
                else if (i == 2) currentHeight = Helpers.ParseFloat(line);
                else if (i == 3) floorHeight = Helpers.ParseFloat(line);
                else if (i == 4) rotationY = Helpers.ParseFloat(line);
                else if (i >= 5 && i <= 8)
                {
                    // Vector of string to save tokens
                    List<string> tokens;
                    // stringstream class check1

                    // Tokenizing w.r.t. space ' '
                    tokens = line.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();

                    if (i == 5)
                    {
                        scaleFactors.X = Helpers.ParseFloat(tokens[0]);
                        scaleFactors.Y = Helpers.ParseFloat(tokens[1]);
                        scaleFactors.Z = Helpers.ParseFloat(tokens[2]);
                    }
                    else if (i == 6)
                    {
                        position.X = Helpers.ParseFloat(tokens[0]);
                        position.Y = Helpers.ParseFloat(tokens[1]);
                        position.Z = Helpers.ParseFloat(tokens[2]);
                    }
                    else if (i == 7)
                    {
                        cpSplineScales.Clear();
                        for (int j = 0; j < tokens.Count; j++)
                        {
                            float scale = Helpers.ParseFloat(tokens[j]);
                            cpSplineScales.Add(scale);
                        }
                    }
                    else if (i == 8)
                    {
                        cpBaseScales.Clear();
                        for (int j = 0; j < tokens.Count; j++)
                        {
                            float scale = Helpers.ParseFloat(tokens[j]);
                            cpSplineScales.Add(scale);
                            cpBaseScales.Add(scale);
                        }
                    }
                }
                i++;
            }

            position.Y = currentHeight / 2;

            build();
        }
        void glutSolidSphere(double rad, int seg1, int seg2)
        {

        }


        public void build()
        {
            int numFloors = getNumFloors();
            float preFloorHeight = initialHeight / numFloors; //Floor height before vertical scaling is applied
            List<List<Vector3d>> floors = new List<List<Vector3d>>();

            //Spline curve function used for determining the extrusion factor for each floor
            verticalSpline = createSpline();

            //Creates vertices for the building
            List<Vector3d> floor1Verts = new List<Vector3d>();
            for (int i = 0; i < numBaseEdges; i++)
            {
                float angle = (360.0f / numBaseEdges) * i;
                if (numBaseEdges == 4)
                {
                    //Better alignment for scaling rectangles/squares
                    angle += 45;
                }
                float x = (float)((Math.Sin(angle * Math.PI / 180.0) * (initialHeight / 2)) * cpBaseScales[(i)]);
                float z = (float)((Math.Cos(angle * Math.PI / 180.0) * (initialHeight / 2)) * cpBaseScales[(i)]);
                floor1Verts.Add(new Vector3d(x, -(initialHeight / 2), z));
            }

            for (int i = 0; i <= numFloors; i++)
            {
                List<Vector3d> nextFloorVerts = floor1Verts;
                for (int j = 0; j < numBaseEdges; j++)
                {
                    nextFloorVerts[(j)] = new Vector3d(nextFloorVerts[(j)].X * (float)verticalSpline.At(i),
                     -(initialHeight / 2) + (preFloorHeight * i),
                    nextFloorVerts[(j)].Z * (float)verticalSpline.At(i));
                }
                floors.Add(nextFloorVerts);
            }

            //Creates quads for the building
            quads.Clear();
            for (int i = 0; i < numFloors; i++)
            {
                for (int j = 0; j < numBaseEdges; j++)
                {
                    Polygon newQuad = new Polygon();
                    newQuad.verts.Add(floors[(i)][(j)]);
                    newQuad.verts.Add(floors[(i + 1)][(j)]);
                    if (j == 0)
                    {
                        newQuad.verts.Add(floors[(i + 1)][(numBaseEdges - 1)]);
                        newQuad.verts.Add(floors[(i)][(numBaseEdges - 1)]);
                    }
                    else
                    {
                        newQuad.verts.Add(floors[(i + 1)][(j - 1)]);
                        newQuad.verts.Add(floors[(i)][(j - 1)]);
                    }
                    newQuad.calculateNormal();
                    quads.Add(newQuad);
                }
            }

            //Creates bottom floor and roof polygons
            baseBottom.verts.Clear();
            baseTop.verts.Clear();
            if (floors.Count > 0)
            {
                baseBottom.verts = floors[0];
                baseTop.verts = floors[(floors.Count - 1)];
            }
            baseBottom.isFrontFacing = false;
            baseBottom.calculateNormal();
            baseTop.calculateNormal();
        }

        private Spline createSpline()
        {
            int numFloors = getNumFloors();
            List<double> cpIndices = new List<double>();
            double cpIndexInterval = numFloors / (numControlPoints - 1);
            for (int i = 0; i < numControlPoints; i++)
            {
                cpIndices.Add(cpIndexInterval * i);
            }
            Spline s = new Spline();
            s.set_points(cpIndices, cpSplineScales);
            return s;
        }

        void changeNumSides(int changeNum)
        {
            float diff = Math.Abs(changeNum);
            for (int i = 0; i < diff; i++)
            {
                if (changeNum < 0 && numBaseEdges > 3)
                {
                    cpBaseScales.RemoveAt(cpBaseScales.Count - 1);
                }
                else if (changeNum > 0)
                {
                    cpBaseScales.Add(1.0);
                }
            }

            numBaseEdges += changeNum;
            if (numBaseEdges < 3)
            {
                numBaseEdges = 3;
            }
            build();
        }

        void initializeCpScales()
        {
            for (int i = 0; i < numControlPoints; i++)
            {
                cpSplineScales.Add(1.0);
            }
            for (int i = 0; i < numBaseEdges; i++)
            {
                cpBaseScales.Add(1.0);
            }
        }

        int getNumFloors()
        {
            return (int)Math.Ceiling(currentHeight / floorHeight);
        }

        public void drawBase()
        {
            GL.PushMatrix();
            Polygon _base = baseBottom;
            //glRotatef(rotationY, 0.0, 1.0, 0.0);
            for (int i = 0; i < _base.verts.Count; i++)
            {
                //_base.verts[i] *= new OpenTK.Vector3d(scaleFactors.X / verticalSpline(0), 1, scaleFactors.Z/ verticalSpline(0));
                GL.PushMatrix();
                GL.Translate(_base.verts[i].X, 0.0, _base.verts[(i)].Z);
                glutSolidSphere(0.5 / numBaseEdges, 20, 20);
                GL.PopMatrix();
            }
            //glScalef(scaleFactors.x, 1.0, scaleFactors.z);
            GL.Translate(0.0, position.Y, 0.0);
            base.draw();
            GL.PopMatrix();
        }
    }
}
