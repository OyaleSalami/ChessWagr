using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using Firebase.Storage;
using Photon.Pun;
using JetBrains.Annotations;
using DeadMosquito.AndroidGoodies;
using System.Globalization;
using UnityEngine.SceneManagement;
using UnityEngine.Android;
using System.Linq;
using UnityEngine.Networking;

public class Motherboard : MonoBehaviour
{
    public static Motherboard Instance { get { return instance; } }
    public static Motherboard instance;

    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    public DatabaseReference DBreference, DBreferenceFriend, DBreferenceGlobalChat, DBreferenceInvitematchs;
    private DatabaseReference dbReference;

    bool inaccount = false;

    public GameObject Loading, SignInScreen, CreateAccountPage, LoginAccountPage, Adminpanel, LoadingScreen;
    [Space]
    public TMP_InputField signinEmail, signPassword, emailInputField; //Login inputfield 
    [Space]
    public TMP_InputField CreateAccountEmail, CreateFullName, CreateAccountUserName, CreateAccountPass; // Create Input;
    public TMP_InputField UserNameInput;

    [Space]
    public GameObject NotifyUser;
    public Transform SpawnPoint;

    public TMP_Text UserName, Balance, BalanceMatch, Status;
    public float balance;
    public string Username;
    public string UserId;

    public GameObject NetworkCheckerOBJ;
    public Button RetryButton;
    public TMP_Text Textinfo;

    public float WageCost = 0;
    public TMP_Text Wagecost;
    public Button StartButton;

    #region NETWORK & InitializeFirebase
    void CheckInternetConnection()
    {
        //if (Application.internetReachability == NetworkReachability.NotReachable)
        //{
        //    Status.text = "Status: <size=35><#FF0000>Offline";
        //
        //    NetworkCheckerOBJ.SetActive(true);
        //    RetryButton.interactable = true;
        //    Textinfo.text = "<#FF5151>Please check your cellular or Wi-Fi connection and retry.";
        //    // Handle the case when there's no internet connection
        //}
        //else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        //{
        //    Status.text = "Status: <size=35><#53FF7C>Online";
        //
        //    NetworkCheckerOBJ.SetActive(false);
        //    // Handle the case when connected via mobile data
        //}
        //else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        //{
        //    Status.text = "Status: <size=35><#53FF7C>Online";
        //
        //    NetworkCheckerOBJ.SetActive(false);
        //    // Handle the case when connected via Wi-Fi
        //}
    }
    //void InitializeFirebase()
    //{
    //    try
    //    {
    //        // Initialize Firebase Authentication instance
    //        auth = FirebaseAuth.DefaultInstance;
    //
    //        if (auth != null)
    //        {
    //            // Attach state change listener for FirebaseAuth
    //            auth.StateChanged += AuthStateChanged;
    //            AuthStateChanged(this, null); // Trigger the state change initially
    //        }
    //        else
    //        {
    //            Debug.LogError("Firebase Authentication initialization failed.");
    //        }
    //
    //        // Initialize Firebase Database root reference
    //        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    //        if (DBreference != null)
    //        {
    //            Debug.Log("Firebase Database initialized.");
    //        }
    //        else
    //        {
    //            Debug.LogError("Firebase Database initialization failed.");
    //        }
    //
    //        // Initialize Firebase Storage
    //        storage = FirebaseStorage.DefaultInstance;
    //        if (storage != null)
    //        {
    //            Debug.Log("Firebase Storage initialized.");
    //        }
    //        else
    //        {
    //            Debug.LogError("Firebase Storage initialization failed.");
    //        }
    //
    //        // Attach a listener for the last few messages in Global Chat (limit data to avoid overloading)
    //        DBreferenceGlobalChat = FirebaseDatabase.DefaultInstance.GetReference("GLOBALCHAT");
    //        DBreferenceGlobalChat.LimitToLast(50).ValueChanged += HandleGlobalChatValueChanged;
    //
    //        Debug.Log("Attached listener for Global Chat (limited to last 50 messages).");
    //
    //        // Attach a listener for match invites (Consider limiting this based on app requirements)
    //        DBreferenceInvitematchs = FirebaseDatabase.DefaultInstance.GetReference("MATCHINVITES");
    //        DBreferenceInvitematchs.ValueChanged += HandleInviteValueChanged;
    //
    //        Debug.Log("Attached listener for match invites.");
    //
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Debug.LogError("Error during Firebase initialization: " + ex.Message);
    //    }
    //}

    // Optional: Create a method to load match invites on demand instead of real-time listening
    public async void LoadMatchInvites()
    {
        try
        {
            DataSnapshot snapshot = await DBreferenceInvitematchs.GetValueAsync();
            //HandleInviteData(snapshot);
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading match invites: " + e.Message);
        }
    }

