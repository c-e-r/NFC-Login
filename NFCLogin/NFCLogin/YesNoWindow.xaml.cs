using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace NFCLogin
{
    /// <summary>
    /// Interaction logic for YesNoWindow.xaml
    /// </summary>
    public partial class YesNoWindow : Window, INotifyPropertyChanged
    {
        public String WindowMessage
        {
            get { return _windowMessageHolder; }
            set
            {
                _windowMessageHolder = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WindowMessage"));
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand YesAction { get; set; }
        public RelayCommand NoAction { get; set; }
        public RelayCommand PhoneReplacedCommand { get; set; }

        private string _windowMessageHolder;
        private string _windowMessage;
        private string _currentUsername;

        private DispatcherTimer _disconnectTimer;
        private NFCReader _reader;

        /**
         * <summary>
         * Creates and shows a YesNoWindow with "Yes" and "No" buttons bound to the given RelayCommands and sets the title and text of the window.
         * </summary>
         * <param name="yesAction">
         * The RelayCommand to bind to the "Yes" button. This RelayCommand is also called at the end of a DestroyAfterTimeout() call.
         * </param>
         * <param name="noAction">
         * The RelayCommand to bind to the "No" button.
         * </param>
         * <param name="phoneReplacedAction">
         * The RelayCommand to fire when the phone is replaced on the reader.
         * </param>
         * <param name="windowTitle">
         * The string that goes in the YesNoWindow's title bar.
         * </param>
         * <param name="message">
         * The string that goes in the YesNoWindow's body.
         * </param>
         * <param name="currentUsername">
         * The username of the curretnly logged in user.
         * </param>
         * <param name="reader">
         * A reference to the connected NFC reader.
         * </param>
         **/
        public YesNoWindow(RelayCommand yesAction, RelayCommand noAction, RelayCommand phoneReplacedCommand, String windowTitle, String message, String currentUsername, ref NFCReader reader)
        {
            InitializeComponent();

            Title = windowTitle;
            DataContext = this;

            // Remove minimize and maximize buttons
            ResizeMode = ResizeMode.NoResize;

            _windowMessage = message;
            _reader = reader;
            _currentUsername = currentUsername;

            _reader.CardDetected += PhoneReplaced;

            PhoneReplacedCommand = phoneReplacedCommand;

            YesAction = new RelayCommand(new Action(() =>
            {
                yesAction.Execute();
                CloseWindowAndDestroyTimer();
            }));

            NoAction = new RelayCommand(new Action(() =>
            {
                noAction.Execute();
                CloseWindowAndDestroyTimer();
            }));

            Show();
        }

        /**
         * <summary>
         * Executes the Yes RelayCommand and closes the window after the given timeout.
         * </summary>         
         */ 
        public void DestroyAfterTimeout(int timeoutSeconds)
        {
            WindowMessage = string.Format(_windowMessage, timeoutSeconds);

            _disconnectTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _disconnectTimer.Tick += (Object o, EventArgs e) =>
            {
                WindowMessage = string.Format(_windowMessage, --timeoutSeconds);

                if (timeoutSeconds == 0)
                {
                    _disconnectTimer.Stop();
                    YesAction.Execute();
                }
            };
            
            _disconnectTimer.Start();
        }

        /**
         * <summary>
         * Calls the phoneReplacedCommand and closes the window. 
         * An extension to the phoneReplacedCommand RelayCommand given in the constructor.
         * </summary>
         */     
        private void PhoneReplaced(Object source = null, ReaderEventArgs args = null)
        {
            if (UsernamesMatch())
            {
                PhoneReplacedCommand.Execute();
                CloseWindowAndDestroyTimer();

            }
        }

        /**
         * <summary>
         * Closes the window and destroys the countdown timer.
         * </summary>
         */
        private void CloseWindowAndDestroyTimer(Object source = null, ReaderEventArgs args = null)
        {
            _reader.CardDetected -= PhoneReplaced;

            _disconnectTimer.Stop();
            Application.Current.Dispatcher.Invoke(() => Close());
        }

        /**
         * <summary>
         * Checks if the username from the phone is the same as the currently logged in user.
         * </summary>
         */
        private bool UsernamesMatch()
        {
            try
            {
                String jsonWebToken = _reader.SelectAID(NFCReader.AID_LOGIN);
                return JWTUtils.GetUserNameFromJWT(jsonWebToken) == _currentUsername;
            }
            catch (CardNotFoundException e)
            {
                return false;
            }
            catch (NFCCommandFailedException e)
            {
                return false;
            }
        }
    }
}
