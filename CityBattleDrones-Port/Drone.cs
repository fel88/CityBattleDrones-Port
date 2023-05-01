using OpenTK;
using System.Numerics;

namespace CityBattleDrones_Port
{
    public class Drone
    {

        float scaleFactor;
        int numArms;
        int numPropBlades;
        float forwardSpeed;
        float rightSpeed;
        float tiltAngle;
        protected Vector3d position;
        Vector3d spawnPoint;
        protected Vector3d rotation;

        Vector3d tiltAxis;
        PrismMesh prism;
        List<DroneArm> arms;
        //boolean array to keep track of when a control is actioned:
        //move up, move down, move forward, move backward,
        //rotate ccw, rotate cw, move right, move left
        bool[] controlActions = new bool[8];
        const float bodyScaleY = 0.65f;
        const float deltaMove = 0.02f;
        int maxNumMissiles;

        public

    Vector3d forward;

        public bool propsSpinning;
        public bool isDestroyed;
        public float timeDestroyed;
        public List<Missile> missiles = new List<Missile>();

        public Drone() { }
        public Drone(double scaleFactor, int numArms, int numPropBlades, Vector3d position, int maxNumMissiles)
        {

        }
        public void draw()
        {

        }
        public void drawArms() { }
        public void createArms(float armLength, float armWidth) { }
        public void changeElevation(float deltaY) { }
        public void changeDirection(float deltaAngle) { }
        public void spinPropellers() { }
        public void move(float deltaForward, float deltaRight) { }
        public void stabilize() { }
        public void drawCockpit() { }
        public void updateDrone() { }
        public Vector3d getPosition()
        {
            return position;
        }

        public double getRotationY() { return rotation.Y; }
        void setAction(int actionIndex, bool set) { }
        void destroy() { }
        void respawn() { }
        void launchMissile() { }
    }
}