using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ImageMagick;

namespace images.functions.zapread.com
{
    public static class Gif
    {
        [FunctionName("GifResize")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string responseMessage = "This HTTP triggered function executed successfully. ";

            string strSize = req.Query["size"];

            int size = 200;
            if (!String.IsNullOrEmpty(strSize))
            {
                int.TryParse(strSize, out size);
            }

            if (req.Method == "GET")
            {
                string name = req.Query["name"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic reqdata = JsonConvert.DeserializeObject(requestBody);
                responseMessage += " GET ";
            }

            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";
            
            try
            {
                var formdata = await req.ReadFormAsync();
                var file = req.Form.Files["file"];

                if (file != null)
                {
                    string msg = $"received file {file.FileName} with length {file.Length.ToString()}";
                    responseMessage += msg;
                    log.LogInformation(msg);
                }

                byte[] data;

                //string _FileName = Path.GetFileName(file.FileName);
                //Image img = Image.FromStream(file.OpenReadStream());
                //byte[] imageData;
                //string contentType = "image/jpeg";
                int maxwidth = size;

                //string contentType = "image/gif";
                ImageMagick.ResourceLimits.LimitMemory(new Percentage(10)); // Don't go wild here!
                                                                            // based on https://github.com/dlemstra/Magick.NET/blob/main/docs/ResizeImage.md
                
                using (var collection = new MagickImageCollection(file.OpenReadStream(), MagickFormat.Gif))
                {
                    // This will remove the optimization and change the image to how it looks at that point
                    // during the animation. More info here: http://www.imagemagick.org/Usage/anim_basics/#coalesce
                    collection.Coalesce();
                    // Resize each image in the collection. When zero is specified for the height
                    // the height will be calculated with the aspect ratio.
                    //if (img.Width > maxwidth)
                    //{
                    foreach (var image in collection)
                    {
                        image.Resize(maxwidth, 0);
                    }
                    //}

                    collection.Optimize();
                    // stream from ImageMagick to a byte array
                    using (var ms = new MemoryStream())
                    {
                        collection.Write(ms);
                        //Image resizedGif = Image.FromStream(ms);
                        data = ms.ToArray();//resizedGif.ToByteArray(ImageFormat.Gif);

                        return new FileContentResult(data, "image/gif");
                    }
                }
            }
            catch (Exception ex)
            {
                responseMessage += " Error reading file.";
                // don't panic
            }            

            return new OkObjectResult(responseMessage);
        }
    }
}
