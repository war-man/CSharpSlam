﻿using remoteApiNETWrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace VrepSimpleTest
{
    class Control :iRobot
    {
        public MapBuilder MapBuilder { get;}
        public Localization Localization;

         int _clientID=-1;
         int _handleNeo;
         int _handleLeftMotor, _handleRightMotor;
         int  _handleSick;
         bool _connected = false;
        IntPtr _signalValuePtr;
        int _signalLength;

        public Control()
        {
            
            MapBuilder = new MapBuilder(this);
            Localization = new Localization(this);
            InitHandlers();

        }

        private void InitHandlers()
        {

            //VREPWrapper.simxGetObjectHandle(_clientID, "neobotix#0", out _handleNeo, simx_opmode.oneshot_wait);
            Debug.WriteLine("Handle neobotix#0: " + _handleNeo);
            VREPWrapper.simxGetObjectHandle(_clientID, "wheel_left#0", out _handleLeftMotor, simx_opmode.oneshot_wait);
            Debug.WriteLine("Handle left motor #0: " + _handleLeftMotor);
            VREPWrapper.simxGetObjectHandle(_clientID, "wheel_right#0", out _handleRightMotor, simx_opmode.oneshot_wait);
            Debug.WriteLine("Handle right motor #0: " + _handleRightMotor);
            VREPWrapper.simxGetObjectHandle(_clientID, "SICK_S300_fast#0", out _handleSick, simx_opmode.oneshot_wait);
        }

        public int  Connect() {
            if (!_connected) // If not connected - try to connect
            {
                try
                {
                    _clientID = VREPWrapper.simxStart("127.0.0.1", 19997, true, true, 5000, 5);
                }
                catch (DllNotFoundException ex)
                {
                    MessageBox.Show("remoteApi.dll missing");
                }

                if (_clientID != -1) // Successfully connected to V-REP
                {
                    Debug.WriteLine("Connected to V-REP");
                    _connected = true;
                    InitHandlers();
                    return 0;
                }
                else // Connection trial failed
                {
                    Debug.WriteLine("Error connecting to V-REP");
                    MessageBox.Show("Error connecting to V-REP :(");
                    _connected = false;
                    return -1;
                }
            }
            else // If connected - try to disconnect 
            {
                VREPWrapper.simxFinish(_clientID);
                _connected = false;
                Debug.WriteLine("Disconnected from V-REP");

                return -2;
            }

        }

        public  void Disconnect()
        {
            if (_connected)
                VREPWrapper.simxFinish(_clientID);

        }

        public  void ResetSimulation() {
            VREPWrapper.simxStopSimulation(_clientID, simx_opmode.oneshot_wait);
            Thread.Sleep(400);
            VREPWrapper.simxStartSimulation(_clientID, simx_opmode.oneshot_wait);

        }

        public double[] GetWheelSpeed()
        {
            throw new NotImplementedException();
            
        }

        public double[,] GetLaserScannerData(){

            
            int i;
            double[,] _laserScannerData;
            // reading the laser scanner stream 
            VREPWrapper.simxReadStringStream(_clientID, "measuredDataAtThisTime0", ref _signalValuePtr, ref _signalLength, simx_opmode.streaming);
            
          //  Debug.WriteLine(String.Format("test: {0:X8} {1:D} {2:X8}", _signalValuePtr, _signalLength, _signalValuePtr+_signalLength));
            float[] f = new float[685 * 3];
            if (_signalLength >= f.GetLength(0))
            {
                //we managed to get the laserdatas from Vrep

                _laserScannerData = new double[3, f.GetLength(0) / 3];

                // todo read the latest stream (this is not the latest)
                unsafe
                {
                    float* pp = (float*)(_signalValuePtr).ToPointer();
                    //Debug.WriteLine("pp: " + *pp);
                    for (i = 0; i < f.GetLength(0); i++)
                        f[i] = (float)*pp++; // pointer to float array 
                }
                i = 0;
                // reshaping the 1D [3*685] data to 2D [3, 685] > x, y, z coordinates
                for (i = 0; i < f.GetLength(0); i++)
                    if (!(Math.Abs((float)f[i]) < 0.000001))
                        _laserScannerData[i % 3, i / 3] = (float)f[i];

                return _laserScannerData;
            }
            else
            {
                // we couldnt get the laserdata, so we return an empty array
                _laserScannerData = new double[0, 0];
                return _laserScannerData;

            }

        }

        public void SetWheelSpeed(double R, double L)
        {
            VREPWrapper.simxSetJointTargetVelocity(_clientID, _handleLeftMotor, (float)R, simx_opmode.oneshot_wait);
            VREPWrapper.simxSetJointTargetVelocity(_clientID, _handleRightMotor, (float)L, simx_opmode.oneshot_wait);
        }

        public void SetWheelSpeed(double[] LinAng)
        {
            throw new NotImplementedException();
        }


    }
}
