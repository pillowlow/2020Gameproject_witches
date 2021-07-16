using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControl
{
    static DragonBones.UnityArmatureComponent DB;
    public static AnimationControl instance;
    
    public AnimationControl(DragonBones.UnityArmatureComponent db)
    {
        DB = db;
        instance = this;
    }
    static int CurrentPlaying = 0;


    public class AnimationBase
    {
        string Name;
        int Id;
        protected float TimeScale = 1.0f;
        public AnimationBase(string name, int id)
        {
            Name = name;
            Id=id;
        }
        public void Play(float FadeTime, bool loop, float Delay=0, Action func=null, MonoBehaviour o=null)
        {
            DB.animation.timeScale = TimeScale;
            DB.animation.FadeIn(Name,FadeTime,loop?-1:1);
            CurrentPlaying = Id;
            if(func != null)o.StartCoroutine(InvokeAfterSec(func, Delay));
        }
        public IEnumerator InvokeAfterSec(Action func, float s)
        {
            yield return new WaitForSeconds(s);
            func();
        }

        public void SetTimeScale(float x)
        {
            TimeScale = x;
            if(CurrentPlaying ==this)
            {
                DB.animation.timeScale = x;
            }
        }

        public bool isCompleted()
        {
            var state = DB.animation.GetState(Name);
            return (state != null && state.isCompleted);
        }

        public static implicit  operator int(AnimationBase r)
        {
            return r.Id;
        }
        public static bool operator ==(AnimationBase l,AnimationBase r)
        {
            return l.Id == r.Id;
        }
        public static bool operator !=(AnimationBase l, AnimationBase r)
        {
            return l.Id != r.Id;
        }
    }
    public int Playing() { return CurrentPlaying; }
    public class Walk : AnimationBase
    {
        public Walk(string name, int id) : base(name, id) { }
        public void StartWalking(MonoBehaviour o,float Delay = 0, Action func = null )
        {
            Play(0.1f, false);
            DB.animation.timeScale = TimeScale;
            CurrentPlaying = this;
            if (func != null) o.StartCoroutine(InvokeAfterSec(func, Delay));
            o.StartCoroutine(StartRunning(o));
        }

        IEnumerator StartRunning( MonoBehaviour o)
        {
            var state = DB.animation.GetState("walk(run)start");
            
            while (state != null && state.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
            if (PlayerManager.state == PlayerManager.StateCode.Moving)
            {
                DB.animation.FadeIn("walk(run)", 0.2f, 1);
                CurrentPlaying = 2;
                o.StartCoroutine(Running(o));
            }
        }
        IEnumerator Running(MonoBehaviour o)
        {
            var state = DB.animation.GetState("walk(run)");
            while (state != null && state.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
            state = DB.animation.GetState("walk(run)");
            if ((PlayerManager.state == PlayerManager.StateCode.Moving) && (state != null && state.isCompleted))
            {
                DB.animation.FadeIn("walk(run)", 0.1f, 1);
                o.StartCoroutine(Running(o));
            }
        }
    };
    public class Falling:AnimationBase
    {
        public float JumpEnd = 0.5f;
        public float JumpRepeatSpeed = 0.5f;
        public float JumpRepeatTime = 0.3f;

        public float RunJumpEnd = 0.5f;
        public float RunJumpRepeatSpeed = 0.5f;
        public float RunJumpTime = 0.3f;
        public Falling(string name, int id) : base(name, id) { }
        public void fall(MonoBehaviour o,bool run=false)
        {
            if (CurrentPlaying == this) { return; }
            DB.animation.timeScale = TimeScale;
            CurrentPlaying = this;
            DB.animation.FadeIn("jump(normal)",0.1f,1);
            o.StartCoroutine(FallingAnimation(0.5f,run,o));
        }
        IEnumerator FallingAnimation(float s, bool c,MonoBehaviour o)
        {
            var state = DB.animation.GetState(c? "jump(run)": "jump(normal)");
            while (state != null && (PlayerManager.state == PlayerManager.StateCode.Jumping || state.currentTime < (c?RunJumpEnd:JumpEnd)))
            {
                state = DB.animation.GetState(c ? "jump(run)" : "jump(normal)");
                yield return new WaitForEndOfFrame();
            }
            if (PlayerManager.state == PlayerManager.StateCode.Falling)
            {
                DB.animation.timeScale = -(c?RunJumpRepeatSpeed:JumpRepeatSpeed);

                o.StartCoroutine(InvokeAfterSec(() =>
                {
                    if (CurrentPlaying != this) { return; }
                    DB.animation.timeScale = c?RunJumpRepeatSpeed:JumpRepeatSpeed;
                    o.StartCoroutine(FallingAnimation(s, c, o));
                }, c?RunJumpTime:JumpRepeatTime));
            }

        }
    }
    public bool isCompleted()
    {
        var state = DB.animation.lastAnimationState;
        return (state != null && state.isCompleted);
    }

    public AnimationBase idle = new AnimationBase("idle", 0);
    public Walk walk = new Walk("walk(run)start", 1);
    public AnimationBase run = new AnimationBase("walk(run)", 2);
    public AnimationBase jump = new AnimationBase("jump(normal)", 3);
    public AnimationBase runjump = new AnimationBase("jump(run)", 4);
    public AnimationBase die = new AnimationBase("death", 5);
    public AnimationBase reborn = new AnimationBase("reborn", 6);
    public AnimationBase attack1 = new AnimationBase("attact1", 7);
    public AnimationBase attack2 = new AnimationBase("attact2", 8);
    public AnimationBase magic = new AnimationBase("usemagic", 9);
    public AnimationBase hurt = new AnimationBase("hurt", 10);
    public Falling falling = new Falling("idle", 11);

}