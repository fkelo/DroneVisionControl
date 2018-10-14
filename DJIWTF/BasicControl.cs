using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelloLib;

namespace DJIWTF
{
    class BasicControl
    {

        private DroneConfiguration Config = null;
        private int X = 0;
        private int Y = 0;
        private int Z = 0;
        private string Runid;
        private int MaxHeight;
        private bool running = false;
        private int currentOrderId;
        private bool completed;
        private List<int> cmdIds = new List<int>();

        public bool AbortState = false;
        private Tello.ConnectionState state;
        private int batState;

        public void ConnectionStateDelegate(Tello.ConnectionState newState)
        {
            // update view with all parameters
            state = newState;
            Console.WriteLine(newState.ToString());
        }

        public void UpdateCmdDelegate(int cmdId)
        {
            // update view with all parameters
            batState = Tello.getBattery();
            if(cmdId == 98)
            {
                Console.WriteLine("Start Picture Download");
            }else if(cmdId == 48)
            {
                Console.WriteLine("Picture Ack");
            }else if(cmdId == 100)
            {
                Console.WriteLine("Picture finished");
            }
            
        }

        public void VideoUpdateDelegate(byte[] data)
        {

        }

        public BasicControl(string baseImgUrl, int maxHeight, bool autoConnect)
        {
            Tello.picPath = baseImgUrl;
            Tello.onConnection += new Tello.connectionDeligate(ConnectionStateDelegate);
            Tello.onUpdate += new Tello.updateDeligate(UpdateCmdDelegate);
            Tello.onVideoData += new Tello.videoUpdateDeligate(VideoUpdateDelegate);
            MaxHeight = maxHeight;
            if (autoConnect)
            {
                Tello.startConnecting();
                int counter = 0;
                while (!Tello.connectionState.Equals(Tello.ConnectionState.Connected))
                {
                    System.Threading.Thread.Sleep(500);
                    counter++;
                    if(counter > 10)
                    {
                        break;
                    }
                }
                if (Tello.connected == true)
                {
                    Tello.setMaxHeight(maxHeight);
                }
            }
        }

        public void Connect(bool force)
        {
            switch (Tello.connectionState)
            {
                case Tello.ConnectionState.Connected:
                    break;
                case Tello.ConnectionState.Connecting:
                    break;
                case Tello.ConnectionState.Disconnected:
                    //Tello.startConnecting();
                    if (Tello.connected == true)
                    {
                        Tello.setMaxHeight(MaxHeight);
                    }
                    break;
                case Tello.ConnectionState.Paused:
                    break;
                case Tello.ConnectionState.UnPausing:
                    break;
            }
        }

        public void Pause(bool force)
        {
            if (Tello.connected == false)
            {
                return;
            }
            if (force)
            {
                Tello.connectionSetPause(true);
            }
            switch (Tello.connectionState)
            {
                case Tello.ConnectionState.Connected:
                    Tello.connectionSetPause(true);
                    break;
                case Tello.ConnectionState.Connecting:
                    break;
                case Tello.ConnectionState.Disconnected:
                    break;
                case Tello.ConnectionState.Paused:
                    break;
                case Tello.ConnectionState.UnPausing:
                    break;
            }
        }

        public void Unpause(bool force)
        {
            if (Tello.connected == false)
            {
                return;
            }
            if (force)
            {
                Tello.connectionSetPause(false);
            }
            switch (Tello.connectionState)
            {
                case Tello.ConnectionState.Connected:
                    break;
                case Tello.ConnectionState.Connecting:
                    break;
                case Tello.ConnectionState.Disconnected:
                    break;
                case Tello.ConnectionState.Paused:
                    Tello.connectionSetPause(false);
                    break;
                case Tello.ConnectionState.UnPausing:
                    break;
            }
        }

        public async Task StartAsync(string runId)
        {
            var task = Task.Run(() => Start(runId, 0, true));
            await task;
        }

