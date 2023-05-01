using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CityBattleDrones_Port
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            gl = new GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 1, 8));
            KeyPreview = true;
            if (gl.Context.GraphicsMode.Samples == 0)
            {
                gl = new GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 1, 8));
            }

            gl.Dock = DockStyle.Fill;

            gl.MouseMove += Gl_MouseMove;
            gl.BorderStyle = BorderStyle.FixedSingle;
            gl.MouseWheel += Gl_MouseWheel;
            KeyDown += Form1_KeyDown;
            gl.KeyDown += Gl_KeyDown;
            Controls.Add(gl);
            gl.Dock = DockStyle.Fill;
        }

        //Initialize a drone object
        static Vector3d playerSpawnPoint = new Vector3d(0.0, 4.0, -3.0);
        static Vector3d enemySpawnPoint = new Vector3d(0.0, 4.0, 4.0);
        // Creates a drone with a scaleFactor of 1.0;
        // with 6 arms and 2 propeller blades per arm;
        // positioned at specified spawnPoint
        Drone dronePlayer = new Drone(0.02, 6, 2, playerSpawnPoint, 20);
        DroneAI droneEnemy = new DroneAI(0.02, 6, 2, enemySpawnPoint, 20);


        static Camera tpCamera;          //third-person camera for the drone
        static Camera fpCamera;          //first-person camera for the drone

        static List<Building> buildings = new List<Building>();                //array of buildings
        static List<Street> streets = new List<Street>();
        static List<int> buildingTextures = new List<int>();
        static List<int> roofTextures = new List<int>();

        //Textures
        public static List<string> texFiles = new List<string>(); //array of texture filenames

        // Callback, called whenever GLUT determines that the window should be redisplayed
        // or glutPostRedisplay() has been called.
        void display()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            for (int j = 0; j < dronePlayer.missiles.Count; j++)
            {
                dronePlayer.missiles[j].setTargetPos(droneEnemy.getPosition());
            }

            for (int j = 0; j < droneEnemy.missiles.Count; j++)
            {
                droneEnemy.missiles[j].setTargetPos(dronePlayer.getPosition());
            }

            droneEnemy.makeDecisions(dronePlayer.getPosition());

            handleCollisions();

            dronePlayer.updateDrone();
            droneEnemy.updateDrone();

            Vector3d newFocus = dronePlayer.getPosition();

            tpCamera.changeFocus(newFocus);
            tpCamera.setAzimuth(180 + dronePlayer.getRotationY());
            tpCamera.update();

            fpCamera.changeFocus(new Vector3d(newFocus.X, newFocus.Y - 0.02, newFocus.Z));
            fpCamera.setAzimuth(dronePlayer.getRotationY());
            fpCamera.update();

            //GL.Viewport(0, 0, (GLsizei)windowWidth * tpViewportRatio, (GLsizei)windowHeight);
            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadIdentity();
            //GL.Perspective(60.0, (GLdouble)(windowWidth * tpViewportRatio / windowHeight), 0.01, 400.0);
            //GL.MatrixMode(GL_MODELVIEW);
            //GL.LoadIdentity();
            //gluLookAt(tpCamera.position.x, tpCamera.position.y, tpCamera.position.z, tpCamera.focus.x, tpCamera.focus.y, tpCamera.focus.z, 0, 1, 0);
            //glLightfv(GL_LIGHT0, GL_POSITION, light_position0);

            //glUniform1i(texMapLocation, 0);
            //glUniform1i(texModeLocation, 0);
            //drawAssets();

            //glViewport(fpViewportX, fpViewportY, (GLsizei)fpViewportWidth, (GLsizei)fpViewportHeight);
            //glMatrixMode(GL_PROJECTION);
            //glLoadIdentity();
            //gluPerspective(60.0, (GLdouble)(fpViewportWidth / fpViewportHeight), 0.005, 400.0);
            //GL.MatrixMode(MatrixMode.Modelview);
            //GL.LoadIdentity();
            //float invertedY = fpCamera.focus.y - (fpCamera.position.y - fpCamera.focus.y);
            //GL.LookAt(fpCamera.focus.x, fpCamera.focus.y, fpCamera.focus.z, fpCamera.position.x, invertedY, fpCamera.position.z, 0, 1, 0);
            //GL.Light(GL_LIGHT0, GL_POSITION, light_position0);

            //glUniform1i(spotlightModeLocation, 1);
            //drawAssets();
            //glUniform1i(spotlightModeLocation, 0);

            //glViewport(cityViewportX, cityViewportY, (GLsizei)cityViewportWidth, (GLsizei)cityViewportHeight);
            //glMatrixMode(GL_PROJECTION);
            //glLoadIdentity();
            //gluPerspective(20.0, (GLdouble)(cityViewportWidth / cityViewportWidth), 0.01, 400.0);
            //glMatrixMode(GL_MODELVIEW);
            //glLoadIdentity();
            //gluLookAt(0.1, 101, 0, 0, 0, 0, 0, 1, 0);
            //glLightfv(GL_LIGHT0, GL_POSITION, light_position0);

            //glUniform1i(texModeLocation, 2);
            drawMap();


            gl.SwapBuffers();   // Double buffering, swap buffers
        }
        public void drawMap()
        {

        }
        private void Gl_MouseMove(object sender, MouseEventArgs e)
        {
            var xpos = Cursor.Position.X;
            var ypos = Cursor.Position.Y;

        }

        void loadCity(string filename)
        {

            string metaData = string.Empty;
            List<Building> loadedBuildings = new List<Building>();
            List<Street> loadedStreets = new List<Street>();

            var lines = File.ReadAllLines(filename);
            foreach (var line in lines)
            {

                if (line == ("---------"))
                {
                    if (loadedBuildings.Count > 0)
                    {
                        loadedBuildings[(loadedBuildings.Count - 1)].processMetaData(metaData);
                    }
                    Building bd = new Building();
                    loadedBuildings.Add(bd);
                    metaData = "";
                }
                else if (line == ("END_BUILDING_LIST"))
                {
                    if (loadedBuildings.Count > 0)
                    {
                        loadedBuildings[(loadedBuildings.Count() - 1)].processMetaData(metaData);
                    }
                }
                else if (line == ("+++++"))
                {
                    if (loadedStreets.Count > 0)
                    {
                        loadedStreets[(loadedStreets.Count - 1)].processMetaData(metaData);
                    }
                    Street bd = new Street();
                    loadedStreets.Add(bd);
                    metaData = "";
                }
                else if (line == ("END_STREET_LIST"))
                {
                    if (loadedStreets.Count > 0)
                    {
                        loadedStreets[(loadedStreets.Count() - 1)].processMetaData(metaData);
                    }
                }
                else
                {
                    metaData += line + "\n";
                }
            }

            foreach (var bld in loadedBuildings)
            {
                buildings.Add(bld);
                int randIndex = rand.Next(6);
                buildingTextures.Add(randIndex);
                randIndex = rand.Next(3);
                roofTextures.Add(randIndex);
            }
            foreach (var strt in loadedStreets)
            {
                streets.Add(strt);
            }

        }

        Random rand = new Random();
        void handleCollisions()
        {
        }

        private void Gl_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            float delta = 0.1f;
            /*if (e.KeyCode == Keys.W)
            {
                camera.ProcessKeyboard(Camera.Camera_Movement.FORWARD, delta);
            }
            if (e.KeyCode == Keys.S)
            {
                camera.ProcessKeyboard(Camera.Camera_Movement.BACKWARD, delta);
            }
            if (e.KeyCode == Keys.A)
            {
                camera.ProcessKeyboard(Camera.Camera_Movement.LEFT, delta);
            }
            if (e.KeyCode == Keys.D)
            {
                camera.ProcessKeyboard(Camera.Camera_Movement.RIGHT, delta);
            }*/
        }

        private void Gl_MouseWheel(object sender, MouseEventArgs e)
        {
            //camera.ProcessMouseScroll(e.Delta / 100f);
        }
        uint planeVAO, planeVBO;
        GLControl gl;
        Shader shader;
        Shader shaderSingleColor;
        // cube VAO
        uint cubeVAO, cubeVBO;
        uint cubeTexture;

        private void timer1_Tick(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, gl.Width, gl.Height);

            // render
            // ------
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit); // don't forget to clear the stencil buffer!

            // set uniforms

            var model = Matrix4.Identity;

            //glm::mat4 model = glm::mat4(1.0f);
            //var view = camera.GetViewMatrix();
            //var projection = Matrix4.CreatePerspectiveFieldOfView((float)Camera.torad(camera.Zoom), (float)gl.Width / (float)gl.Height, 0.1f, 100.0f);



            gl.SwapBuffers();
        }

        //Camera camera = new Camera(new Vector3(0.0f, 0.0f, 3.0f));
    }
}