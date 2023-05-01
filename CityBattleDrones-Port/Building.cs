using System.Numerics;
using System.Runtime.Intrinsics;

namespace CityBattleDrones_Port
{
    public class Building :  PrismMesh
    {
        public void processMetaData(string md)
        {
            var l = File.ReadLines(md);
            int i = 0;
            foreach (var line in l)
            {                
                //if (i == 0) s >> numBaseEdges;
                //else if (i == 1) s >> initialHeight;
                //else if (i == 2) s >> currentHeight;
                //else if (i == 3) s >> floorHeight;
                //else if (i == 4) s >> rotationY;
                //else if (i >= 5 && i <= 8)
                //{
                //    // Vector of string to save tokens
                //    List<string> tokens;
                //    // stringstream class check1
                //    stringstream check1(line);
                //    string intermediate;
                //    // Tokenizing w.r.t. space ' '
                //    while (getline(check1, intermediate, ' '))
                //    {
                //        tokens.push_back(intermediate);
                //    }

                //    if (i == 5)
                //    {
                //        stringstream s1(tokens[0]);
                //        s1 >> scaleFactors.x;
                //        stringstream s2(tokens[1]);
                //        s2 >> scaleFactors.y;
                //        stringstream s3(tokens[2]);
                //        s3 >> scaleFactors.z;
                //    }
                //    else if (i == 6)
                //    {
                //        stringstream s1(tokens[0]);
                //        s1 >> position.x;
                //        stringstream s2(tokens[1]);
                //        //s2 >> position.y;
                //        stringstream s3(tokens[2]);
                //        s3 >> position.z;
                //    }
                //    else if (i == 7)
                //    {
                //        cpSplineScales.clear();
                //        for (int j = 0; j < tokens.size(); j++)
                //        {
                //            stringstream s(tokens[j]);
                //            float scale;
                //            s >> scale;
                //            cpSplineScales.push_back(scale);
                //        }
                //    }
                //    else if (i == 8)
                //    {
                //        cpBaseScales.clear();
                //        for (int j = 0; j < tokens.size(); j++)
                //        {
                //            stringstream s(tokens[j]);
                //            float scale;
                //            s >> scale;
                //            cpBaseScales.push_back(scale);
                //        }
                //    }
                //}
                i++;

            }

          
            
                
            position.Y = currentHeight / 2;

            build();
        }
    }
}