        public async Task ExecuteScriptAsync(string runId)
        {
            var task = Task.Run(() => ExecuteScript(runId));
            await task;
        }

        public void ExecuteScript(String runId)
        {
            if (Tello.connected == false && !Tello.TestMode)
            {
                return;
            }
            Thread.Sleep(500);
            if (!Tello.state.flying)
            {
                Tello.takeoffCmd();
                if (!Tello.TestMode) Thread.Sleep(5000);
            }

            if (Config != null)
            {
                DronePoint currentPoint = new DronePoint();
                currentPoint.X = 0;
                currentPoint.Y = 0;
                currentPoint.Z = 0;
                currentPoint.Rotation = 0;
                int currentView = 0;
                running = true;
                AbortState = false;
                completed = false;
                Runid = runId;
                currentOrderId = -1;

                List<DronePoint> sorted = Config.Path.OrderBy(o => o.OrderId).ToList();

                foreach(DronePoint pnt in sorted)
                {
                    Console.WriteLine("New Point");
                    if (AbortState)
                    {
                        running = false;
                        return;
                    }
                    currentOrderId = pnt.OrderId;
                    currentView = currentView % 360;
                    if (currentView < 360) currentView += 360;
                    DronePoint vector = CalculateVector(currentPoint, pnt, currentView);
                    if(vector.Y != 0)
                    {
                        if(vector.Y > 0)
                        {
                            Console.WriteLine("UP : " + Math.Abs(vector.Y));
                            //Tello.up(Math.Abs(vector.Y));
                        }
                        else
                        {
                            Console.WriteLine("DOWN : " + Math.Abs(vector.Y));
                            //Tello.down(Math.Abs(vector.Y));
                        }
                    }
                    if(vector.Rotation != 0)
                    {
                        if(vector.Rotation > 0)
                        {
                            Console.WriteLine("ROT RIGHT : " + Math.Abs(vector.Rotation));
                            Tello.cw(Math.Abs(vector.Rotation));
                            currentView += Math.Abs(vector.Rotation);
                        }
                        else
                        {
                            Console.WriteLine("ROT LEFT : " + Math.Abs(vector.Rotation));
                            Tello.ccw(Math.Abs(vector.Rotation));
                            currentView -= Math.Abs(vector.Rotation);
                        }
                    }
                    if(vector.X != 0)
                    {
                        Console.WriteLine("FORWARD X : " + Math.Abs(vector.X));
                        Tello.forward(Math.Abs(vector.X));
                    }
                    if(vector.Z != 0)
                    {
                        Console.WriteLine("FORWARD Z : " + Math.Abs(vector.Z));
                        Tello.forward(Math.Abs(vector.Z));
                    }
                    if(pnt.Rotation != 0)
                    {
                        if(pnt.Rotation > 0)
                        {
                            Console.WriteLine("ROT RIGHT : " + Math.Abs(pnt.Rotation));
                            Tello.cw(Math.Abs(pnt.Rotation));
                            currentView += Math.Abs(pnt.Rotation);
                        }
                        else
                        {
                            Console.WriteLine("ROT LEFT : " + Math.Abs(pnt.Rotation));
                            Tello.ccw(Math.Abs(pnt.Rotation));
                            currentView -= Math.Abs(pnt.Rotation);
                        }
                    }
                    if (pnt.TakeImage)
                    {
                        Console.WriteLine("TAKE IMAGE");
                        Tello.takePicture();
                    }
                    currentPoint = pnt;
                    currentPoint.Rotation = (currentPoint.Rotation + vector.Rotation) % 360;
                    if(currentPoint.Rotation < 0)
                    {
                        currentPoint.Rotation = currentPoint.Rotation + 360;
                    }
                    if (!Tello.TestMode)
                    {
                        Thread.Sleep(5000);
                    }
                }
                completed = true;
                Console.WriteLine("FLIP FORWARD");
                Tello.flip("f");
                if (!Tello.TestMode)
                {
                    Thread.Sleep(5000);
                }
                Console.WriteLine("FLIP LEFT");
                Tello.flip("l");
                if (!Tello.TestMode)
                {
                    Thread.Sleep(5000);
                }
                Console.WriteLine("LAND");
                Tello.land();
            }
            running = false;
        }

