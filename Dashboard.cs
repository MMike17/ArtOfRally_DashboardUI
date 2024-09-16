using UnityEngine;
using UnityEngine.UI;

namespace DashboardUI
{
    class Dashboard : MonoBehaviour
    {
        Image rev;
        Image limiter;
        Image tickPrefab;
        Text gear;
        Text speed;
        Text unit;

        bool initialized = false;

        // TODO : Get components refs
        // TODO : Copy init system from Damage UI

        void Init()
        {
            //rev = transform.GetChild(0).GetChild(1).GetComponentInChildren<Image>();

            initialized = true;
        }

        // TODO : Add method to set colors
        // TODO : Add method to update rev
        // TODO : Add method to update speed
        // TODO : Add method to update units
    }
}