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

            gl.Dock = DockStyle.Fill;

            Controls.Add(gl);
        }




        int windowWidth = 1200;
        int windowHeight = 600;

        const int groundLength = 36;          // Default ground length
        const int groundWidth = 36;           // Default ground height
        const int worldSize = 300;            // Size of the world, used for the skybox
        const double tpViewportRatio = 0.7; // Window Width Ratio for the third-person viewport

        //Initialize a drone object
        static Vector3d playerSpawnPoint = new Vector3d(0.0, 4.0, -3.0);
        static Vector3d enemySpawnPoint = new Vector3d(0.0, 4.0, 4.0);
        // Creates a drone with a scaleFactor of 1.0;
        // with 6 arms and 2 propeller blades per arm;
        // positioned at specified spawnPoint
        Drone dronePlayer = new Drone(0.02, 6, 2, playerSpawnPoint, 20);
        DroneAI droneEnemy = new DroneAI(0.02, 6, 2, enemySpawnPoint, 20);


        static Camera tpCamera = new Camera();          //third-person camera for the drone
        static Camera fpCamera = new Camera();          //first-person camera for the drone

        static List<Building> buildings = new List<Building>();                //array of buildings
        static List<Street> streets = new List<Street>();
        static List<int> buildingTextures = new List<int>();
        static List<int> roofTextures = new List<int>();
        static string CityMetaDataFile = "CityMetaData3.txt";
        static Polygon ground = new Polygon();
        static PrismMesh skybox = new PrismMesh();
        static Polygon dpMapIcon = new Polygon();
        static Polygon deMapIcon = new Polygon();

        //Textures
        public static List<string> texFiles = new List<string>(); //array of texture filenames
                                                                  // Light properties
        static float[] light_position0 = { worldSize * 0.5f, worldSize * 0.1f, -worldSize * 0.1f, 1.0F };
        static float[] light_diffuse = { 1.0f, 0.95f, 0.7f, 1.0f };
        static float[] light_specular = { 1.0f, 0.9f, 0.7f, 1.0f };
        static float[] light_ambient = { 0.95F, 0.8F, 0.6F, 1.0F };

        // Set up OpenGL. For viewport and projection setup see reshape(). */
        void initOpenGL(int w, int h)
        {
            // Set up and enable lighting
            GL.Light(LightName.Light0, LightParameter.Ambient, light_ambient);
            GL.Light(LightName.Light0, LightParameter.Diffuse, light_diffuse);
            GL.Light(LightName.Light0, LightParameter.Specular, light_specular);

            GL.Light(LightName.Light0, LightParameter.Position, light_position0);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            // Other OpenGL setup
            GL.Enable(EnableCap.DepthTest);   // Remove hidded surfaces
            GL.ShadeModel(ShadingModel.Smooth);   // Use smooth shading, makes boundaries between polygons harder to see 
            GL.ClearColor(0.6F, 0.6F, 0.6F, 0.0F);  // Color and depth for glClear
            GL.ClearDepth(1.0f);
            GL.Enable(EnableCap.Normalize);    // Renormalize normal vectors 
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);   // Nicer perspective

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            //Load textures
            texFiles.Add("Textures/cityGround2.bmp");      //2000

            texFiles.Add("Textures/steelGradient.bmp");    //2001

            texFiles.Add("Textures/skybox1.bmp");          //2002

            texFiles.Add("Textures/floorTex1.bmp");        //2003
            texFiles.Add("Textures/floorTex2.bmp");        //2004
            texFiles.Add("Textures/floorTex3.bmp");        //2005
            texFiles.Add("Textures/floorTex4.bmp");        //2006
            texFiles.Add("Textures/floorTex5.bmp");        //2007
            texFiles.Add("Textures/floorTex6.bmp");        //2008

            texFiles.Add("Textures/roofTex1.bmp");         //2009
            texFiles.Add("Textures/roofTex2.bmp");         //2010
            texFiles.Add("Textures/roofTex3.bmp");         //2011
            texFiles.Add("Textures/redMetal2.bmp");        //2012

            texFiles.Add("Textures/missileTex1.bmp");      //2013
            texFiles.Add("Textures/smoke1.png");           //2014
            texFiles.Add("Textures/blast.bmp");            //2015

            texFiles.Add("Textures/street.bmp");           //2016

            loadTextures(texFiles.ToArray());

            skybox.changeScaleFactors(new Vector3d(worldSize, worldSize, worldSize));

            ground.verts.Add(new Vector3d(-groundLength / 2, -0.1, -groundWidth / 2));
            ground.verts.Add(new Vector3d(-groundLength / 2, -0.1, groundWidth / 2));
            ground.verts.Add(new Vector3d(groundLength / 2, -0.1, groundWidth / 2));
            ground.verts.Add(new Vector3d(groundLength / 2, -0.1, -groundWidth / 2));
            ground.calculateNormal();

            fpCamera.setElevation(5);

            List<Vector3d> verts = new List<Vector3d>(){new Vector3d(-0.5, 0, -0.75),
                new Vector3d(0, 0, 0.75),new Vector3d(0.5, 0, -0.75)};
            dpMapIcon.verts = verts;
            deMapIcon.verts = verts;
            dpMapIcon.calculateNormal();
            deMapIcon.calculateNormal();

            loadCity(CityMetaDataFile);
        }

        void loadTextures(string[] texFiles)
        {
            for (int i = 0; i < texFiles.Length; i++)
            {
                int x, y, n = 1;
                string filename = texFiles[i];
                //unsigned char* data = stbi_load(filename, &x, &y, &n, 0);

                //glBindTexture(GL_TEXTURE_2D, 2000 + i);
                //  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
                // glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);

                // ... process data if not NULL ...
                // ... x = width, y = height, n = # 8-bit components per pixel ...
                // ... replace '0' with '1'..'4' to force that many components per pixel
                // ... but 'n' will always be the number that it would have been if you said 0
                if (n == 3)
                {
                    //GL.TexImage2D(GL_TEXTURE_2D, 0, GL_RGB, x, y, 0, GL_RGB, GL_UNSIGNED_BYTE, data);
                }
                else if (n == 4)
                {
                    // GL.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, x, y, 0, GL_RGBA, GL_UNSIGNED_BYTE, data);
                }
                //stbi_image_free(data);
            }
        }
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

        bool first = true;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (first)
            {
                // Initialize GL
                initOpenGL(windowWidth, windowHeight);
                first = false;
            }
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