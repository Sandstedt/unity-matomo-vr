// MIT License
// Copyright(c) 2021 Jonas Boetel
//----------------------------------------
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Lumpn.Matomo.Utils;
using UnityEngine;

namespace Lumpn.Matomo
{
    public static class MatomoSessionExtensions
    {
        public static async UniTask<bool> RecordSystemInfo(this MatomoSession session)
        {
            var parameters = new Dictionary<string, string>
            {
                { "new_visit", "1"},
                { "ua", GetUserAgent(Application.unityVersion, Application.platform) },
                { "lang", LanguageUtils.GetLanguageCode(Application.systemLanguage) },
                { "res", string.Format("{0}x{1}", Screen.width, Screen.height)},
                { "dimension1", SystemInfo.processorType},
                { "dimension2", SystemInfo.graphicsDeviceName},
            };
            
            return await session.Record("SystemInfo", 0f, parameters);
        }

        public static async UniTask<bool> RecordEvent(this MatomoSession session, string eventCategory, string eventAction, string eventName, float eventValue, float time, IReadOnlyDictionary<string, string> parameters, bool debug = false)
        {
            using var request = session.CreateWebRequestEvent(eventCategory, eventAction, eventName, eventValue, (int)time, parameters, debug);
            var asyncOp = await request.SendWebRequest().ToUniTask();
            return asyncOp.responseCode == 204;
        }

        public static async UniTask<bool> Record(this MatomoSession session, string page, float time, IReadOnlyDictionary<string, string> parameters, bool debug = false)
        {
            using var request = session.CreateWebRequest(page, (int)time, parameters, debug);
            var asyncOp = await request.SendWebRequest().ToUniTask();
            return asyncOp.responseCode == 204;
        }

        private static string GetUserAgent(string unityVersion, RuntimePlatform platform)
        {
            return string.Format("UnityPlayer/{0} ({1})", GetUnityVersion(unityVersion), PlatformUtils.GetDevice(platform));
        }

        private static string GetUnityVersion(string unityVersion)
        {
            return unityVersion.Substring(0, 6);
        }
    }
}
