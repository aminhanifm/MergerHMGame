using UnityEditor;
using UnityEngine;

namespace Ilumisoft.Editor.GameTemplate
{
    public static class MenuItems
    {
        [MenuItem("Game Template/Welcome")]
        static void OpenPackageUtility()
        {
            PackageUtilityWindow.Init();
        }

        [MenuItem("Game Template/Rate")]
        static void Rate()
        {
            var bundleInfo = ScriptableObjectUtility.Find<PackageInfo>();

            if(bundleInfo != null)
            {
                Application.OpenURL(bundleInfo.PackageURL);
            }
        }
    }
}