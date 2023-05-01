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
        //tk::spline verticalSpline;
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
    }
}