        public void Start(string runid, int orderId, bool asc)
        {
            if (Tello.connected == false && !Tello.TestMode)
            {
                return;
            }
            if (Config != null)
            {
                // Read Configuration
                // Set max height
                // Startup with Point 0,0,0 <-- store current configuration in class
                running = true;
                AbortState = false;
                completed = false;
                Runid = runid;
                currentOrderId = -1;
                X = 0;
                Y = 0;
                Z = 0;
                int Rotation = 0;
                char FlightDirection = 'Z';
                List<DronePoint> sorted;
                if (asc)
                {
                    sorted = Config.Path.OrderBy(o => o.OrderId).ToList();
                }
                else
                {
                    sorted = Config.Path.OrderByDescending(o => o.OrderId).ToList();
                }
                foreach(DronePoint pnt in sorted)
                {
                    Console.WriteLine("NEW POINT");
                    if (AbortState)
                    {
                        running = false;
                        return;
                    }
                    currentOrderId = pnt.OrderId;
                    if (asc)
                    {
                        if (currentOrderId < orderId)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if(currentOrderId > orderId)
                        {
                            continue;
                        }
                    }
                    if (!Tello.state.flying)
                    {
                        Tello.takeoffCmd();
                        if(!Tello.TestMode) Thread.Sleep(5000);
                    }
                    int absDiffX = Math.Abs(X - pnt.X);
                    int absDiffY = Math.Abs(Y - pnt.Y);
                    int absDiffZ = Math.Abs(Z - pnt.Z);
                    if (!Y.Equals(pnt.Y))
                    {
                        
                        if (pnt.Y > Y)
                        {
                            Console.WriteLine("UP : " + absDiffY);
                            Tello.up(absDiffY);
                        }
                        else
                        {
                            Console.WriteLine("DOWN : " + absDiffY);
                            Tello.down(absDiffY);
                        }
                    }
                    if (!X.Equals(pnt.X))
                    {
                        // rotate if flight direction
                        if (Rotation != 0)
                        {
                            int AbsRotation = Math.Abs(Rotation);
                            if (Rotation > 0)
                            {
                                Console.WriteLine("ROT LEFT : " + AbsRotation);
                                Tello.ccw(AbsRotation);
                            }
                            else
                            {
                                Console.WriteLine("ROT RIGHT : " + AbsRotation);
                                Tello.cw(AbsRotation);
                            }
                            Rotation = 0;
                        }
                        if (pnt.X > X)
                        {
                            if (!FlightDirection.Equals("X"))
                            {
                                // turn right
                                Console.WriteLine("ROT RIGHT : 90");
                                Tello.cw(90);
                            }
                        }
                        else
                        {
                            if (!FlightDirection.Equals("X"))
                            {
                                Console.WriteLine("ROT LEFT : 90");
                                Tello.ccw(90);
                            }
                        }
                        Console.WriteLine("FORWARD : " + absDiffX);
                        Tello.forward(absDiffX);
                    }
                    else if (!Z.Equals(pnt.Z))
                    {
                        // rotate if flight direction
                        if (Rotation != 0)
                        {
                            int AbsRotation = Math.Abs(Rotation);
                            if (Rotation > 0)
                            {
                                Console.WriteLine("ROT LEFT : " + AbsRotation);
                                Tello.ccw(AbsRotation);
                            }
                            else
                            {
                                Console.WriteLine("ROT RIGHT : " + AbsRotation);
                                Tello.cw(AbsRotation);
                            }
                            Rotation = 0;
                        }
                        if (pnt.Z > Z)
                        {
                            if (!FlightDirection.Equals("Z"))
                            {
                                // turn right
                                Console.WriteLine("ROT LEFT : 90");
                                Tello.ccw(90);
                            }
                        }
                        else
                        {
                            if (!FlightDirection.Equals("Z"))
                            {
                                Console.WriteLine("ROT RIGHT : 90");
                                Tello.cw(90);
                            }
                            else
                            {
                                Console.WriteLine("U-TURN");
                                Tello.cw(180);
                            }
                        }
                        Console.WriteLine("FORWARD : " + absDiffZ);
                        FlightDirection = 'Z';
                        Tello.forward(absDiffZ);
                    }
                    // set current rotation
                    if (pnt.Rotation != 0)
                    {
                        int AbsRotation = Math.Abs(pnt.Rotation);
                        if (pnt.Rotation > 0)
                        {
                            Console.WriteLine("ROT RIGHT : " + AbsRotation);
                            Tello.cw(AbsRotation);
                        }
                        else
                        {
                            Console.WriteLine("ROT LEFT : " + AbsRotation);
                            Tello.ccw(AbsRotation);
                        }
                    }
                    if (!Tello.TestMode)
                    {
                        Thread.Sleep(5000);
                    }
                    X = pnt.X;
                    Y = pnt.Y;
                    Z = pnt.Z;
                    Rotation = pnt.Rotation;
                }
                completed = true;
                Console.WriteLine("LAND");
                Tello.land();
            }
            running = false;
        }

