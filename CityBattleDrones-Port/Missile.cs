using OpenTK;

namespace CityBattleDrones_Port
{
    public class Missile
    {
        float scaleFactor;
        float speed;
        float length;
        int bodyTexture;
        int headTexture;
        Vector3d position;
        Vector3d forward;
        Vector3d rotation;
        Vector3d targetPos;
        PrismMesh body;

        public
    bool isDestroyed;
        float timeDestroyed;

        Missile()
        {
        }
        public Missile(float scaleFactor, float speed, Vector3d position, Vector3d forward, Vector3d targetPos, int bodyTex, int headTex)
        {
        }
        public void moveForward(float speed) { }
        public void alignRotation(Vector3d direction) { }
        public void changeForward(Vector3d targetPos, float changeDelta) { }
        public Vector3d getPosition() { return new Vector3d(); }
        public void setPosition(Vector3d newPos) { }
        public void setForward(Vector3d newForward) { }
        public void setTargetPos(Vector3d newTargetPos) { }
        public void blowUp() { }
        public void update() { }
        public void draw() { }
    }
}