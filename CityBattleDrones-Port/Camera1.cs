using OpenTK;

namespace CityBattleDrones_Port
{
    public class Camera
    {
        public Camera() { }
        public void update() { }
        public void setAzimuthChangeRate(float rate) { }
        public void setElevationChangeRate(float rate) { }
        public void setZoomChangeRate(float rate) { }
        public void setElevation(float angle) { }
        public void setAzimuth(double angle) { }
        public void setZoom(float distance) { }
        public void changeFocus(Vector3d newFocus) { }
        public void move(float mouseX, float mouseY) { }
        public void setMinElevation(float newMin) { }
        public void setMaxElevation(float newMax) { }
    }
}