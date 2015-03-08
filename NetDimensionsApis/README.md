# Talent Suite API Client Library for .NET

The Talent Suite API Client Library for .NET is intended to provide straightforward access to the Talent
Suite API for .NET applications.

The sample C# code below retrieves **joestudent**'s learning path and prints the name of each assigned job
profile to the console.

```c#
string url = "https://www.example.com/ekp/";  // base URL of site
string userId = "myapp";  // ID of privileged user (with Switch User permission)
string password = "changeme";  // password of privileged user (with Switch User permission)
string onBehalfOf = "joestudent";  // ID of the end user
Client client = Client.From(url, new NetworkCredential(userId, password), onBehalfOf);
string assignmentId = "";  // can be empty if not using multiple assignments
learningPath path = client.GetLearningPath(assignmentId);
foreach (jobProfile p in path.jobProfile)
{
    Console.WriteLine("Job profile: {0}", p.name);
}
```
