//----------------------------------------
// MIT License
// Copyright(c) 2021 Jonas Boetel
//----------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using Lumpn.Matomo.Utils;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace Lumpn.Matomo
{
    public sealed class MatomoSession
    {
        private readonly Random random = new Random();
        private readonly StringBuilder stringBuilder = new StringBuilder();
        private readonly string baseUrl;
        private readonly string trackingVersion;

        public static MatomoSession Create(string matomoUrl, string websiteUrl, int websiteId, byte[] userId, int trackingVersion)
        {
            var sb = new StringBuilder(matomoUrl);
            sb.Append("/matomo.php?apiv=1&rec=1&send_image=0&idsite=");
            sb.Append(websiteId);

            sb.Append("&_id=");
            HexUtils.AppendHex(sb, userId);

            sb.Append("&url=");
            sb.Append(EscapeDataString(websiteUrl));
            sb.Append(EscapeDataString("/"));

            var url = sb.ToString();
            return new MatomoSession(url, trackingVersion);
        }

        private MatomoSession(string baseUrl, int trackingVersion)
        {
            this.baseUrl = baseUrl;
            this.trackingVersion = "v" + trackingVersion;
        }

        public UnityWebRequest CreateWebRequest(string page, int time, IReadOnlyDictionary<string, string> parameters, bool debug)
        {
            var url = BuildUrl(page, time, parameters, debug);

            var downloadHandler = debug ? new DownloadHandlerBuffer() : null;
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, downloadHandler, null);
            return request;
        }
        
        public UnityWebRequest CreateWebRequestEvent(string eventCategory, string eventAction, string eventName, float eventValue, int time, IReadOnlyDictionary<string, string> parameters, bool debug)
        {
            var url = BuildEventString(eventCategory, eventAction, eventName, eventValue, time, parameters, debug);

            var downloadHandler = debug ? new DownloadHandlerBuffer() : null;
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, downloadHandler, null);
            return request;
        }

        private string BuildUrl(string page, int time, IReadOnlyDictionary<string, string> parameters, bool debug)
        {
            var sb = stringBuilder;
            sb.Clear();

            sb.Append(baseUrl);
            sb.Append(trackingVersion);
            sb.Append(EscapeDataString("/"));
            sb.Append(EscapeDataString(page));

            sb.Append("&pf_srv=");
            sb.Append(time);

            foreach (var parameter in parameters)
            {
                sb.Append("&");
                sb.Append(parameter.Key);
                sb.Append("=");
                sb.Append(EscapeDataString(parameter.Value));
            }

            if (debug)
            {
                sb.Append("&debug=1");
            }

            sb.Append("&rand=");
            sb.Append(random.Next());

            var url = sb.ToString();
            sb.Clear();

            if (debug) Debug.Log(url);
            return url;
        }
        
        private string BuildEventString(string eventCategory, string eventAction, string eventName, float eventValue, int time, IReadOnlyDictionary<string, string> parameters, bool debug)
        {
            var sb = stringBuilder;
            sb.Clear();

            sb.Append(baseUrl);
            sb.Append("&e_c=");
            sb.Append(eventCategory);
            sb.Append("&e_a=");
            sb.Append(eventAction);
            sb.Append("&e_n=");
            sb.Append(eventName);
            sb.Append("&e_v=");
            sb.Append(eventValue);
            
            sb.Append("&pf_srv=");
            sb.Append(time);

            foreach (var parameter in parameters)
            {
                sb.Append("&");
                sb.Append(parameter.Key);
                sb.Append("=");
                sb.Append(EscapeDataString(parameter.Value));
            }

            if (debug)
            {
                sb.Append("&debug=1");
            }

            sb.Append("&rand=");
            sb.Append(random.Next());

            var url = sb.ToString();
            sb.Clear();

            if (debug) Debug.Log(url);
            return url;
        }

        private static string EscapeDataString(string str)
        {
            return Uri.EscapeDataString(str);
        }
    }
}
