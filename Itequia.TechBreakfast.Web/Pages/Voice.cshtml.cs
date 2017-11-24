using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Itequia.TechBreakfast.Web.Pages
{
    public class VoiceModel : PageModel
    {

        public void OnGet()
        {
        }

        [HttpPost]
        public IActionResult OnPost()
        {

            var audioFile = Request.Form.Files[0];

            var requestUri = "https://speech.platform.bing.com/speech/recognition/interactive/cognitiveservices/v1?language=es-ES&format=simple";
            HttpWebRequest request = null;
            request = (HttpWebRequest)HttpWebRequest.Create(requestUri);
            request.SendChunked = true;
            request.Accept = @"application/json;text/xml";
            request.Method = "POST";
            request.ProtocolVersion = HttpVersion.Version11;
            request.ContentType = @"audio/wav; codec=audio/pcm; samplerate=16000";
            request.Headers["Ocp-Apim-Subscription-Key"] = "8f3b5a5da78f43dba04a38e8f604c1a3";

            using (Stream requestStream = request.GetRequestStream())
            {
                byte[] audioBytes = null;
                BinaryReader reader = new BinaryReader(audioFile.OpenReadStream());
                audioBytes = reader.ReadBytes((int)audioFile.Length);

                requestStream.Write(audioBytes, 0, (int)audioFile.Length);
                requestStream.Flush();
            }

            var responseString = "";
            using (WebResponse response = request.GetResponse())
            {
                Console.WriteLine(((HttpWebResponse)response).StatusCode);

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    responseString = sr.ReadToEnd();
                }
            }

            return new JsonResult(responseString);
        }
    }
}
