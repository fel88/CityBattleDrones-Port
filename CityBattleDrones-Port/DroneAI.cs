using OpenTK;
using System.Runtime.InteropServices;

namespace CityBattleDrones_Port
{
    public class DroneAI : Drone
    {
        const double RADTODEG = (180.0 / Math.PI);
        public int lastDecisionTime;
        public bool active;

        public DroneAI() { }
        public DroneAI(double scaleFactor, int numArms, int numPropBlades, Vector3d spawnPoint, int maxNumMissiles)
    : base(scaleFactor, numArms, numPropBlades, spawnPoint, maxNumMissiles)
        {
            lastDecisionTime = 0;
            active = true;
        }
        public void lookAt(Vector3d pos)
        {
            Vector3d lookAt = Vector3d.Subtract(pos, position);
            Vector2d lookAtXZ = new Vector2d(lookAt.Z, lookAt.X);
            lookAtXZ.Normalize();

            forward.X = lookAtXZ.Y;
            forward.Z = lookAtXZ.X;
            rotation.Y = RADTODEG * Math.Atan2(lookAtXZ.Y, lookAtXZ.X);
        }
        public void moveToward(Vector3d pos) { }
        public void decideToLaunch(Vector3d pos) { }
        public void makeDecisions(Vector3d pos) { }
    }
}