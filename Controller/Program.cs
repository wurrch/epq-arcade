using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Controller
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int menuOption = 0;

            Console.WriteLine("Please select an option:");
            Console.WriteLine("1 - Only the Wheel as Analogue Input (All Pedals are on/off, Games that use buttons for moving)");
            Console.WriteLine("2 - Wheel, Accelerator and Break as Analogue Input (Clutch is on/off, Games that use triggers for moving)");
            Console.WriteLine("3 - Exit");

            try
            {
                menuOption = Convert.ToInt32(Console.ReadLine());
            }
            catch
            {
                Console.Clear();
                Console.WriteLine("Error: Invalid option");
                Main(null);
            }

            if (menuOption == 1)
            {
                Console.Clear();
                AnalogueWheel();
            }
            else if (menuOption == 2)
            {
                Console.Clear();
                AnalogueAll();
            }
            else if (menuOption == 3)
            {
                Environment.Exit(0);
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Error: Invalid option");
                Main(null);
            }
        }

        static void CalibrationSub(IXbox360Controller virtualX360)
        {
            while (true)
            {
                Console.SetCursorPosition(0, 12);
                if (Console.ReadKey().KeyChar == ' ')
                {
                    virtualX360.SetAxisValue(Xbox360Axis.LeftThumbX, 0);
                    Console.WriteLine("CALIBRATED!");
                }
            }
        }

        static void AnalogueWheel()
        {
            SerialPort arduinoRaw = new SerialPort();
            arduinoRaw.PortName = "COM3";
            arduinoRaw.BaudRate = 9600;
            try
            {
                arduinoRaw.Open();
            }
            catch
            {
                Console.WriteLine("Error opening arduino port, is another process using it?");
                Main(null);
            }

            var client = new ViGEmClient();
            var virtualX360 = client.CreateXbox360Controller();

            virtualX360.Connect();
            virtualX360.SetAxisValue(Xbox360Axis.LeftThumbX, 0);

            Thread calibrationThread = new Thread(() => CalibrationSub(virtualX360));
            calibrationThread.Start();

            string arduinoRawLine;
            string[] arduinoRawString = new string[9];
            int[] arduinoRawInt = new int[9];
            int errorCount = 0;
            short mappedWheelValue = 0;

            while (true)
            {
                try
                {
                    // 'Primary Button','Secondary Button','Joystick Button','Joystick X','Joystick Y','Wheel Angle','Clutch','Break','Accelerator'
                    arduinoRawLine = arduinoRaw.ReadLine();
                    arduinoRawString = arduinoRawLine.Split(',');
                    arduinoRawInt = Array.ConvertAll(arduinoRawString, s => int.Parse(s));

                    // Primary Button = X Button
                    if (arduinoRawInt[0] == 0)
                    {
                        virtualX360.SetButtonState(Xbox360Button.X, true);
                    }
                    else
                    {
                        virtualX360.SetButtonState(Xbox360Button.X, false);
                    }

                    // Secondary Button = Y Button
                    if (arduinoRawInt[1] == 0)
                    {
                        virtualX360.SetButtonState(Xbox360Button.Y, true);
                    }
                    else
                    {
                        virtualX360.SetButtonState(Xbox360Button.Y, false);
                    }

                    // Joystick Button = Left Bumper, Right Bumper and Left Thumbstick (TURNS ON LAN MODE)
                    if (arduinoRawInt[2] == 0)
                    {
                        virtualX360.SetButtonState(Xbox360Button.LeftShoulder, true);
                        virtualX360.SetButtonState(Xbox360Button.RightShoulder, true);
                        virtualX360.SetButtonState(Xbox360Button.LeftThumb, true);
                    }
                    else
                    {
                        virtualX360.SetButtonState(Xbox360Button.LeftShoulder, false);
                        virtualX360.SetButtonState(Xbox360Button.RightShoulder, false);
                        virtualX360.SetButtonState(Xbox360Button.LeftThumb, false);
                    }

                    // Joystick X high = DPad Right
                    if (arduinoRawInt[3] >= 800)
                    {
                        virtualX360.SetButtonState(Xbox360Button.Right, true);
                        virtualX360.SetButtonState(Xbox360Button.Left, false);
                    }
                    else if (arduinoRawInt[3] <= 300) // Joystick X low = DPad Left
                    {
                        virtualX360.SetButtonState(Xbox360Button.Left, true);
                        virtualX360.SetButtonState(Xbox360Button.Right, false);
                    }
                    else // Joystick X middle = Neither
                    {
                        virtualX360.SetButtonState(Xbox360Button.Left, false);
                        virtualX360.SetButtonState(Xbox360Button.Right, false);
                    }

                    // Joystick Y high = DPad Up
                    if (arduinoRawInt[4] >= 800)
                    {
                        virtualX360.SetButtonState(Xbox360Button.Up, true);
                        virtualX360.SetButtonState(Xbox360Button.Down, false);
                    }
                    else if (arduinoRawInt[4] <= 300) // Joystick Y low = DPad Down
                    {
                        virtualX360.SetButtonState(Xbox360Button.Down, true);
                        virtualX360.SetButtonState(Xbox360Button.Up, false);
                    }
                    else // Joystick Y middle = Neither
                    {
                        virtualX360.SetButtonState(Xbox360Button.Down, false);
                        virtualX360.SetButtonState(Xbox360Button.Up, false);
                    }

                    // Wheel = Left Thumbstick X Axis
                    mappedWheelValue = Convert.ToInt16(Map(arduinoRawInt[5], 0, 1023, -32768, 32767));
                    virtualX360.SetAxisValue(Xbox360Axis.LeftThumbX, mappedWheelValue);

                    // Clutch = Right Trigger    If the clutch value is greater than or equal to 800 then pull the right trigger
                    if (arduinoRawInt[6] >= 800)
                    {
                        virtualX360.SetSliderValue(Xbox360Slider.RightTrigger, 255);
                    }
                    else
                    {
                        virtualX360.SetSliderValue(Xbox360Slider.RightTrigger, 0);
                    }

                    // Break = B Button    If the break value is greater than or equal to 800 then press B
                    if (arduinoRawInt[7] >= 800)
                    {
                        virtualX360.SetButtonState(Xbox360Button.B, true);
                    }
                    else
                    {
                        virtualX360.SetButtonState(Xbox360Button.B, false);
                    }

                    // Accelerator = A Button    If the accelerator value is greater than or equal to 800 then press A
                    if (arduinoRawInt[8] >= 800)
                    {
                        virtualX360.SetButtonState(Xbox360Button.A, true);
                    }
                    else
                    {
                        virtualX360.SetButtonState(Xbox360Button.A, false);
                    }

                    // Debugging
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Error Count {0}      ", errorCount);
                    Console.WriteLine("Primary Button {0}      ", arduinoRawInt[0]);
                    Console.WriteLine("Secondary Button {0}      ", arduinoRawInt[1]);
                    Console.WriteLine("Joystick Button {0}      ", arduinoRawInt[2]);
                    Console.WriteLine("Joystick X {0}      ", arduinoRawInt[3]);
                    Console.WriteLine("Joystick Y {0}      ", arduinoRawInt[4]);
                    Console.WriteLine("Wheel Angle {0}      ", arduinoRawInt[5]);
                    Console.WriteLine("Clutch Angle {0}      ", arduinoRawInt[6]);
                    Console.WriteLine("Break Angle {0}      ", arduinoRawInt[7]);
                    Console.WriteLine("Accelerator Angle {0}      ", arduinoRawInt[8]);
                }
                catch
                {
                    errorCount++;
                }
            }
        }
        static void AnalogueAll()
        {
            SerialPort arduinoRaw = new SerialPort();
            arduinoRaw.PortName = "COM3";
            arduinoRaw.BaudRate = 9600;
            try
            {
                arduinoRaw.Open();
            }
            catch
            {
                Console.WriteLine("Error opening arduino port, is another process using it?");
                Main(null);
            }

            var client = new ViGEmClient();
            var virtualX360 = client.CreateXbox360Controller();

            virtualX360.Connect();
            virtualX360.SetAxisValue(Xbox360Axis.LeftThumbX, 0);

            Thread calibrationThread = new Thread(() => CalibrationSub(virtualX360));
            calibrationThread.Start();

            string arduinoRawLine;
            string[] arduinoRawString = new string[9];
            int[] arduinoRawInt = new int[9];
            int errorCount = 0;
            short mappedWheelValue = 0;
            byte mappedBreakValue = 0;
            byte mappedAcceleratorValue = 0;

            while (true)
            {
                try
                {
                    // 'Primary Button','Secondary Button','Joystick Button','Joystick X','Joystick Y','Wheel Angle','Clutch','Break','Accelerator'
                    arduinoRawLine = arduinoRaw.ReadLine();
                    arduinoRawString = arduinoRawLine.Split(',');
                    arduinoRawInt = Array.ConvertAll(arduinoRawString, s => int.Parse(s));

                    // Primary Button = A Button
                    if (arduinoRawInt[0] == 0)
                    {
                        virtualX360.SetButtonState(Xbox360Button.A, true);
                    }
                    else
                    {
                        virtualX360.SetButtonState(Xbox360Button.A, false);
                    }

                    // Secondary Button = X Button
                    if (arduinoRawInt[1] == 0)
                    {
                        virtualX360.SetButtonState(Xbox360Button.X, true);
                    }
                    else
                    {
                        virtualX360.SetButtonState(Xbox360Button.X, false);
                    }

                    // Joystick Button = Left Bumper, Right Bumper and Left Thumbstick (TURNS ON LAN MODE)
                    if (arduinoRawInt[2] == 0)
                    {
                        virtualX360.SetButtonState(Xbox360Button.LeftShoulder, true);
                        virtualX360.SetButtonState(Xbox360Button.RightShoulder, true);
                        virtualX360.SetButtonState(Xbox360Button.LeftThumb, true);
                    }
                    else
                    {
                        virtualX360.SetButtonState(Xbox360Button.LeftShoulder, false);
                        virtualX360.SetButtonState(Xbox360Button.RightShoulder, false);
                        virtualX360.SetButtonState(Xbox360Button.LeftThumb, false);
                    }

                    // Joystick X high = DPad Right
                    if (arduinoRawInt[3] >= 800)
                    {
                        virtualX360.SetButtonState(Xbox360Button.Right, true);
                        virtualX360.SetButtonState(Xbox360Button.Left, false);
                    }
                    else if (arduinoRawInt[3] <= 300) // Joystick X low = DPad Left
                    {
                        virtualX360.SetButtonState(Xbox360Button.Left, true);
                        virtualX360.SetButtonState(Xbox360Button.Right, false);
                    }
                    else // Joystick X middle = Neither
                    {
                        virtualX360.SetButtonState(Xbox360Button.Left, false);
                        virtualX360.SetButtonState(Xbox360Button.Right, false);
                    }

                    // Joystick Y high = DPad Up
                    if (arduinoRawInt[4] >= 800)
                    {
                        virtualX360.SetButtonState(Xbox360Button.Up, true);
                        virtualX360.SetButtonState(Xbox360Button.Down, false);
                    }
                    else if (arduinoRawInt[4] <= 300) // Joystick Y low = DPad Down
                    {
                        virtualX360.SetButtonState(Xbox360Button.Down, true);
                        virtualX360.SetButtonState(Xbox360Button.Up, false);
                    }
                    else // Joystick Y middle = Neither
                    {
                        virtualX360.SetButtonState(Xbox360Button.Down, false);
                        virtualX360.SetButtonState(Xbox360Button.Up, false);
                    }

                    // Wheel = Left Thumbstick X Axis
                    mappedWheelValue = Convert.ToInt16(Map(arduinoRawInt[5], 0, 1023, -32768, 32767));
                    virtualX360.SetAxisValue(Xbox360Axis.LeftThumbX, mappedWheelValue);

                    // Clutch = Left Bumper    If the clutch value is greater than or equal to 800 then pull the right bumper
                    if (arduinoRawInt[6] >= 800)
                    {
                        virtualX360.SetButtonState(Xbox360Button.LeftShoulder, true);
                    }
                    else
                    {
                        virtualX360.SetButtonState(Xbox360Button.LeftShoulder, false);
                    }

                    // Break = Left Trigger
                    mappedBreakValue = Convert.ToByte(Map(arduinoRawInt[7], 0, 1023, 0, 255));
                    virtualX360.SetSliderValue(Xbox360Slider.LeftTrigger, mappedBreakValue);

                    // Accelerator = Right Trigger
                    mappedAcceleratorValue = Convert.ToByte(Map(arduinoRawInt[8], 0, 1023, 0, 255));
                    virtualX360.SetSliderValue(Xbox360Slider.RightTrigger, mappedAcceleratorValue);

                    // Debugging
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Error Count {0}      ", errorCount);
                    Console.WriteLine("Primary Button {0}      ", arduinoRawInt[0]);
                    Console.WriteLine("Secondary Button {0}      ", arduinoRawInt[1]);
                    Console.WriteLine("Joystick Button {0}      ", arduinoRawInt[2]);
                    Console.WriteLine("Joystick X {0}      ", arduinoRawInt[3]);
                    Console.WriteLine("Joystick Y {0}      ", arduinoRawInt[4]);
                    Console.WriteLine("Wheel Angle {0}      ", arduinoRawInt[5]);
                    Console.WriteLine("Clutch Angle {0}      ", arduinoRawInt[6]);
                    Console.WriteLine("Break Angle {0}      ", arduinoRawInt[7]);
                    Console.WriteLine("Accelerator Angle {0}      ", arduinoRawInt[8]);
                }
                catch
                {
                    errorCount++;
                }
            }
        }
        public static int Map(int value, int in_min, int in_max, int out_min, int out_max)
        {
            return (value - in_min) * (out_max - out_min) / (in_max - in_min) + out_min; // Some maths to match the ranges of the wheel to the joystick, to make them align
        }
    }
}
