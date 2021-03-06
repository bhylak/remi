using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using static NotificationManager;

public abstract class NotificationBehaviour : MonoBehaviour
{
    protected Notification _notificationModel;

    public bool isDismissed;
    protected GazeReceiver _gazeReceiver;
    protected State _state;
    public AudioClip _notifSound;
    

//    public NotificationBehaviour(Notification n)
//    {
//        this._notificationModel = n;
//    }

    public abstract Task Launch(int delay = 1000);
    

    protected void Start()
    {
        _gazeReceiver = gameObject.GetComponent<GazeReceiver>();
    }

    public abstract void ShowToDiminish();
    public abstract void DiminishToShow();

    public abstract void HideToShow();

    public virtual void UpdateShow() //turn into a state someday
    {
        
    }

    public abstract Tween Hide();
    
    public abstract class NotificationState : State
    {
        protected NotificationBehaviour _notif;

        public NotificationState(NotificationBehaviour n)
        {
            _notif = n;
        }
    }

    public class ShowState : NotificationState
    {     
        public ShowState(NotificationBehaviour n, bool doShow = true) : base(n)
        {
            if(doShow) _notif.HideToShow();
            _notif._gazeReceiver.ResetLookedAtFlag();
        }

        public override State Update()
        {
            if (_notif._gazeReceiver.hasBeenLookedAt && _notif._gazeReceiver.timeSinceLastGaze > 1.5f)
            {
                return new DiminishedState(_notif);
            }

            _notif.UpdateShow();

            return null;
        }
    }

    public class DiminishedState : NotificationState
    {
        public DiminishedState(NotificationBehaviour n) : base(n)
        {
            Debug.Log("Diminished state");
            
            _notif.ShowToDiminish();
        }

        public override State Update()
        {
            if (_notif._gazeReceiver.currentGazeDuration > 0.4f)
            {
                _notif.DiminishToShow();
                return new ShowState(_notif, doShow: false);
            }

            return null;
        }
    }
}
