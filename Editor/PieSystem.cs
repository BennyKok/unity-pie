using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace BennyKok.Pie.Editor
{
    [InitializeOnLoad]
    public class PieSystem
    {
        private const int Radius = 80;
        private const KeyCode shortcutKey = KeyCode.A;
        private static SceneView targetSceneView;
        private static VisualElement targetSceneRoot;
        private static VisualElement pieRoot;
        private static int lastInstanceID;

        private static Vector2 size = new Vector2(100, 100);

        private static Pie rootPie = new Pie("root");
        private static Pie currentPie;

        public static bool IsVisible => pieRoot?.style.display == DisplayStyle.Flex;

        public class Pie
        {
            public string path;
            public Pie parentPie;

            public Pie(string path)
            {
                this.path = path;
            }

            public List<Pie> subPie = new List<Pie>();
            public Action onTrigger;
        }

        // [Shortcut("PieSystem.Open", KeyCode.A)]
        private static void TogglePie()
        {
            if (!(pieRoot is null))
            {
                pieRoot.style.display = pieRoot.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;

                if (IsVisible)
                {
                    Vector2 mousePosition = Event.current.mousePosition; //GUIUtility.ScreenToGUIPoint(Event.current.mousePosition);
                    // mousePosition += targetSceneView.position.min;
                    // mousePosition -= new Vector2(size.x, size.y) / 2;
                    // mousePosition.y += 20;
                    // mousePosition.y += size.y;

                    mousePosition -= new Vector2(20, 0);
                    pieRoot.transform.position = mousePosition;

                    RefreshPie();

                    targetSceneView.Repaint();
                }
            }
        }

        private static void RefreshPie()
        {
            pieRoot.Clear();

            if (currentPie.parentPie is { })
            {
                pieRoot.Add(NewPieButton("<", () => OpenPie(currentPie.parentPie)));
            }

            foreach (var menu in currentPie.subPie)
            {
                pieRoot.Add(NewPieButton(menu.subPie.Count > 0 ? menu.path + ">" : menu.path, menu.onTrigger));
            }

            int i = 0;
            foreach (var item in pieRoot.Children())
            {
                item.transform.position = Vector3.zero;
                item.experimental.animation.Position(CirclePoint(Vector3.zero, Radius, i, pieRoot.childCount), 120);
                i++;
            }
        }

        private static Button NewPieButton(string label, Action callback)
        {
            return new Button(callback)
            {
                text = label,
                style = {
                            position = Position.Absolute,
                            paddingLeft = 10,
                            paddingRight = 10,
                            paddingTop = 6,
                            paddingBottom = 6,
                        },
            };
        }

        static PieSystem()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;

            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static double lastPieOpenTime;
        private static Vector2 lastMousePosition;

        private static void OnSceneGUI(SceneView view)
        {
            if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown || Event.current.button == 1))
            {
                if (IsVisible)
                    TogglePie();
            }
            else
            {
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == shortcutKey && !IsVisible)
                {
                    lastPieOpenTime = EditorApplication.timeSinceStartup;
                    lastMousePosition = Event.current.mousePosition;
                    TogglePie();
                }

                if (Event.current.type == EventType.KeyUp && Event.current.keyCode == shortcutKey && IsVisible)
                {
                    if (EditorApplication.timeSinceStartup - lastPieOpenTime > 0.2)
                    {
                        var x1 = lastMousePosition;
                        var x2 = Event.current.mousePosition;

                        // If we moved some amount, we may want to invoke that action
                        if ((Vector3.Distance(EditorGUIUtility.PixelsToPoints(x1), EditorGUIUtility.PixelsToPoints(x2))) > 20)
                        {
                            float xDiff = x1.x - x2.x;
                            float yDiff = x1.y - x2.y;
                            var angle = (180 + (float)Math.Atan2(yDiff, xDiff) * (float)(180 / Math.PI) - 90);
                            angle = 360 - angle;
                            angle = Mathf.Abs(angle) % 360;

                            var count = currentPie.subPie.Count;
                            // Counter for the back button
                            if (currentPie.parentPie is { }) count++;

                            var index = Mathf.RoundToInt(angle / (360 / count)) % count;
                            // Debug.Log(index);
                            // Debug.Log(angle);

                            // Counter for the back button
                            if (currentPie.parentPie is { }) index--;

                            if (index == -1)
                            {
                                OpenPie(currentPie.parentPie);
                            }
                            else
                            {
                                var selected = currentPie.subPie[index];
                                selected.onTrigger();
                            }
                        }
                        // Else, close the pie instead
                        else
                        {
                            TogglePie();
                        }
                    }
                }
            }
        }

        static void OnUpdate()
        {
            targetSceneView = SceneView.currentDrawingSceneView ?? SceneView.lastActiveSceneView;

            if (targetSceneView && targetSceneView.GetInstanceID() != lastInstanceID)
            {
                lastInstanceID = targetSceneView.GetInstanceID();

                CleanUpPreviousPie();
            }

            if (pieRoot is null && targetSceneRoot is null && targetSceneView is { })
            {
                targetSceneRoot = targetSceneView.rootVisualElement;

                if (targetSceneRoot is { })
                    Init();
                else
                    Debug.Log("targetSceneRoot is Null");
            }
        }

        private static void CleanUpPreviousPie()
        {
            if (!(pieRoot is null))
            {
                pieRoot.RemoveFromHierarchy();
                pieRoot = null;
            }
        }

        private static Vector2 CirclePoint(Vector2 center, float radius, float xIndex, float xCount)
        {
            var angle = xIndex / xCount * 360f;
            var pos = new Vector2
            {
                x = radius * Mathf.Sin(angle * Mathf.Deg2Rad),
                y = radius * Mathf.Cos(angle * Mathf.Deg2Rad),
            };
            // Debug.Log(angle);
            return pos;
        }

        private static void Init()
        {
            if (pieRoot is { })
                CleanUpPreviousPie();

            // var methods = AppDomain.CurrentDomain.GetAssemblies()
            //                        .SelectMany(x => x.GetTypes())
            //                        .Where(x => x.IsClass)
            //                        .SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            //                        .Where(x => x.GetCustomAttributes(typeof(PieMenuAttribute), false).FirstOrDefault() != null);

            var methods = TypeCache.GetMethodsWithAttribute<PieMenuAttribute>();

            pieRoot = new VisualElement()
            {
                style = {
                position = Position.Absolute,
                }
            };
            pieRoot.style.width = size.x;
            pieRoot.style.height = size.y;
            pieRoot.style.display = DisplayStyle.None;
            // pieRoot.style.backgroundColor = Color.black;
            targetSceneRoot.Add(pieRoot);

            rootPie.subPie.Clear();
            foreach (var method in methods)
            {
                var attribute = (PieMenuAttribute)method.GetCustomAttributes(typeof(PieMenuAttribute), false).First();
                // Debug.Log(attribute.path);

                CreatePie(attribute.path, attribute, method);
            }
            currentPie = rootPie;
        }

        private static void CreatePie(string fullPath, PieMenuAttribute attr, MethodInfo info)
        {
            var paths = fullPath.Split('/');
            var pie = rootPie;

            // Create the sub pie for each path segments
            if (paths.Length > 1)
            {
                for (int i = 0; i < paths.Length - 1; i++)
                {
                    var path = paths[i];

                    // Look for existing pie path
                    var tempPie = pie.subPie.Find(x => x.path == path);

                    // Is null, lets create a new one
                    if (tempPie is null)
                    {
                        tempPie = new Pie(path);
                        tempPie.onTrigger = () => OpenPie(tempPie);
                        tempPie.parentPie = pie;
                        pie.subPie.Add(tempPie);
                    }

                    pie = tempPie;
                }
            }

            // Create the final target pie
            var targetPie = new Pie(paths.Last());
            targetPie.onTrigger = () =>
            {
                TogglePie();
                info.Invoke(null, null);
            };
            pie.subPie.Add(targetPie);
        }
        public static void OpenPie(Pie targetPie)
        {
            currentPie = targetPie;
            RefreshPie();
        }
    }
}