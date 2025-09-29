Wikipedia Biography Creator
===========================

**Wikipedia Biography Creator** is a .NET console application that cross-references obituary archives from **The New York Times** and **The Guardian**, applies fuzzy matching to identify overlapping mentions of deceased individuals, and checks whether those individuals already have a biography article on **Wikipedia**.

Results are displayed per month in a clean console UI.

✨ Features
----------

*   Retrieves obituary data from **The New York Times** and **The Guardian** using their public APIs.    
*   Applies **fuzzy text matching** to match obituary subjects across sources.    
*   Handles ambiguities with a **Disambiguation Resolver**.    
*   Queries **Wikipedia** to check for existing biographies.    
*   Displays results per month in a structured **console UI**.    
*   Modular architecture with clear separation of concerns (services, models, interfaces).
