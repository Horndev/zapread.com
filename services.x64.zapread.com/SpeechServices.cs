using Microsoft.CognitiveServices.Speech;
using System;
using System.Threading.Tasks;

namespace services.x64.zapread.com
{
    /// <summary>
    /// 
    /// </summary>
    public class SpeechServices
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="captchaCode"></param>
        /// <param name="key"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        public static async Task<byte[]> GetAudio(string captchaCode, string key, string region)
        {

            var config = SpeechConfig.FromSubscription(key, region);

            var voiceName = "en-CA-LiamNeural";
            config.SpeechSynthesisLanguage = "en-CA";
            config.SpeechSynthesisVoiceName = voiceName;
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

            // insert spaces between characters
            var captchaCodeSpaced = String.Join(" ", captchaCode.ToCharArray());

            using (var synthesizer = new SpeechSynthesizer(config, null))
            {
                string ssml = "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-US\">" +
                                "  <voice name=\"" + voiceName + "\">" +
                                "    <prosody rate=\"-40.00%\">" +
                                "      <say-as interpret-as=\"characters\">" +
                                            captchaCodeSpaced +
                                "      </say-as>" +
                                "    </prosody>" +
                                "  </voice>" +
                                "</speak>";
                var result = await synthesizer.SpeakSsmlAsync(ssml);

                return result.AudioData;
            }
        }
    }
}
