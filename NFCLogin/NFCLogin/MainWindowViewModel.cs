using System;
using System.ComponentModel;
using System.Windows;

namespace NFCLogin
{
    /**
     * <summary>
     * The ViewModel for the MainWindow class. Handles all the data binding and event listening. Sets up NFCReader and attaches it's events to methods in this class.
     * </summary>
     */
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Properties
        public String Header
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                OnPropertyChanged("Header");
            }
        }

        public String LoginUsername
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("LoginUsername");
            }
        }

        public String LoginPassword
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("LoginPassword");
            }
        }

        public String PatientId
        {
            get { return _patientId; }
            set
            {
                _patientId = value;
                OnPropertyChanged("PatientId");
            }
        }

        public String PhoneDetectedText {
            get { return _phoneDetectedText; }
            set
            {
                _phoneDetectedText = value;
                OnPropertyChanged("PhoneDetectedText");
            }
        }

        public Visibility LoginContainerVisibility
        {
            get { return _loginContainerVisibility; }
            set
            {
                _loginContainerVisibility = value;
                OnPropertyChanged("LoginContainerVisibility");
            }
        }

        public Visibility MainPageVisibility
        {
            get { return _mainPageVisibility; }
            set
            {
                _mainPageVisibility = value;
                OnPropertyChanged("MainPageVisibility");
            }
        }
        public Visibility PhonePresent
        {
            get { return _phonePresent; }
            set
            {
                _phonePresent = value;
                OnPropertyChanged("PhonePresent");
            }
        }
        public Visibility PhoneAbsent
        {
            get { return _phoneAbsent; }
            set
            {
                _phoneAbsent = value;
                OnPropertyChanged("PhoneAbsent");
            }
        }
        #endregion       

        #region Constants
        private const int DISCONNECT_TIMER_SECONDS = 5;

        private const String READER_NOT_FOUND_MESSAGE = "Application will continue without tap or card functionality.\n\nReader will attempt reconnection while user is not logged in.";
        private const String READER_NOT_FOUND_MESSAGE_TITLE = "ERROR: Reader not found";

        private const String CARD_NOT_FOUND_MESSAGE = "Card could not be read. Please try again.";
        private const String CARD_NOT_FOUND_MESSAGE_TITLE = "ERROR: Card not found";

        private const String LOGIN_FAILED_MESSAGE = "Please try logging in on your android device and try again.";
        private const String LOGIN_FAILED_MESSAGE_TITLE = "ERROR: Authentication Failed";

        private const String NFC_DISCONNECT_MESSAGE = "System will log out in {0} seconds";
        private const String NFC_DISCONNECT_MESSAGE_TITLE = "Are you sure you want to log out?";
        #endregion

        #region Instance Variables
        private String _headerText;
        private String _username;
        private String _password;        
        private String _mainPageHeaderPrefix;
        private String _readerAid;
        private String _patientId;
        private String _phoneDetectedText;
        private String _patientIdNumber;        
        private Boolean _isLoggedIn;
        private Visibility _phonePresent = Visibility.Collapsed;
        private Visibility _phoneAbsent = Visibility.Visible;

        private Visibility _loginContainerVisibility;
        private Visibility _mainPageVisibility;

        private NFCReader _reader;

        public RelayCommand SignIn { get; set; }
        public RelayCommand SignOut { get; set; }
        public RelayCommand LaunchWebPage { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        /**
         * <summary>
         * Constructs a MainWindowViewModel. Sets properties that UI elements are bound to. Binds methods in this class to NFCReader events.
         * </summary>
         */ 
        public MainWindowViewModel()
        {
            SetUiBoundedVariables();
            SetupNfcReader();
        }

        /**
         * <summary>
         * Event function that creates and displays a YesNoWindow that logs the user out after
         * a timeout. User can stay logged in by clicking "No" on the window or by placing their
         * phone back on the reader.
         * </summary>
         */
        public void SignOutThroughNfc(Object source, ReaderEventArgs args)
        {
            _reader.HeartbeatChanged -= PatientChanged;
            _reader.CardRemoved -= SignOutThroughNfc;

            RelayCommand SignOutCommand = new RelayCommand(new Action(() => 
            {
                SignOutFromSystem();
                _reader.HeartbeatChanged += PatientChanged;

            }));

            RelayCommand DoNothingCommand = new RelayCommand(new Action(() => 
            {
                _reader.Close();
                _reader.HeartbeatChanged += PatientChanged;

            }));

            RelayCommand PhoneReplacedCommand = new RelayCommand(new Action(() =>
            {
                _reader.CardRemoved += SignOutThroughNfc;
                _reader.HeartbeatChanged += PatientChanged;
            }));

            if (_isLoggedIn)
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    YesNoWindow window = new YesNoWindow(
                        SignOutCommand,
                        DoNothingCommand,
                        PhoneReplacedCommand,
                        NFC_DISCONNECT_MESSAGE_TITLE,
                        NFC_DISCONNECT_MESSAGE,
                        LoginUsername,
                        ref _reader
                    );

                    window.DestroyAfterTimeout(DISCONNECT_TIMER_SECONDS);
                }));
            }
        }

        /**
         * <summary>
         * Event function that signs the user in via NFC.
         * </summary>
         */
        public void SignInThroughNfc(Object source, ReaderEventArgs args)
        {
            NFCReader reader = (NFCReader) source;

            try
            {
                String jsonWebToken = reader.SelectAID(_readerAid);
                LoginUsername = JWTUtils.GetUserNameFromJWT(jsonWebToken);
                SetPatientId(reader.SelectAID(NFCReader.AID_HEARTBEAT));
                SignInThroughSystem(LoginUsername, null);
            }
            catch (CardNotFoundException e)
            {
                MessageBox.Show(CARD_NOT_FOUND_MESSAGE, CARD_NOT_FOUND_MESSAGE_TITLE);
            }
            catch (NFCCommandFailedException e)
            {
                MessageBox.Show(LOGIN_FAILED_MESSAGE, LOGIN_FAILED_MESSAGE_TITLE);
            }
        }

        /**
         * <summary>
         * Invokes the property changed event on the specified property so that their data-bounded
         * GUI elements can update their data.
         * </summary>
         * <param name="name">
         * The name of the changed property
         * </param>
         */
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region Private Functions

        /**
         * <summary>
         * Sets the values of variables that are bound to UI elements like strings and RelayCommands.
         * </summary>
         */
        private void SetUiBoundedVariables()
        {
            SignIn = new RelayCommand(new Action(SignInManual));
            SignOut = new RelayCommand(new Action(SignOutManual));
            LaunchWebPage = new RelayCommand(new Action(LaunchPatientWebPage));

            MainPageVisibility = Visibility.Collapsed;
            LoginContainerVisibility = Visibility.Visible;

            PhoneDetectedText = "Phone Not Detected";
            PatientId = "No patient selected";

            _mainPageHeaderPrefix = "Welcome, ";
        }

        /**
         * <summary>
         * Launches a webpage that displays patient data.
         * </summary>
         */
        private void LaunchPatientWebPage()
        {
            if (_patientIdNumber != null)
            {
                System.Diagnostics.Process.Start("https://ceroberts.com/patient.html?patient=" + _patientIdNumber);
            }
            else
            {
                MessageBox.Show("Please select a patient and try again", "No Patient Selected");
            }
        }

        /**
         * <summary>
         * Binds events and attempts connection to the NFC Reader.
         * </summary>
         */
        private void SetupNfcReader()
        {
            _readerAid = NFCReader.AID_LOGIN;
            _reader = new FongwahReader();

            _reader.CardDetected += SignInThroughNfc;
            _reader.HeartbeatChanged += PatientChanged;
            _reader.CardDetected += ShowPhoneIcon;
            _reader.CardRemoved += HidePhoneIcon;

            try
            {
                _reader.Connect();
            }
            catch (ReaderAlreadyConnectedException e)
            {
                // Do nothing
            }
            catch (ReaderNotFoundException e)
            {
                MessageBox.Show(READER_NOT_FOUND_MESSAGE, READER_NOT_FOUND_MESSAGE_TITLE);
            }
        }

        /**
         * <summary>
         * Manually signs the user out and reconnects the NFC Reader.
         * </summary>
         */
        private void SignOutManual()
        {
            try
            {
                _reader.Connect();
            }            
            catch (ReaderAlreadyConnectedException e)
            {
                // Do nothing
            }

            SignOutFromSystem();
        }

        /**
         * <summary>
         * Signs the user out and takes the user back to the main page.
         * </summary>
         */
        private void SignOutFromSystem()
        {
            _reader.CardDetected += SignInThroughNfc;
            _reader.CardRemoved -= SignOutThroughNfc;

            if (_isLoggedIn)
            {
				GoToMainPage();
            }
        }

        /**
         * <summary>
         * Manually signs the user in and closes the NFC Reader.
         * </summary>
         */
        private void SignInManual()
        {
            _reader.Close();
            SignInThroughForm();
        }

        /**
         * <summary>
         * Signs the user in and takes them to the login page.
         * </summary>
         * <param name="login">
         * THe login username.
         * </param>
         * <param name="password">
         * The user's password
         * </param>
         */
        private void SignInThroughSystem(String login, String password)
        {
            _reader.CardDetected -= SignInThroughNfc;
            _reader.CardRemoved += SignOutThroughNfc;

            if (!_isLoggedIn)
            {
                if (login == "" || login == null)
                {
                    LoginUsername = "User";
                }

                if (CredentialsAreCorrect())
                {
					GoToLoginPage();
                }
            }
        }

        /**
         * <summary>
         * Signs the user in using credentials from the GUI form.
         * </summary>
         */
        private void SignInThroughForm()
        {
            SignInThroughSystem(LoginUsername, LoginPassword);
        }

        /**
         * <summary>
         * Takes the user to the login page.
         * </summary>
         */
        private void GoToLoginPage()
        {
            Header = _mainPageHeaderPrefix + _username;
            LoginContainerVisibility = Visibility.Collapsed;
            MainPageVisibility = Visibility.Visible;
            _isLoggedIn = true;
        }

        /**
         * <summary>
         * Takes the user to the main page.
         * </summary>
         */
        private void GoToMainPage()
        {
            LoginUsername = "";
            LoginPassword = "";
            PatientId = "No patient selected";

            LoginContainerVisibility = Visibility.Visible;
            MainPageVisibility = Visibility.Collapsed;

            _isLoggedIn = false;
        }

        /**
         * <summary>
         * Validates the user's credentials. Currently always returns true.
         * </summary>
         */
        private Boolean CredentialsAreCorrect()
        {
            //TODO: Validate user's credentials

            return true;
        }

        /**
         * <summary>
         * Event function that sets the patient ID if the user selects a different one on their phone.
         * </summary>
         */
        public void PatientChanged(Object source, ReaderEventArgs args)
        {
            if (_isLoggedIn)
            {
                _patientIdNumber = args.Message;

                SetPatientId(args.Message);
            }
        }

        /**
         * <summary>
         * Sets the patient ID based on the data received from the NFC Reader.
         * </summary>
         * <param name="patientId">
         * The patient ID from the NFC Reader
         * </param>
         */
        private void SetPatientId(String patientId)
        {

            if (patientId == null)
            {
                PatientId = "No patient selected";
            }
            else
            {
                string name = "";

                switch ((Patients)Int32.Parse(patientId))
                {
                    case Patients.Ben:
                        name = "Ben";
                        break;

                    case Patients.Simon:
                        name = "Simon";
                        break;

                    case Patients.Shoban:
                        name = "Shoban";
                        break;

                    case Patients.Cameron:
                        name = "Cameron";
                        break;

                    default:
                        name = "Unknown";
                        break;

                };

                PatientId = "Patient " + name + " selected";
            }
        }

        /**
         * <summary>
         * Event function that shows the phone icon when the phone is placed on the NFC Reader.
         * </summary>
         */
        private void ShowPhoneIcon(Object source, ReaderEventArgs args)
        {
            PhoneDetectedText = "Phone Detected";
            PhoneAbsent = Visibility.Collapsed;
            PhonePresent = Visibility.Visible;

        }

        /**
         * <summary>
         * Event function that hides th ephone icon when the phone is removed from the NFC Reader.
         * </summary>
         */
        private void HidePhoneIcon(Object source, ReaderEventArgs args)
        {
            PhoneDetectedText = "Phone Not Detected";
            PhonePresent = Visibility.Collapsed;
            PhoneAbsent = Visibility.Visible;
        }

        #endregion

        /**
         * <summary>
         * An enumeration mapping patient names to IDs.
         * </summary>
         */
        private enum Patients { Ben = 1, Simon, Shoban, Cameron, Phat, Jacky, Ian, Keishi  };  
    }
}
