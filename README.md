Wikipedia Biography Creator
===========================

**Wikipedia Biography Creator** is a .NET console application that looks for candidates for which to create a biography article on Wikipedia using obituaries as sources. To achieve that it cross-references obituary archives from **The New York Times** and **The Guardian**, applies fuzzy matching to identify overlapping mentions of deceased individuals, and checks whether those individuals already have a biography article on **Wikipedia**.<br>
Results are displayed per month in a clean console UI. Candidates need to checked for existence on Wikipedia manually before creating the actual page.

## ✨ Features
- Fetches obituaries from:
  - 📰 The Guardian API  
  - 🗞️ The New York Times API    
- Applies **fuzzy text matching** to match obituary subjects across sources.    
- Handles ambiguities with a **Disambiguation Resolver**.    
- Queries **Wikipedia** to check for existing biographies.    
- Displays results per month in a structured **console UI**.    
- Modular architecture with clear separation of concerns (services, models, interfaces).

## 🧱 Prerequisites

Before building the project, make sure you have:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) or later  
- Internet connection (the app queries live APIs)  
- API keys for:
  - **[The Guardian Open Platform](https://open-platform.theguardian.com/access/)**
  - **[The New York Times Archive API](https://developer.nytimes.com/apis)**

## ⚙️ Configuration

Create or edit the file **`appsettings.json`** in the project root.

Here’s an example configuration:

```json
{
  "GuardianApi": {
    "ApiKey": "YOUR_GUARDIAN_API_KEY",
    "BaseUrl": "https://content.guardianapis.com"
  },
  "NYTimesApi": {
    "ApiKey": "YOUR_NYTIMES_API_KEY",
    "BaseUrl": "https://api.nytimes.com"
  },
  "WikipediaApi": {
    "BaseUrl": "https://en.wikipedia.org/w/api.php"
  },
  "Fuzzy search": {
    "Score threshold": 85
  }
}
```

💡 **Note:**
- **API keys are required for The Guardian and The New York Times services.**
- The Wikipedia API is public and does not require a key.
- The Fuzzy search score threshold determines when names of deceased in the two sources are matched.

## 🏗️ Building and running the Application

From the project root:
`
dotnet build
`<br>
This will restore dependencies and compile the project.

To run it:
`
dotnet run
`<br>
The UI should explain itself.

## 🪪 License

MIT License © 2025