        private DronePoint CalculateVector(DronePoint currentState, DronePoint nextPoint, int currentView)
        {
            int dX = nextPoint.X - currentState.X;
            int dY = nextPoint.Y - currentState.Y;
            int dZ = nextPoint.Z - currentState.Z;
            int rotation = 0;
            if(dX != 0)
            {
                if(dX < 0)
                {
                    // -X view --> 270
                    rotation = 270 - currentView;
                }
                else
                {
                    // X view --> 90
                    rotation = 90 - currentView;
                }
            }else if(dZ != 0)
            {
                if(dZ < 0)
                {
                    // -Z view --> 180
                    rotation = 180 - currentView;
                }
                else
                {
                    // Z view --> 0
                    rotation = 0 - currentView;
                }
            }
            rotation = rotation % 360;
            if(rotation > 180)
            {
                rotation -= 360;
            }else if(rotation < -180)
            {
                rotation += 180;
            }
            DronePoint retVal = new DronePoint();
            retVal.X = dX;
            retVal.Y = dY;
            retVal.Z = dZ;
            retVal.Rotation = rotation;
            return retVal;
            //int rotation = 0;
            //if(dX != 0)
            //{
            //    if(dX > 0)
            //    {
            //        // desired direction 90°
            //        rotation = (90 - currentState.Rotation) % 360;
            //    }
            //    else if(dX < 0)
            //    {
            //        // desired direction 270°
            //        rotation = (270 - currentState.Rotation) % 360;
            //    }
            //}
            //else if(dZ != 0)
            //{
            //    if(dZ > 0)
            //    {
            //        // desired direction 0°
            //        rotation = (0 - currentState.Rotation) % 360;
            //    }
            //    else if(dZ < 0)
            //    {
            //        // desired direction 180°
            //        rotation = (180 - currentState.Rotation) % 360;
            //    }
            //}
            //// change direction if bigger than +/- 180 deg
            //if(rotation > 180)
            //{
            //    rotation = rotation - 360;
            //}else if(rotation < -180)
            //{
            //    rotation = rotation + 360;
            //}

        }

        //public async Task StopAsync(string runId)
        //{
        //    var task = Task.Run(() => Stop(runId));
        //    await task;
        //}

        //public void Stop(string runId)
        //{
        //    if (Tello.connected == false)
        //    {
        //        return;
        //    }
        //    if (running == true && completed == false)
        //    {
        //        AbortState = true;
        //        while (running == true)
        //        {
        //            System.Threading.Thread.Sleep(50);
        //        }
        //        Start(runId, currentOrderId, false);
        //    }
        //}

