using DesktopWPFAppLowLevelKeyboardHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using static WpfApplication6.rawStructs;
using static WpfApplication6.sendStructs;
using System.Threading;
//using rawStructs;
namespace WpfApplication6
{
 
    public partial class MainWindow : Window
    {
        public string switchKey = "J"; 
        private bool inputMouse = true;
        private const int RIDEV_INPUTSINK = 0x00000100;
        public string swKey;
        private LowLevelKeyboardListener _listener;
        public MainWindow()
        {
            InitializeComponent();
            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];
        }
        //public float multilplierX = float.Parse(this.xMultiplier.Text);
        //public float mulitplierY;

        public bool builderMode = false;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];
            IntPtr windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            rid[0].usUsagePage = 0x01;
            rid[0].usUsage = 0x02;
            rid[0].dwFlags = RIDEV_INPUTSINK;
            rid[0].ThwndTarget = windowHandle;

            RegisterRawInputDevices(rid, (int)rid.Length, (int)Marshal.SizeOf(rid[0]));
            source.AddHook(WndProc);
        }
        public const int WM_INPUT = 0x00FF;
        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        private void HandleInput(IntPtr hDevice, out int x, out int y)
        {
           
            RAWINPUT input = new RAWINPUT();
            int outSize = 0;
            int size = Marshal.SizeOf(typeof(RAWINPUT));

            outSize = GetRawInputData(hDevice, RawInputCommand.Input, out input, ref size, Marshal.SizeOf(typeof(RAWINPUTHEADER)));

            x = input.Data.Mouse.lLastX;
            y = input.Data.Mouse.lLastY;


        }
        public int xParam, yParam;
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //int x, y;
            //HandleInput(lParam, out x, out y);
            //this.textBox_DisplayKeyboardInput.Text += x.ToString();
            
            if (msg == 0x00FF && inputMouse == true && builderMode == true)
            {
                
                
                HandleInput(lParam, out xParam, out yParam);
                Input[] inputs = new Input[]
                {
                    new Input
                    {
                        type = (int) InputTypes.Mouse,
                        u = new InputUnion
                        {
                            mi = new MouseInput
                            {
                               

                                dx = (int)Math.Round(xParam*float.Parse(this.xMultiplier.Text)),
                                dy = (int)Math.Round(yParam*float.Parse(this.yMultiplier.Text)),
                                
                                
                                dwFlags = (uint)(MouseEventF.Move),
                                dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    }
                };
                
                SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
                inputMouse = false;

            }
            else if (msg == 0x00FF && inputMouse == false)
            {
               
                
                inputMouse = true;
                
            }
            
            return IntPtr.Zero;
        }
        
        public bool editButton = false;

        // tester alo bre

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
             
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += _listener_OnKeyPressed;
            _listener.OnMousePressed += _listener_OnMousePressed;
            _listener.HookKeyboard();
            _listener.HookMouse();
        }

        [DllImport("User32.dll")]
        static extern Boolean SystemParametersInfo(
            UInt32 uiAction,
            UInt32 uiParam,
            UInt32 pvParam,
            UInt32 fWinIni);

        public const UInt32 SPI_SETMOUSESPEED = 0x0071;
        public inputController inpController = new inputController();

        private void inputControler(inputClass e) { 
        
            
        
        }
        public bool bindKey;
        private void _listener_OnMousePressed(object sender, MousePressedArgs e)
        {
            if (bindKey == true)
            {
                
                this.textBox_displayBind.IsReadOnly = true;
                this.textBox_displayBind.Text = e.MousePressed.ToString();
                

            }
            if (e.MousePressed.ToString() == switchKey && builderMode == false)
            {

                builderMode = true;

            }
            else if (e.MousePressed.ToString() == switchKey && builderMode == true)
            {

                builderMode = false;



            }



            //this.textBox_DisplayKeyboardInput.Text += e.MousePressed.ToString();
        }



        
        
        void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            
            swKey = e.KeyPressed.ToString();
            if (bindKey == true) {
                
                this.textBox_displayBind.IsReadOnly = true;
                this.textBox_displayBind.Text = e.KeyPressed.ToString();



            }
            if (e.KeyPressed.ToString() == switchKey && builderMode == false)
            {

                builderMode = true;

            }
            else if (e.KeyPressed.ToString() == switchKey && builderMode == true)
            {

                builderMode = false;



            }
            /*
            if (e.KeyPressed.ToString() == "J" && builderMode == false)
            {

                builderMode = true;
                
            }
            else if(e.KeyPressed.ToString() == "J" && builderMode == true)
            {

                builderMode = false;
                


            }
            */



            //this.textBox_DisplayKeyboardInput.Text += e.KeyPressed.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int codes = 0x0002;
            
            KeyPressedArgs buttoner = new KeyPressedArgs(KeyInterop.KeyFromVirtualKey(codes));
           
            _listener.UnHookMouse();
            _listener.UnHookKeyboard();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switchKey = textBox_switchKey.Text.ToString();
        }
        private void TextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox source = e.Source as TextBox;
            
            if (source != null)
            {
                // Change the TextBox color when it obtains focus.
                source.Background = Brushes.LightBlue;

                // Clear the TextBox.
                bindKey = true;
                
            }
        }
        
        private void TextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox source = e.Source as TextBox;

            if (source != null)
            {
                source.Background = Brushes.White;
                bindKey = false;

            }


            
        }

    }
}
