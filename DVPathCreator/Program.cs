using DJIWTF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVPathCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            DroneConfiguration config = new DroneConfiguration();
            config.Speed = 25;
            config.MaxHeight = 250;
            List<DronePoint> path = new List<DronePoint>();
            DronePoint p1 = new DronePoint()
            {
                OrderId = 1,
                Rotation = 0,
                TakeImage = false,
                X = 0,
                Y = 100,
                Z = 0
            };
            DronePoint p2 = new DronePoint()
            {
                OrderId = 2,
                Rotation = 90,
                TakeImage = false,
                X = -75,
                Y = 100,
                Z = 0
            };
            DronePoint p3 = new DronePoint()
            {
                OrderId = 3,
                Rotation = 90,
                TakeImage = false,
                X = -150,
                Y = 100,
                Z = 0
            };
            DronePoint p4 = new DronePoint()
            {
                OrderId = 4,
                Rotation = 90,
                TakeImage = false,
                X = -225,
                Y = 100,
                Z = 0
            };
            DronePoint p5 = new DronePoint()
            {
                OrderId = 5,
                Rotation = 0,
                TakeImage = false,
                X = -300,
                Y = 100,
                Z = 0
            };
            DronePoint p6 = new DronePoint()
            {
                OrderId = 6,
                Rotation = 0,
                TakeImage = false,
                X = -300,
                Y = 100,
                Z = -100
            };
            DronePoint p7 = new DronePoint()
            {
                OrderId = 7,
                Rotation = 90,
                TakeImage = false,
                X = -225,
                Y = 100,
                Z = -100
            };
            DronePoint p8 = new DronePoint()
            {
                OrderId = 8,
                Rotation = 90,
                TakeImage = false,
                X = -150,
                Y = 100,
                Z = -100
            };
            DronePoint p9 = new DronePoint()
            {
                OrderId = 9,
                Rotation = 90,
                TakeImage = false,
                X = -75,
                Y = 100,
                Z = -100
            };
            DronePoint p10 = new DronePoint()
            {
                OrderId = 10,
                Rotation = 90,
                TakeImage = false,
                X = 0,
                Y = 100,
                Z = -100
            };
            DronePoint p11 = new DronePoint()
            {
                OrderId = 11,
                Rotation = 0,
                TakeImage = false,
                X = 75,
                Y = 100,
                Z = -100
            };
            path.Add(p1);
            path.Add(p2);
            path.Add(p3);
            path.Add(p4);
            path.Add(p5);
            path.Add(p6);
            path.Add(p7);
            path.Add(p8);
            path.Add(p9);
            path.Add(p10);
            path.Add(p11);
            config.Path = path;
            string json = JsonConvert.SerializeObject(config);
            Console.WriteLine(json);
            Console.ReadKey();
        }
    }
}