        public void Abort()
        {
            if (Tello.connected == false)
            {
                return;
            }
            AbortState = true;
            int counter = 0;
            while (running == true)
            {
                System.Threading.Thread.Sleep(50);
                counter++;
                if(counter > 40)
                {
                    break;
                }
            }
            Tello.land();
        }

        public void SetConfiguration(DroneConfiguration configuration)
        {
            if (configuration != null)
            {
                // Check Configuration

                // Check Max Height > 0
                if (!(configuration.MaxHeight > 0))
                {
                    return;
                }
                // Check every point for max height violation
                foreach(DronePoint pnt in configuration.Path)
                {
                    if(pnt.Y > configuration.MaxHeight)
                    {
                        return;
                    }
                }
                // Check only X or Z is changing from point to point
                int lX = 0;
                int lZ = 0;
                List<DronePoint> sorted = configuration.Path.OrderBy(o => o.OrderId).ToList();
                foreach(DronePoint pnt in sorted)
                {
                    int cX = pnt.X;
                    int cZ = pnt.Z;
                    if(!cX.Equals(lX) && !cZ.Equals(lZ))
                    {
                        return;
                    }
                    lX = cX;
                    lZ = cZ;
                }
                Config = configuration;
            }
        }

        public void ManualControl()
        {
            String msg = "";
            Console.WriteLine("Basic Control of Drone; help for help");
            while (true)
            {
                msg = Console.ReadLine();
                if (msg.ToLower().Equals("help"))
                {
                    Console.WriteLine("[exit]\n[help]\n[connect]\n[maxheight xx]\n[imgq xx]\n[imgm xx]\n[imgm?]\n[takeoff]\n[land]\n[up xx]\n[down xx]\n[left xx]\n[right xx]\n[forward xx]\n[back xx]\n[cw xx]\n[ccw xx]\n[flip l|r|f|b|bl|rb|fl|fr]\n[speed xx]\n[speed?]\n[battery?]\n[time?]\n[tp]\n");
                }
                else if (msg.ToLower().Equals("connect"))
                {
                    Tello.startConnecting();
                }
                else if (msg.ToLower().StartsWith("imgq"))
                {
                    string[] splitted = msg.Split(' ');
                    int qual = 1;
                    if (splitted.Length >= 2)
                    {
                        bool succ = Int32.TryParse(splitted[1], out qual);
                        if (!succ)
                        {
                            qual = 1;
                        }
                    }
                    Tello.setJpgQuality(qual);
                }
                else if (msg.ToLower().Equals("imgm?"))
                {
                    Console.WriteLine("Image Mode: " + Tello.getImageMode());
                }
                else if (msg.ToLower().StartsWith("imgm"))
                {
                    string[] splitted = msg.Split(' ');
                    int mode = 0;
                    if (splitted.Length >= 2)
                    {
                        bool succ = Int32.TryParse(splitted[1], out mode);
                        if (!succ)
                        {
                            mode = 0;
                        }
                    }
                    Tello.setPicVidMode(mode);
                }
                else if (msg.ToLower().Equals("maxheight"))
                {
                    string[] splitted = msg.Split(' ');
                    int amountCm = 20;
                    if (splitted.Length >= 2)
                    {
                        bool succ = Int32.TryParse(splitted[1], out amountCm);
                        if (!succ)
                        {
                            amountCm = 20;
                        }
                        else
                        {
                            if (amountCm < 20)
                            {
                                amountCm = 20;
                            }
                        }
                    }
                    Tello.setMaxHeight(amountCm);
                }
                else if (msg.ToLower().Equals("takeoff"))
                {
                    Tello.takeoffCmd();
                }
                else if (msg.ToLower().Equals("land"))
                {
                    Tello.land();
                }
                else if (msg.ToLower().StartsWith("up"))
                {
                    string[] splitted = msg.Split(' ');
                    int amountCm = 20;
                    if(splitted.Length >= 2)
                    {
                        bool succ = Int32.TryParse(splitted[1], out amountCm);
                        if (!succ)
                        {
                            amountCm = 20;
                        }
                        else
                        {
                            if(amountCm < 20)
                            {
                                amountCm = 20;
                            }
                        }
                    }
                    Tello.up(amountCm);
                }
                else if (msg.ToLower().StartsWith("down"))
                {
                    string[] splitted = msg.Split(' ');
                    int amountCm = 20;
                    if (splitted.Length >= 2)
                    {
                        bool succ = Int32.TryParse(splitted[1], out amountCm);
                        if (!succ)
                        {
                            amountCm = 20;
                        }
                        else
                        {
                            if (amountCm < 20)
                            {
                                amountCm = 20;
                            }
                        }
                    }
                    Tello.down(amountCm);
                }
                else if (msg.ToLower().StartsWith("left"))
                {
                    
                    string[] splitted = msg.Split(' ');
                    int amountCm = 20;
                    if (splitted.Length >= 2)
                    {
                        bool succ = Int32.TryParse(splitted[1], out amountCm);
                        if (!succ)
                        {
                            amountCm = 20;
                        }
                        else
                        {
                            if (amountCm < 20)
                            {
                                amountCm = 20;
                            }
                        }
                    }
                    Tello.left(amountCm);
                }
                else if (msg.ToLower().StartsWith("right"))
                {
                    string[] splitted = msg.Split(' ');
                    int amountCm = 20;
                    if (splitted.Length >= 2)
                    {
                        bool succ = Int32.TryParse(splitted[1], out amountCm);
                        if (!succ)
                        {
                            amountCm = 20;
                        }
                        else
                        {
                            if (amountCm < 20)
                            {
                                amountCm = 20;
                            }
                        }
                    }
                    Tello.right(amountCm);
                }
                else if (msg.ToLower().StartsWith("forward"))
                {
                    string[] splitted = msg.Split(' ');
                    int amountCm = 20;
                    if (splitted.Length >= 2)
                    {
                        bool succ = Int32.TryParse(splitted[1], out amountCm);
                        if (!succ)
                        {
                            amountCm = 20;
                        }
                        else
                        {
                            if (amountCm < 20)
                            {
                                amountCm = 20;
                            }
                        }
                    }
                    Tello.forward(amountCm);
                }
                else if (msg.ToLower().StartsWith("back"))
                {
                    string[] splitted = msg.Split(' ');
                    int amountCm = 20;
                    if (splitted.Length >= 2)
                    {
                        bool succ = Int32.TryParse(splitted[1], out amountCm);
                        if (!succ)
                        {
                            amountCm = 20;
                        }
                        else
                        {
                            if (amountCm < 20)
                            {
                                amountCm = 20;
                            }
                        }
                    }
                    Tello.back(amountCm);
                }
                else if (msg.ToLower().StartsWith("cw"))
                {
                    string[] splitted = msg.Split(' ');
                    int rotationDeg = 1;
                    if (splitted.Length >= 2)
                    {
                        bool succ = Int32.TryParse(splitted[1], out rotationDeg);
                        if (!succ)
                        {
                            rotationDeg = 1;
                        }
                        else
                        {
                            if (rotationDeg < 1)
                            {
                                rotationDeg = 1;
                            }
                        }
                    }
                    Tello.cw(rotationDeg);
                }
                else if (msg.ToLower().StartsWith("ccw"))
                {
                    string[] splitted = msg.Split(' ');
                    int rotationDeg = 1;
                    if (splitted.Length >= 2)
                    {
                        bool succ = Int32.TryParse(splitted[1], out rotationDeg);
                        if (!succ)
                        {
                            rotationDeg = 1;
                        }
                        else
                        {
                            if (rotationDeg < 1)
                            {
                                rotationDeg = 1;
                            }
                        }
                    }
                    Tello.ccw(rotationDeg);
                }
                else if (msg.ToLower().Equals("flip"))
                {
                    Console.WriteLine("Zu gefaehrlich ohne die kontrolle ueber die Drone... wird mal ignoriert");
                }
                else if (msg.ToLower().StartsWith("speed"))
                {
                    string[] splitted = msg.Split(' ');
                    int speedCmS = 1;
                    if (splitted.Length >= 2)
                    {
                        bool succ = Int32.TryParse(splitted[1], out speedCmS);
                        if (!succ)
                        {
                            speedCmS = 1;
                        }
                        else
                        {
                            if (speedCmS < 1)
                            {
                                speedCmS = 1;
                            }
                        }
                    }
                    Tello.speed(speedCmS);
                }
                else if (msg.ToLower().Equals("speed?"))
                {
                    Console.WriteLine("current Speed: " + Tello.getSpeed().ToString());
                }
                else if (msg.ToLower().Equals("battery?"))
                {
                    Console.WriteLine("Current Battery: " + Tello.getBattery().ToString() + " %");
                }
                else if (msg.ToLower().Equals("time?"))
                {
                    Console.WriteLine("Elapsed Flight Time: " + Tello.getFlightTime());
                }else if (msg.ToLower().Equals("tp"))
                {
                    Tello.takePicture();
                }
                if (msg.ToLower().Equals("exit"))
                {
                    break;
                }
            }
        }

