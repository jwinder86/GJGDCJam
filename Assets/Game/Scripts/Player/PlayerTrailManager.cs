using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerTrailManager : MonoBehaviour {

    public TrailRenderer[] hoverTrails;
    public TrailRenderer[] flightTrails;
    public TrailRenderer[] stunTrails;

    public float disabledTrailTime = 0.1f;

    private HashSet<TrailRenderer> hoverTrailSet;
    private HashSet<TrailRenderer> flightTrailSet;
    private HashSet<TrailRenderer> stunTrailSet;
    private Dictionary<TrailRenderer, float> trailTimes;

    private PlayerModeManager.PlayerMode mode;

    void Start() {
        hoverTrailSet = new HashSet<TrailRenderer>(hoverTrails);
        flightTrailSet = new HashSet<TrailRenderer>(flightTrails);
        stunTrailSet = new HashSet<TrailRenderer>(stunTrails);
        trailTimes = new Dictionary<TrailRenderer, float>();

        foreach (TrailRenderer t in hoverTrails) {
            if (!trailTimes.ContainsKey(t)) {
                trailTimes.Add(t, t.time);
            }
        }

        foreach (TrailRenderer t in flightTrails) {
            if (!trailTimes.ContainsKey(t)) {
                trailTimes.Add(t, t.time);
            }
        }

        foreach (TrailRenderer t in stunTrails) {
            if (!trailTimes.ContainsKey(t)) {
                trailTimes.Add(t, t.time);
            }
        }

        hoverTrails = null;
        flightTrails = null;
        stunTrails = null;

        this.mode = PlayerModeManager.PlayerMode.Stun;
        SetMode(PlayerModeManager.PlayerMode.Hover);
    }

    public void SetMode(PlayerModeManager.PlayerMode mode) {
        if (this.mode == mode) {
            Debug.Log("No action");
            return;
        }

        HashSet<TrailRenderer> selected;

        switch (mode) {
            case PlayerModeManager.PlayerMode.Flight:
                selected = flightTrailSet;
                break;
            case PlayerModeManager.PlayerMode.Hover:
                selected = hoverTrailSet;
                break;
            case PlayerModeManager.PlayerMode.Stun:
                selected = stunTrailSet;
                break;
            default:
                Debug.LogError("No trail set for mode: " + mode);
                return;
        }

        foreach (TrailRenderer t in trailTimes.Keys) {
            if (selected.Contains(t)) {
                t.time = trailTimes[t];
            } else {
                t.time = disabledTrailTime;
            }
        }

        Debug.Log(trailTimes.Keys.Count + " trail managers updated");

        this.mode = mode;
    }
}
