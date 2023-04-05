using System.Linq;
using System.IO.Ports;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace ArduinoControllerPC
{
    internal class Program
    {
        static SerialPort port;
        static ViGEmClient client;
        static IXbox360Controller controller;

        static void Main(string[] args)
        {
            port = new SerialPort("COM5", 115200);
            client = new ViGEmClient();

            controller = client.CreateXbox360Controller();
            controller.Connect();

            port.DataReceived += SerialPort_DataReceived;

            port.Open();
            Console.ReadLine();

            port.Close();
        }

        static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string data = sp.ReadLine();

            Console.WriteLine(data);
            string[] splitData = data.Split(',');
            int[] intData = Array.ConvertAll(splitData, s => int.Parse(s));

            if (intData[0] == 0)
            {
                controller.SetButtonState(Xbox360Button.A, true);
            }
            else
            {
                controller.SetButtonState(Xbox360Button.A, false);
            }

            if (intData[1] == 0)
            {
                controller.SetButtonState(Xbox360Button.B, true);
            }
            else
            {
                controller.SetButtonState(Xbox360Button.B, false);
            }

            controller.SetAxisValue(Xbox360Axis.LeftThumbX, (short)Map(intData[2], 920, 40, -32768, 32767));

            controller.SetSliderValue(Xbox360Slider.RightTrigger, (byte)Map(intData[3], 1000, 30, 0, 255));

            controller.SetSliderValue(Xbox360Slider.LeftTrigger, (byte)Map(intData[4], 1023, 65, 0, 255));
        }

        public static int Map(int value, int in_min, int in_max, int out_min, int out_max)
        {
            return (value - in_min) * (out_max - out_min) / (in_max - in_min) + out_min; // Some maths to match the ranges of the wheel to the joystick, to make them align
        }
    }
}