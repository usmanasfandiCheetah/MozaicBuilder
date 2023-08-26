using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace MosaicUtility
{
    public class ApiHelper
    {
        public string ApiUrl { get; set; }
        public string ApiMethod { get; set; }

        BackgroundWorker worker;
        RestClient client;

        public ApiHelper()
        {
            client = new RestClient();

        }

        public ApiHelper(string url)
        {
            this.ApiUrl = url;

        }

        public void Start()
        {
            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        public void Stop()
        {
            if (worker != null)
            {
                if (worker.IsBusy)
                {
                    if (!worker.CancellationPending)
                        worker.CancelAsync();
                }
            }
        }

        public object GetData(string methodName, object parameters)
        {
            this.ApiMethod = methodName;

            // /GetImageList/
            try
            {
                //worker.RunWorkerAsync(parameters);
                client = new RestClient(this.ApiUrl);
                RestRequest r = new RestRequest(methodName, Method.POST);

                //r.AddJsonBody(new EventData() { EventID = EventID, ID = Convert.ToInt32(LastId) });
                r.AddJsonBody(parameters);

                var response = client.Execute<List<EventData>>(r);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var data = response.Data;
                    return data;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                File.AppendAllLines("error.log", new string[] { "Get Images error : " + ex.Message });
                return null;
            }
        }

        public void DownloadItem(string serverPathUrl, string pathToDownload)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(serverPathUrl), pathToDownload);

                //OR 

                //client.DownloadFileAsync(new Uri(url), @"c:\temp\image35.png");
            }


        }
    }
}