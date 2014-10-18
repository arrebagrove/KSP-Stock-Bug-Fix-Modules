﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;

namespace ClawKSP
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ChuteFixer : UnityEngine.MonoBehaviour
    {

        public void Awake()
        {
            Debug.LogWarning("ChuteFixer: Awake");
        } // Awake()

        public void Start()
        {
            Debug.LogWarning("ChuteFixer: Start");

            GameEvents.onVesselLoaded.Add(ResetChutes); // Reset chutes whenever vessel is loaded

            // Reset chutes whenever vessel goes off rails. This seems to be called whever a new ship is loaded
            // or after a quicksave, and not GameEvents.onVesselLoaded.
            GameEvents.onVesselGoOffRails.Add(ResetChutes);

            Debug.LogWarning("ChuteFixer: Start complete.");
        } // Start()

        public void ResetChutes(Vessel VesselToFix)
        {

            if (null == VesselToFix)
            {
                Debug.LogError("ChuteFixer: Was passed a null vessel.");
                return;
            }
            
            // Need to check if the vessel is in atmosphere or not. If not, we can return.

            Debug.LogWarning("ChuteFixer: Attempting to Reset Chutes (" + VesselToFix.Parts.Count + " parts.)");

            // Loop through all the parts in the vessel
            for (int PartsIndex = 0; PartsIndex < VesselToFix.Parts.Count; PartsIndex++)
            {
                Part CurrentPart = VesselToFix.Parts[PartsIndex];
                if (null == CurrentPart) continue;

                foreach (ModuleParachute ChuteModule in CurrentPart.Modules.OfType<ModuleParachute>())
                {
                    Debug.LogWarning("ChuteFixer: Looping chutes.");

                    // ModuleParachute acts differently than other Modules. ModuleParachute does not store its
                    // values in a valuelist, but rather has public variables that store the values.
                    // This block checks if the chute is deployed or semideployed (which would happen in atmosphere
                    // and resets it to an Active state (staged, but not deployed). 
                    if (ModuleParachute.deploymentStates.DEPLOYED == ChuteModule.deploymentState ||
                        ModuleParachute.deploymentStates.SEMIDEPLOYED == ChuteModule.deploymentState)
                    {
                        // Set both the active and persistent states to avoid errors.
                        ChuteModule.deploymentState = ModuleParachute.deploymentStates.ACTIVE;
                        ChuteModule.persistentState = "ACTIVE";

                        Debug.LogWarning("Chute Reset: Resetting chute with persistent state = " + ChuteModule.persistentState);
                        if (ModuleParachute.deploymentStates.ACTIVE != ChuteModule.deploymentState)
                        {
                            Debug.LogWarning("ChuteFixer: Failed to reset chute.");
                            break;
                        }
                        ChuteModule.Deploy();  // Force the parachute to deploy so the ModuleParachute is error checked correctly.
                    }

                    ScreenMessages.PostScreenMessage("Chute Reset: Complete", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                }
            }

            Debug.LogWarning("ChuteFixer: Reset attempt complete.");

        } // ResetChutes()

        public void OnDestroy()
        {
            Debug.LogWarning("ChuteFixer: OnDestroy");

            GameEvents.onVesselLoaded.Remove(ResetChutes);
            GameEvents.onVesselGoOffRails.Remove(ResetChutes);

            Debug.LogWarning("ChuteFixer: OnDestroy complete.");
            
        } // Destroy()

    }
} // ClawKSP