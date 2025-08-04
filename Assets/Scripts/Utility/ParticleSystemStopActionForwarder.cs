using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemStopActionForwarder : MonoBehaviour
{
    public UnityEvent OnParticleSystemStoppedEvent;
    new private ParticleSystem particleSystem;

    public void Awake(){
        particleSystem = GetComponent<ParticleSystem>();
        if(!particleSystem){
            enabled = false;
        }
    }

    void Reset(){
        particleSystem = GetComponent<ParticleSystem>();
        if(particleSystem.main.stopAction != ParticleSystemStopAction.Callback){
            ParticleSystem.MainModule mainModule = particleSystem.main;
            mainModule.stopAction = ParticleSystemStopAction.Callback;
            mainModule.playOnAwake = false;
        }
    }

    public void Start(){
        if(particleSystem.main.stopAction != ParticleSystemStopAction.Callback){
            Debug.LogWarning($"ParticleSystem with Forwarder {this.name} not set up for stop action callback.", this);
        }
    }

    public void OnParticleSystemStopped(){
        OnParticleSystemStoppedEvent?.Invoke();
    }

    public void DebugLogParticleSystemStopped(string s){
        Debug.Log(s);
    }
}
