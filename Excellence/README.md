﻿# Excellence C# sample code

# Expanded learning path

The class `NetDimensions.Excellence.Excellence` provides a `GetExpandedLearningPath()` method that returns a
representation of a user's learning path that includes submodules of programmes in which the learner is not
enrolled.

To use, first create an instance of the
[generic client](https://github.com/rmlowe/netdimensions-api-dotnet-client/tree/master/NetDimensionsApis),
then use this to instantiate an `Excellence` instance as demonstrated by the C# code below.

```c#
// Create generic API client instance
string url = "https://www.example.com/ekp/";  // base URL of site
string userId = "myapp";  // ID of privileged user (with Switch User permission)
string password = "changeme";  // password of privileged user (with Switch User permission)
string onBehalfOf = "joestudent";  // ID of the end user
Client client = Client.From(url, new NetworkCredential(userId, password), onBehalfOf);

// Create Excellence wrapper
string assignmentId = "ekp000467350";
Excellence ex = new Excellence(client, assignmentId);

// Get the expanded learning path, and do something with it
learningPath path = ex.GetExpandedLearningPath();
foreach (jobProfile p in path.jobProfile)
{
	foreach (competency c in p.competency) {
		if (c.sequence != null) {
			foreach (sequence s in c.sequence) {
				foreach (item i in s.item) {
					// This could be an unenrolled programme
					if (i.sequence != null) {
						foreach (sequence s2 in i.sequence) {
							foreach (item i2 in s2.item) {
								Console.WriteLine("Submodule: {0}", i2.module.title);
							}
						}
					}
				}
			}
		}
	}
}
```

# Unit refresh

```c#
IEnumerable<Unit> units = ex.GetUnitRefreshAsync().Result;
foreach (Unit unit in units) {
	module programme = unit.Module;  // The programme representing the unit in need of refresh
	competency c = unit.Competency;  // The associated competency
	jobProfile p = unit.JobProfile;  // The associated job profile

	// Do something with these ...
}
```
