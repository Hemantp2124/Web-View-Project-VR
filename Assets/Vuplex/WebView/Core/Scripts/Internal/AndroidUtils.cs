// Copyright (c) 2023 Vuplex Inc. All rights reserved.
//
// Licensed under the Vuplex Commercial Software Library License, you may
// not use this file except in compliance with the License. You may obtain
// a copy of the License at
//
//     https://vuplex.com/commercial-library-license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace Vuplex.WebView.Internal {

    /// <summary>
    /// Static utility methods used internally by 3D WebView on Android.
    /// </summary>
    public static class AndroidUtils {

        public static void AssertMainThread(string methodName) {

            // Unity's AndroidJavaObject and AndroidJavaClass APIs can only be called on the Unity main thread
            // and result in cryptic JNI errors if called from other threads.
            if (!ThreadDispatcher.CurrentlyOnMainThread) {
                var publicMethodName = methodName[0].ToString().ToUpper() + methodName.Substring(1);
                throw new InvalidOperationException($"{publicMethodName}() can only be called on the Unity main thread but was called from a background thread instead.");
            }
        }

        public static Material CreateAndroidMaterial() {

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan) {
                return VXUtils.CreateDefaultMaterial();
            }
            // Construct a new material because Resources.Load<T>() returns a singleton.
            return new Material(Resources.Load<Material>("AndroidWebMaterial"));
        }

        public static byte[] ConvertFromJavaByteArray(AndroidJavaObject arrayObject) {

            // Unity 2019.1 and newer logs a warning that converting from byte[] is obsolete
            // but older versions are incapable of converting from sbyte[].
            #if UNITY_2019_1_OR_NEWER
                return (byte[])(Array)AndroidJNIHelper.ConvertFromJNIArray<sbyte[]>(arrayObject.GetRawObject());
            #else
                return AndroidJNIHelper.ConvertFromJNIArray<byte[]>(arrayObject.GetRawObject());
            #endif
        }

        public static bool DeviceIsMetaQuest() {

            #if UNITY_2017_2_OR_NEWER
                // Some devices are now named "Meta Quest 2" instead of "Oculus Quest 2".
                var name = SystemInfo.deviceName;
                return name.Contains("Meta Quest") || name.Contains("Oculus Quest");
            #else
                return false;
            #endif
        }

        public static bool ScreenHasNotchAndRenderOutsideSafeAreaIsDisabled() {

            if (_cachedScreenHasNotchAndRenderOutsideSafeAreaIsDisabled == null) {
                _cachedScreenHasNotchAndRenderOutsideSafeAreaIsDisabled = _screenHasNotchAndRenderOutsideSafeAreaIsDisabled();
            }
            return (bool)_cachedScreenHasNotchAndRenderOutsideSafeAreaIsDisabled;
        }

        public static void RunOnAndroidUIThread(Action function) {

            _getActivity().Call("runOnUiThread", new AndroidJavaRunnable(function));
        }

        public static void ThrowVulkanExtensionException() {

            throw new InvalidOperationException("The Vulkan Graphics API is in use, but this device does not support the VK_ANDROID_external_memory_android_hardware_buffer Vulkan API required by 3D WebView. Please switch to the OpenGLES Graphics API in Player Settings. For more info, see this page: https://support.vuplex.com/articles/vulkan");
        }

        public static AndroidJavaObject ToJavaMap(Dictionary<string, string> dictionary) {

            AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");
            IntPtr putMethod = AndroidJNIHelper.GetMethodID(map.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
            foreach (var entry in dictionary) {
                AndroidJNI.CallObjectMethod(
                    map.GetRawObject(),
                    putMethod,
                    AndroidJNIHelper.CreateJNIArgArray(new object[] { entry.Key, entry.Value })
                );
            }
            return map;
        }

        public static AndroidJavaObject ToJavaObject(IntPtr jobject) {

            if (jobject == IntPtr.Zero) {
                return null;
            }
            return (AndroidJavaObject)_androidJavaObjectIntPtrConstructor.Invoke(new object[] { jobject });
        }

        // Get a reference to AndroidJavaObject's hidden constructor that takes
        // the IntPtr for a jobject as a parameter.
        readonly static ConstructorInfo _androidJavaObjectIntPtrConstructor = typeof(AndroidJavaObject).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new []{ typeof(IntPtr) },
            null
        );

        static bool? _cachedScreenHasNotchAndRenderOutsideSafeAreaIsDisabled;
        static AndroidJavaClass _unityPlayerClass;

        static AndroidJavaObject _getActivity() {

            if (_unityPlayerClass == null) {
                _unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            }
            return _unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        }

        static bool _screenHasNotchAndRenderOutsideSafeAreaIsDisabled() {

            using (var version = new AndroidJavaClass("android.os.Build$VERSION")) {
                if (version.GetStatic<int>("SDK_INT") < 28) {
                    // layoutInDisplayCutoutMode and getDisplayCutout both require API level 28.
                    return false;
                }
            }
            using (var window = _getActivity().Call<AndroidJavaObject>("getWindow"))
            using (var attributes = window.Call<AndroidJavaObject>("getAttributes")) {
                var mode = attributes.Get<int>("layoutInDisplayCutoutMode");
                // 0 = LAYOUT_IN_DISPLAY_CUTOUT_MODE_DEFAULT
                // https://developer.android.com/reference/android/view/WindowManager.LayoutParams#LAYOUT_IN_DISPLAY_CUTOUT_MODE_DEFAULT
                var renderOutsideSafeAreaIsDisabled = mode == 0;
                if (!renderOutsideSafeAreaIsDisabled) {
                    return false;
                }
            }
            // It's not possible to detect to detect if a screen has a cutout using Screen.safeArea or Screen.cutouts
            // because when "Render outside safe area" is disabled, Screen.cutouts is empty.
            // Also, activity.getWindow().getDecorView().getRootWindowInsets().getDisplayCutout() isn't used
            // because it returns null when accessed outside of onAttachedToWindow() when the app is fullscreen.
            // https://stackoverflow.com/a/61264362
            using (var windowManager = _getActivity().Call<AndroidJavaObject>("getWindowManager"))
            using (var display = windowManager.Call<AndroidJavaObject>("getDefaultDisplay"))
            using (var cutout = display.Call<AndroidJavaObject>("getCutout")) {
                var displayHasCutout = cutout != null;
                return displayHasCutout;
            }
        }
    }
}
#endif
