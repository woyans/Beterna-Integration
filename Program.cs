﻿using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beterna_Integration
{
    class Program
    {
        static void Main(string[] args)
        {
            //https://ad-ctp-10-38eb6867baef10230aos.cloudax.dynamics.com/soap/services/TSTimesheetServices?singleWsdl
            //https://github.com/tomogricnik/TestProject/blob/master/Program.cs
          
            string token = Authenticate();

            long worker = 5637145078;
            string date = "2021-11-25";
            string hours = "4";
            string project = "00000101";
            string activity = "W00002480";
            string dataAreaId = "ussi";

            var timesheetNbr = WriteEntry(token, worker, date);
            WriteDetails(token, worker, date, timesheetNbr, project, activity, hours, dataAreaId);

            date = "2021-11-26";
            hours = "5";
            activity = "W00002388";

            timesheetNbr = WriteEntry(token, worker, date);
            WriteDetails(token, worker, date, timesheetNbr, project, activity, hours, dataAreaId);
           
            Console.ReadLine();
        }



        private static string Authenticate()
        {
            string id = "abcd3f06-f260-48f7-adad-6ae62a81374f";
            string secret = "KbYg0_S.E88~8l.395.w3oONN_lO~pBg98";
            string resource = "00000015-0000-0000-c000-000000000000";

            var client = new RestClient("https://login.windows.net/adacta-group.com/oauth2/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials&scope=all&client_id=" + id + "&client_secret=" + secret + "&resource=" + resource, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            dynamic resp = JObject.Parse(response.Content);
            string token = resp.access_token;

            return token;
        }


        private static string WriteEntry(string token, long worker, string date)
        {

            var client = new RestClient("https://ad-ctp-10-38eb6867baef10230aos.cloudax.dynamics.com/api/services/TSTimesheetServices/TSTimesheetSubmissionService/findOrCreateTimesheet");
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new { _resource = worker, _timesheetDate = date });

            var response = client.Execute(request);
            dynamic resp = JObject.Parse(response.Content);

            Console.WriteLine($"StatusCode: {response.StatusCode}, ErrorMessage: {response.ErrorMessage}, Content: {response.Content}");

            var timesheetNbr = resp.parmTimesheetNbr;

            return timesheetNbr;
        }

        private static void WriteDetails(string token, long worker, string date, string timesheetNbr, string project, string activity, string hours, string dataAreaId)
        {
            var entry = new
            {
                parmResource = worker,
                parmTimesheetNumber = timesheetNbr,
                parmProjectDataAreaId = dataAreaId,
                parmProjId = project,
                parmProjActivityNumber = activity,
                parmEntryDate = date,
                parmHrsPerDay = hours
            };

            object[] eList = new object[1];
            eList[0] = entry;

            var entryList = new
            {
                entryList = eList
            };

            var tsEntryList = new
            {
                _tsTimesheetEntryList = entryList
            };

            var client = new RestClient("https://ad-ctp-10-38eb6867baef10230aos.cloudax.dynamics.com/api/services/TSTimesheetServices/TSTimesheetSubmissionService/createOrUpdateTimesheetLine");
            var request = new RestRequest(Method.POST);
            request.AddHeader("authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "application/json");

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(tsEntryList);

            IRestResponse response = client.Execute(request);
            dynamic resp = JObject.Parse(response.Content);

            Console.WriteLine($"StatusCode: {response.StatusCode}, ErrorMessage: {response.ErrorMessage}, Content: {response.Content}");
        }

    }

}
