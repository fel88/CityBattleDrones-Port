using OpenTK;

namespace CityBattleDrones_Port
{
    public class PrismMesh
    {

        protected
    int numBaseEdges;
        protected float rotationY;
        protected float initialHeight;
        protected float currentHeight;
        protected Vector3d scaleFactors;
        const double minScaleFactor = 0.05;

        public

    Polygon baseBottom;
        public Polygon baseTop;
        public Vector3d position;
        public List<Polygon> quads;

        public PrismMesh() { }
        public PrismMesh(int numEdges) { }
        public PrismMesh(int numEdges, float height, float rotY, float posX, float posZ, Vector3d scale)
        {

        }
        public void draw() { }
        public void draw(int quadTexID, Vector2d[] stQuadCoords, bool baseTex, int baseTexID) { }
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