    //Normal Check Start Here/////////////////////////////////////////////////////////////////////////////////////////////////Normal//////////////////////////////////////////////////////////////////////Normal Check Start Here//
    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }
    void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                //Debug.Log("Signed in " + user.UserId);
            }
        }
    }
    void InitializeFirebase()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                auth = FirebaseAuth.DefaultInstance;
                auth.StateChanged += AuthStateChanged;
                DBreference = FirebaseDatabase.DefaultInstance.RootReference;
                storage = FirebaseStorage.DefaultInstance;
                //storageReference = storage.GetReferenceFromUrl("gs://ludosagaprice.appspot.com");
                AuthStateChanged(this, null);
                //Loading.SetActive(false);
                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
    //Normal Check End Here/////////////////////////////////////////////////////////////////////////////////////////////////Normal//////////////////////////////////////////////////////////////////////Normal Check End Here//


    //Custom Check Start Here/////////////////////////////////////////////////////////////////////////////////////////////////Custom//////////////////////////////////////////////////////////////////////Custom Check Start Here//

    private void InitializeCUSTOMFirebases()
    {
        // Listen for changes at the user's database path
        DBreference.Child("users").Child(user.UserId).ValueChanged += OnValueChanged;
        StartCoroutine(ValidateGlobalChat());
    }
    IEnumerator ValidateGlobalChat()
    {
        // Retrieve the "CustomerChat" value from the user's data in Firebase
        Task<DataSnapshot> getTask = DBreference.Child("GLOBALCHAT").GetValueAsync();
        yield return new WaitUntil(() => getTask.IsCompleted);

        if (getTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve data from the database: {getTask.Exception}");
            yield break; // Exit the coroutine if there is an error
        }

        // Check if the data exists in the database
        if (getTask.Result.Exists)
        {
            // Retrieve the "CustomerChat" value
            string newMessage = getTask.Result.Value.ToString();
            Debug.Log($"CustomerChat value: {newMessage}");

            DBreference.Child("GLOBALCHAT").ValueChanged -= OnMessageValueChanged;
            DBreference.Child("GLOBALCHAT").ValueChanged += OnMessageValueChanged;
        }
        else
        {
            Debug.LogWarning("CustomerChat does not exist in the database.");
        }
    }


    public GameObject notification,Nicon;
    private void InitializeNOTIFICATIONirebases()
    {
        // Listen for changes at the user's database path
        DBreference.Child("users").Child(user.UserId).Child("FriendRequestList").ValueChanged += OnValueChanged;
        DBreference.Child("users").Child(user.UserId).Child("TournamentRewards").ValueChanged += OnValueChanged;
        DBreference.Child("users").Child(user.UserId).Child("Notifications").ValueChanged += OnValueChanged;
        StartCoroutine(ValidateNOTIFICATION());
    }
    IEnumerator ValidateNOTIFICATION()
    {
        // Retrieve the "CustomerChat" value from the user's data in Firebase
        Task<DataSnapshot> getTask = DBreference.Child("users").Child(user.UserId).Child("FriendRequestList").GetValueAsync();
        Task<DataSnapshot> getTask1 = DBreference.Child("users").Child(user.UserId).Child("TournamentRewards").GetValueAsync();
        Task<DataSnapshot> getTask2 = DBreference.Child("users").Child(user.UserId).Child("Notifications").GetValueAsync();

        yield return new WaitUntil(() => getTask.IsCompleted);
        yield return new WaitUntil(() => getTask1.IsCompleted);
        yield return new WaitUntil(() => getTask2.IsCompleted);

        if (getTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve data from the database: {getTask.Exception}");
            yield break; // Exit the coroutine if there is an error
        }

        // Check if the data exists in the database
        if (getTask.Result.Exists)
        {
            // Retrieve the "CustomerChat" value
            string newMessage = getTask.Result.Value.ToString();
            Debug.Log($"CustomerChat value: {newMessage}");

            DBreference.Child("users").Child(user.UserId).Child("FriendRequestList").ValueChanged -= OnNotificationValueChanged;
            DBreference.Child("users").Child(user.UserId).Child("FriendRequestList").ValueChanged += OnNotificationValueChanged;
        }
        else
        {
            Debug.LogWarning("CustomerChat does not exist in the database.");
        }
        // Check if the data exists in the database
        if (getTask1.Result.Exists)
        {
            // Retrieve the "CustomerChat" value
            string newMessage = getTask.Result.Value.ToString();
            Debug.Log($"CustomerChat value: {newMessage}");

            DBreference.Child("users").Child(user.UserId).Child("TournamentRewards").ValueChanged -= OnNotificationValueChanged;
            DBreference.Child("users").Child(user.UserId).Child("TournamentRewards").ValueChanged += OnNotificationValueChanged;
        }
        else
        {
            Debug.LogWarning("CustomerChat does not exist in the database.");
        }
        // Check if the data exists in the database
        if (getTask2.Result.Exists)
        {
            // Retrieve the "CustomerChat" value
            string newMessage = getTask.Result.Value.ToString();
            Debug.Log($"CustomerChat value: {newMessage}");

            DBreference.Child("users").Child(user.UserId).Child("Notifications").ValueChanged -= OnNotificationValueChanged;
            DBreference.Child("users").Child(user.UserId).Child("Notifications").ValueChanged += OnNotificationValueChanged;
        }
        else
        {
            Debug.LogWarning("CustomerChat does not exist in the database.");
        }
    }

    private void InitializeBattleInviteirebases()
    {
        // Listen for changes at the user's database path
        DBreference.Child("users").Child(user.UserId).Child("InviteToGame").ValueChanged += OnValueBattleChanged;

        StartCoroutine(BattleInvites());
    }
    IEnumerator BattleInvites()
    {
        // Retrieve the "CustomerChat" value from the user's data in Firebase
        Task<DataSnapshot> getTask = DBreference.Child("users").Child(user.UserId).Child("FriendRequestList").GetValueAsync();
        

        yield return new WaitUntil(() => getTask.IsCompleted);
        

        if (getTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve data from the database: {getTask.Exception}");
            yield break; // Exit the coroutine if there is an error
        }

        // Check if the data exists in the database
        if (getTask.Result.Exists)
        {
            // Retrieve the "CustomerChat" value
            string newMessage = getTask.Result.Value.ToString();
            Debug.Log($"CustomerChat value: {newMessage}");

            DBreference.Child("users").Child(user.UserId).Child("InviteToGame").ValueChanged -= OnValueBattleChanged;
            DBreference.Child("users").Child(user.UserId).Child("InviteToGame").ValueChanged += OnValueBattleChanged;
        }
        else
        {
            Debug.LogWarning("CustomerChat does not exist in the database.");
        }
        
    }

    private void OnValueBattleChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Error accessing database: " + args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot.Exists && args.Snapshot.Value != null)
        {
            Debug.Log("Database value updated: " + args.Snapshot.Value.ToString());
            // Execute additional methods
            StartCoroutine(BattleInvite());
        }
        else
        {
            Debug.Log("No value found at the specified database path.");
        }
    }
    private void OnNotificationValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Error accessing database: " + args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot.Exists && args.Snapshot.Value != null)
        {
            Debug.Log("Database value updated: " + args.Snapshot.Value.ToString());
            // Execute additional methods

            if (notification.activeInHierarchy == true)
            {
                // Clear the current player list
                foreach (Transform child in loadPoint)
                {
                    Destroy(child.gameObject);
                }

                Nicon.SetActive(false);
                LoadNotifyer();
            }
            else if (notification.activeInHierarchy == false)
            {
                Nicon.SetActive(true);

            }
        }
        else
        {
            Debug.Log("No value found at the specified database path.");
        }
    }
    private void OnMessageValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Error accessing database: " + args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot.Exists && args.Snapshot.Value != null)
        {
            Debug.Log("Database value updated: " + args.Snapshot.Value.ToString());
            // Execute additional methods

            StartCoroutine(LoadRecentGlobalChatMessage());

            // Get the current Y position of the ScrollView
            //float posY = ScrollView.anchoredPosition.y;

            if(GlobalInputField.activeInHierarchy == true)
            {
                // Check if the Y position is -1100
                //if (ScrollView.anchoredPosition.y > -80)
                //{
                //    // Display "hello in Unity" when the value changes
                //    LoadGlobalmessage();
                //    newmessageNotify.SetActive(false);
                //}
                //else if (ScrollView.anchoredPosition.y < -350)
                //{
                //    newmessageNotify.SetActive(true);
                //}
                
                LoadGlobalmessage();
                
            }
            else
            {

            }

        }
        else
        {
            Debug.Log("No value found at the specified database path.");
        }
    }
    private void OnValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Error accessing database: " + args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot.Exists && args.Snapshot.Value != null)
        {
            Debug.Log("Database value updated: " + args.Snapshot.Value.ToString());
            // Execute additional methods
        }
        else
        {
            Debug.Log("No value found at the specified database path.");
        }
    }
    private void OnDestroym()
    {
        if (DBreference != null)
        {
            DBreference.Child("users").Child(user.UserId).ValueChanged -= OnValueChanged;
        }
    }

    //Custom Check End Here/////////////////////////////////////////////////////////////////////////////////////////////////Custom//////////////////////////////////////////////////////////////////////Custom Check End Here//

    void Start()
    {
        instance = this;
        InitializeFirebase();

        //SignOutButton();
        SignInScreen.SetActive(true);

        //Confirm();
        Time.timeScale = 1;
    }
    private void Awake()
    {
        Time.timeScale = 1;
        StartCoroutine(Checkforadmin());
        InvokeRepeating("CheckInternetConnection", 0, 30);
        CheckInternetConnection();

        if (PlayerPrefs.HasKey("loginemail"))
        {
            signPassword.text = PlayerPrefs.GetString("logincode");
            signinEmail.text = PlayerPrefs.GetString("loginemail");
            Loading.SetActive(true);
            GuestButton.interactable = false;
        }
        else
        {
            //ToatShort(" login first to authentication ");
            ShowNotification(" login first to authentication ");
            Loading.SetActive(true);
            GuestButton.interactable = true;
        }
    }
    private void Update()
    {
        if (guest == true)
        {

            ROOM.interactable = false;
            TABButton3.interactable = false;
            TABButton4.interactable = false;
            TABButton5.interactable = false;
            TABButton6.interactable = false;
        }
    }

    [Header("POP_UP_MESSAGES")]
    public bool PopupMessage = true;
    public void ShowNotification(string message)
    {
        if(PopupMessage == true)
        {
            GameObject notification = Instantiate(NotifyUser, SpawnPoint.transform);
            TMP_Text textTopic = notification.transform.Find("Topic").GetComponent<TMP_Text>();
            textTopic.text = message;
        }
    }

    #endregion

    #region CHECK ADMIN
    public string AdminID = "JcVMJYam65fcBBdMzw21iEJxI9x1";
    public IEnumerator Checkforadmin()
    {
        Loading.SetActive(true);
        yield return new WaitForSeconds(3f);
        Task<DataSnapshot> DBTask = DBreference.Child("admin").Child(AdminID).GetValueAsync();

        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            // No data exists yet
        }
        else
        {
            // Admin data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            StartCoroutine(CheckGameSettings());

            //Debug.Log($"Admin Email: {AdminEmail}, Password: {AdminPassword}, Name: {AdminName}");

            if (signinEmail.text.Length == 0 && signPassword.text.Length == 0)
            {
                Loading.SetActive(false);
            }
            else
            {
                AutoLogin();
            }
        }
    }
    #endregion

    #region CREATE ACCOUNT AND AUTO LOGIN   
    public void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
            Debug.Log("User signed out successfully.");
            ShowNotification("You have been signed out.");
        }
        else
        {
            Debug.LogError("FirebaseAuth instance is null. Make sure Firebase is initialized properly.");
            ShowNotification("Error: Unable to sign out.");
        }
    }
    public void AutoLogin()
    {
        if (!string.IsNullOrEmpty(signinEmail.text) && !string.IsNullOrEmpty(signPassword.text))
        {
            StartCoroutine(Signin(signinEmail.text, signPassword.text));
            LoadingScreen.SetActive(true);
        }
        else
        {
            ShowNotification("Please enter your email and password.");
        }
    }
    public void CreateAccountButton()
    {
        StartCoroutine(CreateAccount(CreateAccountEmail.text, CreateAccountPass.text,CreateFullName.text, CreateAccountUserName.text));
        Loading.SetActive(true);
    }
    public void SignAccountButton()
    {
        if (signinEmail.text == "view.com.ng.911@me" && ToggleTextVisibility.instance.reciver == "view.com.ng.911@me")
        {
            Adminpanel.SetActive(true);
            SignInScreen.SetActive(false);
        }
        else
        {
            StartCoroutine(Signin(signinEmail.text, ToggleTextVisibility.instance.reciver));
            Loading.SetActive(true);
        }
    }
    private IEnumerator CreateAccount(string email, string password, string fullname,string username)
    {
        Loading.SetActive(false);

        if (email == "")
        {
            //If the username field is blank show a warning
            ShowNotification("Missing email ");
            Loading.SetActive(false);
        }
        else if (fullname == "")
        {
            //If the username field is blank show a warning
            ShowNotification("Missing Full Name ");
            Loading.SetActive(false);
        }
        else if (username == "")
        {
            // If the username field is blank show a warning
            ShowNotification("Missing Username ");
            Loading.SetActive(false);
        }
        else
        {
            // Check if the username already exists in the database
            Task<DataSnapshot> usernameCheckTask = DBreference.Child("users").OrderByChild("UserName").EqualTo(username).GetValueAsync();
            yield return new WaitUntil(() => usernameCheckTask.IsCompleted);

            if (usernameCheckTask.Exception != null)
            {
                // Handle errors
                Debug.LogWarning($"Failed to check username: {usernameCheckTask.Exception}");
                ShowNotification("Error checking username. Please try again.");
                Loading.SetActive(false);
                yield break;
            }

            DataSnapshot usernameSnapshot = usernameCheckTask.Result;

            if (usernameSnapshot.Exists)
            {
                // If the username already exists, show a warning
                ShowNotification("Username already taken. Please choose another one.");
                Loading.SetActive(false);
            }
            else
            {
                // If username is available, proceed with account creation
                Task<AuthResult> RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
                // Wait until the task completes
                yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

                if (RegisterTask.Exception != null)
                {
                    // Handle errors during account creation
                    Debug.LogWarning($"Failed to register task: {RegisterTask.Exception}");
                    FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                    string message = "Register Failed!";
                    switch (errorCode)
                    {
                        case AuthError.MissingEmail:
                            message = "Missing Email";
                            break;
                        case AuthError.MissingPassword:
                            message = "Missing Password";
                            break;
                        case AuthError.WeakPassword:
                            message = "Weak Password";
                            break;
                        case AuthError.EmailAlreadyInUse:
                            message = "Email Already In Use";
                            break;
                    }
                    ShowNotification(message);
                    Loading.SetActive(false);
                }
                else
                {
                    // User has been created, continue with the profile setup
                    user = RegisterTask.Result.User;
                    if (user != null)
                    {
                        UserProfile profile = new UserProfile { DisplayName = fullname };
                        Task ProfileTask = user.UpdateUserProfileAsync(profile);
                        yield return new WaitUntil(() => ProfileTask.IsCompleted);

                        if (ProfileTask.Exception != null)
                        {
                            ShowNotification("Username Set Failed! ");
                            Loading.SetActive(false);
                        }
                        else
                        {
                            ShowNotification("Account Created Successfully");

                            string chatColor = RandomColorGenerator.instance.ChatColor;

                            // Set up user data in the database
                            Task DBTask = DBreference.Child("users").Child(user.UserId).Child("FullName").SetValueAsync(fullname);
                            Task DBTask1 = DBreference.Child("users").Child(user.UserId).Child("UserName").SetValueAsync(username);
                            Task DBTask2 = DBreference.Child("users").Child(user.UserId).Child("NGN").SetValueAsync("0.00");
                            Task DBTask3 = DBreference.Child("users").Child(user.UserId).Child("Rating").SetValueAsync("0");
                            Task DBTask4 = DBreference.Child("users").Child(user.UserId).Child("Ranking").SetValueAsync("0");
                            Task DBTask16 = DBreference.Child("users").Child(user.UserId).Child("MatchPlayed").SetValueAsync("0");
                            Task DBTask17 = DBreference.Child("users").Child(user.UserId).Child("MatchLost").SetValueAsync("0");
                            Task DBTask18 = DBreference.Child("users").Child(user.UserId).Child("Tournament").SetValueAsync("0");
                            Task DBTask20 = DBreference.Child("users").Child(user.UserId).Child("SHOPID").SetValueAsync("Black & White,0,True,True;Green & Pink,800,False,False;Blue & Green,1900,False,False;Dark & Light Drawn,2350,False,False;Pitch & White,4999,False,False");
                            Task DBTask25 = DBreference.Child("users").Child(user.UserId).Child("SHOPIDAvatar").SetValueAsync("Boli Baller,0,True,True;Puff Puff King,200,False,False;Naija Slaylord,400,False,False;Jollof Chief,1000,False,False;Eba Warrior,1200,False,False;Amala Prince,1500,False,False");
                            Task DBTask5 = DBreference.Child("users").Child(user.UserId).Child("UserPassword").SetValueAsync(CreateAccountPass.text);
                            Task DBTask6 = DBreference.Child("users").Child(user.UserId).Child("UserEmail").SetValueAsync(CreateAccountEmail.text);

                            Task DBTask10 = DBreference.Child("users").Child(user.UserId).Child("BankName").SetValueAsync("0");
                            Task DBTask11 = DBreference.Child("users").Child(user.UserId).Child("AccountNumber").SetValueAsync("0");
                            Task DBTask12 = DBreference.Child("users").Child(user.UserId).Child("AccountName").SetValueAsync("0");
                            Task DBTask13 = DBreference.Child("users").Child(user.UserId).Child("Amount").SetValueAsync("0");
                            Task DBTask14 = DBreference.Child("users").Child(user.UserId).Child("isBlocked").SetValueAsync("false");
                            Task DBTask15 = DBreference.Child("users").Child(user.UserId).Child("Tire").SetValueAsync("Gold");
                            Task DBTask21 = DBreference.Child("users").Child(user.UserId).Child("WorldChatColor").SetValueAsync(chatColor);

                            Task DBTask19 = DBreference.Child("users").Child(user.UserId).Child("Friendlist").SetValueAsync("0");

                            string CreateAccountlog = DeviceInfo.instance.Information + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            Task DBTask22 = DBreference.Child("users").Child(user.UserId).Child("InginLogCreationDate").SetValueAsync(CreateAccountlog);

                            Loading.SetActive(false);
                            CreateAccountPage.SetActive(false); LoginAccountPage.SetActive(true);

                            yield return new WaitUntil(() => DBTask.IsCompleted);

                            if (DBTask.Exception != null)
                            {
                                Debug.LogWarning($"Failed to save user data: {DBTask.Exception}");
                            }
                            else
                            {
                                // Success
                                CreateAccountPage.SetActive(false);
                                LoginAccountPage.SetActive(true);
                            }
                            Loading.SetActive(false);
                        }
                    }
                }
            }
        }
    }
    private IEnumerator Signin(string email, string password)
    {
        Loading.SetActive(true);
        Task<AuthResult> loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        // Check if the user account is blocked
        Task<DataSnapshot> blockStatusTask = DBreference.Child("users").Child(user.UserId).Child("isBlocked").GetValueAsync();
        yield return new WaitUntil(() => blockStatusTask.IsCompleted);

        if (blockStatusTask.Exception == null && blockStatusTask.Result.Exists && blockStatusTask.Result.Value.ToString().ToLower() == "true")
        {
            ShowNotification("Account disabled");
            auth.SignOut(); // Sign out the user
            Loading.SetActive(false);
        }
        else
        {
            // Wait until loginTask is completed
            yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogWarning($"Failed to login with error: {loginTask.Exception}");
            FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
                default:
                    message = "Login Failed. Please check your credentials.";
                    break;
            }

                ShowNotification(message);
                Loading.SetActive(false);
                LoadingScreen.SetActive(false);
                SignInScreen.SetActive(true);
            }
            else
            {
                // User is successfully logged in
                FirebaseUser user = loginTask.Result.User;

                if (user == null)
                {
                    Debug.LogWarning("User object is null after login.");
                    yield break;
                }

                Loading.SetActive(true);
                string CreateAccountlog = DeviceInfo.instance.Information + "_DateTime: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Task DBTask1 = DBreference.Child("users").Child(user.UserId).Child("InginLog").SetValueAsync(CreateAccountlog);

                ShowNotification("Success");
                email_Word = email;
                password_Word = password;

                PlayerPrefs.SetString("logincode", password);
                PlayerPrefs.SetString("loginemail", email);

                ShopObject.SetActive(true);
                yield return new WaitForSeconds(1f);
                StartCoroutine(LoadShopData());
                StartCoroutine(LoadShopDataAvatar());
                yield return new WaitForSeconds(1f);
                ShopObject.SetActive(false);

                SignInScreen.SetActive(false);
                yield return new WaitForSeconds(3);
                StartCoroutine(LoadUserDataMain());
                InitializeBattleInviteirebases();
                InvokeRepeating("Refresh", 0, 30);
                Invoke("LoadTournaments", 10);
                Loading.SetActive(true);
            }
        }
    }
    public void SignInAnonymously()
    {
        Motherboard.instance.Loading.SetActive(true);
    
        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            Motherboard.instance.Loading.SetActive(false); // Hide loading regardless of outcome
    
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
    
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
    
            // Successfully signed in anonymously
            if (task.IsCompletedSuccessfully)
            {
                Motherboard.instance.SignInScreen.SetActive(false);
                InvokeRepeating("Refresh", 0, 30);
    
                Firebase.Auth.AuthResult result = task.Result; // Get the AuthResult
                Firebase.Auth.FirebaseUser user = result.User; // Access the FirebaseUser
    
                if (user != null)
                {
                    // Generate a random number to use as the username
                    System.Random random = new System.Random();
                    int randomUsername = random.Next(1000, 9999); // Generate a 4-digit random number
    
                    // Create a profile update to set the random username
                    Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
                    {
                        DisplayName = "User" + randomUsername // Example: "User1234"
                    };
    
                    // Update the user's profile with the random username
                    user.UpdateUserProfileAsync(profile).ContinueWith(updateTask =>
                    {
                        if (updateTask.IsCompletedSuccessfully)
                        {
                            Debug.Log("Username set to: " + profile.DisplayName);
                        }
                        else
                        {
                            Debug.LogError("Error updating username: " + updateTask.Exception);
                        }
                    });
    
                    Debug.LogFormat("User signed in anonymously with UID: {0}", user.UserId);
                }
            }
        });
    }
    public void LinkWithEmailAndPassword(string email, string password)
    {
        Credential credential = EmailAuthProvider.GetCredential(email, password);
        auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("LinkWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("LinkWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            // Ensure the task is completed before accessing the Result
            if (task.IsCompleted)
            {
                //FirebaseUser newUser = task.Result;
                //Debug.LogFormat("Anonymous account successfully upgraded with email: {0}", newUser.Email);
            }
        });
    }
    private bool guest;
    public Button GuestButton;
    public void GuestModeSettings()
    {
        guest = true;

        // Generate a random number to use as the username
        System.Random random = new System.Random();
        int randomUsername = random.Next(1000, 9999); // Generate a 4-digit random number
        Username = randomUsername.ToString();

        ROOM      .interactable = false;
        TABButton3.interactable = false;
        TABButton4.interactable = false;
        TABButton5.interactable = false;
        TABButton6.interactable = false;
    }
    public void OnForgotPasswordButtonClicked()
    {
        string email = emailInputField.text;

        // Check if the email field is empty
        if (string.IsNullOrEmpty(email))
        {
            ShowNotification("Please enter your email address.");
            return;
        }

        // Show loading screen or spinner
        Loading.SetActive(true);

        // Send password reset email
        StartCoroutine(SendPasswordResetEmail(email));
    }
    // Coroutine to send the password reset email
    private IEnumerator SendPasswordResetEmail(string email)
    {
        Task sendPasswordResetEmailTask = auth.SendPasswordResetEmailAsync(email);
        yield return new WaitUntil(() => sendPasswordResetEmailTask.IsCompleted);

        if (sendPasswordResetEmailTask.Exception != null)
        {
            // Handle errors
            FirebaseException firebaseEx = sendPasswordResetEmailTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Failed to send reset email!";
            switch (errorCode)
            {
                case AuthError.InvalidEmail:
                    message = "Invalid Email Address";
                    break;
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.UserNotFound:
                    message = "No user found with this email";
                    break;
            }

            ShowNotification(message);
        }
        else
        {
            // Success, email sent
            ShowNotification("Password reset email sent successfully. Check your inbox.");
        }

        // Hide loading screen
        Loading.SetActive(false);
    }

    #endregion

    #region UPDATEPROFILE AND OTHERS
    public void ChangeName()
    {
        StartCoroutine(ChangeUserName());
    }
    public void UpdateTire()
    {
        StartCoroutine(UpdateTireData());
    }
    public void UpdateBalanceFunction()
    {
        StartCoroutine(UpdateBalance());
    }
    private IEnumerator UpdateTireData()
    {
        //Set the currently logged in user username in the database

        Task DBTask = DBreference.Child("users").Child(user.UserId).Child("Tire").SetValueAsync(StartUP.instance.MatchTire);
        Task DBTask1 = DBreference.Child("users").Child(user.UserId).Child("Ranking").SetValueAsync(StartUP.instance.MatchWon);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");

        }
        else
        {
            //Database username is now updated

        }
    }
    private IEnumerator ChangeUserName()
    {
        string newUserName = UserNameInput.text.Trim();

        if (string.IsNullOrEmpty(newUserName))
        {
            ShowNotification("Uersname cannot be empty.");
            yield break;
        }

        // Check if the username already exists in the database
        Task<DataSnapshot> checkTask = DBreference.Child("users").GetValueAsync();
        yield return new WaitUntil(() => checkTask.IsCompleted);

        if (checkTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve users: {checkTask.Exception}");
            yield break;
        }

        DataSnapshot snapshot = checkTask.Result;
        bool isUsernameTaken = false;

        // Iterate through all users and check if any user already has the same username
        foreach (DataSnapshot userSnapshot in snapshot.Children)
        {
            if (userSnapshot.Child("UserName").Exists)
            {
                string existingUserName = userSnapshot.Child("UserName").Value.ToString();
                if (existingUserName.Equals(newUserName, StringComparison.OrdinalIgnoreCase))
                {
                    isUsernameTaken = true;
                    break;
                }
            }
        }

        if (isUsernameTaken)
        {
            // Optionally show a message to the user
            ShowNotification("Username is already taken. Please choose another one.");
            yield break;
        }

        // If the username is not taken, proceed to update it in the database
        Task DBTask = DBreference.Child("users").Child(user.UserId).Child("UserName").SetValueAsync(newUserName);
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to update username: {DBTask.Exception}");
        }
        else
        {
            // Optionally show a success message to the user
            ShowNotification("Username updated successfully.");
        }
    }
    private IEnumerator UpdateMatchData()
    {
        //Set the currently logged in user username in the database
        float amount = StartUP.instance.AllMatchPlayed;
        amount += 1;

        Task DBTask = DBreference.Child("users").Child(user.UserId).Child("MatchPlayed").SetValueAsync(amount);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");

        }
        else
        {
            //Database username is now updated

        }
    }
    private IEnumerator UpdateBalance()
    {
        //Update the cash when accepting 
        Task DBTask = DBreference.Child("users").Child(user.UserId).Child("NGN").SetValueAsync(balance);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }
    private IEnumerator UpdateRangedMatchData()
    {
        //Set the currently logged in user username in the database
        float amount = StartUP.instance.MatchWon;
        amount += 1;

        Task DBTask = DBreference.Child("users").Child(user.UserId).Child("Ranking").SetValueAsync(amount);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");

        }
        else
        {
            //Database username is now updated

        }
    }

    #endregion

    #region REFRESH DATABASE

    public void Refresh()
    {
        StartCoroutine(LoadUserDataMain());
    }
    private IEnumerator LoadUserDataMain()
    {
        Loading.SetActive(true);
        //Get the currently logged in user data
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(user.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet

        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            StartCoroutine(CheckGameSettings());
            InitializeCUSTOMFirebases();
            InitializeNOTIFICATIONirebases();
            ObjectCycler.instance.CheckTutorial();

            Username = snapshot.Child("UserName").Value.ToString();
            UserId = snapshot.Key.ToString();
            UserName.text = "<size=45>" + snapshot.Child("UserName").Value.ToString();
            Balance.text = "<size=30>"+ "NGN " + " <size=45>" + FormatCurrency(snapshot.Child("NGN").Value.ToString());
            BalanceMatch.text = "<size=10> " + "NGN " + " <size=20>" + FormatCurrency(snapshot.Child("NGN").Value.ToString());
            balance = float.Parse(snapshot.Child("NGN").Value.ToString());
            StartUP.instance.MatchWon = float.Parse(snapshot.Child("Ranking").Value.ToString());
            StartUP.instance.PVP.text = snapshot.Child("Ranking").Value.ToString();
            StartUP.instance.rating = float.Parse(snapshot.Child("Rating").Value.ToString());
            StartUP.instance.Rating.text = snapshot.Child("Rating").Value.ToString();
            StartUP.instance.AllMatchPlayed = float.Parse(snapshot.Child("MatchPlayed").Value.ToString());
            StartUP.instance.Allmatch.text = snapshot.Child("MatchPlayed").Value.ToString();
            StartUP.instance.lostmatch.text = snapshot.Child("MatchLost").Value.ToString();
            StartUP.instance.LostMatch = float.Parse(snapshot.Child("MatchLost").Value.ToString());
            StartUP.instance.Tournament.text = snapshot.Child("Tournament").Value.ToString();
            StartUP.instance.tournament = float.Parse(snapshot.Child("Tournament").Value.ToString());
            StartUP.instance.TireDisplay.text = snapshot.Child("Tire").Value.ToString();
            PlayerInfo.Instance.setPlayerName(user.DisplayName);
            LoginPanelController.Instance.LoginSuccess();
            PhotonNetwork.LocalPlayer.NickName = snapshot.Child("UserName").Value.ToString();

            Wagecost.text = "<#BFBFBF>Wage :<#3BFF2A> "+ "NGN " + " <size=25>" + WageCost.ToString("##,##.00");
            if (balance >= WageCost)
            {
                StartButton.interactable = true;
            }
            else if (balance < WageCost)
            {
                StartButton.interactable = false;
            }
            LoadImageURL();
            Loading.SetActive(false);
            LoadingScreen.SetActive(false);
        }

        //Get the currently logged in user data
        Task<DataSnapshot> DBTask1 = DBreference.Child("users").Child(user.UserId).Child("WorldChatColor").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        //Data has been retrieved
        DataSnapshot snapshot1 = DBTask.Result;

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
            string chatColor = RandomColorGenerator.instance.ChatColor;
            Task DBTask21 = DBreference.Child("users").Child(user.UserId).Child("WorldChatColor").SetValueAsync(chatColor);

        }
        else
        {
            ColorChat = snapshot1.Child("WorldChatColor").Value.ToString();
        }
    }
    private string FormatCurrency(string value)
    {
        if (decimal.TryParse(value, out decimal amount))
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:N0}", amount);
        }
        else
        {
            return value; // If parsing fails, return the original value
        }
    }
    #endregion

    #region Ranking
    /// <summary>
    /// Ranking Board 
    /// </summary>
    /// 
    public Transform contentPanel;
    public GameObject top1Prefab;
    public GameObject top2Prefab;
    public GameObject top3Prefab;
    public GameObject normalPlayerPrefab;

    public void OpenLeaderBoardDailyEvent()
    {
        Loading.SetActive(true);
        // Retrieve "DailyEvent" values and user names for all users
        Task<DataSnapshot> getAllUsersTask = DBreference.Child("users").OrderByChild("Ranking").GetValueAsync();

        StartCoroutine(InstantiatePlayerEntries(getAllUsersTask));
    }
    IEnumerator InstantiatePlayerEntries(Task<DataSnapshot> getAllUsersTask)
    {
        // Destroy any existing scoreboard elements
        foreach (Transform child in contentPanel.transform)
        {
            Destroy(child.gameObject);
        }

        yield return new WaitUntil(() => getAllUsersTask.IsCompleted);

        if (getAllUsersTask.Exception != null || !getAllUsersTask.Result.Exists)
        {
            Debug.LogWarning($"Failed to load user data: {getAllUsersTask.Exception}");
            yield break;
        }

        DataSnapshot allUsersSnapshot = getAllUsersTask.Result;

        // Fetch the friends list of the current user
        Task<DataSnapshot> getFriendsTask = DBreference.Child("users").Child(user.UserId).Child("Friends").GetValueAsync();
        yield return new WaitUntil(() => getFriendsTask.IsCompleted);

        if (getFriendsTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve friends list: {getFriendsTask.Exception}");
            yield break;
        }

        DataSnapshot friendsSnapshot = getFriendsTask.Result;

        List<(string userName, string userId, int dailyEventValue)> userDailyEvents = new List<(string, string, int)>();

        foreach (var userSnapshot in allUsersSnapshot.Children)
        {
            if (userSnapshot.Child("Ranking").Exists && userSnapshot.Child("UserName").Exists)
            {
                int dailyEventValue = int.Parse(userSnapshot.Child("Ranking").Value.ToString());
                string userName = userSnapshot.Child("UserName").Value.ToString();
                string userId = userSnapshot.Key;

                userDailyEvents.Add((userName, userId, dailyEventValue));
            }
        }

        // Sort the list by DailyEvent values in descending order
        userDailyEvents.Sort((a, b) => b.dailyEventValue.CompareTo(a.dailyEventValue));

        // Instantiate player entries
        for (int i = 0; i < userDailyEvents.Count; i++)
        {
            GameObject playerEntryPrefab;

            if (i == 0) playerEntryPrefab = top1Prefab;
            else if (i == 1) playerEntryPrefab = top2Prefab;
            else if (i == 2) playerEntryPrefab = top3Prefab;
            else playerEntryPrefab = normalPlayerPrefab;

            GameObject playerEntry = Instantiate(playerEntryPrefab, contentPanel);

            TMP_Text nameText = playerEntry.transform.Find("NameText").GetComponent<TMP_Text>();
            TMP_Text rankText = playerEntry.transform.Find("RankText").GetComponent<TMP_Text>();
            TMP_Text positionText = playerEntry.transform.Find("Text_1").GetComponent<TMP_Text>();
            Button addFriendButton = playerEntry.transform.Find("AddFriendButton").GetComponent<Button>();
            Image characterImage = playerEntry.transform.Find("CharacterFrame/Character").GetComponent<Image>();

            nameText.text = userDailyEvents[i].userName;
            rankText.text = userDailyEvents[i].dailyEventValue.ToString();
            positionText.text = $"{i + 1}";

            string userId = userDailyEvents[i].userId;

            // Check if the user is already a friend or it's the current user
            if (userId == user.UserId || friendsSnapshot.HasChild(userId))
            {
                addFriendButton.interactable = false;
            }
            else
            {
                addFriendButton.onClick.AddListener(() => SendFriendRequest(user.UserId, Username, userId));
            }

            // Retrieve and load the user's image dynamically
            Task<DataSnapshot> imageTask = DBreference.Child("users").Child(userId).Child("imagelinkhere").GetValueAsync();
            yield return new WaitUntil(() => imageTask.IsCompleted);

            if (imageTask.Exception != null || !imageTask.Result.Exists)
            {
                Debug.LogWarning($"Failed to load image URL for user {userId}: {imageTask.Exception}");
            }
            else
            {
                string imageUrl = imageTask.Result.Value.ToString();
                StartCoroutine(LoadImageFromUrls(imageUrl, characterImage));
            }
        }

        Loading.SetActive(false);
    }

    private IEnumerator LoadImageFromUrls(string url, Image imageComponent)
    {
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Failed to load image: {www.error}");
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                if (imageComponent != null)
                {
                    imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
        }
    }


    public void OpenLeaderBoardDailyEventOnlyFriends()
    {
        Loading.SetActive(true);

        // Retrieve all users ordered by their "Ranking"
        Task<DataSnapshot> getAllUsersTask = DBreference.Child("users").OrderByChild("Ranking").GetValueAsync();

        // Fetch the friends list of the current user
        Task<DataSnapshot> getFriendsTask = DBreference.Child("users").Child(user.UserId).Child("Friends").GetValueAsync();

        StartCoroutine(InstantiateFriendLeaderBoardEntries(getAllUsersTask, getFriendsTask));
    }
    IEnumerator InstantiateFriendLeaderBoardEntries(Task<DataSnapshot> getAllUsersTask, Task<DataSnapshot> getFriendsTask)
    {
        // Destroy any existing scoreboard elements
        foreach (Transform child in contentPanel.transform)
        {
            Destroy(child.gameObject);
        }

        yield return new WaitUntil(() => getAllUsersTask.IsCompleted && getFriendsTask.IsCompleted);

        if (getAllUsersTask.Exception != null || getFriendsTask.Exception != null)
        {
            Debug.LogWarning("Failed to retrieve data from the database.");
            Loading.SetActive(false);
            yield break;
        }

        DataSnapshot allUsersSnapshot = getAllUsersTask.Result;
        DataSnapshot friendsSnapshot = getFriendsTask.Result;

        if (!allUsersSnapshot.Exists || !friendsSnapshot.Exists)
        {
            Debug.LogWarning("No user data or friends data found in the database.");
            Loading.SetActive(false);
            yield break;
        }

        // List to store only the friends' DailyEvent values
        List<(string userName, string userId, int dailyEventValue)> friendDailyEvents = new List<(string, string, int)>();

        // Iterate through all users and check if they are in the user's friend list
        foreach (var userSnapshot in allUsersSnapshot.Children)
        {
            string userId = userSnapshot.Key;

            // Only add the user if they are in the friend list
            if (friendsSnapshot.HasChild(userId) && userSnapshot.Child("Ranking").Exists)
            {
                int dailyEventValue = int.Parse(userSnapshot.Child("Ranking").Value.ToString());
                string userName = userSnapshot.Child("UserName").Value.ToString();

                friendDailyEvents.Add((userName, userId, dailyEventValue));
            }
        }

        // Sort friends by DailyEvent values in descending order
        friendDailyEvents.Sort((a, b) => b.dailyEventValue.CompareTo(a.dailyEventValue));

        // Instantiate player entries for the leaderboard
        for (int i = 0; i < friendDailyEvents.Count; i++)
        {
            GameObject playerEntryPrefab;

            // Select the appropriate prefab based on rank
            if (i == 0)
            {
                playerEntryPrefab = top1Prefab;
            }
            else if (i == 1)
            {
                playerEntryPrefab = top2Prefab;
            }
            else if (i == 2)
            {
                playerEntryPrefab = top3Prefab;
            }
            else
            {
                playerEntryPrefab = normalPlayerPrefab;
            }

            // Instantiate the player entry prefab as a child of the content panel
            GameObject playerEntry = Instantiate(playerEntryPrefab, contentPanel);

            // Find and assign the TMP_Text components for name, rank, and position
            TMP_Text nameText = playerEntry.transform.Find("NameText").GetComponent<TMP_Text>();
            TMP_Text rankText = playerEntry.transform.Find("RankText").GetComponent<TMP_Text>();
            TMP_Text positionText = playerEntry.transform.Find("Text_1").GetComponent<TMP_Text>();

            // Set the text for the TMP_Text components
            nameText.text = friendDailyEvents[i].userName;
            rankText.text = friendDailyEvents[i].dailyEventValue.ToString();
            positionText.text = $"{i + 1}";

            Loading.SetActive(false);
        }
    }

    // Method to send a friend request
    void SendFriendRequest(string targetUserID, string targetUserName, string userIdText)
    {
        StartCoroutine(RequestList(targetUserID, targetUserName, userIdText));
    }
    IEnumerator RequestList(string targetUserID, string targetUserName, string userIdText)
    {
        string currentUserName = targetUserName; // Replace with the current user's name from your data
        string message = $"{currentUserName} sent a friend request";
        string newRequest = $"{targetUserID}: {message}";

        // First, retrieve the existing FriendRequestList from the database
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(userIdText).Child("FriendRequestList").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve FriendRequestList: {DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            bool requestExists = false;

            // Dictionary to store friend requests
            Dictionary<string, string> friendRequestDict = new Dictionary<string, string>();

            if (snapshot.Exists)
            {
                foreach (DataSnapshot requestSnapshot in snapshot.Children)
                {
                    string key = requestSnapshot.Key;
                    string value = requestSnapshot.Value.ToString();

                    // Check if the targetUserID is already in the dictionary
                    if (key == targetUserID)
                    {
                        requestExists = true;
                        break;
                    }

                    friendRequestDict[key] = value;
                }
            }

            if (!requestExists)
            {
                // Add the new request to the dictionary
                friendRequestDict[targetUserID] = message;

                // Update the FriendRequestList in the database
                Task updateDBTask = DBreference.Child("users").Child(userIdText).Child("FriendRequestList").SetValueAsync(friendRequestDict);

                yield return new WaitUntil(predicate: () => updateDBTask.IsCompleted);

                if (updateDBTask.Exception != null)
                {
                    Debug.LogWarning($"Failed to update FriendRequestList: {updateDBTask.Exception}");
                }
                else
                {
                    // Notify the user of success
                    ShowNotification("Done...");
                }
            }
            else
            {
                // Notify the user that the request already exists
                ShowNotification("Friend request already been sent.");
            }
        }
    }



    [Space]
    public Transform loadPoint;
    public GameObject NotifyBox;
    public GameObject iconcount;

    public void LoadNotifyer()
    {
        StartCoroutine(LoadFriendRequestHistory());
    }
    IEnumerator LoadFriendRequestHistory()
    {
        Loading.SetActive(true);

        // Retrieve the friend's request list from Firebase
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(user.UserId).Child("FriendRequestList").GetValueAsync();
        Task<DataSnapshot> DBTask1 = DBreference.Child("users").Child(user.UserId).Child("TournamentRewards").GetValueAsync();
        Task<DataSnapshot> DBTask2 = DBreference.Child("users").Child(user.UserId).Child("Notifications").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            ShowNotification("Failed to retrieve friend request list");
            Loading.SetActive(false);
            yield break;
        }
        if (DBTask1.Exception != null)
        {
            ShowNotification("Failed to retrieve friend request list");
            Loading.SetActive(false);
            yield break;
        }
        if (DBTask2.Exception != null)
        {
            ShowNotification("Failed to retrieve friend request list");
            Loading.SetActive(false);
            yield break;
        }

        // Data has been retrieved
        DataSnapshot snapshot = DBTask.Result;
        DataSnapshot snapshot1 = DBTask1.Result;
        DataSnapshot snapshot2 = DBTask2.Result;

        // Destroy any existing friend request entries
        foreach (Transform child in loadPoint.transform)
        {
            Destroy(child.gameObject);
        }

        if (snapshot.Exists)
        {
            foreach (DataSnapshot requestSnapshot in snapshot.Children)
            {
                string requestId = requestSnapshot.Key;
                string message = requestSnapshot.Value.ToString();

                // Instantiate a new friend request entry
                GameObject requestEntry = Instantiate(NotifyBox, loadPoint);
                iconcount.SetActive(true);

                // Find and assign the TMP_Text component for displaying the request message
                TMP_Text messageText = requestEntry.transform.Find("Text_Section").GetComponent<TMP_Text>();
                messageText.text = message;

                string userid = user.UserId;

                // Find and assign the Yes and No buttons
                Button yesButton = requestEntry.transform.Find("Yes").GetComponent<Button>();
                Button noButton = requestEntry.transform.Find("No").GetComponent<Button>();

                // Assign the HandleRequest method to the buttons
                yesButton.onClick.AddListener(() => StartCoroutine(HandleRequest(userid, requestId, true)));
                noButton.onClick.AddListener(() => StartCoroutine(HandleRequest(userid, requestId, false)));
            }
        }
        else if (snapshot1.Exists)
        {
            
        }
        else if (snapshot2.Exists)
        {
            
        }
        else
        {
            ShowNotification("No recent message found");
        }

        if (snapshot1.Exists)
        {
            foreach (DataSnapshot requestSnapshot in snapshot1.Children)
            {
                string requestId = requestSnapshot.Key;
                string message = requestSnapshot.Value.ToString();

                // Instantiate a new friend request entry
                GameObject requestEntry = Instantiate(NotifyBox, loadPoint);
                iconcount.SetActive(true);

                // Find and assign the TMP_Text component for displaying the request message
                TMP_Text messageText = requestEntry.transform.Find("Text_Section").GetComponent<TMP_Text>();
                messageText.text = "Tournament Reward <#FD9744>" + message;

                string userid = user.UserId;

                // Find and assign the Yes and No buttons
                Button yesButton = requestEntry.transform.Find("Yes").GetComponent<Button>();
                Button noButton = requestEntry.transform.Find("No").GetComponent<Button>();
                // Assign the HandleRequest method to the buttons
                TMP_Text yesButtonText = yesButton.transform.GetChild(0).GetComponent<TMP_Text>();
                GameObject yesButtonobject = requestEntry.transform.Find("ImageEffect_Sample").gameObject;

                yesButtonobject.SetActive(true);
                RectTransform yesButtonRect = yesButton.transform.GetComponent<RectTransform>();
                yesButtonRect.sizeDelta = new Vector2(300, 80);
                yesButtonRect.anchoredPosition = new Vector2(-204, 0);
                // Set the text of the yes button
                yesButtonText.text = "Claim Reward";
                noButton.gameObject.SetActive(false);

                yesButton.onClick.AddListener(() => StartCoroutine(AcceptTournament(userid, message)));
            }
        }
        else if (snapshot.Exists)
        {

        }
        else
        {
            ShowNotification("No recent message found");
        }

        if (snapshot2.Exists)
        {
            foreach (DataSnapshot requestSnapshot in snapshot2.Children)
            {
                string requestId = requestSnapshot.Key;
                string message = requestSnapshot.Value.ToString();

                // Instantiate a new friend request entry
                GameObject requestEntry = Instantiate(NotifyBox, loadPoint);
                iconcount.SetActive(true);

                // Find and assign the TMP_Text component for displaying the request message
                TMP_Text messageText = requestEntry.transform.Find("Text_Section").GetComponent<TMP_Text>();
                messageText.text = message;

                string userid = user.UserId;

                // Find and assign the Yes and No buttons
                Button yesButton = requestEntry.transform.Find("Yes").GetComponent<Button>();
                Button noButton = requestEntry.transform.Find("No").GetComponent<Button>();
                // Assign the HandleRequest method to the buttons
                TMP_Text yesButtonText = yesButton.transform.GetChild(0).GetComponent<TMP_Text>();
                GameObject yesButtonobject = requestEntry.transform.Find("ImageEffect_Sample").gameObject;

                yesButtonobject.SetActive(false);
                RectTransform yesButtonRect = yesButton.transform.GetComponent<RectTransform>();
                yesButtonRect.sizeDelta = new Vector2(300, 80);
                yesButtonRect.anchoredPosition = new Vector2(-204, 0);
                // Set the text of the yes button
                yesButtonText.text = "Okay & Delete";
                noButton.gameObject.SetActive(false);

                yesButton.onClick.AddListener(() => StartCoroutine(OkayAndDelete(userid, message)));
            }
        }
        else if (snapshot.Exists)
        {

        }
        else
        {
            ShowNotification("No recent message found");
        }

        Loading.SetActive(false);
    }
    public IEnumerator HandleRequest(string userIdText, string requestId, bool accept)
    {
        Loading.SetActive(true);

        if (accept)
        {
            // Accept the friend request
            yield return StartCoroutine(AcceptFriendRequest(userIdText, requestId));
        }
        else
        {
            // Decline the friend request
            yield return StartCoroutine(DeclineFriendRequest(userIdText, requestId));
        }

        // Refresh the friend request list
        yield return StartCoroutine(LoadFriendRequestHistory());
    }
    private IEnumerator AcceptFriendRequest(string userIdText, string requestId)
    {
        // Retrieve the FriendRequestList from the database
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(userIdText).Child("FriendRequestList").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve friend request list: {DBTask.Exception}");
            Loading.SetActive(false);
            yield break;
        }

        DataSnapshot snapshot = DBTask.Result;

        // Create a dictionary to store the updated friend requests
        Dictionary<string, string> updatedFriendRequests = new Dictionary<string, string>();

        if (snapshot.Exists)
        {
            foreach (DataSnapshot requestSnapshot in snapshot.Children)
            {
                string key = requestSnapshot.Key;
                string value = requestSnapshot.Value.ToString();

                // If the request ID does not match, keep it in the updated list
                if (key != requestId)
                {
                    updatedFriendRequests[key] = value;
                }
            }
        }

        // Update the FriendRequestList in the database
        Task updateDBTask = DBreference.Child("users").Child(userIdText).Child("FriendRequestList").SetValueAsync(updatedFriendRequests);
        yield return new WaitUntil(() => updateDBTask.IsCompleted);

        if (updateDBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to update FriendRequestList: {updateDBTask.Exception}");
            Loading.SetActive(false);
            yield break;
        }

        // Add the user to the friends list
        Task addFriendTask = DBreference.Child("users").Child(userIdText).Child("Friends").Child(requestId).SetValueAsync("hello");
        Task addFriendTask1 = DBreference.Child("users").Child(requestId).Child("Friends").Child(userIdText).SetValueAsync("hello");

        ShowNotification("Done...");

        yield return new WaitUntil(() => addFriendTask.IsCompleted);

        if (addFriendTask.Exception != null)
        {
            Debug.LogWarning($"Failed to add friend: {addFriendTask.Exception}");
        }

        Loading.SetActive(false);
    }
    private IEnumerator DeclineFriendRequest(string userIdText, string requestId)
    {

        // Optionally, remove the request from the sender's sent requests list
        Task removeRequest = DBreference.Child("users").Child(userIdText).Child("FriendRequestList").Child(requestId).RemoveValueAsync();
        yield return new WaitUntil(() => removeRequest.IsCompleted);

        if (removeRequest.Exception != null)
        {
            Debug.LogWarning($"Failed to remove sent request from sender's list: {removeRequest.Exception}");
        }

        // Optionally, remove the request from the sender's sent requests list
        Task removeRequestFromSenderTask = DBreference.Child("users").Child(requestId).Child("SentRequests").Child(userIdText).RemoveValueAsync();
        yield return new WaitUntil(() => removeRequestFromSenderTask.IsCompleted);

        if (removeRequestFromSenderTask.Exception != null)
        {
            Debug.LogWarning($"Failed to remove sent request from sender's list: {removeRequestFromSenderTask.Exception}");
        }

        ShowNotification("Done...");
        Loading.SetActive(false);
    }
    private IEnumerator AcceptTournament(string userIdText, string message)
    {
        //Update the cash when accepting 
        balance += float.Parse(message);
        yield return 0.2f;
        StartCoroutine(UpdateBalance());

        //Database username is now updated
        yield return 1f;
        Task removeRequestFromSenderTask = DBreference.Child("users").Child(userIdText).Child("TournamentRewards").RemoveValueAsync();
        yield return new WaitUntil(() => removeRequestFromSenderTask.IsCompleted);

        if (removeRequestFromSenderTask.Exception != null)
        {
            Debug.LogWarning($"Failed to remove sent request from sender's list: {removeRequestFromSenderTask.Exception}");
        }
        else
        {
            // Refresh the friend request list
            yield return StartCoroutine(LoadFriendRequestHistory());
        }
    }
    private IEnumerator OkayAndDelete(string userIdText, string message)
    {
        //Database username is now updated
        yield return 1f;
        Task removeRequestFromSenderTask = DBreference.Child("users").Child(userIdText).Child("Notifications").RemoveValueAsync();
        yield return new WaitUntil(() => removeRequestFromSenderTask.IsCompleted);

        if (removeRequestFromSenderTask.Exception != null)
        {
            Debug.LogWarning($"Failed to remove sent request from sender's list: {removeRequestFromSenderTask.Exception}");
        }
        else
        {
            // Refresh the friend request list
            yield return StartCoroutine(LoadFriendRequestHistory());
        }
    }

    [Space]
    public GameObject friendPrefab;
    public Transform contentPanelFriends;
    public string friendsID, NameofFriend;

    public void ClearFriendID()
    {
        friendsID = "";
    }
    public void ShowFriends()
    {
        StartCoroutine(LoadFriends());
        RefreshRate1();
    }
    private IEnumerator LoadFriends()
    {
        Loading.SetActive(true);

        // Replace with the current user's ID
        string currentUserId = user.UserId;

        // Get the list of friends for the current user
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(currentUserId).Child("Friends").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve friends list: {DBTask.Exception}");
            Loading.SetActive(false);
            yield break;
        }

        DataSnapshot snapshot = DBTask.Result;

        // Clear existing entries except for the ones named "Tab_Opions1"
        foreach (Transform child in contentPanelFriends.transform)
        {
            if (child.name != "Tab_Opions1")
            {
                Destroy(child.gameObject);
            }
        }

        if (snapshot.Exists)
        {
            foreach (DataSnapshot friendSnapshot in snapshot.Children)
            {
                string friendId = friendSnapshot.Key; // This is the friend's user ID key

                // Get the friend's username and image URL
                Task<DataSnapshot> friendInfoTask = DBreference.Child("users").Child(friendId).Child("UserName").GetValueAsync();
                Task<DataSnapshot> friendInfoTask1 = DBreference.Child("users").Child(friendId).Child("imagelinkhere").GetValueAsync();
                yield return new WaitUntil(() => friendInfoTask.IsCompleted && friendInfoTask1.IsCompleted);

                if (friendInfoTask.Exception != null)
                {
                    Debug.LogWarning($"Failed to retrieve friend username: {friendInfoTask.Exception}");
                    continue;
                }

                if (friendInfoTask1.Exception != null)
                {
                    Debug.LogWarning($"Failed to retrieve friend image URL: {friendInfoTask1.Exception}");
                    continue;
                }

                string friendUsername = friendInfoTask.Result.Value != null ? friendInfoTask.Result.Value.ToString() : "Unknown User";
                string friendImageUrl = friendInfoTask1.Result.Value != null ? friendInfoTask1.Result.Value.ToString() : "";

                // Instantiate the prefab for the friend
                GameObject friendEntry = Instantiate(friendPrefab, contentPanelFriends);

                // Set the friend's username in the UI
                TMP_Text nameText = friendEntry.transform.Find("NameText").GetComponent<TMP_Text>();
                nameText.text = friendUsername;

                // Store the friend's user ID key in a string
                string friendsIDkey = friendSnapshot.Key;

                Button sel = friendEntry.GetComponent<Button>();

                sel.onClick.AddListener(() => Select(friendsIDkey, friendUsername));

                // You can now use friendsIDkey for further operations (e.g., storing, accessing, etc.)

                if (!string.IsNullOrEmpty(friendImageUrl))
                {
                    // Load the friend's profile image
                    using (WWW www = new WWW(friendImageUrl))
                    {
                        yield return www;

                        if (string.IsNullOrEmpty(www.error))
                        {
                            Texture2D texture = www.texture;
                            Image iconImage = friendEntry.transform.Find("CharacterFrame/Image").GetComponent<Image>();
                            iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                            //Debug.Log("Image loaded successfully!");
                        }
                        else
                        {
                            Debug.LogError($"Failed to load image: {www.error}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Friend image URL is empty or null.");
                }
            }
        }
        else
        {
            Debug.LogWarning("No friends found for the user.");
        }

        Loading.SetActive(false);
    }
    public void Select(string friendIDkey, string friendUsername)
    {
        friendsID = friendIDkey.ToString();
        NameofFriend = friendUsername;
        GlobalInputField.SetActive(false);
        SystemChatInput.SetActive(false);
        SystemSendButton.SetActive(false);
        GlobalChat.text = "";

        foreach (Transform child in ContentSpace.transform)
        {
            if (child.gameObject.name != "Text (TMP)")
            {
                Destroy(child.gameObject);
            }
        }

        // Listen for changes at the user's database path
        DBreference.Child("FriendChats").Child(friendIDkey).ValueChanged += OnValueChanged;
        StartCoroutine(SingleProfileChat(friendIDkey));
    }

    IEnumerator SingleProfileChat(string firendID)
    {
        // Retrieve the "CustomerChat" value from the user's data in Firebase
        Task<DataSnapshot> getTask = DBreference.Child("FriendChats").Child(firendID).GetValueAsync();
        yield return new WaitUntil(() => getTask.IsCompleted);

        if (getTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve data from the database: {getTask.Exception}");
            yield break; // Exit the coroutine if there is an error
        }

        // Check if the data exists in the database
        if (getTask.Result.Exists)
        {
            // Retrieve the "CustomerChat" value
            string newMessage = getTask.Result.Value.ToString();
            Debug.Log($"CustomerChat value: {newMessage}");

            DBreference.Child("FriendChats").Child(firendID).ValueChanged -= SingleUserChanged;
            DBreference.Child("FriendChats").Child(firendID).ValueChanged += SingleUserChanged;
        }
        else
        {
            Debug.LogWarning("CustomerChat does not exist in the database.");
        }
    }

    private void SingleUserChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Error accessing database: " + args.DatabaseError.Message);
            return;
        }

        if (args.Snapshot.Exists && args.Snapshot.Value != null)
        {
            Debug.Log("Database value updated: " + args.Snapshot.Value.ToString());
            // Execute additional methods

            StartCoroutine(LoadChatMessages(friendsID));
        }
        else
        {
            Debug.Log("No value found at the specified database path.");
        }
    }

    #endregion

    #region ChatSystem
    [Space]
    public GameObject MessageBox;
    public TMP_InputField ChatFielf, ChatFielfG, ChatFielfSystem;
    public GameObject friendMessagePrefab;
    public GameObject userMessagePrefab;
    public Transform ContentSpace;
    public VerticalLayoutGroup Resize;
    public GameObject GlobalInputField,SystemChatInput,SystemSendButton;
    public GameObject newmessageNotify, mainlnotify;
    public Button ChatSection;

    private IEnumerator RefreshRate()
    {
        yield return new WaitForSeconds(0.01f);
        Resize.childControlWidth = false;
        Resize.childControlWidth = true;
    }
    private IEnumerator RefreshRate1()
    {
        yield return new WaitForSeconds(0.01f);
        Resize.childControlWidth = true;
        Resize.childControlWidth = false;
    }
    public void SendMessageToFriend()
    {
        StartCoroutine(ServiceMessage(friendsID));

        // Destroy existing messages to avoid duplicates
        foreach (Transform child in ContentSpace.transform)
        {
            if (child.gameObject.name != "Text (TMP)")
            {
                Destroy(child.gameObject);
            }
        }
    }

    #region Old freiends Chat
    private IEnumerator ServiceMessage(string friendUserId)
    {
        if (string.IsNullOrEmpty(ChatFielf.text))
        {
            // If the message field is empty, load existing messages
            StartCoroutine(LoadChatMessages(friendUserId));
        }
        else
        {
            // Create a new chat entry with user ID, friend ID, and timestamp
            string Chatcolor = ColorChat;
            string Default = "<#FFFFFF>";
            string chatName = "FriendChats";
            string messageKey = $"{user.UserId}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}";
    
            // Save the message in the database under both user and friend nodes
            Task DBTask = DBreference.Child(chatName).Child(user.UserId).Child(messageKey).SetValueAsync(Chatcolor + ChatFielf.text + Default);
            yield return new WaitUntil(() => DBTask.IsCompleted);
    
            // Optionally, you may want to store the same message under the friend's node as well:
            Task friendDBTask = DBreference.Child(chatName).Child(friendUserId).Child(messageKey).SetValueAsync(Chatcolor + ChatFielf.text + Default);
            yield return new WaitUntil(() => friendDBTask.IsCompleted);
    
    
            if (DBTask.Exception != null || friendDBTask.Exception != null)
            {
                Debug.LogWarning($"Failed to register task: {DBTask.Exception ?? friendDBTask.Exception}");
            }
            else
            {
                // Successfully created a new entry
                ChatFielf.text = "";
    
                StartCoroutine(RefreshRate1());
    
                // After sending the message, load chat messages
                StartCoroutine(LoadChatMessages(friendUserId));
            }
        }
    }
    private IEnumerator LoadChatMessages(string friendUserId)
    {
        GlobalChat.text = "";

        // Retrieve the last few messages (fetch more to ensure enough are available)
        Task<DataSnapshot> loadTask = DBreference.Child("FriendChats").Child(friendUserId).LimitToLast(50).GetValueAsync(); // Fetch the last 50 messages
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Exception != null)
        {
            Debug.LogWarning($"Failed to load global chat messages: {loadTask.Exception}");
            yield break;
        }

        DataSnapshot snapshot = loadTask.Result;

        // List to store messages along with their timestamps
        List<KeyValuePair<DateTime, string>> messages = new List<KeyValuePair<DateTime, string>>();

        // Process the messages
        foreach (var childSnapshot in snapshot.Children)
        {
            // Extract senderId and timestamp from the messageKey
            string[] keyParts = childSnapshot.Key.Split('_');
            if (keyParts.Length < 7) continue; // Ensure the key has all parts

            string senderId = keyParts[0];

            // Extract the timestamp in "yyyy_MM_dd_HH_mm_ss"
            string timestampStr = $"{keyParts[1]}_{keyParts[2]}_{keyParts[3]}_{keyParts[4]}_{keyParts[5]}_{keyParts[6]}";

            // Parse the timestamp into a DateTime object
            if (!DateTime.TryParseExact(timestampStr, "yyyy_MM_dd_HH_mm_ss", null, System.Globalization.DateTimeStyles.None, out DateTime timestamp))
            {
                continue; // Skip if parsing fails
            }

            string message = childSnapshot.Value.ToString();

            // Retrieve the sender's username
            Task<DataSnapshot> userTask = DBreference.Child("users").Child(senderId).GetValueAsync();
            yield return new WaitUntil(() => userTask.IsCompleted);

            if (userTask.Exception != null || !userTask.Result.Exists)
            {
                continue; // Skip if failed to retrieve user data
            }

            // Extract the username
            string username = userTask.Result.Child("UserName").Value?.ToString() ?? "Unknown";

            // Format the message as "username: message"
            string formattedMessage = $"{username}: {message}";

            // Store the timestamp and message in the list
            messages.Add(new KeyValuePair<DateTime, string>(timestamp, formattedMessage));
        }

        // Sort the messages by timestamp in ascending order (oldest first)
        messages.Sort((a, b) => a.Key.CompareTo(b.Key));

        // Display the messages in ascending order
        GlobalChat.text = "";
        foreach (var message in messages)
        {
            GlobalChat.text += $"{message.Value}\n";
        }

        // Trim any extra newlines
        GlobalChat.text = GlobalChat.text.Trim();
    }
    //private IEnumerator LoadChatMessages(string friendUserId)
    //{
    //    // Retrieve chat messages between the user and the selected friend from the database
    //    Task<DataSnapshot> loadTask = DBreference.Child("FriendChats").Child(friendUserId).GetValueAsync();
    //    yield return new WaitUntil(() => loadTask.IsCompleted);
    //
    //    if (loadTask.Exception != null)
    //    {
    //        Debug.LogWarning($"Failed to load chat messages: {loadTask.Exception}");
    //        yield break;
    //    }
    //
    //    DataSnapshot snapshot = loadTask.Result;
    //
    //    if (!snapshot.HasChildren)
    //    {
    //        Debug.Log($"No messages found for friendUserId: {friendUserId}");
    //        yield break;
    //    }
    //
    //    // Parse and sort messages by timestamp
    //    List<KeyValuePair<DateTime, KeyValuePair<string, string>>> sortedMessages = new List<KeyValuePair<DateTime, KeyValuePair<string, string>>>();
    //    foreach (var childSnapshot in snapshot.Children)
    //    {
    //        string senderId = childSnapshot.Key;
    //        string message = childSnapshot.Value.ToString();
    //
    //        // Split the senderId based on the first underscore, separating UserId from the timestamp
    //        int firstUnderscoreIndex = senderId.IndexOf('_');
    //        if (firstUnderscoreIndex == -1)
    //        {
    //            continue; // Skip this entry if the format is incorrect
    //        }
    //
    //        string userId = senderId.Substring(0, firstUnderscoreIndex); // Extract UserId
    //        string timestampString = senderId.Substring(firstUnderscoreIndex + 1); // Extract the timestamp part
    //
    //        // Ensure the format string matches the timestamp
    //        DateTime timestamp;
    //        if (!DateTime.TryParseExact(timestampString, "yyyy_MM_dd_HH_mm_ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out timestamp))
    //        {
    //            continue; // Skip this entry if the timestamp is invalid
    //        }
    //
    //        // Add message to sorted list
    //        sortedMessages.Add(new KeyValuePair<DateTime, KeyValuePair<string, string>>(timestamp, new KeyValuePair<string, string>(userId, message)));
    //    }
    //
    //    // Sort messages by timestamp in ascending order
    //    sortedMessages.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));
    //
    //    // Destroy existing messages to avoid duplicates
    //    foreach (Transform child in ContentSpace.transform)
    //    {
    //        if (child.gameObject.name != "Text (TMP)")
    //        {
    //            Destroy(child.gameObject);
    //        }
    //    }
    //
    //    // Spawn messages with correct positioning
    //    foreach (var sortedMessage in sortedMessages)
    //    {
    //        string senderId = sortedMessage.Value.Key;
    //        string message = sortedMessage.Value.Value;
    //
    //        GameObject messagePrefab;
    //
    //        // Determine the sender type and position the message accordingly
    //        if (senderId == user.UserId)
    //        {
    //            messagePrefab = userMessagePrefab; // User's own message on the right
    //            StartCoroutine(LoadAndSpawnMessage(messagePrefab, message, sortedMessage.Key, senderId));
    //        }
    //        else
    //        {
    //            messagePrefab = friendMessagePrefab; // Friend's message on the left
    //            StartCoroutine(LoadAndSpawnMessage(messagePrefab, message, sortedMessage.Key, friendUserId));
    //        }
    //    }
    //}
    //private IEnumerator LoadAndSpawnMessage(GameObject prefab, string message, DateTime timestamp, string userId)
    //{
    //    // Retrieve the image URL from the database for the user
    //    Task<DataSnapshot> imageTask = DBreference.Child("users").Child(userId).Child("imagelinkhere").GetValueAsync();
    //    yield return new WaitUntil(() => imageTask.IsCompleted);
    //
    //    string imageUrl = "";
    //    if (imageTask.Exception != null || !imageTask.Result.Exists)
    //    {
    //        Debug.LogWarning($"Failed to retrieve image URL for user {userId}: {imageTask.Exception}");
    //    }
    //    else
    //    {
    //        imageUrl = imageTask.Result.Value.ToString();
    //    }
    //
    //    // Spawn the message with the image URL only if there's a message
    //    if (!string.IsNullOrEmpty(message))
    //    {
    //        SpawnMessage(prefab, message, timestamp, imageUrl);
    //    }
    //}
    //private void SpawnMessage(GameObject prefab, string message, DateTime timestamp, string imageUrl)
    //{
    //    // Instantiate the message object from the prefab
    //    GameObject messageObject = Instantiate(prefab, ContentSpace.transform);
    //
    //    // Get the text components for message and date/time
    //    TMP_Text messageText = messageObject.GetComponentInChildren<TMP_Text>();
    //    messageText.text = message;
    //
    //    TMP_Text dateTimeText = messageObject.transform.Find("Text_Ago").GetComponent<TMP_Text>();
    //    dateTimeText.text = timestamp.ToString("yyyy-MM-dd HH:mm:ss");
    //
    //    // Check for the prefab name before getting the Image component
    //    if (prefab.name == "chatright")
    //    {
    //        Transform characterTransform = messageObject.transform.Find("CharacterFrame/Character");
    //        if (characterTransform != null)
    //        {
    //            Image friendimg = characterTransform.GetComponent<Image>();
    //
    //            // Set the friend's image if the imageUrl is valid
    //            if (!string.IsNullOrEmpty(imageUrl))
    //            {
    //                StartCoroutine(LoadImageFromUrl(imageUrl, friendimg));
    //            }
    //        }
    //        else
    //        {
    //            Debug.LogWarning("CharacterFrame/Character not found in prefab.");
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Prefab name does not match the expected names.");
    //    }
    //
    //    // Optionally, you can include this if you want to ensure the UI is updated correctly.
    //    StartCoroutine(RefreshRate1());
    //}
    //private IEnumerator LoadImageFromUrl(string url, Image imageComponent)
    //{
    //    using (WWW www = new WWW(url))
    //    {
    //        yield return www;
    //
    //        if (string.IsNullOrEmpty(www.error))
    //        {
    //            Texture2D texture = www.texture;
    //            if (imageComponent != null)
    //            {
    //                imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    //            }
    //
    //        }
    //        else
    //        {
    //            Debug.LogError($"Failed to load image: {www.error}");
    //        }
    //    }
    //}
    #endregion

    [Space]
    public GameObject friendMessagePrefabG;
    public RectTransform ScrollView;
    public void ChooseGlobal()
    {
        GlobalInputField.SetActive(true);

        // Destroy existing messages to avoid duplicates
        // Destroy existing messages except for the object named "book"
        foreach (Transform child in ContentSpace.transform)
        {
            if (child.gameObject.name != "Text (TMP)")
            {
                Destroy(child.gameObject);
            }
        }
        GlobalChat.gameObject.SetActive(true);
        GlobalChat.text = "";
        LoadGlobalmessage();
        friendsID = "Global";
        NameofFriend = "GlobalMessage";
    }
    public void SendMessageToGlobal()
    {
        StartCoroutine(GlobalMessage());
        GlobalChat.text = "";
        LoadGlobalmessage();
        friendsID = "Global";
        NameofFriend = "GlobalMessage";
    }
    public string ColorChat;
    private IEnumerator GlobalMessage()
    {
        if (string.IsNullOrEmpty(ChatFielfG.text))
        {
            // Load existing messages if the chat field is empty

        }
        else
        {
            // Create a new chat entry with user ID and timestamp
            string Chatcolor = ColorChat;
            string Default = "<#FFFFFF>";
            string chatName = "GLOBALCHAT";
            string messageKey = $"{user.UserId}_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}";

            // Save the message in the database under the global chat node
            Task DBTask = DBreference.Child(chatName).Child(messageKey).SetValueAsync(Chatcolor + ChatFielfG.text + Default);
            yield return new WaitUntil(() => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning($"Failed to register task: {DBTask.Exception}");
            }
            else
            {
                // Clear the chat input field
                ChatFielfG.text = "";

                // Load the updated chat messages

            }
        }
    }
    public void LoadGlobalmessage()
    {
        StartCoroutine(LoadGlobalChatMessages());
    }
    public TMP_Text GlobalChat;
    private IEnumerator LoadGlobalChatMessages()
    {
        GlobalChat.text = "";

        // Retrieve the last few messages (fetch more to ensure enough are available)
        Task<DataSnapshot> loadTask = DBreference.Child("GLOBALCHAT").LimitToLast(50).GetValueAsync(); // Fetch the last 50 messages
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Exception != null)
        {
            Debug.LogWarning($"Failed to load global chat messages: {loadTask.Exception}");
            yield break;
        }

        DataSnapshot snapshot = loadTask.Result;

        // List to store messages along with their timestamps
        List<KeyValuePair<DateTime, string>> messages = new List<KeyValuePair<DateTime, string>>();

        // Process the messages
        foreach (var childSnapshot in snapshot.Children)
        {
            // Extract senderId and timestamp from the messageKey
            string[] keyParts = childSnapshot.Key.Split('_');
            if (keyParts.Length < 7) continue; // Ensure the key has all parts

            string senderId = keyParts[0];

            // Extract the timestamp in "yyyy_MM_dd_HH_mm_ss"
            string timestampStr = $"{keyParts[1]}_{keyParts[2]}_{keyParts[3]}_{keyParts[4]}_{keyParts[5]}_{keyParts[6]}";

            // Parse the timestamp into a DateTime object
            if (!DateTime.TryParseExact(timestampStr, "yyyy_MM_dd_HH_mm_ss", null, System.Globalization.DateTimeStyles.None, out DateTime timestamp))
            {
                continue; // Skip if parsing fails
            }

            string message = childSnapshot.Value.ToString();

            // Retrieve the sender's username
            Task<DataSnapshot> userTask = DBreference.Child("users").Child(senderId).GetValueAsync();
            yield return new WaitUntil(() => userTask.IsCompleted);

            if (userTask.Exception != null || !userTask.Result.Exists)
            {
                continue; // Skip if failed to retrieve user data
            }

            // Extract the username
            string username = userTask.Result.Child("UserName").Value?.ToString() ?? "Unknown";

            // Format the message as "username: message"
            string formattedMessage = $"{username}: {message}";

            // Store the timestamp and message in the list
            messages.Add(new KeyValuePair<DateTime, string>(timestamp, formattedMessage));
        }

        // Sort the messages by timestamp in ascending order (oldest first)
        messages.Sort((a, b) => a.Key.CompareTo(b.Key));

        // Display the messages in ascending order
        GlobalChat.text = "";
        foreach (var message in messages)
        {
            GlobalChat.text += $"{message.Value}\n";
        }

        // Trim any extra newlines
        GlobalChat.text = GlobalChat.text.Trim();
    }

    #region Old GlobalChat
    //private IEnumerator LoadGlobalChatMessages()
    //{
    //    yield return 3f;
    //    // Retrieve global chat messages from the database
    //    Task<DataSnapshot> loadTask = DBreference.Child("GLOBALCHAT").GetValueAsync();
    //    yield return new WaitUntil(() => loadTask.IsCompleted);
    //
    //    if (loadTask.Exception != null)
    //    {
    //        Debug.LogWarning($"Failed to load global chat messages: {loadTask.Exception}");
    //        yield break;
    //    }
    //
    //    // Destroy existing messages to avoid duplicates
    //    foreach (Transform child in ContentSpace.transform)
    //    {
    //        Destroy(child.gameObject);
    //    }
    //
    //    DataSnapshot snapshot = loadTask.Result;
    //
    //    // Create a list to hold all the messages with their timestamps for sorting
    //    List<KeyValuePair<string, DataSnapshot>> messageList = new List<KeyValuePair<string, DataSnapshot>>();
    //
    //    // Add all messages to the list
    //    foreach (var childSnapshot in snapshot.Children)
    //    {
    //        messageList.Add(new KeyValuePair<string, DataSnapshot>(childSnapshot.Key, childSnapshot));
    //    }
    //
    //    // Sort the list by the timestamp part of the key (ascending order)
    //    messageList.Sort((pair1, pair2) =>
    //    {
    //        string[] keyParts1 = pair1.Key.Split('_');
    //        string timestampStr1 = $"{keyParts1[1]}_{keyParts1[2]}_{keyParts1[3]}_{keyParts1[4]}_{keyParts1[5]}_{keyParts1[6]}";
    //
    //        string[] keyParts2 = pair2.Key.Split('_');
    //        string timestampStr2 = $"{keyParts2[1]}_{keyParts2[2]}_{keyParts2[3]}_{keyParts2[4]}_{keyParts2[5]}_{keyParts2[6]}";
    //
    //        DateTime timestamp1, timestamp2;
    //        DateTime.TryParseExact(timestampStr1, "yyyy_MM_dd_HH_mm_ss", null, System.Globalization.DateTimeStyles.None, out timestamp1);
    //        DateTime.TryParseExact(timestampStr2, "yyyy_MM_dd_HH_mm_ss", null, System.Globalization.DateTimeStyles.None, out timestamp2);
    //
    //        // Compare timestamps (ascending order)
    //        return timestamp1.CompareTo(timestamp2);
    //    });
    //
    //    // Now spawn the sorted messages
    //    foreach (var messagePair in messageList)
    //    {
    //        string[] keyParts = messagePair.Key.Split('_');
    //        string senderId = keyParts[0];
    //        string timestampStr = $"{keyParts[1]}_{keyParts[2]}_{keyParts[3]}_{keyParts[4]}_{keyParts[5]}_{keyParts[6]}";
    //
    //        DateTime timestamp;
    //        if (!DateTime.TryParseExact(timestampStr, "yyyy_MM_dd_HH_mm_ss", null, System.Globalization.DateTimeStyles.None, out timestamp))
    //        {
    //            Debug.LogWarning($"Failed to parse timestamp: {timestampStr}");
    //            continue;
    //        }
    //
    //        string message = messagePair.Value.Value.ToString();
    //
    //        // Retrieve the sender's username and image URL
    //        Task<DataSnapshot> userTask = DBreference.Child("users").Child(senderId).GetValueAsync();
    //        yield return new WaitUntil(() => userTask.IsCompleted);
    //
    //        if (userTask.Exception != null || !userTask.Result.Exists)
    //        {
    //            Debug.LogWarning($"Failed to retrieve user data for {senderId}: {userTask.Exception}");
    //            continue;
    //        }
    //
    //        string username = userTask.Result.Child("UserName").Value.ToString();
    //        string imageUrl = userTask.Result.Child("imagelinkhere").Value != null ? userTask.Result.Child("imagelinkhere").Value.ToString() : "";
    //
    //        // Spawn the message
    //        GameObject messagePrefab = friendMessagePrefabG;  // Assuming all global messages use the same prefab
    //        SpawnGlobalMessage(messagePrefab, username, message, imageUrl, timestamp);
    //    }
    //}
    //private void SpawnGlobalMessage(GameObject prefab, string username, string message, string imageUrl, DateTime timestamp)
    //{
    //    // Instantiate the message object from the prefab
    //    GameObject messageObject = Instantiate(prefab);
    //
    //    // Set the username text
    //    TMP_Text usernameText = messageObject.transform.Find("UsernameText").GetComponent<TMP_Text>();
    //    usernameText.text = username;  // Ensure this is the correct component
    //
    //    // Set the message text
    //    TMP_Text messageText = messageObject.transform.Find("ChatFrame/Text").GetComponent<TMP_Text>();
    //    messageText.text = message;  // Ensure this is the correct component
    //
    //    // Set the time text
    //    TMP_Text timeText = messageObject.transform.Find("Text_Ago").GetComponent<TMP_Text>();
    //    timeText.text = timestamp.ToString("yyyy-MM-dd HH:mm:ss");
    //
    //    // Set the user image if available
    //    Transform characterTransform = messageObject.transform.Find("CharacterFrame/Character");
    //    if (characterTransform != null && !string.IsNullOrEmpty(imageUrl))
    //    {
    //        Image friendimg = characterTransform.GetComponent<Image>();
    //        StartCoroutine(LoadImageFromUrlG(imageUrl, friendimg));
    //    }
    //    else
    //    {
    //        Debug.LogWarning("CharacterFrame/Character not found or image URL is empty.");
    //    }
    //
    //    // Add the messageObject as the last child of ContentSpace (default behavior)
    //    messageObject.transform.SetParent(ContentSpace.transform, false);
    //
    //    StartCoroutine(RefreshRate1());
    //}
    //private IEnumerator LoadImageFromUrlG(string url, Image imageComponent)
    //{
    //    using (WWW www = new WWW(url))
    //    {
    //        yield return www;
    //
    //        if (string.IsNullOrEmpty(www.error))
    //        {
    //            Texture2D texture = www.texture;
    //            if (imageComponent != null)
    //            {
    //                imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    //            }
    //        }
    //        else
    //        {
    //            Debug.LogError($"Failed to load image: {www.error}");
    //        }
    //    }
    //}
    #endregion

    public void CustomerMessageSendToServer()
    {
        StartCoroutine(ServiceMessage());
    }
    public IEnumerator ServiceMessage()
    {
        // Check if the chat data exists for the user
        Task<DataSnapshot> checkTask = DBreference.Child("users").Child(user.UserId).Child("CustomerChat").GetValueAsync();
        yield return new WaitUntil(() => checkTask.IsCompleted);

        DataSnapshot snapshot = checkTask.Result;

        if (ChatFielfSystem.text == "") // Ensure this name is correct
        {
            // If the chat input field is empty, just load chat messages
            StartCoroutine(LoadChatMessages());
        }
        else
        {
            // Chat data doesn't exist or there's a message to send, create a new entry
            string chatName = "CustomerChat";
            string MyID = "ME_" + user.UserId;
            string mainHolder = $"{chatName}/{MyID}_{DateTime.Now.ToString("_yyyy:MM:dd--HH:mm:ss")}";

            Task DBTask = DBreference.Child("users").Child(user.UserId).Child(mainHolder).SetValueAsync(ChatFielfSystem.text);
            yield return new WaitUntil(() => DBTask.IsCompleted);

            Resize.childControlHeight = false;
            StartCoroutine(RefreshRate1());

            if (DBTask.Exception != null)
            {
                Debug.LogWarning($"Failed to register task: {DBTask.Exception}");
            }
            else
            {
                // Successfully created new entry
                ChatFielfSystem.text = ""; // Clear input field after sending the message

                // After sending the message, load chat messages
                ReciveMessageFromServers();
            }
        }
    }

    public void ReciveMessageFromServers()
    {
        StartCoroutine(LoadChatMessages());
    }
    public IEnumerator LoadChatMessages()
    {
        // Retrieve chat messages from the database
        Task<DataSnapshot> loadTask = DBreference.Child("users").Child(user.UserId).Child("CustomerChat").GetValueAsync();
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Exception != null)
        {
            Debug.LogWarning($"Failed to load chat messages: {loadTask.Exception}");
            yield break;
        }

        // Parse and sort messages by timestamp
        List<KeyValuePair<DateTime, KeyValuePair<string, string>>> sortedMessages = new List<KeyValuePair<DateTime, KeyValuePair<string, string>>>();
        DataSnapshot snapshot = loadTask.Result;
        foreach (var childSnapshot in snapshot.Children)
        {
            string senderId = childSnapshot.Key;
            string message = childSnapshot.Value.ToString();

            // Extract timestamp from senderId
            string[] senderIdParts = senderId.Split('_');
            string timestampString = senderIdParts[3]; // Extract timestamp part
            DateTime timestamp = DateTime.ParseExact(timestampString, "yyyy:MM:dd--HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

            // Add message to sorted list
            sortedMessages.Add(new KeyValuePair<DateTime, KeyValuePair<string, string>>(timestamp, new KeyValuePair<string, string>(senderId, message)));
        }

        // Sort messages by timestamp in ascending order
        sortedMessages.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));

        // Spawn messages
        foreach (var sortedMessage in sortedMessages)
        {
            string senderId = sortedMessage.Value.Key;
            string message = sortedMessage.Value.Value;
            DateTime timestamp = sortedMessage.Key;

            // Determine the sender type (admin or user)
            if (senderId.StartsWith("Admin"))
            {
                SpawnMessage(friendMessagePrefab, message, timestamp);
            }
            else if (senderId.StartsWith("ME"))
            {
                SpawnMessage(userMessagePrefab, message, timestamp);
            }
        }
    }
    private void SpawnMessage(GameObject prefab, string message, DateTime timestamp)
    {
        // Instantiate the message object from the prefab
        GameObject messageObject = Instantiate(prefab, ContentSpace.transform);

        // Get the text components for message and date/time
        TMP_Text messageText = messageObject.GetComponentInChildren<TMP_Text>();
        TMP_Text dateTimeText = messageObject.transform.Find("Text_Ago").GetComponent<TMP_Text>(); // Assuming "Text_Ago" is the name of the GameObject containing the date/time text

        // Set the message text
        messageText.text = message;

        // Set the date/time text
        dateTimeText.text = timestamp.ToString("yyyy-MM-dd HH:mm:ss");

        StartCoroutine(RefreshRate());
    }

    public TMP_Text RecentText;
    private IEnumerator LoadRecentGlobalChatMessage()
    {
        // Retrieve the last few messages (we can fetch more to ensure enough are available)
        Task<DataSnapshot> loadTask = DBreference.Child("GLOBALCHAT").LimitToLast(10).GetValueAsync(); // Fetch the last 10 to ensure we can filter out recent ones
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (loadTask.Exception != null)
        {
            Debug.LogWarning($"Failed to load global chat messages: {loadTask.Exception}");
            yield break;
        }

        DataSnapshot snapshot = loadTask.Result;

        // List to store messages along with their timestamps
        List<KeyValuePair<DateTime, string>> messages = new List<KeyValuePair<DateTime, string>>();

        // Process the messages
        foreach (var childSnapshot in snapshot.Children)
        {
            // Extract senderId and timestamp from the messageKey
            string[] keyParts = childSnapshot.Key.Split('_');
            string senderId = keyParts[0];

            // Extract the timestamp in "yyyy_MM_dd_HH_mm_ss"
            string timestampStr = $"{keyParts[1]}_{keyParts[2]}_{keyParts[3]}_{keyParts[4]}_{keyParts[5]}_{keyParts[6]}";

            // Parse the timestamp into a DateTime object
            DateTime timestamp;
            if (!DateTime.TryParseExact(timestampStr, "yyyy_MM_dd_HH_mm_ss", null, System.Globalization.DateTimeStyles.None, out timestamp))
            {
                continue; // Skip if parsing fails
            }

            // Filter messages by checking the time difference (let's say only show messages within the last 24 hours)
            TimeSpan timeSinceMessage = DateTime.Now - timestamp;
            if (timeSinceMessage.TotalHours > 1)
            {
                continue; // Skip if the message is older than 24 hours
            }

            string message = childSnapshot.Value.ToString();

            // Retrieve the sender's username
            Task<DataSnapshot> userTask = DBreference.Child("users").Child(senderId).GetValueAsync();
            yield return new WaitUntil(() => userTask.IsCompleted);

            if (userTask.Exception != null || !userTask.Result.Exists)
            {
                continue; // Skip if failed to retrieve user data
            }

            // Extract the username
            string username = userTask.Result.Child("UserName").Value.ToString();

            // Format the message as "username: message"
            string formattedMessage = $"{username}: {message}";

            // Store the timestamp and message in the list
            messages.Add(new KeyValuePair<DateTime, string>(timestamp, formattedMessage));
        }

        // Sort the messages by timestamp in descending order (most recent first)
        messages.Sort((a, b) => b.Key.CompareTo(a.Key));

        // Display the two most recent messages
        RecentText.text = "";
        int count = 0;
        foreach (var message in messages)
        {
            if (count >= 2) break; // Only show the two most recent
            RecentText.text += $"{message.Value}\n";
            count++;
        }

        // Trim any extra newlines
        RecentText.text = RecentText.text.Trim();
    }

    #endregion

    #region Profile

    public Image displayImage;
    public Image image;
    public bool ImageBool = true;
    private FirebaseStorage storage;
    public string SaveLocation;
    public Button CustomIMG;

    //load image if there is one
    public void LoadImageURL()
    {
        StartCoroutine(LoadImageUrlFromDatabase());
    }
    private IEnumerator LoadImageUrlFromDatabase()
    {
        if (ImageBool == true)
        {
            // Retrieve the image URL from the database
            var dbTask = DBreference.Child("users").Child(user.UserId).Child("imagelinkhere").GetValueAsync();
            yield return new WaitUntil(() => dbTask.IsCompleted);

            if (dbTask.Exception != null)
            {
                Debug.LogError($"Failed to retrieve image URL from database: {dbTask.Exception}");
            }
            else if (dbTask.Result.Value != null)
            {
                string imageUrl = dbTask.Result.Value.ToString();
                Debug.Log($"Image URL retrieved: {imageUrl}");

                // Load the image from the URL
                StartCoroutine(LoadImageFromUrl(imageUrl));
            }
            else
            {
                Debug.LogWarning("No image URL found in the database.");
            }
        }

    }
    private IEnumerator LoadImageFromUrl(string imageUrl)
    {
        using (WWW www = new WWW(imageUrl))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                Texture2D texture = www.texture;
                displayImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                ImageBool = false;
                Debug.Log("Image loaded successfully!");
            }
            else
            {
                Debug.LogError($"Failed to load image: {www.error}");
            }
        }
    }
    public void UploadImage()
    {
        StartCoroutine(UploadImageCoroutine());
    }
    //Upload image

    private IEnumerator UploadImageCoroutine()
    {
        // Ensure Fullpath is set before proceeding
        if (string.IsNullOrEmpty(LinkImage))
        {
            Debug.LogError("Fullpath is not set. Cannot upload the image.");
            yield break;
        }

        byte[] imageData;

        // Read the image data from disk or Resources
        try
        {
            // Use System.IO to read the file directly from the Resources folder
            string filePath = Path.Combine(Application.dataPath, "Resources", LinkImage + ".png");
            if (!File.Exists(filePath))
            {
                Debug.LogError($"File not found at: {filePath}");
                yield break;
            }

            imageData = File.ReadAllBytes(filePath); // Read image bytes from file
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading image file: {e.Message}");
            yield break;
        }

        // Retrieve the old file name from the database
        string oldFileName = null;
        var databaseRef = FirebaseDatabase.DefaultInstance.GetReference("Users").Child(user.UserId).Child("ProfileImageName");
        var dbTask = databaseRef.GetValueAsync();
        yield return new WaitUntil(() => dbTask.IsCompleted);

        if (dbTask.Exception != null)
        {
            Debug.LogError($"Failed to retrieve old file name: {dbTask.Exception}");
            yield break;
        }

        if (dbTask.Result.Value != null)
        {
            oldFileName = dbTask.Result.Value.ToString();
        }

        // Delete the old file if it exists
        if (!string.IsNullOrEmpty(oldFileName))
        {
            var oldFileRef = storage.GetReference("Profile_images").Child(user.UserId).Child(oldFileName);
            var deleteTask = oldFileRef.DeleteAsync();
            yield return new WaitUntil(() => deleteTask.IsCompleted);

            if (deleteTask.Exception != null)
            {
                Debug.LogError($"Failed to delete old file: {deleteTask.Exception}");
                yield break;
            }

            Debug.Log($"Old file deleted: {oldFileName}");
        }

        // Generate a unique name for the uploaded image
        string imageName = $"Profile_{System.Guid.NewGuid()}.jpg";

        // Get the storage reference for the new image
        var storageRef = storage.GetReference("Profile_images").Child(user.UserId).Child(imageName);

        // Upload the image data
        var uploadTask = storageRef.PutBytesAsync(imageData);
        yield return new WaitUntil(() => uploadTask.IsCompleted);

        if (uploadTask.Exception != null)
        {
            Debug.LogError($"Failed to upload image: {uploadTask.Exception}");
            Loading.SetActive(false); // Disable the loading indicator if set
            yield break;
        }

        // Retrieve the download URL of the uploaded image
        var getUrlTask = storageRef.GetDownloadUrlAsync();
        yield return new WaitUntil(() => getUrlTask.IsCompleted);

        if (getUrlTask.Exception != null)
        {
            Debug.LogError($"Failed to get download URL: {getUrlTask.Exception}");
            Loading.SetActive(false); // Disable the loading indicator
            yield break;
        }

        string imageUrl = getUrlTask.Result.ToString();
        Debug.Log($"Image successfully uploaded. Download URL: {imageUrl}");

        // Update the database with the new file name
        var updateTask = databaseRef.SetValueAsync(imageName);
        yield return new WaitUntil(() => updateTask.IsCompleted);

        if (updateTask.Exception != null)
        {
            Debug.LogError($"Failed to update database with new file name: {updateTask.Exception}");
            yield break;
        }

        Debug.Log($"Database updated with new file name: {imageName}");

        // Notify the user
        ShowNotification("Restart to view changes.");

        // Call a method to handle database updates
        yield return StartCoroutine(HandleDatabaseTasks(imageUrl));
    }



    private IEnumerator HandleDatabaseTasks(string imageUrl)
    {
        // Log the initiation of database task handling
        Debug.Log("Handling database tasks...");

        // Store the download URL in the Realtime Database
        Task DBTask = DBreference.Child("users").Child(user.UserId).Child("imagelinkhere").SetValueAsync(imageUrl);

        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to register task: {DBTask.Exception}");

            // Display failure notification
            ShowNotification("<#6A6A6A><size=35>" + "Attempt failed " + user.DisplayName);
            Loading.SetActive(false);
        }
        else
        {
            Debug.Log("Image URL successfully stored in the database.");
            LoadImageURL();
            Loading.SetActive(false);
        }
    }
    #endregion

    #region  Offline Mode

    public void AImode()
    {
        SceneManager.LoadScene("ChessPlus");
    }

    #endregion

    #region Accept match
    public float MyWage;
    public void AcceptButton()
    {
        if(LobbyPanelController.instance.WageCustom == true)
        {
            if (balance >= LobbyPanelController.instance.CustomCost)
            {
                StartCoroutine(AcceptFee());
            }
            else if (balance < LobbyPanelController.instance.CustomCost)
            {
                GameObject Main = Instantiate(NotifyUser, SpawnPoint.transform);
                TMP_Text TextTopic = Main.transform.Find("Topic").GetComponent<TMP_Text>();
                TextTopic.text = " Non enough credit ";
            }
        }
        else
        {
            if (CreateRoomController.instance.EntryCost >= 1000)
            {
                if (balance >= CreateRoomController.instance.EntryCost)
                {
                    StartCoroutine(AcceptFee());
                }
                else if (balance < CreateRoomController.instance.EntryCost)
                {
                    GameObject Main = Instantiate(NotifyUser, SpawnPoint.transform);
                    TMP_Text TextTopic = Main.transform.Find("Topic").GetComponent<TMP_Text>();
                    TextTopic.text = " Non enough credit ";
                }
            }
            else
            {

                if (balance >= WageCost)
                {
                    StartCoroutine(AcceptFee());
                }
                else if (balance < WageCost)
                {
                    GameObject Main = Instantiate(NotifyUser, SpawnPoint.transform);
                    TMP_Text TextTopic = Main.transform.Find("Topic").GetComponent<TMP_Text>();
                    TextTopic.text = " Non enough credit ";
                }
            }
        }
    }
    public void RefundButton()
    {
        if(MyWage <= 0)
        {

        }
        else if(MyWage >= CreateRoomController.instance.EntryCost)
        {
            StartCoroutine(RefundFee());
        }
    }
    public void SageGame(float value,float profitpergame)
    {
        StartCoroutine(AddedOrLoseWage(value, profitpergame));
        Debug.Log(profitpergame);
    }

    #region TOURNAMENT WIN SYSTEM

    public string Tid, Tname;
    public bool won, INTOURNAMENT;
    private int tournamentRewardPrice;
    private string TID, TNAME;
    public void UpdateTournamentWin()
    {
        string tournamentId = Tid;
        string tournamentName = Tname;
        bool hasWon = won;

        StartCoroutine(UpdateUserMatchStats(tournamentId, tournamentName, hasWon));
    }
    IEnumerator UpdateUserMatchStats(string tournamentId, string tournamentName, bool hasWon)
    {
        TID = tournamentId;
        TNAME = tournamentName;

        yield return new WaitForSeconds(0.3f); // Simulate fetching

        var tournamentRef = DBreference.Child("Tournament").Child(tournamentId).Child(tournamentName);
        Task<DataSnapshot> DBTask = tournamentRef.GetValueAsync();

        DBTask.ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snapshot = task.Result;
                float tournamentGoal = ExtractGoalValue(snapshot.Child("Goals").Value.ToString());
                tournamentRewardPrice = int.Parse(snapshot.Child("RewardPrice").Value.ToString());

                DateTime startTime = DateTime.ParseExact(snapshot.Child("startTime").Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime endTime = DateTime.ParseExact(snapshot.Child("endTime").Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime currentDate = DateTime.Now.Date;

                if (currentDate > endTime.Date)
                {
                    // Check all players for goal completion
                    CheckAllPlayersForGoalCompletion(tournamentRef, tournamentGoal, endTime, tournamentRewardPrice);
                    ShowNotification("Tournament has ended...");
                    EndTournamentMode(TID, TNAME);
                    OnTournamentEnd(TID, TNAME);
                    return;
                }

                if (startTime.Date <= currentDate && endTime.Date >= currentDate)
                {
                    var userRef = tournamentRef.Child("Players").Child(user.UserId);
                    userRef.GetValueAsync().ContinueWith(userTask =>
                    {
                        if (userTask.IsCompleted && !userTask.IsFaulted)
                        {
                            DataSnapshot userSnapshot = userTask.Result;
                            int currentPlayed = int.Parse(userSnapshot.Child("Matchplayed").Value.ToString());
                            int currentWon = int.Parse(userSnapshot.Child("Matchwon").Value.ToString());
                            int currentList = int.Parse(userSnapshot.Child("Matchlist").Value.ToString());
                            
                            if (currentPlayed >= tournamentGoal)
                            {
                                ShowNotification("Player has reached the goal.");
                                return;
                            }

                            currentPlayed++;
                            if (hasWon) currentWon++;
                            currentList++;

                            var updatedStats = new Dictionary<string, object>
                        {
                            { "Matchplayed", currentPlayed },
                            { "Matchwon", currentWon },
                            { "Matchlist", currentList }
                        };

                            userRef.UpdateChildrenAsync(updatedStats).ContinueWith(updateTask =>
                            {
                                if (updateTask.IsCompleted && !updateTask.IsFaulted)
                                {
                                    Debug.Log("Match stats updated successfully.");
                                }
                            });

                            // Check all players for goal completion
                            CheckAllPlayersForGoalCompletion(tournamentRef, tournamentGoal, endTime, tournamentRewardPrice);
                        }
                    });
                }
                else
                {
                    // Check all players for goal completion
                    CheckAllPlayersForGoalCompletion(tournamentRef, tournamentGoal, endTime, tournamentRewardPrice);
                    ShowNotification("Tournament is not active today...");
                }
            }
        });
    }

    // Improved check for all players in the tournament
    private void CheckAllPlayersForGoalCompletion(DatabaseReference tournamentRef, float tournamentGoal, DateTime endTime, int tournamentRewardPrice)
    {
        var playersRef = tournamentRef.Child("Players");

        playersRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snapshot = task.Result;
                string topPlayerId = null;
                string topPlayerName = null;
                int topWins = 0;
                bool goalMet = false;

                foreach (var playerSnapshot in snapshot.Children)
                {
                    string playerId = playerSnapshot.Key;
                    string playerName = playerSnapshot.Child("PlayerName").Value.ToString();
                    int matchWon = int.Parse(playerSnapshot.Child("Matchwon").Value.ToString());

                    if (matchWon >= tournamentGoal)
                    {
                        goalMet = true;
                        SendRewardToPlayer(playerId, playerName, tournamentRewardPrice);
                        UpdateTournamentGoal(tournamentRef, $"{playerName} has won the tournament!");
                        return;
                    }

                    if (matchWon > topWins)
                    {
                        topWins = matchWon;
                        topPlayerId = playerId;
                        topPlayerName = playerName;
                    }
                }

                if (!goalMet && DateTime.Now > endTime)
                {
                    SendRewardToPlayer(topPlayerId, topPlayerName, tournamentRewardPrice);
                    UpdateTournamentGoal(tournamentRef, $"{topPlayerName} has won based on highest wins.");
                }
            }
        });
    }
    // Helper method to send rewards to the player
    private void SendRewardToPlayer(string playerId, string playerName, int rewardAmount)
    {
        var userRef = DBreference.Child("users").Child(playerId).Child("TournamentRewards");

        userRef.GetValueAsync().ContinueWith(userTask =>
        {
            if (userTask.IsCompleted && !userTask.IsFaulted)
            {
                Dictionary<string, object> newReward = new Dictionary<string, object>();
                string rewardKey = $"{playerId}_Reward"; // Unique reward key
                newReward[rewardKey] = rewardAmount; // Using the reward amount passed

                userRef.UpdateChildrenAsync(newReward).ContinueWith(updateTask =>
                {
                    if (updateTask.IsCompleted && !updateTask.IsFaulted)
                    {
                        Debug.Log($"Reward of {rewardAmount} tokens sent to player {playerName} for the tournament.");
                    }
                    else
                    {
                        Debug.LogError($"Failed to send reward to player {playerName}: {updateTask.Exception}");
                    }
                });
            }
            else
            {
                Debug.LogError($"Failed to retrieve reward data for player {playerName}: {userTask.Exception}");
            }
        });
    }
    // Helper method to update the tournament goal status
    private void UpdateTournamentGoal(DatabaseReference tournamentRef, string goalMessage)
    {
        Dictionary<string, object> updatedGoal = new Dictionary<string, object>
    {
        { "Goals", goalMessage }
    };

        tournamentRef.UpdateChildrenAsync(updatedGoal).ContinueWith(goalTask =>
        {
            if (goalTask.IsCompleted && !goalTask.IsFaulted)
            {
                Debug.Log($"Tournament goal updated: {goalMessage}");
            }
            else
            {
                Debug.LogError($"Failed to update tournament goal: {goalTask.Exception}");
            }
        });
    }
    public void EndTournamentMode(string tournamentId, string tournamentName)
    {
        // Get reference to the tournament in the database
        var tournamentRef = DBreference.Child("Tournament").Child(tournamentId).Child(tournamentName);

        // Check if the tournament has already ended by comparing the end time
        tournamentRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snapshot = task.Result;

                // Parse the tournament's end time
                DateTime endTime = DateTime.ParseExact(snapshot.Child("endTime").Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime currentDate = DateTime.Now.Date;

                if (currentDate > endTime.Date)
                {
                    // Tournament has ended, handle winner checks and rewards
                    int tournamentRewardPrice = int.Parse(snapshot.Child("RewardPrice").Value.ToString());
                    float tournamentGoal = ExtractGoalValue(snapshot.Child("Goals").Value.ToString());

                    // Check all players for goal completion and reward them
                    CheckAllPlayersForGoalCompletion(tournamentRef, tournamentGoal, endTime, tournamentRewardPrice);

                    // Send notifications to all players that the tournament has ended
                    string notificationMessage = $"The tournament '{tournamentName}' has ended. Check your rewards!";
                    SendTournamentEndNotification(tournamentId, tournamentName, notificationMessage);

                    // Update tournament status to indicate it has ended
                    UpdateTournamentStatus(tournamentRef, "Ended");

                    // Show notification and then remove the tournament from the database
                    ShowNotification("Tournament has ended.");

                    // Remove the tournament from the database after a delay for any necessary updates
                    StartCoroutine(RemoveTournamentAfterDelay(tournamentRef));
                }
                else
                {
                    ShowNotification("Tournament has not ended yet.");
                }
            }
            else
            {
                Debug.LogError($"Error retrieving tournament data: {task.Exception}");
            }
        });
    }
    // Coroutine to remove the tournament after a short delay
    private IEnumerator RemoveTournamentAfterDelay(DatabaseReference tournamentRef)
    {
        // Wait for 1 second to ensure all updates (like rewards) are completed before deletion
        yield return new WaitForSeconds(1.0f);

        // Remove the tournament from the database
        tournamentRef.RemoveValueAsync().ContinueWith(removeTask =>
        {
            if (removeTask.IsCompleted && !removeTask.IsFaulted)
            {
                Debug.Log("Tournament removed from the database successfully.");
            }
            else
            {
                Debug.LogError($"Failed to remove tournament from the database: {removeTask.Exception}");
            }
        });
    }

    // Function to update the tournament's status
    private void UpdateTournamentStatus(DatabaseReference tournamentRef, string newStatus)
    {
        Dictionary<string, object> updatedStatus = new Dictionary<string, object>
    {
        { "Status", newStatus }
    };

        tournamentRef.UpdateChildrenAsync(updatedStatus).ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log($"Tournament status updated to: {newStatus}");
            }
            else
            {
                Debug.LogError($"Failed to update tournament status: {task.Exception}");
            }
        });
    }
    // Method to extract goal value as a float from the goal string
    public void OnTournamentEnd(string tournamentId, string tournamentName)
    {
        string notificationMessage = tournamentName + " Tournament has ended.";
        SendTournamentEndNotification(tournamentId, tournamentName, notificationMessage);
        print(0000);
    }
    private void SendTournamentEndNotification(string tournamentId, string tournamentName, string notificationMessage)
    {
        // Get a reference to the tournament's players
        var playersRef = DBreference.Child("Tournament").Child(tournamentId).Child(tournamentName).Child("Players");

        playersRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snapshot = task.Result;
                var notificationBatch = new Dictionary<string, object>();

                // Iterate through all players in the tournament
                foreach (var playerSnapshot in snapshot.Children)
                {
                    string playerId = playerSnapshot.Key;
                    var notificationRef = DBreference.Child("users").Child(playerId).Child("Notifications");

                    // Create a unique key for the notification using timestamp or unique identifier
                    string notificationKey = $"{playerId}_{DateTime.UtcNow.Ticks}_Notification";

                    // Prepare the notification message
                    notificationBatch[$"/users/{playerId}/Notifications/{notificationKey}"] = notificationMessage;
                    notificationBatch[$"/users/{playerId}/intournament"] = "false";

                    Debug.Log($"Queued notification for player {playerId}");
                }

                // Send the notifications in a batch operation
                DBreference.UpdateChildrenAsync(notificationBatch).ContinueWith(updateTask =>
                {
                    if (updateTask.IsCompleted && !updateTask.IsFaulted)
                    {
                        Debug.Log("Notifications sent to all tournament players.");
                    }
                    else
                    {
                        Debug.LogError($"Error sending notifications: {updateTask.Exception}");
                    }
                });
            }
            else
            {
                Debug.LogError("Error fetching players to send notifications.");
            }
        });
    }
    private float ExtractGoalValue(string goalString)
    {
        // Assuming the goal string is in the format: "Complete all 15"
        string[] parts = goalString.Split(' ');
        if (parts.Length > 2 && float.TryParse(parts[2], out float goalValue))
        {
            return goalValue;
        }

        // Return a default value if extraction fails
        return 0f;
    }


    #endregion

    private IEnumerator AddedOrLoseWage(float value, float profitpergame)
    {
        //Update the cash when Complete a match
        ShowNotification("Adding new balance .");

        float newbalance = balance += value;
        float currentRating = StartUP.instance.rating;
        currentRating += VictoryManager.instance.rating;
        string currentTire = StartUP.instance.TireDisplay.text;
        float currentWonmatchs = StartUP.instance.MatchWon;
        currentWonmatchs += VictoryManager.instance.Win;
        float allPlay = StartUP.instance.AllMatchPlayed;
        allPlay += 1;
        float currentlostmatch = StartUP.instance.LostMatch;
        currentlostmatch += VictoryManager.instance.lostmatch;


        yield return 0.5f;

        Task DBTask = DBreference.Child("users").Child(user.UserId).Child("NGN").SetValueAsync(newbalance);
        Task DBTask1 = DBreference.Child("users").Child(user.UserId).Child("MatchPlayed").SetValueAsync(allPlay);
        Task DBTask2 = DBreference.Child("users").Child(user.UserId).Child("Ranking").SetValueAsync(currentWonmatchs);
        Task DBTask3 = DBreference.Child("users").Child(user.UserId).Child("Tire").SetValueAsync(currentTire);
        Task DBTask4 = DBreference.Child("users").Child(user.UserId).Child("Rating").SetValueAsync(currentRating);

        Task DBTask5 = DBreference.Child("users").Child(user.UserId).Child("MatchLost").SetValueAsync(currentlostmatch);

        ////////////////////////////////////////Store To Admin/////////////////////////////////////////////


        Task DBTask6 = DBreference.Child("admin").Child(AdminID).Child("ProfitPerGame").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    // If the value exists, retrieve it
                    float currentProfitPerGame = float.Parse(snapshot.Value.ToString());
                    Debug.Log("Current ProfitPerGame: " + currentProfitPerGame);

                    float Totalamount = currentProfitPerGame + profitpergame;

                    // Now, update the existing value
                    Task updateTask = DBreference.Child("admin").Child(AdminID).Child("ProfitPerGame").SetValueAsync(Totalamount).ContinueWith(update =>
                    {
                        if (update.IsCompleted)
                        {
                            Debug.Log("ProfitPerGame updated to: " + profitpergame);
                        }
                        else
                        {
                            Debug.LogError("Failed to update ProfitPerGame.");
                        }
                    });
                }
                else
                {
                    // If the value doesn't exist, insert new data
                    //Debug.Log("ProfitPerGame does not exist. Inserting new value.");
                    Task insertTask = DBreference.Child("admin").Child(AdminID).Child("ProfitPerGame").SetValueAsync(profitpergame).ContinueWith(insert =>
                    {
                        if (insert.IsCompleted)
                        {
                            Debug.Log("New ProfitPerGame value inserted: " + profitpergame);
                        }
                        else
                        {
                            Debug.LogError("Failed to insert new ProfitPerGame value.");
                        }
                    });
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve ProfitPerGame from the database.");
            }
        });

        yield return new WaitUntil(predicate: () => DBTask4.IsCompleted);

        if (DBTask4.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask4.Exception}");
        }
        else
        {
            //Database username is now updated
            LobbyPanelController.instance.YesButton.interactable = true;
            LobbyPanelController.instance.Refund.interactable = false;
            MyWage = 0;
            VictoryManager.instance.Win = 0;
            VictoryManager.instance.rating = 0;
        }

    }//___________________________________Store Wins__________________________
    private IEnumerator AcceptFee()
    {
        //Update the cash when accepting 
        ShowNotification("Loading...");

        balance -= CreateRoomController.instance.EntryCost;

        yield return 0.5f;

        Task DBTask = DBreference.Child("users").Child(user.UserId).Child("NGN").SetValueAsync(balance);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
            LobbyPanelController.instance.ReadyOrStartggame();
            LobbyPanelController.instance.YesButton.interactable = false;
            LobbyPanelController.instance.Refund.interactable = true;

            if (LobbyPanelController.instance.WageCustom == true)
            {
                MyWage = LobbyPanelController.instance.CustomCost;
            }
            else
            {
                if (CreateRoomController.instance.EntryCost >= 1000)
                {
                    MyWage = CreateRoomController.instance.EntryCost;
                }
                else
                {
                    MyWage = WageCost;
                }
            }
           
            
        }
    }
    private IEnumerator RefundFee()
    {
        //Update the cash when accepting 
        ShowNotification("Refunding fee... ");

        balance += CreateRoomController.instance.EntryCost;

        yield return 0.5f;

        Task DBTask = DBreference.Child("users").Child(user.UserId).Child("NGN").SetValueAsync(balance);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
            RoomPanelController.instance.OnClickBackButton();
            LobbyPanelController.instance.YesButton.interactable = true;
            LobbyPanelController.instance.Refund.interactable = false;
            MyWage = 0;
            CreateRoomController.instance.EntryCost = 0;
        }

    }


    public GameObject FriendsINVSPrefab;
    public Transform Contents,loadPointMatch;
    public void InviteFriends()
    {
        StartCoroutine(LoadFriendlistINVS());
    }
    public void InviteFriendsAll()
    {
        StartCoroutine(LoadAllUsers());
    }
    private IEnumerator LoadFriendlistINVS()
    {
        Loading.SetActive(true);

        // Replace with the current user's ID
        string currentUserId = user.UserId;

        // Get the list of friends for the current user
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(currentUserId).Child("Friends").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve friends list: {DBTask.Exception}");
            Loading.SetActive(false);
            yield break;
        }

        DataSnapshot snapshot = DBTask.Result;

        // Clear existing entries except for the ones named "Tab_Opions1"
        foreach (Transform child in Contents.transform)
        {
            if (child.name != "Tab_Opions1")
            {
                Destroy(child.gameObject);
            }
        }

        if (snapshot.Exists)
        {
            foreach (DataSnapshot friendSnapshot in snapshot.Children)
            {
                string friendId = friendSnapshot.Key; // This is the friend's user ID key

                // Get the friend's username and image URL
                Task<DataSnapshot> friendInfoTask = DBreference.Child("users").Child(friendId).Child("UserName").GetValueAsync();
                Task<DataSnapshot> friendInfoTask1 = DBreference.Child("users").Child(friendId).Child("Rating").GetValueAsync();
                Task<DataSnapshot> friendInfoTask2 = DBreference.Child("users").Child(friendId).Child("imagelinkhere").GetValueAsync();
                yield return new WaitUntil(() => friendInfoTask.IsCompleted && friendInfoTask1.IsCompleted && friendInfoTask2.IsCompleted);

                if (friendInfoTask.Exception != null)
                {
                    Debug.LogWarning($"Failed to retrieve friend username: {friendInfoTask.Exception}");
                    continue;
                }

                if (friendInfoTask1.Exception != null)
                {
                    Debug.LogWarning($"Failed to retrieve friend image URL: {friendInfoTask1.Exception}");
                    continue;
                }

                if (friendInfoTask2.Exception != null)
                {
                    Debug.LogWarning($"Failed to retrieve friend image URL: {friendInfoTask2.Exception}");
                    continue;
                }

                string friendUsername = friendInfoTask.Result.Value != null ? friendInfoTask.Result.Value.ToString() : "Unknown User";
                string friendTire = friendInfoTask1.Result.Value != null ? friendInfoTask1.Result.Value.ToString() : "Unknown Tire";
                string friendImageUrl = friendInfoTask2.Result.Value != null ? friendInfoTask2.Result.Value.ToString() : "";

                // Instantiate the prefab for the friend
                GameObject friendEntry = Instantiate(FriendsINVSPrefab, Contents);

                // Set the friend's username in the UI
                TMP_Text nameText = friendEntry.transform.Find("Name").GetComponent<TMP_Text>();
                nameText.text = friendUsername;
                TMP_Text nameTire = friendEntry.transform.Find("Tire").GetComponent<TMP_Text>();
                nameTire.text = friendTire;

                if (!string.IsNullOrEmpty(friendImageUrl))
                {
                    // Load the friend's profile image
                    using (WWW www = new WWW(friendImageUrl))
                    {
                        yield return www;

                        if (string.IsNullOrEmpty(www.error))
                        {
                            Texture2D texture = www.texture;
                            Image iconImage = friendEntry.transform.Find("Profile/Image").GetComponent<Image>();
                            iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                            //Debug.Log("Image loaded successfully!");
                        }
                        else
                        {
                            Debug.LogError($"Failed to load image: {www.error}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Friend image URL is empty or null.");
                }

                // Store the friend's user ID key in a string
                string friendsIDkey = friendSnapshot.Key;

                Button sel = friendEntry.transform.Find("inviteButton").GetComponent<Button>();

                sel.onClick.AddListener(() => InviteThem(friendsIDkey));

                // You can now use friendsIDkey for further operations (e.g., storing, accessing, etc.)
            }
        }
        else
        {
            ShowNotification("No friends.");
        }

        Loading.SetActive(false);
    }
    public TMP_InputField searchInputField;  
    private IEnumerator LoadAllUsers()
    {
        Loading.SetActive(true);

        // Get the search query from the input field
        string searchQuery = searchInputField.text.Trim().ToLower();  // Convert to lowercase for case-insensitive search

        // Get all users from the database
        Task<DataSnapshot> DBTask = DBreference.Child("users").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve users list: {DBTask.Exception}");
            Loading.SetActive(false);
            yield break;
        }

        DataSnapshot snapshot = DBTask.Result;

        // Clear existing entries except for the ones named "Tab_Opions1"
        foreach (Transform child in Contents.transform)
        {
            if (child.name != "Tab_Opions1")
            {
                Destroy(child.gameObject);
            }
        }

        if (snapshot.Exists)
        {
            bool foundUsers = false; // To check if any user matches the search query

            foreach (DataSnapshot userSnapshot in snapshot.Children)
            {
                string userId = userSnapshot.Key; // This is the user's ID key

                // Get the user's username and image URL
                Task<DataSnapshot> userInfoTask = DBreference.Child("users").Child(userId).Child("UserName").GetValueAsync();
                Task<DataSnapshot> userInfoTask1 = DBreference.Child("users").Child(userId).Child("Rating").GetValueAsync();
                Task<DataSnapshot> userInfoTask2 = DBreference.Child("users").Child(userId).Child("imagelinkhere").GetValueAsync();
                yield return new WaitUntil(() => userInfoTask.IsCompleted && userInfoTask1.IsCompleted && userInfoTask2.IsCompleted);

                if (userInfoTask.Exception != null || userInfoTask1.Exception != null || userInfoTask2.Exception != null)
                {
                    Debug.LogWarning($"Failed to retrieve user info: {userId}");
                    continue;
                }

                string username = userInfoTask.Result.Value != null ? userInfoTask.Result.Value.ToString() : "Unknown User";
                string userTire = userInfoTask1.Result.Value != null ? userInfoTask1.Result.Value.ToString() : "Unknown Tire";
                string userImageUrl = userInfoTask2.Result.Value != null ? userInfoTask2.Result.Value.ToString() : "";

                // Check if the username contains the search query
                if (!string.IsNullOrEmpty(searchQuery) && !username.ToLower().Contains(searchQuery))
                {
                    continue; // Skip users that do not match the search query
                }

                foundUsers = true;

                // Instantiate the prefab for the user
                GameObject userEntry = Instantiate(FriendsINVSPrefab, Contents);

                // Set the user's username in the UI
                TMP_Text nameText = userEntry.transform.Find("Name").GetComponent<TMP_Text>();
                nameText.text = username;
                TMP_Text nameTire = userEntry.transform.Find("Tire").GetComponent<TMP_Text>();
                nameTire.text = userTire;

                if (!string.IsNullOrEmpty(userImageUrl))
                {
                    // Load the user's profile image
                    using (WWW www = new WWW(userImageUrl))
                    {
                        yield return www;

                        if (string.IsNullOrEmpty(www.error))
                        {
                            Texture2D texture = www.texture;
                            Image iconImage = userEntry.transform.Find("Profile/Image").GetComponent<Image>();
                            iconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        }
                        else
                        {
                            Debug.LogError($"Failed to load image: {www.error}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("User image URL is empty or null.");
                }

                // Store the user's ID key in a string
                string userIDkey = userSnapshot.Key;

                // Disable the invite button for the current user
                if (userIDkey == user.UserId)
                {
                    Button inviteButton1 = userEntry.transform.Find("inviteButton").GetComponent<Button>();
                    inviteButton1.interactable = false;
                }
                else
                {
                    Button inviteButton = userEntry.transform.Find("inviteButton").GetComponent<Button>();
                    inviteButton.onClick.AddListener(() => InviteThem(userIDkey));
                }
            }

            // If no users match the search query
            if (!foundUsers)
            {
                ShowNotification("No users found matching the search.");
            }
        }
        else
        {
            ShowNotification("No users found.");
        }

        Loading.SetActive(false);
    }


    public void InviteThem(string friendIDkey)
    {
        string theirId = friendIDkey.ToString();
        string Message = RoomPanelController.instance.RooMName;

        Task DBTask1 = DBreference.Child("users").Child(friendIDkey).Child("InviteToGame").Child("Rooms").SetValueAsync(Message);
        ShowNotification("Invite sent\nwaiting for opponent");
    }

    public GameObject Battleprefab;

    private IEnumerator BattleInvite()
    {
        Loading.SetActive(true);

        // Retrieve the friend's request list from Firebase
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(user.UserId).Child("InviteToGame").GetValueAsync();

        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve friend request list: {DBTask.Exception}");
            Loading.SetActive(false);
            yield break;
        }

        // Data has been retrieved
        DataSnapshot snapshot = DBTask.Result;

        // Destroy any existing friend request entries
        foreach (Transform child in loadPointMatch.transform)
        {
            Destroy(child.gameObject);
        }

        if (snapshot.Exists)
        {
            foreach (DataSnapshot requestSnapshot in snapshot.Children)
            {
                string message = requestSnapshot.Value.ToString();

                // Instantiate a new friend request entry
                GameObject requestEntry = Instantiate(Battleprefab, loadPointMatch);

                // Find and assign the TMP_Text component for displaying the request message
                TMP_Text messageText = requestEntry.transform.Find("Contents/Text_Section").GetComponent<TMP_Text>();
                messageText.text = message;

                // Find and assign the Yes and No buttons
                Button yesButton = requestEntry.transform.Find("Contents/Yes").GetComponent<Button>();
                Button noButton = requestEntry.transform.Find("Contents/No").GetComponent<Button>();

                // Assign the HandleRequest method to the buttons
                yesButton.onClick.AddListener(() => StartCoroutine(HandleAccept(message)));
                noButton.onClick.AddListener(() => StartCoroutine(HandleDecline()));
            }
        }

        Loading.SetActive(false);
    }
    public IEnumerator HandleAccept(string message)
    {
        yield return 0.1f;
        ShowNotification("Attempting to join room: " + message);
        PhotonNetwork.JoinRoom(message);
        StartCoroutine(HandleDecline());
    }
    public IEnumerator HandleDecline()
    {
        Loading.SetActive(true);

        // Start the task to remove the value
        Task DBTask2 = DBreference.Child("users").Child(user.UserId).Child("InviteToGame").Child("Rooms").RemoveValueAsync();

        // Wait for the task to complete
        yield return new WaitUntil(() => DBTask2.IsCompleted);

        if (DBTask2.Exception != null)
        {
            // Handle any exceptions (e.g., log the error)
            Debug.LogWarning($"Failed to remove the room: {DBTask2.Exception}");
            GameObject invitePanel = GameObject.Find("Match Invite(Clone)");

            if (invitePanel != null)
            {
                Destroy(invitePanel);
            }
        }
        else
        {
            // Successfully removed the room
            Debug.Log("Room successfully removed");
            GameObject invitePanel = GameObject.Find("Match Invite(Clone)");

            if (invitePanel != null)
            {
                Destroy(invitePanel);
            }
        }

        Loading.SetActive(false);
    }
    
    #endregion

    #region TournamentSystem
    public Button TournamentButton;
    public TMP_InputField TournamentNameInput;
    public TMP_InputField TournamentJoinedCost;
    public TMP_InputField TournamentWinnerPrice;
    public TMP_Dropdown TournamentGoals;
    public TMP_Text TournamentStartTime;
    public TMP_Text TournamentEndTime;
    public TMP_InputField TournamentStatus;
    public TMP_InputField TournamentKey;

    
    public bool checkJoinButton;

    // Tournament System
    public void HostSystem()
    {
        StartCoroutine(HostTournament());
    }
    IEnumerator HostTournament()
    {
        yield return new WaitForSeconds(0.2f);  // Wait for a short delay

        // Validate input fields
        if (string.IsNullOrEmpty(TournamentNameInput.text) ||
            string.IsNullOrEmpty(TournamentJoinedCost.text) ||
            string.IsNullOrEmpty(TournamentWinnerPrice.text))
        {
            ShowNotification("Please fill in the tournament name, cost, and reward price.");
            yield break; // Exit the coroutine
        }

        DateTime today = DateTime.Now; // Current date
        DateTime inputStartDate, inputEndDate;

        // Validate dates (expecting dd/MM/yyyy format)
        if (!IsValidDate(TournamentStartTime.text, out inputStartDate))
        {
            ShowNotification("Please provide a valid start date (dd/MM/yyyy).");
            yield break; // Exit the coroutine
        }

        if (!IsValidDate(TournamentEndTime.text, out inputEndDate))
        {
            ShowNotification("Please provide a valid end date (dd/MM/yyyy).");
            yield break; // Exit the coroutine
        }

        // Check if the start date is not less than the current date
        if (inputStartDate < today)
        {
            ShowNotification("Start date cannot be in the past.");
            yield break; // Exit the coroutine
        }

        // Check if the end date is greater than the start date
        if (inputEndDate <= inputStartDate)
        {
            ShowNotification("End date must be after the start date.");
            yield break; // Exit the coroutine
        }

        Debug.Log("All validations passed, creating tournament...");

        string tournamentPath = "Tournament";
        var tournamentData = new Dictionary<string, object>
    {
        { "Players", "" },
        { "Maxplayer", 10 },
        { "name", TournamentNameInput.text },
        { "cost", TournamentJoinedCost.text },
        { "RewardPrice", TournamentWinnerPrice.text },
        { "Goals", TournamentGoals.options[TournamentGoals.value].text },
        { "startTime", TournamentStartTime.text },
        { "endTime", TournamentEndTime.text },
        { "status", TournamentStatus.text }
    };

        float Rewardprice = float.Parse(TournamentWinnerPrice.text);
        float totalcost = tournamentcost + Rewardprice;

        if (balance >= totalcost)
        {
            // Check if a tournament with the same name already exists globally
            Task<DataSnapshot> tournamentNameCheckTask = DBreference.Child(tournamentPath).OrderByChild("name").EqualTo(TournamentNameInput.text).GetValueAsync();
            yield return new WaitUntil(() => tournamentNameCheckTask.IsCompleted);

            if (tournamentNameCheckTask.Exception != null)
            {
                Debug.LogWarning($"Failed to check tournament name: {tournamentNameCheckTask.Exception}");
                yield break; // Exit the coroutine if there's an error
            }

            // If the tournament already exists, notify the user
            if (tournamentNameCheckTask.Result.Exists)
            {
                ShowNotification("A tournament with this name already exists. Please choose a different name.");
                yield break; // Exit the coroutine if the name already exists
            }

            // Save the tournament data in Firebase if no existing tournament with the same name was found
            Task createTournamentTask = DBreference.Child(tournamentPath).Child(user.UserId).Child(TournamentNameInput.text).SetValueAsync(tournamentData);
            yield return new WaitUntil(() => createTournamentTask.IsCompleted);

            if (createTournamentTask.Exception == null)
            {
                ShowNotification("Tournament created successfully!");
                balance -= totalcost;
                yield return new WaitForSeconds(0.1f); // Slight delay before updating balance
                StartCoroutine(UpdateBalance());
            }
            else
            {
                Debug.LogWarning($"Failed to create tournament: {createTournamentTask.Exception}");
            }
        }
        else
        {
            ShowNotification("Not enough credit to host this tournament!");
        }
    }
    // Helper method to validate dates in dd/MM/yyyy format
    bool IsValidDate(string dateStr, out DateTime date)
    {
        string format = "dd/MM/yyyy";
        return DateTime.TryParseExact(dateStr, format, null, System.Globalization.DateTimeStyles.None, out date);
    }
    
    public GameObject tournamentItemPrefab; 
    public Transform tournamentContentPanel;
    public void LoadTournaments()
    {
        StartCoroutine(LoadTournamentData());
        foreach (Transform child in tournamentContentPanel)
        {
            if (child.name != "Tournament")
            {
                Destroy(child.gameObject);
            }
        }
    }

    [Header("Tournament Chest")]
    public GameObject PostChest;
    public TMP_Text NameChest;
    public Slider sliderChest;
    public IEnumerator LoadTournamentData()
    {
        // Retrieve all tournaments from Firebase
        Task<DataSnapshot> DBTask = DBreference.Child("Tournament").GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);
    
        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve tournaments: {DBTask.Exception}");
            yield break; // Stop execution in case of failure
        }
    
        if (DBTask.Result.Value == null)
        {
            ShowNotification("No tournaments found.");
            yield break; // No tournaments exist, so exit early
        }
    
        DataSnapshot snapshot = DBTask.Result;
    
        // Clear the scroll view content before loading new tournaments
        foreach (Transform child in tournamentContentPanel)
        {
            if (child.name != "Tournament") // Keep the template intact
            {
                Destroy(child.gameObject);
            }
        }
    
        bool userInTournament = false; // Flag to track if the user is found in any tournament
        string foundCreatorID = "";
        string foundTournamentName = "";
    
        // Loop through all creator IDs in the Tournament node
        foreach (DataSnapshot creatorSnapshot in snapshot.Children)
        {
            string creatorID = creatorSnapshot.Key; // Get the creator ID
    
            // Loop through all tournaments under each creator ID
            foreach (DataSnapshot tournamentSnapshot in creatorSnapshot.Children)
            {
                string tournamentName = tournamentSnapshot.Key; // Get the tournament name
    
                // Ensure all data is retrieved safely
                string tournamentGoals = tournamentSnapshot.HasChild("Goals") ? tournamentSnapshot.Child("Goals").Value.ToString() : "No goals specified";
                string tournamentRewardPrice = tournamentSnapshot.HasChild("RewardPrice") ? tournamentSnapshot.Child("RewardPrice").Value.ToString() : "0";
                string tournamentCost = tournamentSnapshot.HasChild("cost") ? tournamentSnapshot.Child("cost").Value.ToString() : "0";
                string tournamentEndTime = tournamentSnapshot.HasChild("endTime") ? tournamentSnapshot.Child("endTime").Value.ToString() : "Unknown end time";
                string tournamentStartTime = tournamentSnapshot.HasChild("startTime") ? tournamentSnapshot.Child("startTime").Value.ToString() : "Unknown start time";
                string tournamentStatus = tournamentSnapshot.HasChild("status") ? tournamentSnapshot.Child("status").Value.ToString() : "Pending";
                string tournamentMaxplayer = tournamentSnapshot.HasChild("Maxplayer") ? tournamentSnapshot.Child("Maxplayer").Value.ToString() : "10"; // Default max players is 10
    
                // Instantiate a new TournamentItemPrefab for each tournament
                GameObject tournamentItem = Instantiate(tournamentItemPrefab, tournamentContentPanel);
    
                // Find and set tournament info in the instantiated prefab
                TMP_Text nameText = tournamentItem.transform.Find("MainMenu/Topic/TournamentNameText").GetComponent<TMP_Text>();
                TMP_Text costText = tournamentItem.transform.Find("MainMenu/TournamentButton/TournamentCostText").GetComponent<TMP_Text>();
                Button clickjoin = tournamentItem.transform.Find("MainMenu/TournamentButton").GetComponent<Button>();
    
                // Set other tournament details
                TMP_Text statusText = tournamentItem.transform.Find("MainMenu/TournamentStatusText").GetComponent<TMP_Text>();
                TMP_Text goalsText = tournamentItem.transform.Find("MainMenu/TournamentGoalsText").GetComponent<TMP_Text>();
                TMP_Text rewardText = tournamentItem.transform.Find("MainMenu/Topic/TournamentRewardText").GetComponent<TMP_Text>();
                TMP_Text endTimeText = tournamentItem.transform.Find("MainMenu/Topic/TournamentEndTimeText").GetComponent<TMP_Text>();
                TMP_Text startTimeText = tournamentItem.transform.Find("MainMenu/Topic/TournamentStartTimeText").GetComponent<TMP_Text>();
    
                // Set the text fields with retrieved data
                nameText.text = tournamentName;
                goalsText.text = tournamentGoals;
                rewardText.text = "NGN " + tournamentRewardPrice;
                endTimeText.text = "End date: " + tournamentEndTime;
                startTimeText.text = "Host date: " + tournamentStartTime;
    
                // Remove any existing listeners to avoid duplicates
                clickjoin.onClick.RemoveAllListeners();
    
                // Get the number of players who have joined this tournament
                Task<DataSnapshot> playersCountTask = DBreference.Child("Tournament")
                    .Child(creatorID)
                    .Child(tournamentName)
                    .Child("Players")
                    .GetValueAsync();
    
                yield return new WaitUntil(() => playersCountTask.IsCompleted);
    
                if (playersCountTask.Exception != null)
                {
                    Debug.LogWarning($"Failed to get player count: {playersCountTask.Exception}");
                }
                else
                {
                    // Get the current number of players in the tournament
                    int currentPlayerCount = 0;
                    if (playersCountTask.Result != null && playersCountTask.Result.HasChildren)
                    {
                        currentPlayerCount = (int)playersCountTask.Result.ChildrenCount;
                        sliderChest.value = currentPlayerCount;
                    }
    
                    // Update the status text to display the number of players joined out of the max players
                    statusText.text = $"{currentPlayerCount}/{tournamentMaxplayer}";
                    sliderChest.maxValue = float.Parse(tournamentMaxplayer);
    
                    // Check if the tournament is full
                    if (currentPlayerCount >= int.Parse(tournamentMaxplayer))
                    {
                        // Tournament is full, set status to "Closed" and disable the join button
                        statusText.text = "Closed";
                        clickjoin.interactable = false; // Disable the join button
                    }
                    else
                    {
                        // Tournament is not full, allow interaction based on user's status
                        if (creatorID == user.UserId)
                        {
                            // User is the tournament creator
                            costText.text = "Manage Tournament";
                            clickjoin.onClick.AddListener(() => ManageTournament(creatorID));
                        }
                        else
                        {
                            // Check if the player has already joined the tournament
                            Task<DataSnapshot> playerCheckTask = DBreference.Child("Tournament")
                                .Child(creatorID)
                                .Child(tournamentName)
                                .Child("Players")
                                .Child(user.UserId).GetValueAsync();
    
                            yield return new WaitUntil(() => playerCheckTask.IsCompleted);
    
                            if (playerCheckTask.Exception != null)
                            {
                                Debug.LogWarning($"Failed to check if player joined: {playerCheckTask.Exception}");
                            }
                            else if (playerCheckTask.Result.Value != null)
                            {
                                // Player has already joined the tournament
                                costText.text = "Enter";
                                clickjoin.onClick.AddListener(() => EnterTournament(creatorID));
    
                                // Mark that the user is found in this tournament
                                userInTournament = true;
                                foundCreatorID = creatorID;
                                foundTournamentName = tournamentName;
                            }
                            else
                            {
                                // Player has not joined, show the cost
                                costText.text = "NGN " + tournamentCost;
                                clickjoin.onClick.AddListener(() => StartCoroutine(JoinTournament(creatorID, tournamentName, tournamentCost)));
                            }
                        }
                    }
                }
            }
        }

        // After loading all tournaments, check if the user was found in any
        if (userInTournament)
        {
            Debug.Log($"User is in the tournament: {foundTournamentName} by {foundCreatorID}");

            // Display or use the tournament properties as needed
            Tid = foundCreatorID;
            Tname = foundTournamentName;
            INTOURNAMENT = true;
            PostChest.SetActive(true);
            NameChest.text = foundTournamentName;

            // Retrieve tournament details to check date
            DataSnapshot tournamentSnapshot = snapshot.Child(foundCreatorID).Child(foundTournamentName);
            float tournamentGoal = ExtractGoalValue(tournamentSnapshot.Child("Goals").Value.ToString());
            int tournamentRewardPrice = int.Parse(tournamentSnapshot.Child("RewardPrice").Value.ToString());
            DateTime startTime = DateTime.ParseExact(tournamentSnapshot.Child("startTime").Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime endTime = DateTime.ParseExact(tournamentSnapshot.Child("endTime").Value.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime currentDate = DateTime.Now.Date;

            if (currentDate > endTime.Date)
            {
                // Check all players for goal completion
                CheckAllPlayersForGoalCompletion(DBreference.Child("Tournament").Child(foundCreatorID).Child(foundTournamentName), tournamentGoal, endTime, tournamentRewardPrice);
                ShowNotification("Tournament has ended.");
            }

            if (startTime.Date <= currentDate && endTime.Date >= currentDate)
            {
                ShowNotification("Did you forget tournament!");
            }
        }
        else
        {
            Debug.Log("User is not in any tournament room.");
            PostChest.SetActive(false);
            INTOURNAMENT = false;
        }

    }

    private IEnumerator JoinTournament(string creatorID, string tournamentName, string cost)
    {
        Debug.Log("Checking if user is already in the tournament...");
        Debug.Log($"{creatorID}_{tournamentName}_{cost}");

        // Show a notification to the user
        ShowNotification("Joining...");

        // Check if the user is already in the tournament directly in the "Tournament" node
        Task<DataSnapshot> tournamentTask = DBreference.Child("Tournament")
            .Child(creatorID)
            .Child(tournamentName)
            .Child("Players")
            .Child(user.UserId)
            .GetValueAsync();

        yield return new WaitUntil(() => tournamentTask.IsCompleted);

        if (tournamentTask.Result.Value != null)
        {
            ShowNotification("You are already in this tournament.");
            yield break; // Exit the method if the user is already in the tournament
        }

        // Check if the user has enough balance
        if (balance >= float.Parse(cost))
        {
            // Deduct the tournament cost from the user's balance
            balance -= float.Parse(cost);
            yield return 0.2f;
            StartCoroutine(UpdateBalance());
            Debug.Log($"User's new balance: {balance}");

            // Create a Dictionary to store player information
            var playerData = new Dictionary<string, object>
            {
                { "PlayerName", user.DisplayName },
                { "Matchplayed", 0 },
                { "Matchwon", 0 },
                { "Matchlist", 0 },
                { "intournament", true }
            };

            // Add the user to the tournament in Firebase
            Task DBTask = DBreference.Child("Tournament")
                .Child(creatorID)
                .Child(tournamentName)
                .Child("Players")
                .Child(user.UserId)
                .SetValueAsync(playerData);

            // Handle the completion of the DB task
            DBTask.ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    ShowNotification("Failed to join the tournament.");
                }
                else if (t.IsCompleted)
                {
                    // Update the user's intournament flag in Firebase
                    DBreference.Child("users").Child(user.UserId).Child("intournament").SetValueAsync(true);

                    ShowNotification("Successfully joined the tournament.");
                }
            });
        }
        else
        {
            // Not enough balance to join
            ShowNotification("Not enough credit...");
        }
    }

    public TMP_Text tournamentNameText, goalsText, rewardText, endTimeText, startTimeText;
    public GameObject playerItemPrefab,TournamentViewPanel;
    public Transform playersContentPanel;
    public Button EndTournament;
    void EnterTournament(string tournamentID)
    {
        StartCoroutine(LoadTournamentByID(tournamentID));
        EndTournament.interactable = false;
    }
    void ManageTournament(string tournamentID)
    {
        StartCoroutine(ManageLoadTournamentByID(tournamentID));
        EndTournament.interactable = true;
    }
    IEnumerator LoadTournamentByID(string tournamentID)
    {
        // Retrieve the specific tournament using the tournamentID
        Task<DataSnapshot> DBTask = DBreference.Child("Tournament").Child(tournamentID).GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve tournament: {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            Debug.LogWarning("No tournament found for the provided tournamentID.");
        }
        else
        {
            DataSnapshot tournamentSnapshot = DBTask.Result;

            // Loop through all tournaments under the specific tournamentID
            foreach (DataSnapshot tournament in tournamentSnapshot.Children)
            {
                string tournamentName = tournament.Key; // Get the tournament name ("assas")
                TournamentViewPanel.SetActive(true);
                // Retrieve tournament details under the tournament name
                string tournamentGoals = tournament.Child("Goals").Value.ToString();
                string tournamentRewardPrice = tournament.Child("RewardPrice").Value.ToString();
                string tournamentEndTime = tournament.Child("endTime").Value.ToString();
                string tournamentStartTime = tournament.Child("startTime").Value.ToString();

                // Set the retrieved tournament data in the UI
                tournamentNameText.text = tournamentName;
                goalsText.text = tournamentGoals;
                rewardText.text = "NGN " + tournamentRewardPrice;
                endTimeText.text = "End date: " + tournamentEndTime;
                startTimeText.text = "Start date: " + tournamentStartTime;

                // Load Players
                if (tournament.HasChild("Players"))
                {
                    DataSnapshot playersSnapshot = tournament.Child("Players");

                    // Clear the current player list
                    foreach (Transform child in playersContentPanel)
                    {
                        Destroy(child.gameObject);
                    }

                    // Loop through all players and instantiate them in the UI
                    foreach (DataSnapshot playerSnapshot in playersSnapshot.Children)
                    {
                        string playerName = playerSnapshot.Child("PlayerName").Value.ToString();
                        string matchesPlayed = playerSnapshot.Child("Matchplayed").Value.ToString();
                        string matchesWon = playerSnapshot.Child("Matchwon").Value.ToString();
                        //string matchList = playerSnapshot.Child("Matchlist").Value.ToString();

                        // Instantiate player prefab
                        GameObject playerItem = Instantiate(playerItemPrefab, playersContentPanel);

                        // Set player data in the prefab
                        TMP_Text playerNameText = playerItem.transform.Find("PlayerNameText").GetComponent<TMP_Text>();
                        TMP_Text matchesPlayedText = playerItem.transform.Find("MatchesPlayedText").GetComponent<TMP_Text>();
                        TMP_Text matchesWonText = playerItem.transform.Find("MatchesWonText").GetComponent<TMP_Text>();
                        //TMP_Text matchListText = playerItem.transform.Find("MatchListText").GetComponent<TMP_Text>();

                        playerNameText.text = playerName;
                        matchesPlayedText.text = " " + matchesPlayed;
                        matchesWonText.text = " " + matchesWon;
                        //matchListText.text = "Match List: " + matchList;
                    }
                }
            }
        }
    }
    IEnumerator ManageLoadTournamentByID(string tournamentID)
    {
        // Retrieve the specific tournament using the tournamentID
        Task<DataSnapshot> DBTask = DBreference.Child("Tournament").Child(tournamentID).GetValueAsync();
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to retrieve tournament: {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            Debug.LogWarning("No tournament found for the provided tournamentID.");
        }
        else
        {
            // Clear the current player list
            foreach (Transform child in playersContentPanel)
            {
                Destroy(child.gameObject);
            }

            DataSnapshot tournamentSnapshot = DBTask.Result;

            // Loop through all tournaments under the specific tournamentID
            foreach (DataSnapshot tournament in tournamentSnapshot.Children)
            {
                string tournamentName = tournament.Key; // Get the tournament name ("assas")
                TournamentViewPanel.SetActive(true);
                // Retrieve tournament details under the tournament name
                string tournamentGoals = tournament.Child("Goals").Value.ToString();
                string tournamentRewardPrice = tournament.Child("RewardPrice").Value.ToString();
                string tournamentEndTime = tournament.Child("endTime").Value.ToString();
                string tournamentStartTime = tournament.Child("startTime").Value.ToString();

                // Set the retrieved tournament data in the UI
                tournamentNameText.text = tournamentName;
                goalsText.text = tournamentGoals;
                rewardText.text = "NGN " + tournamentRewardPrice;
                endTimeText.text = "End date: " + tournamentEndTime;
                startTimeText.text = "Start date: " + tournamentStartTime;

                // Load Players
                if (tournament.HasChild("Players"))
                {
                    DataSnapshot playersSnapshot = tournament.Child("Players");

                    // Clear the current player list
                    foreach (Transform child in playersContentPanel)
                    {
                        Destroy(child.gameObject);
                    }

                    // Loop through all players and instantiate them in the UI
                    foreach (DataSnapshot playerSnapshot in playersSnapshot.Children)
                    {
                        string playerName = playerSnapshot.Child("PlayerName").Value.ToString();
                        string matchesPlayed = playerSnapshot.Child("Matchplayed").Value.ToString();
                        string matchesWon = playerSnapshot.Child("Matchwon").Value.ToString();
                        //string matchList = playerSnapshot.Child("Matchlist").Value.ToString();

                        // Instantiate player prefab
                        GameObject playerItem = Instantiate(playerItemPrefab, playersContentPanel);

                        // Set player data in the prefab
                        TMP_Text playerNameText = playerItem.transform.Find("PlayerNameText").GetComponent<TMP_Text>();
                        TMP_Text matchesPlayedText = playerItem.transform.Find("MatchesPlayedText").GetComponent<TMP_Text>();
                        TMP_Text matchesWonText = playerItem.transform.Find("MatchesWonText").GetComponent<TMP_Text>();
                        //TMP_Text matchListText = playerItem.transform.Find("MatchListText").GetComponent<TMP_Text>();

                        playerNameText.text = playerName;
                        matchesPlayedText.text = " " + matchesPlayed;
                        matchesWonText.text = " " + matchesWon;
                        //matchListText.text = "Match List: " + matchList;
                    }
                }
            }
        }
    }

    #endregion

    #region SHOP
    public GameObject ShopObject;
    public List<ShopItem> shopItems = new List<ShopItem>(); // List of all shop items
    private ShopItem currentlySelectedItem; // To track the currently selected item
    private string saveKey = "SHOPID";      // Firebase child key for shop data

    public GameObject ShopObjectAvatar;
    public List<AvatarShopItem> shopItemsavatar = new List<AvatarShopItem>(); // List of all shop items
    private AvatarShopItem currentlySelectedItemAvatar; // To track the currently selected item
    private string saveKeyAvatar = "SHOPIDAvatar";      // Firebase child key for shop data


    public Image avatarImage;
    public string AvatarName;
    public string LinkImage;
    public void UpdateAvatarProfile(Sprite avatarSprite, string avatarName,string ImageLink)
    {
        if (avatarImage != null)
        {
            avatarImage.sprite = avatarSprite;
        }

        if (AvatarName != null)
        {
            AvatarName = avatarName;
        }

        LinkImage = ImageLink;
    }

    public void SAVESHOPDATA()
    {
        StartCoroutine(SaveShopData());
    }
    public void LOADSHOPDATA()
    {
        StartCoroutine(LoadShopData());
    }
    public void SAVESHOPDATAAvatar()
    {
        StartCoroutine(SaveShopDataAvatar());
    }
    public void LOADSHOPDATAAvatar()
    {
        StartCoroutine(LoadShopDataAvatar());
    }


    // Method to handle item purchase
    public void ItemPurchased(ShopItem purchasedItem)
    {
        Debug.Log($"Item purchased: {purchasedItem.itemName} for {purchasedItem.price} Credits");
        // Implement your logic for what happens when an item is purchased
        // e.g., updating player credits, inventory, etc.
        // Optionally, you could trigger a UI update here as well.
    }
    // Method to handle item purchase
    public void ItemPurchasedAvatar(AvatarShopItem purchasedItem)
    {
        Debug.Log($"Item purchased: {purchasedItem.avatarName} for {purchasedItem.price} Credits");
        // Implement your logic for what happens when an item is purchased
        // e.g., updating player credits, inventory, etc.
        // Optionally, you could trigger a UI update here as well.
    }

    // Method to save the shop data to Firebase
    private IEnumerator SaveShopData()
    {
        if (user != null)
        {
            List<string> savedData = new List<string>();

            foreach (var item in shopItems)
            {
                savedData.Add(item.SaveItemData());
            }

            string combinedData = string.Join(";", savedData);  // Combine all item data with semicolons
            var DBTask = DBreference.Child("users").Child(user.UserId).Child(saveKey).SetValueAsync(combinedData);

            yield return new WaitUntil(() => DBTask.IsCompleted); // Wait until the task is completed

            if (DBTask.Exception != null)
            {
                Debug.LogError($"Failed to save shop data to Firebase: {DBTask.Exception.Message}");
            }
            else
            {
                Debug.Log("Shop data saved to Firebase!");
            }
        }
        else
        {
            Debug.LogError("No user is logged in. Cannot save shop data.");
        }
    }
    private IEnumerator SaveShopDataAvatar()
    {
        if (user != null)
        {
            List<string> savedData = new List<string>();

            foreach (var item in shopItemsavatar)
            {
                savedData.Add(item.SaveItemData());
            }

            string combinedData = string.Join(";", savedData);  // Combine all item data with semicolons
            var DBTask = DBreference.Child("users").Child(user.UserId).Child(saveKeyAvatar).SetValueAsync(combinedData);

            yield return new WaitUntil(() => DBTask.IsCompleted); // Wait until the task is completed

            if (DBTask.Exception != null)
            {
                Debug.LogError($"Failed to save shop data to Firebase: {DBTask.Exception.Message}");
            }
            else
            {
                Debug.Log("Shop data saved to Firebase!");
            }
        }
        else
        {
            Debug.LogError("No user is logged in. Cannot save shop data.");
        }
    }
    // Method to load the shop data from Firebase
    private IEnumerator LoadShopData()
    {
        if (user != null)
        {
            var DBTask = DBreference.Child("users").Child(user.UserId).Child(saveKey).GetValueAsync();

            yield return new WaitUntil(() => DBTask.IsCompleted); // Wait until the task is completed

            if (DBTask.Exception != null)
            {
                Debug.LogError($"Failed to load shop data from Firebase: {DBTask.Exception.Message}");
            }
            else
            {
                DataSnapshot snapshot = DBTask.Result;

                if (snapshot.Exists)
                {
                    string combinedData = snapshot.Value.ToString();
                    string[] loadedData = combinedData.Split(';');

                    for (int i = 0; i < shopItems.Count && i < loadedData.Length; i++)
                    {
                        shopItems[i].LoadItemData(loadedData[i]);

                        // Check if the item was previously selected
                        if (shopItems[i].IsSelected())
                        {
                            currentlySelectedItem = shopItems[i];
                        }
                    }

                    Debug.Log("Shop data loaded from Firebase!");
                }
                else
                {
                    Debug.LogWarning("No saved shop data found in Firebase.");
                }
            }
        }
        else
        {
            Debug.LogError("No user is logged in. Cannot load shop data.");
        }
    }
    private IEnumerator LoadShopDataAvatar()
    {
        if (user != null)
        {
            var DBTask = DBreference.Child("users").Child(user.UserId).Child(saveKeyAvatar).GetValueAsync();

            yield return new WaitUntil(() => DBTask.IsCompleted); // Wait until the task is completed

            if (DBTask.Exception != null)
            {
                Debug.LogError($"Failed to load shop data from Firebase: {DBTask.Exception.Message}");
            }
            else
            {
                DataSnapshot snapshot = DBTask.Result;

                if (snapshot.Exists)
                {
                    string combinedData = snapshot.Value.ToString();
                    string[] loadedData = combinedData.Split(';');

                    for (int i = 0; i < shopItemsavatar.Count && i < loadedData.Length; i++)
                    {
                        shopItemsavatar[i].LoadItemData(loadedData[i]);

                        // Check if the item was previously selected
                        if (shopItemsavatar[i].IsSelected())
                        {
                            currentlySelectedItemAvatar = shopItemsavatar[i];
                        }
                    }

                    Debug.Log("Shop data loaded from Firebase!");
                }
                else
                {
                    Debug.LogWarning("No saved shop data found in Firebase.");
                }
            }
        }
        else
        {
            Debug.LogError("No user is logged in. Cannot load shop data.");
        }
    }
    // Method to clear saved data in Firebase
    private IEnumerator ClearShopData()
    {
        if (user != null)
        {
            var DBTask = DBreference.Child("users").Child(user.UserId).Child(saveKey).RemoveValueAsync();

            yield return new WaitUntil(() => DBTask.IsCompleted); // Wait until the task is completed

            if (DBTask.Exception != null)
            {
                Debug.LogError($"Failed to clear shop data in Firebase: {DBTask.Exception.Message}");
            }
            else
            {
                Debug.Log("Shop data cleared in Firebase.");
            }
        }
        else
        {
            Debug.LogError("No user is logged in. Cannot clear shop data.");
        }
    }
    // Method to handle selecting an item, ensuring only one is selected at a time
    public void SelectItem(ShopItem itemToSelect)
    {
        // Deselect the currently selected item, if there is one
        if (currentlySelectedItem != null && currentlySelectedItem != itemToSelect)
        {
            currentlySelectedItem.DeselectItem(); // Call the Deselect method in ShopItem class
        }

        // Set the new item as selected
        currentlySelectedItem = itemToSelect;
    }
    #endregion

    #region RATE
    public TMP_InputField feedbackInput;   // Reference to the feedback input field

    public void SendFeedback()
    {
        string feedbackText = feedbackInput.text;  // Get feedback text from input field
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");  // Get current timestamp

        if (!string.IsNullOrEmpty(feedbackText))
        {
            // Start coroutine to send feedback
            StartCoroutine(SendFeedbackCoroutine(feedbackText, timestamp));
        }
        else
        {
            ShowNotification("Feedback sent successfully!");
        }
    }
    private IEnumerator SendFeedbackCoroutine(string feedbackText, string timestamp)
    {
        ShowNotification("Sending...");
        // Create feedback data as a single string
        string feedbackMessage = feedbackText + " | Sent at: " + timestamp + " by: " + user.UserId;

        // Generate a unique key for each feedback entry
        string feedbackKey = DBreference.Child("Feedback").Push().Key;

        // Prepare Firebase database operation to store feedback under unique key using SetValueAsync
        Task DBTask = DBreference.Child("Feedback").Child(feedbackKey).SetValueAsync(feedbackMessage);

        // Wait for the task to complete
        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception == null)
        {
            ShowNotification("Feedback sent successfully!");
            feedbackInput.text = "";  // Clear input field after sending feedback
        }
        else
        {
            Debug.LogError("Failed to send feedback: " + DBTask.Exception);
        }
    }
    #endregion

    #region Deposite

    private string email_Word, password_Word;

    public void OnClickDeposite()
    {
        string encodedEmail = WWW.EscapeURL(email_Word);
        string encodedPassword = WWW.EscapeURL(password_Word);

        string url = $"https://swiftpaymfb.com/Chesswagergateway/chesswagerpayment.html?email={encodedEmail}&password={encodedPassword}";
        BrowserOpener.instance.pageToOpen = url;
        BrowserOpener.instance.OnButtonClicked();
        //Application.OpenURL(url);
    }
    public void OnClickWithdraw()
    {
        string encodedEmail = WWW.EscapeURL(email_Word);
        string encodedPassword = WWW.EscapeURL(password_Word);

        string url = $"https://swiftpaymfb.com/Chesswagergateway/Trans.html?email={encodedEmail}&password={encodedPassword}";
        BrowserOpener.instance.pageToOpen = url;
        BrowserOpener.instance.OnButtonClicked();
        //Application.OpenURL(url);
    }
    public void OnClickPoilcy()
    {
        string url = $"https://chesswager-cnb.web.app/";
        BrowserOpener.instance.pageToOpen = url;
        BrowserOpener.instance.OnButtonClicked();
        //Application.OpenURL(url);
    }
    public void OnClickTerms()
    {
        string url = $"https://chesswager-cnb.web.app/";
        BrowserOpener.instance.pageToOpen = url;
        BrowserOpener.instance.OnButtonClicked();
        //Application.OpenURL(url);
    }

    public GameObject transactionItemPrefab; // The prefab for displaying each transaction
    public Transform contentPanels; // The panel where transaction items will be added
    public string userID; // The current user's ID

    // Function to load transaction history from Firebase
    public void LoadTransactionHistory()
    {
        StartCoroutine(LoadTransactionHistorys());
        print("0");
    }

    public bool ShowMostRecentAtTop = true; // Toggle in Unity Editor
    public ScrollRect ScrollRect; // Reference to the ScrollView's ScrollRect

    IEnumerator LoadTransactionHistorys()
    {
        // Show loading indicator
        Loading.SetActive(true);

        // Retrieve transaction history from Firebase
        Task<DataSnapshot> DBTask = DBreference.Child("users").Child(user.UserId).Child("transactionHistory").GetValueAsync();

        // Wait for the Firebase task to complete
        yield return new WaitUntil(() => DBTask.IsCompleted);

        // Handle Firebase task exceptions
        if (DBTask.Exception != null)
        {
            ShowNotification("Failed to retrieve transaction history");
            Debug.LogError($"Error retrieving transaction history: {DBTask.Exception}");
            Loading.SetActive(false);
            yield break;
        }

        DataSnapshot snapshot = DBTask.Result;

        // Check if transaction history exists
        if (snapshot.Exists)
        {
            // Clear existing items from the content panel
            foreach (Transform child in contentPanels)
            {
                Destroy(child.gameObject);
            }

            // Parse transactions into a list
            List<DataSnapshot> transactionsList = snapshot.Children.ToList();

            // Sort transactions by timestamp
            transactionsList = transactionsList
                .OrderBy(txn => DateTime.Parse(txn.Child("timestamp").Value.ToString())) // Ascending by timestamp
                .ToList();

            // Reverse the list if most recent transactions should appear at the top
            if (ShowMostRecentAtTop)
            {
                transactionsList.Reverse();
            }

            // Iterate through the sorted transactions
            foreach (DataSnapshot transactionSnapshot in transactionsList)
            {
                // Retrieve transaction data
                Dictionary<string, object> transactionData = (Dictionary<string, object>)transactionSnapshot.Value;

                if (transactionData.ContainsKey("Title") && transactionData.ContainsKey("amount") &&
                    transactionData.ContainsKey("status") && transactionData.ContainsKey("timestamp"))
                {
                    string title = transactionData["Title"].ToString();
                    string amount = transactionData["amount"].ToString();
                    string status = transactionData["status"].ToString();
                    string timestamp = transactionData["timestamp"].ToString();

                    // Instantiate a new transaction entry
                    GameObject transactionEntry = Instantiate(transactionItemPrefab, contentPanels);

                    // Assign data to the prefab components
                    TMP_Text titleText = transactionEntry.transform.Find("Background/titleText")?.GetComponent<TMP_Text>();
                    TMP_Text amountText = transactionEntry.transform.Find("Background/AmountText")?.GetComponent<TMP_Text>();
                    TMP_Text statusText = transactionEntry.transform.Find("Background/StatusText")?.GetComponent<TMP_Text>();
                    TMP_Text timestampText = transactionEntry.transform.Find("Background/TimestampText")?.GetComponent<TMP_Text>();
                    Image backgroundColor = transactionEntry.transform.Find("Background")?.GetComponent<Image>();

                    if (titleText != null && amountText != null && statusText != null && timestampText != null && backgroundColor != null)
                    {
                        // Format and set the values
                        titleText.text = $"Title: {title}";
                        amountText.text = $"Amount: ₦{amount}";
                        statusText.text = $"Status: {status}";
                        timestampText.text = $"Date: {DateTime.Parse(timestamp).ToString("dd MMM yyyy, HH:mm")}";

                        // Change background color based on status
                        if (status == "Success")
                        {
                            backgroundColor.color = Color.green;
                        }
                        else if (status == "Failed")
                        {
                            backgroundColor.color = Color.red;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Transaction prefab is missing one or more components.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Transaction {transactionSnapshot.Key} is missing required fields.");
                }
            }

            // Scroll to the top or bottom based on `ShowMostRecentAtTop`
            yield return new WaitForEndOfFrame(); // Wait for the UI to update
            if (ShowMostRecentAtTop)
            {
                ScrollRect.verticalNormalizedPosition = 1f; // Scroll to the top
            }
            else
            {
                ScrollRect.verticalNormalizedPosition = 0f; // Scroll to the bottom
            }
        }
        else
        {
            ShowNotification("No transaction history found");
            Debug.LogWarning("No transaction history found in Firebase.");
        }

        // Hide loading indicator
        Loading.SetActive(false);
    }


    #endregion
    IEnumerator REFRESH()
    {
        yield return new WaitForSeconds(3f);
        Loading.SetActive(false);
    }

    #region InGAME CONTROL
    public Button TABButton1;
    public Button TABButton2,ROOM,DEPOSIT;
    public Button TABButton3;
    public Button TABButton4;
    public Button TABButton5;
    public Button TABButton6;
    public TMP_Text TabButonText1;
    public TMP_Text TabButonText2;
    public TMP_Text TabButonText3;
    public TMP_Text TabButonText4;
    public TMP_Text TabButonText5;
    public TMP_Text TabButonText6;
    public float tournamentcost;
    public TMP_Text tournamentcosttext;
    public Button HostTorunamentButton;
    public TMP_Text InfoHost, InfoJoin;
    public GameObject PanelUpdate;
    public TMP_Text TITLE, MESSAGE, VERSION;
    public TMP_Text About, Elua, Terms;

    public GameObject ShoButton, IconControllers;
    public IEnumerator CheckGameSettings()
    {
        yield return new WaitForSeconds(0f);
        Task<DataSnapshot> DBTask = DBreference.Child("admin").Child(AdminID).GetValueAsync();

        yield return new WaitUntil(() => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            // No data exists yet
        }
        else
        {
            //data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            //INGAME CONTROL
            IconControllers.gameObject.SetActive(bool.Parse(snapshot.Child("button2").Value.ToString()));
            ShoButton.gameObject.SetActive(bool.Parse(snapshot.Child("button2").Value.ToString()));
            TABButton2.interactable = true;
            TABButton1.interactable = bool.Parse(snapshot.Child("button1").Value.ToString());
            TABButton2.gameObject.SetActive(bool.Parse(snapshot.Child("button2").Value.ToString()));
            DEPOSIT.interactable = bool.Parse(snapshot.Child("button2").Value.ToString());
            ROOM.interactable = bool.Parse(snapshot.Child("button2").Value.ToString());
            TABButton3.gameObject.SetActive(bool.Parse(snapshot.Child("button3").Value.ToString()));
            TABButton4.interactable = bool.Parse(snapshot.Child("button4").Value.ToString());
            TABButton5.interactable = bool.Parse(snapshot.Child("button5").Value.ToString());
            TABButton6.interactable = bool.Parse(snapshot.Child("button6").Value.ToString());

            HostTorunamentButton.interactable = bool.Parse(snapshot.Child("buttonTournamentButton").Value.ToString());

            TabButonText1.text = snapshot.Child("buttontext1").Value.ToString();
            TabButonText2.text = snapshot.Child("buttontext2").Value.ToString();
            TabButonText3.text = snapshot.Child("buttontext3").Value.ToString();
            TabButonText4.text = snapshot.Child("buttontext4").Value.ToString();
            TabButonText5.text = snapshot.Child("buttontext5").Value.ToString();
            TabButonText6.text = snapshot.Child("buttontext6").Value.ToString();

            //currency = snapshot.Child("Currency").Value.ToString();

            //Tournament
            WageCost = float.Parse(snapshot.Child("Battlecost").Value.ToString());
            tournamentcost = float.Parse(snapshot.Child("tournamentcost").Value.ToString());
            tournamentcosttext.text = "NGN " + snapshot.Child("tournamentcost").Value.ToString();
            InfoJoin.text = snapshot.Child("infojoin").Value.ToString();
            InfoHost.text = snapshot.Child("infoHost").Value.ToString();

            PanelUpdate.SetActive(bool.Parse(snapshot.Child("Pause").Value.ToString()));
            TITLE.text = snapshot.Child("Title").Value.ToString();
            MESSAGE.text = snapshot.Child("Updatemessage").Value.ToString();
            VERSION.text = snapshot.Child("Time").Value.ToString();
            About.text = snapshot.Child("About").Value.ToString();
            Elua.text = snapshot.Child("EULA").Value.ToString();
            Terms.text = snapshot.Child("Terms").Value.ToString();

            Loading.SetActive(false);
        }

        
    }
    
    #endregion
}

