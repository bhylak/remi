using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BensToolBox;
using UniRx;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using System.Linq;
using BensToolBox.AR.Scripts;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class BigKahuna: MonoBehaviour
{
    //TODO: make singleton
    //also make singleton streamer

    public static BigKahuna Instance;
    public List<BurnerBehaviour> _burnerBehaviours;
    private State _timerState;
    public GameObject Stove;
    public SpeechRecognizer speechRecognizer;
    private bool isSetup = true;
    public IEnumerable<GameObject> _debugObjects;
    private NotificationManager _notificationManager;
    public MeshRenderer raycastVisualizer;
    public RamenUI ramenUI;

    public PushButtonBehavior pushButton;
    
    public bool UsePushButton = false;

    public bool DisableOtherListeners;
 
    public void Start()
    {
        Instance = this;

        raycastVisualizer.enabled = false;
        
        DatabaseManager.Instance.getBurners()
            .ObserveAdd()
            .Subscribe(burnerData => 
                _burnerBehaviours
                    .Find(x => x._position == burnerData.Value.Position)
                    ._model = burnerData.Value);
//
//        DatabaseManager
//            .Instance
//            .getBurners()
//            .ObserveCountChanged()
//            .Where(x => x == 4)
//            .Subscribe(_ =>
//                {
//                    Debug.Log("Monitoring...");
//                    _timerState = SetTimerStateMachine.GetInitialState();
//                }
//            );

        MLInput.OnControllerTouchpadGestureEnd += (id, gesture) =>
        {
            if (gesture.Type == MLInputControllerTouchpadGestureType.ForceTapDown)
            {
                raycastVisualizer.enabled = !raycastVisualizer.enabled;
            }
            else if (gesture.Type == MLInputControllerTouchpadGestureType.LongHold)
            {
                UsePushButton = !UsePushButton;

                if (UsePushButton)
                {
                    pushButton.gameObject.SetActive(true);
                    ramenUI.inputIsEnabled = false;
                }
                else
                {
                    ramenUI.inputIsEnabled = true;
                    pushButton.gameObject.SetActive(false);
                }
            }
        };
        
        MLInput.OnControllerButtonDown += (b, button) =>
        {
            if (button == MLInputControllerButton.Bumper)
            {
                ToggleSetup();
            }
            else if (button == MLInputControllerButton.HomeTap)
            {
                ResetBurners();
                RecipeManager.Instance.ClearRecipe();
            }
         
        }; //toggle setup mode.

        MLInput.OnTriggerDown += (_, __) =>
        {
            ramenUI.headTrackingEnabled = !ramenUI.headTrackingEnabled;
        };

        MLInput.OnTriggerUp += (_, __) =>
        {
           // _burnerBehaviours.ForEach(x => x.IsLookedAt = false);
        };
        
        _burnerBehaviours.ForEach(b => b.OnBurnerNotification += 
            notif => _notificationManager.AddNotification(notif)
        );

        _notificationManager = GetComponent<NotificationManager>();
        
        _debugObjects = GameObject.FindGameObjectsWithTag("Debug");
    }

    public void ResetBurners()
    {
        foreach (var burner in _burnerBehaviours)
        {
            burner.SetStateToDefault();
        }
    }
    
    public void ResetWorld()
    {
        SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);    
    }
    
    public void Update()
    {
        if (_timerState != null)
        {
            var resultState = _timerState.Update();
            _timerState = resultState ?? _timerState;
        }
    }

    public void ToggleSetup()
    {       
        isSetup = !isSetup;

        if (!isSetup) raycastVisualizer.enabled = false;
        
        foreach (var debugObj in _debugObjects)
        {
            debugObj.SetActive(isSetup);
        }

        Stove.GetComponent<ImageTrackerLerper>().IsTrackingEnabled = isSetup;
    }
}