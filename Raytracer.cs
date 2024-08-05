using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace raytracer
{


    class Ray
    {
        public Vector3 direction;
        public Vector3 position;

        public Ray(Vector3 p, Vector3 d){
            this.direction = d;
            this.position = p;
        }
    }


    class Camera
    {
        public Vector3 cameraposition;
        public Vector3 viewDirection;
        public float fieldOfview;
        public float aspectRadio;

        public Vector3 topleft;
        public float height;
        public float width;
        public Vector3 initialup;
        public Vector3 up;
        public Vector3 right;

        
        public Camera(Vector3 p, Vector3 v, float f, float a){
            this.cameraposition = p;
            this.viewDirection = v.Normalized();
            this.fieldOfview = f;
            this.aspectRadio = a;


            initialup = new Vector3 (0,1,0);
            height = 2*((float)Math.Tan(fieldOfview/2));
            width = aspectRadio * height;
            right = Vector3.Cross(viewDirection, initialup);
            right.Normalize();
            up = Vector3.Cross(right, viewDirection);
            up.Normalize();
            topleft = cameraposition+ viewDirection+ up*height/2 - right*width/2;
        }

        public Ray GetCameraRay(float x, float y){
            Vector3 planepos = topleft+ right*width*x - up*height*y;
            Vector3 direct = planepos - cameraposition;
            direct.Normalize();
            Ray planeray = new Ray (cameraposition, direct);
            return planeray;
        }
    }
       // class wrapper{
       //     public Vector3 
       // }
 
        class Sphere{
            public Vector3 center;
            public Vector3 color;
            public float radius;

            public Sphere(Vector3 center, Vector3 color, float radius){
                this.center=center;
                this.color=color;
                this.radius=radius;
            }

            public float Intersects(Ray ray){
                Vector3 q = ray.position- this.center;
                float a = (ray.direction.X)*(ray.direction.X)+(ray.direction.Y)*(ray.direction.Y)+(ray.direction.Z)*(ray.direction.Z);
                float b = 2*q.X*ray.direction.X + 2*q.Y*ray.direction.Y + 2*q.Z*ray.direction.Z;
                float c = q.X*q.X+q.Y*q.Y+q.Z*q.Z-this.radius*this.radius;
                float discriminant = b*b-4*a*c;
                if((discriminant)>0){
                    float point1 =( -b+MathF.Sqrt(discriminant))/2*a;
                    float point2 =( -b-MathF.Sqrt(discriminant))/2*a;
                    if (Math.Min(point1, point2)<0)
                    {
                        return Math.Max(point1, point2);
                    }
                    else {
                        return Math.Min(point1, point2);
                    }


                }
                else if((discriminant)==0){
                    float point0 = ( -b-MathF.Sqrt(discriminant))/2*a;
                    return point0;
                }
                else{
                    return -1;
                }


            }
        }


    class Intersection{
            public float t;
            public Sphere sphere;

            public Intersection(float t, Sphere sphere){
                this.t=t;
                this.sphere=sphere;
            }
        }

    

    class Scene{
        List<Sphere> spheres = new List <Sphere>();
        public Scene(){
            spheres.Add(new Sphere(new Vector3(10,5,6), new Vector3(1,0,1), 1f));
            spheres.Add(new Sphere(new Vector3(5,3,3), new Vector3(1,0,0), 1f));
            spheres.Add(new Sphere(new Vector3(3,5,6), new Vector3(1,0.8f,0.3f), 2f));
            spheres.Add(new Sphere(new Vector3(10,4,0), new Vector3(0.5f, 0.3f,0f), 1.5f));
        }
        public Intersection FindClosestIntersection(Ray r)
        {
            Intersection closestIntersection = null; 
            foreach(Sphere s in spheres){
                float t = s.Intersects(r);
                if(t<0) continue;

                if(closestIntersection==null){
                    closestIntersection = new Intersection(t,s);
                }
                else if(t<closestIntersection.t){
                    closestIntersection.t =t;
                    closestIntersection.sphere= s;
                }
            }
            return closestIntersection;

        }
    }

    class RayTracer
    {
        Surface surface;
        Game window;
        Random rng =new Random();
        Image<Rgba32> image;
        Image<Rgba32> background;

        public RayTracer(Surface surface, Game window)
        {
            this.surface = surface;
            this.window = window;

            image = Image.Load<Rgba32>("picture.jpg");
            background = Image.Load<Rgba32>("background.jpg");
        }

        public void Render(){
            float samplesNumber=0;

            Camera cam = new Camera (new Vector3(2,5,0), new Vector3 (1,-0.3f,1), 0.5f* (float)Math.PI, 1.7f);
            Scene scene= new Scene ();

            for (int x = 0; x < surface.width; x++){
            {
                for (int y = 0; y < surface.height; y++)
                {
                    Vector3 finalColor = new Vector3(0,0,0);
                    for ( samplesNumber=0; samplesNumber<16; samplesNumber++)
                    {   

                    float h = (y+(float)rng.NextDouble())/(float)surface.height;
                    float w = (x+(float)rng.NextDouble())/(float)surface.width;

                    Ray PixelRay = cam.GetCameraRay(w,h);

                    float planedistance=PixelRay.position.Y/-PixelRay.direction.Y;
                    Intersection closestIntersection = scene.FindClosestIntersection(PixelRay);
                    Vector3 color = new Vector3(0,0,0);
                            float X = (int)(PixelRay.direction.Y*3000);
                            float Y = (int)(PixelRay.direction.Z*3000);
                            
                            while(X>(background.Width-1)){
                                X=X-background.Width;
                            }
                            if (X<0){
                                X=Math.Abs(X);
                            }
                            
                            while(Y>(background.Height-1)){
                                Y=Y-background.Height;
                            }
                            if (Y<0){
                                Y=Math.Abs(Y);
                            }
                            Rgba32 d = background[(int)X,(int)Y];   
                            color = (d.R/255f, d.G/255f,d.B/255f);

                    Vector3 normal= new Vector3(0,1,0);
                    Vector3 intersectionPoint = new Vector3(1,1,1);
                    float closestDistance = 1000000;
                    
                   
                    


                        if(closestIntersection != null){
                            closestDistance=closestIntersection.t;
                            intersectionPoint = PixelRay.position+PixelRay.direction*closestDistance;
                            normal = (intersectionPoint-closestIntersection.sphere.center).Normalized();
                            color = closestIntersection.sphere.color;
                            //float yaw = Math.Atan(normal.XZ)/360;
                            //float pitch = Math.Acos(normal.Y)/360;

                        }
                        if (planedistance > 0 && planedistance < closestDistance){
                            closestDistance=planedistance; 
                            intersectionPoint = PixelRay.position+PixelRay.direction*closestDistance;
                            normal = (0,1,0);
                            float K = (int)(intersectionPoint.X*500);
                            float L = (int)(intersectionPoint.Z*500);
                            
                            if (K<0){
                                K=Math.Abs(K);
                            }
                            while((K>image.Width-1)){
                                K=K-image.Width;
                            }
                            
                            if (L<0){
                                L=Math.Abs(L);
                            }
                            while((L>image.Height-1)){
                                L=L-image.Height;
                            }
                            Rgba32 c = image[(int)K,(int)L];   
                            color = (c.R/255f, c.G/255f,c.B/255f);
                        }                     
                        

                        Vector3 lightSource = new Vector3 (7,8,0);
                        Vector3 v = new Vector3 ((float)rng.NextDouble()*2-1,(float)rng.NextDouble()*2-1,(float)rng.NextDouble()*2-1);
                            while(v.Length > 1) {
                                v = new Vector3 ((float)rng.NextDouble()*2-1,(float)rng.NextDouble()*2-1,(float)rng.NextDouble()*2-1);
                            };
                        Vector3 randomPoint = lightSource + v;
                        Vector3 toLight = (randomPoint - intersectionPoint).Normalized();
                        float toLightDistance = Vector3.Distance(randomPoint, intersectionPoint);
                        float lightIntensity = 1/toLightDistance/toLightDistance;
                        Vector3 lightColor= new Vector3(50*lightIntensity,50*lightIntensity,50*lightIntensity);
                        float lambert = System.Math.Clamp(Vector3.Dot(toLight, normal),0f,1f);
                        


                        Ray schadowRay = new Ray (intersectionPoint+toLight*0.001f, toLight);
                        Intersection schadowIntersection = scene.FindClosestIntersection(schadowRay);

                        if(schadowIntersection != null){
                            finalColor += (0,0,0);
                        }
                        else {
                            finalColor += color*lightColor*lambert;
                        }
                    }
                    finalColor=finalColor/samplesNumber;

                    surface.SetPixel(x, y, finalColor);
                    

                


                   // else{


                     //   Vector3 point = PixelRay.position + planedistance*PixelRay.direction;
                    //    float color = (MathF.Sin(point.Z)+1)/4;
                    //    }

                        //    surface.SetPixel(x,y,1f,color+0.3f,color+0.3f);
                    /*if(PixelRay.direction.Y < 0){
                        Vector3 point = PixelRay.position +
                         distance*PixelRay.direction;
                        float color = (MathF.Sin(point.Z)+1)/4;
                        //    surface.SetPixel(x,y,1f,color+0.3f,color+0.3f);
                        // if((int)point.Z % 4 ==0 ){
                        //    surface.SetPixel(x,y,1f,1f,1f);
                        //  }
                        //  else{
                        //      surface.SetPixel(x,y,0f,1f,0f);
                        //  }
                    }*/
                
                    //surface.SetPixel(x, y, PixelRay.direction.X, PixelRay.direction.Y, PixelRay.direction.Z);
                    
                
                }
            }
            }
        }
        }
    }