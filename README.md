# RaceDay System - Frontend (MVC)
 
RaceDay is a comprehensive web application designed to streamline the management and participation of racing events. This repository contains the ASP.NET Core MVC frontend, providing dedicated portals for both Event Organisers and Participants.
 
## Features
 
* **Participant Portal:** Browse upcoming races, manage event enrolments, view personal race results, and update profile information (including Azure Blob Storage profile picture uploads).

* **Organiser Dashboard:** Create new race categories, monitor event analytics, view participant enrolments, and publish official race results.

* **Role-Based Access Control:** Secure routing and views tailored to the user's authenticated role.
 
## Prerequisites
 
* [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

* Docker Desktop (optional, for containerised execution)
 
## Docker Containerisation
 
This application is fully containerised. To build and run the RaceDay MVC frontend locally using Docker, execute the following commands from your terminal:
 
### 1. Build the Image
 
Navigate into the MVC project directory and build the image:
 
```bash

cd RaceDayMVC

docker build -t raceday-mvc .
### 2. Run the Container
 
```bash
docker run -d -p 8080:8080 --name raceday-frontend raceday-mvc
```
 
The application will be accessible at http://localhost:8080
