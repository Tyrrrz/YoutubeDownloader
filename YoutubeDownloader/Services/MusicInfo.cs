using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YoutubeDownloader.Services
{
    public static class ShazamMusicInfo
    {
        public static bool TryExtractArtistAndTitle(
            string videoTitle,
            List<string> shazamapikeys,
            List<string> vagalumeapikeys,
            out string artist,
            out string title,
            out string picturelink,
            out string track)
        {
            foreach (var key in shazamapikeys)
            {
                var json = GetShazaminfo(videoTitle, key);
                if (!string.IsNullOrEmpty(json))
                {
                    var data = JsonConvert.DeserializeObject<Root>(json);
                    if (data.tracks != null)
                    {
                        artist = data.tracks.hits.FirstOrDefault()?.track.subtitle;
                        title = data.tracks.hits.FirstOrDefault()?.track.title;
                        picturelink = data.tracks.hits.FirstOrDefault()?.track.images.coverarthq;
                        track = null;
                        return true;
                    }
                }
            }
            foreach (var key in vagalumeapikeys)
            {
                var json = Getvagalumeinfo(videoTitle, key);
                if (!string.IsNullOrEmpty(json))
                {
                    var data = JsonConvert.DeserializeObject<vagalume>(json);
                    if (data?.response?.docs != null && data?.response?.docs?.Count > 0)
                    {
                        var infofound = data.response.docs.OrderBy(t => string.IsNullOrEmpty(t.title)).ThenByDescending(t => t?.fmRadios?.Count ?? 0).FirstOrDefault();
                        artist = infofound.band;
                        title = infofound.title;
                        picturelink = null;
                        track = null;
                        return true;
                    }
                }
            }
            artist = null;
            title = null;
            picturelink = null;
            track = null;
            return false;
        }

        public static string GetShazaminfo(string name, string apikey)
        {
            apikey = apikey.Trim();
            string search = System.Net.WebUtility.HtmlEncode(name);
            var client = new RestClient("https://shazam.p.rapidapi.com/search?term=" + search + "&locale=en-US&offset=0&limit=5");
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-key", apikey);
            request.AddHeader("x-rapidapi-host", "shazam.p.rapidapi.com");
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
                return response.Content;
            else return null;
        }

        public static string Getvagalumeinfo(string name, string apikey)
        {
            apikey = apikey.Trim();
            string search = System.Net.WebUtility.HtmlEncode(name);
            var client = new RestClient($"https://api.vagalume.com.br/search.artmus?apikey={apikey}&q={search}&limit=2");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            if (response.IsSuccessful)
                return response.Content;
            else return null;
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class Share
        {
            public string subject { get; set; }
            public string text { get; set; }
            public string href { get; set; }
            public string image { get; set; }
            public string twitter { get; set; }
            public string html { get; set; }
            public string avatar { get; set; }
            public string snapchat { get; set; }
        }

        public class Images
        {
            public string background { get; set; }
            public string coverart { get; set; }
            public string coverarthq { get; set; }
            public string joecolor { get; set; }
            public string overflow { get; set; }
            public string @default { get; set; }
        }

        public class Action
        {
            public string name { get; set; }
            public string type { get; set; }
            public string id { get; set; }
            public string uri { get; set; }
        }

        public class Beacondata
        {
            public string type { get; set; }
            public string providername { get; set; }
        }

        public class Option
        {
            public string caption { get; set; }
            public List<Action> actions { get; set; }
            public Beacondata beacondata { get; set; }
            public string image { get; set; }
            public string type { get; set; }
            public string listcaption { get; set; }
            public string overflowimage { get; set; }
            public bool colouroverflowimage { get; set; }
            public string providername { get; set; }
        }

        public class Provider
        {
            public string caption { get; set; }
            public Images images { get; set; }
            public List<Action> actions { get; set; }
            public string type { get; set; }
        }

        public class Hub
        {
            public string type { get; set; }
            public string image { get; set; }
            public List<Action> actions { get; set; }
            public List<Option> options { get; set; }
            public List<Provider> providers { get; set; }
            public bool @explicit { get; set; }
            public string displayname { get; set; }
        }

        public class Artist
        {
            public string id { get; set; }
            public string adamid { get; set; }
            public List<Hit> hits { get; set; }
        }

        public class Track
        {
            public string layout { get; set; }
            public string type { get; set; }
            public string key { get; set; }
            public string title { get; set; }
            public string subtitle { get; set; }
            public Share share { get; set; }
            public Images images { get; set; }
            public Hub hub { get; set; }
            public List<Artist> artists { get; set; }
            public string url { get; set; }
        }

        public class Hit
        {
            public Track track { get; set; }
            public Artist artist { get; set; }
        }

        public class Tracks
        {
            public List<Hit> hits { get; set; }
        }

        public class Artists
        {
            public List<Artist2> hits { get; set; }
        }

        public class Artist2
        {
            public string avatar { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public bool verified { get; set; }
            public string adamid { get; set; }
        }

        public class Root
        {
            public Tracks tracks { get; set; }
            public Artists artists { get; set; }
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class Doc
        {
            public string id { get; set; }
            public string url { get; set; }
            public string band { get; set; }
            public int? langID { get; set; }
            public string title { get; set; }
            public List<string> fmRadios { get; set; }
        }
        public class Response
        {
            public int numFound { get; set; }
            public int start { get; set; }
            public bool numFoundExact { get; set; }
            public List<Doc> docs { get; set; }
        }

        public class Highlighting
        {

        }

        public class vagalume
        {
            public Response response { get; set; }
            public Highlighting highlighting { get; set; }
        }

    }
}
