using OpenTK;

namespace CityBattleDrones_Port
{
    public class Camera
    {
        public double azimuth;
        public float elevation;
        public double zoomDistance;
        public float azimuthChangeRate;
        public float elevationChangeRate;
        public float zoomChangeRate;
        public float clickX;
        public float clickY;
        public bool clickAndDrag;
        public float minElevation = 5;
        public float maxElevation = 85;
        public double minZoomDistance = 0.01f;
        public float maxZoomDistance = 20;
        public Vector3d position;
        public Vector3d focus;
        public Vector3d forward;
        //boolean array to keep track of when a control is actioned:
        //changing azimuth, changing elevation, zooming in and out
        public bool[] controlActions = new bool[3];
        const double DEGTORAD = (Math.PI / 180.0);
        const double DEFAULT_ZOOM = 0.5;
        public Camera() {
            azimuth = 0;
            elevation = (18);
            zoomDistance=(DEFAULT_ZOOM);
            clickX=(0);
            clickY=(0);
            clickAndDrag=(false);
            azimuthChangeRate=(3);
            elevationChangeRate=(3);
            zoomChangeRate=(3);
            position=(new Vector3d());
            forward=(new Vector3d());
            focus=(new Vector3d(0, 0, 0));
            minElevation=(5);
            maxElevation=(85);
            minZoomDistance=(0.01);
            maxZoomDistance=20;
            controlActions[0] = false;
            controlActions[1] = false;
            controlActions[2] = false;
            update();
        }
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
        public void setAzimuthChangeRate(float rate) { azimuthChangeRate = rate; }
        public void setElevationChangeRate(float rate) { elevationChangeRate = rate; }
        public void setZoomChangeRate(float rate) { zoomChangeRate = rate; }
        public void setElevation(float angle) { elevation = angle; }
        public void setAzimuth(double angle) { azimuth = angle; }
        public void setZoom(float distance) { zoomDistance = distance; }
        public void changeFocus(Vector3d newFocus) { focus = newFocus; }
        public void move(float mouseX, float mouseY) {
            azimuth += (clickX - mouseX) / 800;
            elevation += (mouseY - clickY) / 8;
            clickX = mouseX;
            clickY = mouseY;
        }
        public void setMinElevation(float newMin) {
            minElevation = newMin;
        }
        public void setMaxElevation(float newMax) {
            maxElevation = newMax;
        }
    }
}