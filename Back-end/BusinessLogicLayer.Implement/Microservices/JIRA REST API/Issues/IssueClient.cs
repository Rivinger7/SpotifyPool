using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SetupLayer.Setting.Microservices.Jira;

namespace BusinessLogicLayer.Implement.Microservices.JIRA_REST_API.Issues
{
    public class IssueClient
    {
        private readonly string? _domain;
        private readonly HttpClient _httpClient;
        private readonly ILogger<IssueClient> _logger;

        public IssueClient(JiraSetting jiraSettings, ILogger<IssueClient> logger)
        {
            _domain = jiraSettings.Domain;

            _httpClient = new HttpClient();
            var authToken = Encoding.ASCII.GetBytes($"{jiraSettings.UserName}:{jiraSettings.ApiKey}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _logger = logger;
        }

        #region Create Issue
        public async Task<string?> CreateIssue(string projectKey, string issueType, string summary, string description)
        {
            try
            {
                var baseUrl = $"https://{_domain}.atlassian.net";
                var url = $"{baseUrl}/rest/api/2/issue";

                var data = new
                {
                    fields = new
                    {
                        project = new { key = projectKey },
                        summary = summary,
                        description = description,
                        issuetype = new { name = issueType }
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                string? responseBody = await response.Content.ReadAsStringAsync();
                dynamic? responseJson = JsonConvert.DeserializeObject(responseBody);

                // Log the issue data if needed
                _logger.LogInformation("Issue data created: {IssueData}", responseBody);

                return responseJson?.key;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e.Message, e);
                return null;
            }
        }
        #endregion

        #region Get Issue By ID
        public async Task<dynamic?> GetIssueByID(string issueKey)
        {
            try
            {
                var baseUrl = $"https://{_domain}.atlassian.net";
                var url = $"{baseUrl}/rest/api/2/issue/{issueKey}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string? responseBody = await response.Content.ReadAsStringAsync();
                dynamic? issue = JsonConvert.DeserializeObject(responseBody);

                // Log the issue data if needed
                _logger.LogInformation("Issue data retrieved: {IssueData}", responseBody);

                return issue;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError("Error retrieving issue: {Message}", e.Message);
                return null;
            }
        }
        #endregion

        #region Get All Issues
        public async Task<dynamic?> GetIssues()
        {
            try
            {
                var baseUrl = $"https://{_domain}.atlassian.net";
                var url = $"{baseUrl}/rest/api/2/search";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string? responseBody = await response.Content.ReadAsStringAsync();
                dynamic? issues = JsonConvert.DeserializeObject(responseBody);

                // Log the issues data if needed
                _logger.LogInformation("Issues data retrieved: {IssuesData}", responseBody);

                return issues;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError("Error retrieving issues: {Message}", e.Message);
                return null;
            }
        }
        #endregion

        #region Get All Users
        public async Task<dynamic?> GetUsers()
        {
            try
            {
                var baseUrl = $"https://{_domain}.atlassian.net";
                var url = $"{baseUrl}/rest/api/2/users";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string? responseBody = await response.Content.ReadAsStringAsync();
                dynamic? users = JsonConvert.DeserializeObject(responseBody);

                // Log the users data if needed
                _logger.LogInformation("Users data retrieved: {UsersData}", responseBody);

                return users;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError("Error retrieving users: {Message}", e.Message);
                return null;
            }
        }
        #endregion

        #region Delete Issue By ID
        public async Task<bool> DeleteIssueByID(string issueKey)
        {
            #region How to use Delete Method
            //        var issueClient = new IssueClient(configuration, logger);
            //        bool isDeleted = await issueClient.DeleteIssueByIdAsync("SP-8");

            //        if (isDeleted)
            //        {
            //          Console.WriteLine("Issue deleted successfully.");
            //        }
            //        else
            //        {
            //          Console.WriteLine("Failed to delete issue.");
            //        }
            #endregion
            try
            {
                var baseUrl = $"https://{_domain}.atlassian.net";
                var url = $"{baseUrl}/rest/api/2/issue/{issueKey}";

                var response = await _httpClient.DeleteAsync(url);
                response.EnsureSuccessStatusCode();

                // Log the deletion result
                _logger.LogInformation("Issue {IssueKey} deleted successfully.", issueKey);

                return true;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError("Error deleting issue: {Message}", e.Message);
                return false;
            }
        }
        #endregion
    }
}

