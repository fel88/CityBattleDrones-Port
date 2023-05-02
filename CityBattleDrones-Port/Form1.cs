using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.MacOS;
using OpenTK.Platform.Windows;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
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

        // Skybox texture grid s&t coordinates (5x4 grid lines)
        // Example: st12 represents the intersection of vertical line 1 and horizontal line 2
        // in a zero-indexed grid
        static Vector2d st10 = new Vector2d(0.25, 0);
        static Vector2d st20 = new Vector2d(0.5, 0);

        static Vector2d st01 = new Vector2d(0, 0.334);
        static Vector2d st11 = new Vector2d(0.25, 0.334);
        static Vector2d st21 = new Vector2d(0.5, 0.334);
        static Vector2d st31 = new Vector2d(0.75, 0.334);
        static Vector2d st41 = new Vector2d(1, 0.334);

        static Vector2d st02 = new Vector2d(0, 0.666);
        static Vector2d st12 = new Vector2d(0.25, 0.666);
        static Vector2d st22 = new Vector2d(0.5, 0.666);
        static Vector2d st32 = new Vector2d(0.75, 0.666);
        static Vector2d st42 = new Vector2d(1, 0.666);

        static Vector2d st13 = new Vector2d(0.25, 1);
        static Vector2d st23 = new Vector2d(0.5, 1);

        // Skybox texture coordinates
        static Vector2d[] stSkyTopCoords = new Vector2d[] { st12, st13, st23, st22 };
        static Vector2d[] stSkyBottomCoords = new Vector2d[] { st21, st20, st10, st11 };
        static Vector2d[][] stSkySideCoords = new Vector2d[][]{
          new  Vector2d[]  { st11, st12, st22, st21},
                           new  Vector2d[]                       { st01, st02, st12, st11},
                                                new  Vector2d[]  { st31, st32, st42, st41},
                                                new  Vector2d[]  { st21, st22, st32, st31}
        };


        int windowWidth = 1200;
        int windowHeight = 600;

        const int groundLength = 36;          // Default ground length
        const int groundWidth = 36;           // Default ground height
        const int worldSize = 300;            // Size of the world, used for the skybox
        const double tpViewportRatio = 0.7; // Window Width Ratio for the third-person viewport

        static int program = 0;
        static int texMapLocation;
        static int texModeLocation;
        static int spotlightModeLocation;

        //Boundaries of the city viewport
        static double cityViewportX;
        static double cityViewportY;
        static double cityViewportWidth;
        static double cityViewportHeight;

        //Boundaries of the first-person viewport
        static double fpViewportX;
        static double fpViewportY;
        static double fpViewportWidth;
        static double fpViewportHeight;

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

        //Material properties for the ground blocks
        static float[] block_mat_ambient = { 0.3F, 0.2F, 0.2F, 1.0F };
        static float[] block_mat_specular = { 0.4F, 0.3F, 0.3F, 1.0F };
        static float[] block_mat_diffuse = { 0.9F, 0.8F, 0.8F, 1.0F };
        static float[] block_mat_shininess = { 0.8F };

        // Ground material properties
        static float[] ground_ambient = { 0.6F, 0.5F, 0.5F, 1.0F };
        static float[] ground_specular = { 0.1F, 0.1F, 0.1F, 1.0F };
        static float[] ground_diffuse = { 0.3F, 0.3F, 0.3F, 1.0F };
        static float[] ground_shininess = { 0.1F };

        // Street material properties
        static float[] street_ambient = { 0.6F, 0.5F, 0.5F, 1.0F };
        static float[] street_specular = { 0.15F, 0.1F, 0.1F, 1.0F };
        static float[] street_diffuse = { 0.4F, 0.3F, 0.3F, 1.0F };
        static float[] street_shininess = { 0.1F };

        // Street Map material properties
        static float[] streetMap_ambienе = { 0.4F, 0.4F, 0.4F, 1.0F };
        static float[] streetMap_specular = { 0.15F, 0.1F, 0.1F, 1.0F };
        static float[] streetMap_diffuse = { 0.3F, 0.3F, 0.3F, 1.0F };
        static float[] streetMap_shininess = { 0.1F };

        // Drone Player Map Icon material properties
        static float[] dpMap_ambient = { 0.05F, 0.05F, 1.0F, 1.0F };
        static float[] dpMap_specular = { 1.0F, 1.0F, 1.0F, 1.0F };
        static float[] dpMap_diffuse = { 1.0F, 1.0F, 1.0F, 1.0F };
        static float[] dpMap_shininess = { 0.8F };

        // Drone Enemy Map Icon material properties
        static float[] deMap_ambient = { 1.0F, 0.05F, 0.05F, 1.0F };
        static float[] deMap_specular = { 1.0F, 1.0F, 1.0F, 1.0F };
        static float[] deMap_diffuse = { 1.0F, 1.0F, 1.0F, 1.0F };
        static float[] deMap_shininess = { 0.8F };

        /* shader reader */
        /* creates null terminated string from file */

        static string readShaderSource(string shaderFile)
        {
            var txt = System.IO.File.ReadAllText(shaderFile);
            return txt;
        }

        /* GLSL initialization */

        static void initShader(string vShaderFile, string fShaderFile)
        {

            bool status = GL.GetError() == ErrorCode.NoError;
            string vSource, fSource;
            int vShader, fShader;


            /* read shader files */

            vSource = readShaderSource(vShaderFile);
            //checkError(status, "Failed to read vertex shader");

            fSource = readShaderSource(fShaderFile);
            //checkError(status, "Failed to read fragment shader");

            ///* create program and shader objects */

            vShader = GL.CreateShader(ShaderType.VertexShader);
            fShader = GL.CreateShader(ShaderType.FragmentShader);
            program = GL.CreateProgram();

            ///* attach shaders to the program object */

            GL.AttachShader(program, vShader);
            GL.AttachShader(program, fShader);

            ///* read shaders */

            GL.ShaderSource(vShader, vSource);
            GL.ShaderSource(fShader, fSource);

            ///* compile shaders */

            GL.CompileShader(vShader);
            GL.CompileShader(fShader);

            ///* error check */

            //glGetShaderiv(vShader, GL_COMPILE_STATUS, &status);
            //checkError(status, "Failed to compile the vertex shader.");

            //glGetShaderiv(fShader, GL_COMPILE_STATUS, &status);
            //checkError(status, "Failed to compile the fragment shader.");

            ///* link */

            GL.LinkProgram(program);
            //glGetShaderiv(program, GL_LINK_STATUS, &status);
            ////checkError(status, "Failed to link the shader program object.");

            ///* use program object */

            GL.UseProgram(program);

            ///* set up uniform parameter */

            texMapLocation = GL.GetUniformLocation(program, "texMap");
            texModeLocation = GL.GetUniformLocation(program, "texMode");
            spotlightModeLocation = GL.GetUniformLocation(program, "spotlightMode");
        }


        // Callback, called at initialization and whenever user resizes the window.
        void reshape(int w, int h)
        {
            windowWidth = w;
            windowHeight = h;
            float padding = 5;

            //Boundaries of the city viewport
            cityViewportX = windowWidth * tpViewportRatio + padding;
            cityViewportY = 0;
            cityViewportWidth = windowWidth - cityViewportX;
            cityViewportHeight = windowHeight * 0.5 - padding / 2.0;


            //Boundaries of the first-person viewport
            fpViewportX = windowWidth * tpViewportRatio + padding;
            fpViewportY = cityViewportHeight + padding;
            fpViewportWidth = windowWidth - fpViewportX;
            fpViewportHeight = windowHeight * 0.5 - padding / 2.0;
        }

        void defaultSetup()
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
        }
        // Set up OpenGL. For viewport and projection setup see reshape(). */
        void initOpenGL(int w, int h)
        {
            defaultSetup();

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
                var bitmap = Bitmap.FromFile(filename) as Bitmap;
                //RenderHelpers.LoadTexture(bmp);
                //unsigned char* data = stbi_load(filename, &x, &y, &n, 0);

                GL.BindTexture(TextureTarget.Texture2D, 2000 + i);
                BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
           ImageLockMode.ReadOnly, bitmap.PixelFormat);
                var format = PixelInternalFormat.Rgba;
                var format2 = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;

                var ps = System.Drawing.Image.GetPixelFormatSize(bitmap.PixelFormat);
                if (ps == 32)
                {
                    n = 4;
                }
                else if (ps == 24)
                {
                    format = PixelInternalFormat.Rgb;
                    format2 = OpenTK.Graphics.OpenGL.PixelFormat.Bgr;
                    n = 3;
                }
                if (ps == 8)
                {
                    format = PixelInternalFormat.R8;
                    format2 = OpenTK.Graphics.OpenGL.PixelFormat.Red;
                }
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);


                // ... process data if not NULL ...
                // ... x = width, y = height, n = # 8-bit components per pixel ...
                // ... replace '0' with '1'..'4' to force that many components per pixel
                // ... but 'n' will always be the number that it would have been if you said 0
                if (n == 3)
                {
                    //GL.TexImage2D(GL_TEXTURE_2D, 0, GL_RGB, x, y, 0, GL_RGB, GL_UNSIGNED_BYTE, data);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, format, data.Width, data.Height, 0,
                format2, PixelType.UnsignedByte, data.Scan0);
                }
                else if (n == 4)
                {
                    // GL.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, x, y, 0, GL_RGBA, GL_UNSIGNED_BYTE, data);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, format, data.Width, data.Height, 0,
                format2, PixelType.UnsignedByte, data.Scan0);
                }

                bitmap.UnlockBits(data);
                bitmap.Dispose();
                //stbi_image_free(data);
            }
        }

        // Callback, called whenever GLUT determines that the window should be redisplayed
        // or glutPostRedisplay() has been called.
        void display()
        {
            defaultSetup();

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

            GL.Viewport(0, 0, (int)(windowWidth * tpViewportRatio), windowHeight);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            var pp = Matrix4d.CreatePerspectiveFieldOfView(Helpers.ToRadians(60.0), (windowWidth * tpViewportRatio / windowHeight), 0.01, 400.0);
            GL.LoadMatrix(ref pp);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            pp = Matrix4d.LookAt(tpCamera.position.X, tpCamera.position.Y, tpCamera.position.Z, tpCamera.focus.X, tpCamera.focus.Y, tpCamera.focus.Z, 0, 1, 0);
            GL.LoadMatrix(ref pp);


            GL.Light(LightName.Light0, LightParameter.Position, light_position0);

            GL.Uniform1(texMapLocation, 0);
            GL.Uniform1(texModeLocation, 0);

            drawAssets();

            GL.Viewport((int)fpViewportX, (int)fpViewportY, (int)fpViewportWidth, (int)fpViewportHeight);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            pp = Matrix4d.CreatePerspectiveFieldOfView(Helpers.ToRadians(60.0),
                (fpViewportWidth / fpViewportHeight), 0.005, 400.0);
            GL.LoadMatrix(ref pp);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref pp);

            //float invertedY = fpCamera.focus.y - (fpCamera.position.y - fpCamera.focus.y);
            //GL.LookAt(fpCamera.focus.x, fpCamera.focus.y, fpCamera.focus.z, fpCamera.position.x, invertedY, fpCamera.position.z, 0, 1, 0);
            //GL.Light(GL_LIGHT0, GL_POSITION, light_position0);

            //glUniform1i(spotlightModeLocation, 1);
            drawAssets();
            //glUniform1i(spotlightModeLocation, 0);

            GL.Viewport((int)cityViewportX, (int)cityViewportY, (int)cityViewportWidth, (int)cityViewportHeight);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            //gluPerspective(20.0, (GLdouble)(cityViewportWidth / cityViewportWidth), 0.01, 400.0);

            pp = Matrix4d.CreatePerspectiveFieldOfView(Helpers.ToRadians(20),
                (cityViewportWidth / cityViewportWidth), 0.01, 400.0);
            GL.LoadMatrix(ref pp);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            //gluLookAt(0.1, 101, 0, 0, 0, 0, 0, 1, 0);
            pp = Matrix4d.LookAt(0.1, 101, 0, 0, 0, 0, 0, 1, 0);


            GL.LoadMatrix(ref pp);


            //glLightfv(GL_LIGHT0, GL_POSITION, light_position0);

            //glUniform1i(texModeLocation, 2);
            drawMap();


            //gl.SwapBuffers();   // Double buffering, swap buffers
        }

        void drawAssets()
        {
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)OpenTK.Graphics.OpenGL.All.Modulate);

            Vector2d[] stCoordinates = new Vector2d[]{ new Vector2d(0, 0),
                new Vector2d(0, 1), new Vector2d(1, 1), new Vector2d(1, 0)};


            // Set material properties of the streets
            GL.Material(MaterialFace.Front, MaterialParameter.Ambient, street_ambient);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, street_specular);
            GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, street_diffuse);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, street_shininess);

            for (int i = 0; i < streets.Count; i++)
            {
                GL.PushMatrix();
                GL.Translate(0, 0.001 * i, 0);
                streets[i].draw(2016);
                GL.PopMatrix();
            }

            //skybox
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)OpenTK.Graphics.OpenGL.All.Decal);
            GL.Disable(EnableCap.CullFace);
            GL.Uniform1(texModeLocation, 1);
            GL.PushMatrix();
            GL.Translate(0, 0, 0);
            skybox.draw(2002, stSkySideCoords, stSkyTopCoords, stSkyBottomCoords);
            GL.PopMatrix();
            GL.Uniform1(texModeLocation, 0);
            //GL.TexEnv(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_MODULATE);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            // Set ground block material properties
            //GL.Material(GL_FRONT, GL_AMBIENT, block_mat_ambient);
            //GL.Material(GL_FRONT, GL_SPECULAR, block_mat_specular);
            //GL.Material(GL_FRONT, GL_DIFFUSE, block_mat_diffuse);
            //GL.Material(GL_FRONT, GL_SHININESS, block_mat_shininess);

            for (int i = 0; i < buildings.Count; i++)
            {
                buildings[i].draw(2003 + buildingTextures[i], stCoordinates, true, 2009 + roofTextures[i]);
            }

            // Set material properties of the ground
            //GL.Materialfv(GL_FRONT, GL_AMBIENT, ground_ambient);
            //GL.Materialfv(GL_FRONT, GL_SPECULAR, ground_specular);
            //GL.Materialfv(GL_FRONT, GL_DIFFUSE, ground_diffuse);
            //GL.Materialfv(GL_FRONT, GL_SHININESS, ground_shininess);

            //Draw ground quad
            ground.draw(2000, stCoordinates, false);

            droneEnemy.draw();
            dronePlayer.draw();
        }
        public void drawMap()
        {

            // Set material properties of the streets
            /*glMaterialfv(GL_FRONT, GL_AMBIENT, streetMap_ambient);
            glMaterialfv(GL_FRONT, GL_SPECULAR, streetMap_specular);
            glMaterialfv(GL_FRONT, GL_DIFFUSE, streetMap_diffuse);
            glMaterialfv(GL_FRONT, GL_SHININESS, streetMap_shininess);*/

            for (int i = 0; i < streets.Count; i++)
            {
                streets[i].draw();
            }

            // Set ground block material properties
            /*glMaterialfv(GL_FRONT, GL_AMBIENT, block_mat_ambient);
            glMaterialfv(GL_FRONT, GL_SPECULAR, block_mat_specular);
            glMaterialfv(GL_FRONT, GL_DIFFUSE, block_mat_diffuse);
            glMaterialfv(GL_FRONT, GL_SHININESS, block_mat_shininess);*/

            for (int i = 0; i < buildings.Count; i++)
            {
                buildings[i].draw();
            }

            // Set material properties of the ground
            /*glMaterialfv(GL_FRONT, GL_AMBIENT, ground_ambient);
            glMaterialfv(GL_FRONT, GL_SPECULAR, ground_specular);
            glMaterialfv(GL_FRONT, GL_DIFFUSE, ground_diffuse);
            glMaterialfv(GL_FRONT, GL_SHININESS, ground_shininess);*/

            //Draw ground quad
            ground.draw();

            // Set material properties of the the drone player's map icon
            /*glMaterialfv(GL_FRONT, GL_AMBIENT, dpMap_ambient);
            glMaterialfv(GL_FRONT, GL_SPECULAR, dpMap_specular);
            glMaterialfv(GL_FRONT, GL_DIFFUSE, dpMap_diffuse);
            glMaterialfv(GL_FRONT, GL_SHININESS, dpMap_shininess);*/

            GL.PushMatrix();
            GL.Translate(dronePlayer.getPosition().X, 10, dronePlayer.getPosition().Z);
            GL.Rotate(dronePlayer.getRotationY(), 0, 1, 0);
            dpMapIcon.draw();
            GL.PopMatrix();

            // Set material properties of the the drone player's map icon
            /*glMaterialfv(GL_FRONT, GL_AMBIENT, deMap_ambient);
            glMaterialfv(GL_FRONT, GL_SPECULAR, deMap_specular);
            glMaterialfv(GL_FRONT, GL_DIFFUSE, deMap_diffuse);
            glMaterialfv(GL_FRONT, GL_SHININESS, deMap_shininess);*/

            GL.PushMatrix();
            GL.Translate(droneEnemy.getPosition().X, 10, droneEnemy.getPosition().Z);
            GL.Rotate(droneEnemy.getRotationY(), 0, 1, 0);
            deMapIcon.draw();
            GL.PopMatrix();
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

            var lines = System.IO.File.ReadAllLines(filename);
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
                reshape(Width, Height);

                initShader("Shaders/vShader.glsl", "Shaders/fShader.glsl");
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

            GL.Viewport(0, 0, (int)(windowWidth * tpViewportRatio), windowHeight);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            var pp = Matrix4d.CreatePerspectiveFieldOfView(Helpers.ToRadians(60.0), (windowWidth * tpViewportRatio / windowHeight), 0.01, 400.0);
            GL.LoadMatrix(ref pp);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            pp = Matrix4d.LookAt(20, 20, 20, 0, 0, 0, 0, 1, 0);
            GL.LoadMatrix(ref pp);

            /////////
            GL.Disable(EnableCap.Lighting);
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(100, 0, 0);
            GL.Color3(Color.Blue);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 100, 0);
            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 100);
            GL.End();

            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.Lighting);

            Vector2d[] stCoordinates = new Vector2d[]{ new Vector2d(0, 0),
                new Vector2d(0, 1), new Vector2d(1, 1), new Vector2d(1, 0)};
            ground.draw(2000, stCoordinates, false);

            foreach (var item in buildings)
            {
                item.draw();
            }
            foreach (var item in streets)
            {
                item.draw();
            }
            //display();

            gl.SwapBuffers();
        }

        //CameraOld camera = new CameraOld(new OpenTK.Vector3(0.0f, 0.0f, 3.0f));
    }
}