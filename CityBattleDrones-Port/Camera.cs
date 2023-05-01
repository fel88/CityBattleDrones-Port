using OpenTK;

namespace CityBattleDrones_Port
{
    public class Camera
    {
        public float azimuth;
        public float elevation;
        public float zoomDistance;
        public float azimuthChangeRate;
        public float elevationChangeRate;
        public float zoomChangeRate;
        public float clickX;
        public float clickY;
        public bool clickAndDrag;
        public float minElevation = 5;
        public float maxElevation = 85;
        public float minZoomDistance = 0.01f;
        public float maxZoomDistance = 20;
        public Vector3d position;
        public Vector3d focus;
        public Vector3d forward;
        //boolean array to keep track of when a control is actioned:
        //changing azimuth, changing elevation, zooming in and out
        public bool[] controlActions = new bool[3];
        const double DEGTORAD = (Math.PI / 180.0);
        public Camera() { }
        public void update()
        {
            if (controlActions[0]) azimuth += azimuthChangeRate;
            if (controlActions[1]) elevation += elevationChangeRate;
            if (controlActions[2]) zoomDistance -= zoomChangeRate;

            if (azimuth >= 360)
            {
                azimuth -= 360;
            }
            else if (azimuth < 0)
            {
                azimuth += 360;
            }

            if (elevation < minElevation)
            {
                elevation = minElevation;
            }
            else if (elevation > maxElevation)
            {
                elevation = maxElevation;
            }

            if (zoomDistance < minZoomDistance)
            {
                zoomDistance = minZoomDistance;
            }
            else if (zoomDistance > maxZoomDistance)
            {
                zoomDistance = maxZoomDistance;
            }

            //change elevation
            position.X = 0;
            position.Y = zoomDistance * Math.Sin(elevation * DEGTORAD);
            position.Z = zoomDistance * Math.Cos(elevation * DEGTORAD);

            //change azimuth
            position.X = position.Z * Math.Sin(azimuth * DEGTORAD);
            position.Z = position.Z * Math.Cos(azimuth * DEGTORAD);

            position = Vector3d.Add(position, focus);

            forward = Vector3d.Subtract(focus, position);
            forward.Normalize();
        }
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