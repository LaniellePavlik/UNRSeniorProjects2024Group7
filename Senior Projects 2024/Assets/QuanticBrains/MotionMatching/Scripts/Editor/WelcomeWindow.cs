using System;
using UnityEditor;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor
{
    [InitializeOnLoad]
    public class WelcomeWindow : EditorWindow
    {
        private const string Version = "1.3.4";
        private const string Title = "Motion Matching System for Unity";
        private const string ReviewPath = "https://u3d.as/362P#reviews";
        private const string DocumentationURL =
            "https://docs.google.com/document/d/1Z5nf8H7vYYVHvkle0CqVThX3sSzWVuN12gjpDws-y18/edit?usp=drive_link";
        private const string GettingStartedURL = "https://docs.google.com/document/d/147ZonzZyjqQwIJZMZjg00PrugioguF352G75PP6p9ng/edit?usp=drive_link";
        private const string DiscordUrl = "https://discord.gg/hnnwgDyHtq";
        private const string Email = "mailto:support-videogames@quanticbrains.com";
        
        private const string TwitterIconB64 =
            "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAABmJLR0QA/wD/AP+gvaeTAAABzUlEQVQ4jaWSQWsTURSFv/dmdDraWEpdCNEIQotG48aFmxYXLtxZVyJSpJuqBXf+Af+BbkRqoQtLcWnxJ4iCoATaaiISxIKIJhrbppqJmfeui8lMpppa0LN4cC/3nHvevRf+E6pXMjfXyGPNuKBGoiJ5q8RZXJ3KlP8qcGSmPtDW+h5wqYe4AA8930xXJoY2/hDokJ8CJ3ZwveL5ZjQW0XG203knMkDhZ1Pf3eIgN9fIizGvAHXmoMuzjyGhjQpunupjMr+bl58Ny18Mt4sBgGic46tTmbIGEGMuxGJjWZcH5/aS7Y/MXT/pMeApzuZcvgU2aSyYcQA3CtUwIgC8W7dM5j2eXMxQrIZ4Ttd7qZ4IIMJwdwbSYQMvPoVUf1h2aTh9wE0Im23h9VfTVVNIeoiVOF9tCqW6YbMtpHGn2OJ7Kqc6HB09ziOiPTMy6FCqW9ZbknS+9Tzg/korrWe11YvJFgAOza4tAJe1gsJ+h2y/Zi0QijVDEG51IyLzH64OXukOEfB8M91qOgUrFJZqhqWaYRss9+2xN+IgOaTKxNCG55tRhSzE3/kNVkTmPd+M9TzlNA7PNo6JmPOi1VEAQcqu0Y/fX9v3Zjtb/4xfOwe7tBT0mWkAAAAASUVORK5CYII=";
        private const string DiscordIconB64 =
            "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAB1UlEQVQ4jZWTz2sTYRCGn9ls/Y1rQa2xIGm1RwvdQg8tRQgIBc9iDh4E0+Ax4Nn/QQpesvWenAoFISDkErwITUrTPyAUqlbbwHoIsm53PHy7STZasO9xeJ935pvdEcZUWPddlJIKeZRcXO6K0ACpVL3rrVG/DMGfV1DdUOGlDMtjUhQ8oFzznP4gwMDUEVbPIMdjmsBazXP6lqno2/+FTVdZBTYApFD0XYSd5DWZDCw8tLl9U2jthQC48zbfj5V2J+T0NJ5CFYRFW6E0+uZXLy6xsmQD8PzpxVTnT59D3r3/ZaYQQVVLFpBPDNNZawD/SytLNtNZa7SUt2DwqQbwSU+pbgUEAQQBVLcCTnoKwHK6QS4VN5vLALC7H7JdDzg4jDg4jNiuB+zum33cjz2JbKALzAFcu2p28Wh5grt3LB7MmPw3ry8zN2vAxBOrawONJKDdCZm5d4HjnvLlKCKKjOvbj4jJGxZTt4R2JxziQkMKRd9V2BExydkpC3c+w4ePv1OjPnk8QWsv5OuRxhUFWBSAZ0W/IiLrnEOKejXPKSVLLKPaPAfeBMoAFkBt0+krrKmqh+rZmCqq6qmaOwD+PrtC0XeBkpofLBcbuphlV6qbTuqc/wAwW6w+yzHv8gAAAABJRU5ErkJggg==";

        private const string standardPath = "Assets/QuanticBrains/MotionMatching/Materials/MaterialLoader/standardMaterials.unitypackage";
        private const string urpPath = "Assets/QuanticBrains/MotionMatching/Materials/MaterialLoader/urpMaterials.unitypackage";
        private const string hdrpPath = "Assets/QuanticBrains/MotionMatching/Materials/MaterialLoader/hdrpMaterials.unitypackage";
        
        [MenuItem("Tools/Motion Matching/Welcome")]
        private static void ShowWindow()
        {
            var window = GetWindow<WelcomeWindow>();
            window.titleContent = new GUIContent(Title);
            window.Show();
        }

        private Texture2D Base64ToTexture(string base64)
        {
            var texture2D = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            
            texture2D.LoadImage(Convert.FromBase64String(base64));
            return texture2D;
        }
        
        private static void TryLaunchWindowAtStart()
        {
            var appName = Application.dataPath.Split("/")[^2];
            if (EditorPrefs.GetBool(appName + Version + "_FirstTimeSeen")) return;
            
            ShowWindow();
            EditorPrefs.SetBool(appName + Version + "_FirstTimeSeen", true);
        }
        
        static WelcomeWindow()
        {
            EditorApplication.delayCall += TryLaunchWindowAtStart;
        }
        
        private void OnGUI()
        {
            var size = new Vector2(350, 623);
            minSize = size;
            maxSize = size;

            var reviewButton = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 35, 
                fontSize = 18, 
                richText = true
            };

            var buttonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 25, 
                fontSize = 14
            };

            var centerLabelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            var headerLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 20, 
                fontStyle = FontStyle.Bold, 
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Space(20);
            GUILayout.Label($"Thank you for using \n{Title}!", headerLabelStyle);
            GUILayout.Space(30);
            
            var installedVersion = Version;
            GUILayout.Label($"Installed version: {installedVersion}", centerLabelStyle);
            
            GUILayout.Space(15);
            
            EditorGUILayout.LabelField("Please select your render pipeline to correctly \nvisualize the package materials:",
                EditorStyles.boldLabel, GUILayout.Height(50));
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Standard"))
            {
                AssetDatabase.ImportPackage(standardPath, false);
            }

            if (GUILayout.Button("URP"))
            {
                AssetDatabase.ImportPackage(urpPath, false);
            }
            
            if (GUILayout.Button("HDRP"))
            {
                AssetDatabase.ImportPackage(hdrpPath, false);
            }

            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(30);
            GUILayout.Label($"If you like {Title}, \nplease consider leaving a review.\nIt really helps a lot!", centerLabelStyle);
            GUILayout.Space(15);
            
            if (GUILayout.Button("Leave a review <color=#d54930ff>❤</color>", reviewButton)) 
            {
                Application.OpenURL(ReviewPath);
            }

            GUILayout.Space(20);

            GUILayout.Label("Documentation", headerLabelStyle);
            GUILayout.Space(5);

            if (GUILayout.Button("Getting Started", buttonStyle))
            {
                Application.OpenURL(GettingStartedURL);
            }
            if (GUILayout.Button("Documentation", buttonStyle))
            {
                Application.OpenURL(DocumentationURL);
            }

            GUILayout.Space(20);

            GUILayout.Label("Support", headerLabelStyle);
            GUILayout.Space(5);
            
            /*var twitterTitle = new GUIContent(" Twitter", TwitterIcon);
            if (GUILayout.Button(twitterTitle, mediumButtonStyle))
            {
                Application.OpenURL(Urls.TwitterProfile);
            }*/

            if (GUILayout.Button("E-mail", buttonStyle))
            {
                Application.OpenURL(Email);
            }
            
            var discordTitle = new GUIContent(" Discord", Base64ToTexture(DiscordIconB64));
            if (GUILayout.Button(discordTitle, buttonStyle))
            {
                Application.OpenURL(DiscordUrl);
            }
        }
    }
}