        public void TestConfig()
        {
            Tello.TestMode = true;
            string config = "{\"max_height\":250,\"speed\":25,\"path\":[{\"x\":0,\"y\":100,\"z\":0,\"rotation\":0,\"take_image\":false,\"order_id\":1},{\"x\":-75,\"y\":100,\"z\":0,\"rotation\":90,\"take_image\":false,\"order_id\":2},{\"x\":-150,\"y\":100,\"z\":0,\"rotation\":90,\"take_image\":false,\"order_id\":3},{\"x\":-225,\"y\":100,\"z\":0,\"rotation\":90,\"take_image\":false,\"order_id\":4},{\"x\":-300,\"y\":100,\"z\":0,\"rotation\":0,\"take_image\":false,\"order_id\":5},{\"x\":-300,\"y\":100,\"z\":-100,\"rotation\":0,\"take_image\":false,\"order_id\":6},{\"x\":-250,\"y\":100,\"z\":-100,\"rotation\":90,\"take_image\":false,\"order_id\":7},{\"x\":-200,\"y\":100,\"z\":-100,\"rotation\":90,\"take_image\":false,\"order_id\":8},{\"x\":-150,\"y\":100,\"z\":-100,\"rotation\":90,\"take_image\":false,\"order_id\":9},{\"x\":-100,\"y\":100,\"z\":-100,\"rotation\":90,\"take_image\":false,\"order_id\":10},{\"x\":-50,\"y\":100,\"z\":-100,\"rotation\":0,\"take_image\":false,\"order_id\":11}]}";
            DroneConfiguration conf = JsonConvert.DeserializeObject<DroneConfiguration>(config);
            SetConfiguration(conf);
            ExecuteScript("test");
        }

        public void TestConfigOnline()
        {
            string config = "{\"max_height\":200,\"speed\":25,\"path\":[{\"x\":0,\"y\":100,\"z\":0,\"rotation\":0,\"take_image\":false,\"order_id\":1},{\"x\":100,\"y\":100,\"z\":0,\"rotation\":0,\"take_image\":true,\"order_id\":2},{\"x\":100,\"y\":100,\"z\":0,\"rotation\":90,\"take_image\":false,\"order_id\":3},{\"x\":100,\"y\":100,\"z\":100,\"rotation\":0,\"take_image\":false,\"order_id\":4}]}";
            DroneConfiguration conf = JsonConvert.DeserializeObject<DroneConfiguration>(config);
            SetConfiguration(conf);
            while (true)
            {
                if (Tello.connected)
                {
                    break;
                }
            }
            if (Tello.connected)
            {
                ExecuteScript("test");
            }
        }
    }
}
