using UnityEngine;
using System.Collections;

using uAdventure.Runner;
using System;
using uAdventure.Core;

namespace uAdventure.Geo
{
    [CustomEffectRunner(typeof(NavigationControlEffect))]
    public class NavigationControlRunner : CustomEffectRunner
    {
        private NavigationControlEffect navEffect;
        public IEffect Effect { get { return navEffect; } set { navEffect = value as NavigationControlEffect; } }

        public NavigationControlRunner()
        {
            if (NavigationController.Instance == null)
            {
                var navigationPrefab = Resources.Load<GameObject>("navigation");
                if (navigationPrefab != null)
                {
                    GameObject.Instantiate(navigationPrefab);
                }
            }
        }

        public bool execute()
        {
            return false;
        }
    }
}