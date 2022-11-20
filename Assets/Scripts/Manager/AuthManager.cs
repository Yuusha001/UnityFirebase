using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;

public class AuthManager : MonoBehaviour
{
    [Header("Firebase")]
    [SerializeField]
    private DependencyStatus dependencyStatus;

    [SerializeField]
    private FirebaseAuth firebaseAuth;

    [SerializeField]
    private FirebaseUser firebaseUser;

    [Header("Login")]
    [SerializeField]
    private TMP_InputField userName;

    [SerializeField]
    private TMP_InputField userPass;

    [SerializeField]
    private TMP_Text loginWarning;

    [SerializeField]
    private TMP_Text loginConfirm;

    [Header("Register")]
    [SerializeField]
    private TMP_InputField userName_Register;

    [SerializeField]
    private TMP_InputField userPass_Register;

    [SerializeField]
    private TMP_InputField userPassCfm_Register;

    [SerializeField]
    private TMP_Text registerWarning;

    [SerializeField]
    private TMP_Text registerConfirm;

    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp
            .CheckAndFixDependenciesAsync()
            .ContinueWith(task =>
            {
                dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    //If they are avalible Initialize Firebase
                    InitializeFirebase();
                }
                else
                {
                    Debug.LogError(
                        "Could not resolve all Firebase dependencies: " + dependencyStatus
                    );
                }
            });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        firebaseAuth = FirebaseAuth.DefaultInstance;
    }

    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(userName.text, userPass.text));
    }

    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(
            Register(userName_Register.text, userPass_Register.text, userPassCfm_Register.text)
        );
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = firebaseAuth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx =
                LoginTask.Exception.GetBaseException() as FirebaseException;
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
            }
            loginWarning.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            firebaseUser = LoginTask.Result;
            Debug.LogFormat(
                "User signed in successfully: {0} ({1})",
                firebaseUser.DisplayName,
                firebaseUser.Email
            );
            loginWarning.text = "";
            loginConfirm.text = "Logged In";
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            registerWarning.text = "Missing Username";
        }
        else if (userPass_Register.text != userPassCfm_Register.text)
        {
            //If the password does not match show a warning
            registerWarning.text = "Password Does Not Match!";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = firebaseAuth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx =
                    RegisterTask.Exception.GetBaseException() as FirebaseException;
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
                registerWarning.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                firebaseUser = RegisterTask.Result;

                if (firebaseUser != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = firebaseUser.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(
                            message: $"Failed to register task with {ProfileTask.Exception}"
                        );
                        FirebaseException firebaseEx =
                            ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        registerWarning.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        UIManager.instance.LoginShow();
                        registerWarning.text = "";
                    }
                }
            }
        }
    }
}
